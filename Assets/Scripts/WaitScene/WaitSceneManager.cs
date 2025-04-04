using Mirror;
using UnityEngine;

public class WaitSceneManager : MonoBehaviour {
    public float TimeoutAfterFirstConnect = 20f;
    public float TimeoutMax = 180f;
    private float timer = 0f;
    private float timerMax = 0f;
    private bool switched = false;

    private void Update() {
        if (!NetworkServer.active) {
            return;
        }

        timerMax += Time.deltaTime;
        if (NetworkManagerKnockout.singleton.IsPlayer1Authenticated ||
            NetworkManagerKnockout.singleton.IsPlayer2Authenticated) {
            // Update the timer only if one is connected
            timer += Time.deltaTime;
        }
        // Check for both players being authenticated
        if (!switched &&
            NetworkManagerKnockout.singleton.IsPlayer1Authenticated &&
            NetworkManagerKnockout.singleton.IsPlayer2Authenticated) {
            NetworkManagerKnockout.singleton.ServerChangeScene("Level");
            switched = true;
        }

        // Check for timeout
        if (!switched && timer >= TimeoutAfterFirstConnect) {
            StartCoroutine(Global.QuitWithMatchCancel());
            switched = true;
        }

        if (!switched && timerMax >= TimeoutMax) {
            StartCoroutine(Global.QuitWithMatchCancel());
            switched = true;
        }
    }
}
