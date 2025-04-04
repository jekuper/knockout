using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerKnockout : NetworkBehaviour {
    public static GameManagerKnockout Instance;

    public List<PuckManager> allPucks = new List<PuckManager>();
    public Transform GameFieldSquare;
    public float TimeToThink = 15f;
    public float TimeToShow = 3f;
    public float VelocityEps = 0.05f;

    public float Timer => timer;
    public GameState CurrentState => currentState;

    private Dictionary<uint, Vector2> savedTurns = new Dictionary<uint, Vector2>();
    private List<Rigidbody2D> allPucksRb = new List<Rigidbody2D>();

    public enum GameState { WaitingForInput, ShowingAllInput, ProcessingTurn, WaitingForPucks, EvaluateWinner, Resetting }
    
    [SyncVar()]
    private GameState currentState = GameState.WaitingForInput;
    [SyncVar()]
    private float timer;

    private void Start() {
        if (Instance != null) {
            Debug.LogError("Multiple instances of GameManager!!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadPucks();

        if (isServer)
            StartCoroutine(StateMachine());
    }

    private void LoadPucks() {
        foreach (var puck in allPucks) {
            allPucksRb.Add(puck.GetComponent<Rigidbody2D>());
            allPucksRb[allPucksRb.Count - 1].bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private IEnumerator StateMachine() {
        while (true) {
            Debug.Log($"Current State: {currentState}");
            switch (currentState) {
                case GameState.WaitingForInput:
                    yield return StartCoroutine(WaitForPlayerInput());
                    break;
                case GameState.ShowingAllInput:
                    yield return StartCoroutine(ShowAllInput());
                    break;
                case GameState.ProcessingTurn:
                    yield return StartCoroutine(ProcessTurn());
                    break;
                case GameState.WaitingForPucks:
                    yield return StartCoroutine(WaitForPucksToStop());
                    break;
                case GameState.EvaluateWinner:
                    yield return StartCoroutine(FindWinner());
                    break;
                case GameState.Resetting:
                    yield return StartCoroutine(ResetRound());
                    break;
            }
        }
    }

    private IEnumerator WaitForPlayerInput() {
        timer = TimeToThink;
        while (timer > 0) {
            timer -= Time.deltaTime;

            if (savedTurns.Count == allPucks.Count) {
                Debug.Log($"All {allPucks.Count} pucks have a stored charge. Skipping Wait.");
                break;
            }

            yield return null;
        }
        currentState = GameState.ShowingAllInput;
    }

    private IEnumerator ShowAllInput() {
        foreach (var puckInfo in savedTurns) {
            RpcShowPuckCharge(puckInfo.Key, puckInfo.Value);
        }
        yield return new WaitForSeconds(TimeToShow);
        currentState = GameState.ProcessingTurn;
    }

    private IEnumerator ProcessTurn() {
        RpcClearAllPuckCharges();

        foreach (var puck in allPucksRb) {
            puck.bodyType = RigidbodyType2D.Dynamic;
            if (savedTurns.TryGetValue(puck.GetComponent<NetworkIdentity>().netId, out Vector2 force)) {
                puck.AddForce(-force, ForceMode2D.Impulse);
            }
        }
        currentState = GameState.WaitingForPucks;
        yield return null;
    }

    private IEnumerator WaitForPucksToStop() {
        bool allStopped;
        do {
            allStopped = true;
            List<int> toRemove = new List<int>(); // List to store indices of pucks to be removed

            for (int i = allPucksRb.Count - 1; i >= 0; i--) { // Iterate backward for safe removal
                var puck = allPucksRb[i];

                if (!IsWithinGameField(puck.GetComponent<CircleCollider2D>())) {
                    Debug.Log($"Destroying {puck.name} while it is moving");
                    NetworkServer.Destroy(puck.gameObject);
                    toRemove.Add(i);
                }

                if (puck.linearVelocity.magnitude > VelocityEps) {
                    allStopped = false;
                }
            }

            // Remove destroyed pucks after iteration
            foreach (var index in toRemove) {
                allPucks.RemoveAt(index);
                allPucksRb.RemoveAt(index);
            }

            yield return null;
        } while (!allStopped);

        List<int> finalRemove = new List<int>();

        for (int i = allPucksRb.Count - 1; i >= 0; i--) { // Iterate backward again for final cleanup
            var puck = allPucksRb[i];
            puck.linearVelocity = Vector2.zero;
            puck.bodyType = RigidbodyType2D.Kinematic;

            if (!IsWithinGameField(puck.GetComponent<CircleCollider2D>())) {
                Debug.Log($"Destroying {puck.name}");
                NetworkServer.Destroy(puck.gameObject);
                finalRemove.Add(i);
            }
        }

        // Remove remaining destroyed pucks
        foreach (var index in finalRemove) {
            allPucks.RemoveAt(index);
            allPucksRb.RemoveAt(index);
        }

        currentState = GameState.EvaluateWinner;
    }

    private IEnumerator FindWinner() {
        bool player1Alive = false;
        bool player2Alive = false;
        foreach (var puck in allPucks) {
            if (puck.ownerPlayer == 1)
                player1Alive = true;
            if (puck.ownerPlayer == 2)
                player2Alive = true;
        }

        if (player1Alive && !player2Alive) {
            Debug.Log("player 1 won");
            StartCoroutine(Global.QuitWithPlayer1Winner());
        }
        if (player2Alive && !player1Alive) {
            Debug.Log("player 1 won");
            StartCoroutine(Global.QuitWithPlayer2Winner());
        }

        currentState = GameState.Resetting;
        yield return null;
    }


    private IEnumerator ResetRound() {
        savedTurns.Clear();
        yield return new WaitForSeconds(3);
        RpcUnlockAll();
        currentState = GameState.WaitingForInput;
    }

    private bool IsWithinGameField(CircleCollider2D puckCollider) {
        BoxCollider2D fieldCollider = GameFieldSquare.GetComponent<BoxCollider2D>();

        Vector2 puckCenter = puckCollider.bounds.center; // Get puck center (ignores Z)
        float puckRadius = puckCollider.radius * puckCollider.transform.lossyScale.x; // Adjust for scale

        Vector2 fieldMin = fieldCollider.bounds.min; // Bottom-left corner of field
        Vector2 fieldMax = fieldCollider.bounds.max; // Top-right corner of field

        // Clamp puck's center to the nearest point on the rectangle
        float clampedX = Mathf.Clamp(puckCenter.x, fieldMin.x, fieldMax.x);
        float clampedY = Mathf.Clamp(puckCenter.y, fieldMin.y, fieldMax.y);
        Vector2 closestPoint = new Vector2(clampedX, clampedY);

        // Check if the distance from the puck's center to this point is <= radius
        return Vector2.Distance(puckCenter, closestPoint) <= puckRadius;
    }

    [Server]
    public void StorePlayerAction(NetworkIdentity playerIdentity, uint puckNetId, Vector2 direction, float strength) {
        if (currentState != GameState.WaitingForInput) {
            Debug.LogWarning($"Discarding puck due to game state {currentState}");
            return;
        }

        int playerIndex = -1;
        if (playerIdentity.netId == NetworkManagerKnockout.singleton.Player1Object.netId) {
            playerIndex = 1;
        }
        if (playerIdentity.netId == NetworkManagerKnockout.singleton.Player2Object.netId) {
            playerIndex = 2;
        }
        if (playerIndex == -1) {
            Debug.Log("Storing data failed, invalid player");
            return;
        }

        try {
            PuckManager puck = NetworkServer.spawned[puckNetId].GetComponent<PuckManager>();
            if (puck.ownerPlayer != playerIndex) {
                Debug.LogWarning($"This player does not own this puck. Puck: {puck.name}, Player: {playerIndex}");
                return;
            }

            // Validate on server
            strength = Mathf.Min(strength, Global.MaxPuckStrength);
            direction.Normalize();

            if (savedTurns.ContainsKey(puck.netId)) {
                Debug.Log($"Discarding dublicate puck charge info. Puck {puck.name}, Player: {playerIndex}");
                return;
            }
            else {
                Vector2 charge = direction * strength;
                Debug.Log($"Storing puck info. Puck {puck.name} {charge}");

                savedTurns.Add(puck.netId, charge);
                playerIdentity.GetComponent<PlayerManager>().TargetPuckChargeSaved(playerIdentity.connectionToClient, puck.netId, charge);
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    [ClientRpc]
    private void RpcShowPuckCharge(uint puckNetId, Vector2 charge) {
        PuckManager puck = NetworkClient.spawned[puckNetId].GetComponent<PuckManager>();
        puck.LockCharge(charge);
    }

    [ClientRpc]
    private void RpcClearAllPuckCharges() {
        foreach (var puck in allPucks) {
            puck.HideArrow();
        }
    }

    [ClientRpc]
    private void RpcUnlockAll() {
        foreach (var puck in allPucks) {
            puck.Unlock();
        }
    }


    [ClientRpc]
    public void RpcNotifyGameResults(int code) {
        Global.GameFinishCode = code;
    }
}
