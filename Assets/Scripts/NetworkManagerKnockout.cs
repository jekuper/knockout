using Mirror;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class NetworkManagerKnockout : NetworkManager {
    public int Player1Connection = -1;
    public int Player2Connection = -1;
    public bool AreAllReady {
        get { return NetworkServer.connections.All(connection => connection.Value.isReady); }
    }


    //private int totalPlayersChangingScene = 0;
    //private int readyPlayers = 0;

    public NetworkIdentity Player1Object {
        get {
            if (Player1Connection == -1 || !NetworkServer.connections.ContainsKey(Player1Connection))
                return null;
            return NetworkServer.connections[Player1Connection].identity;
        }
    }
    public NetworkIdentity Player2Object {
        get {
            if (Player2Connection == -1 || !NetworkServer.connections.ContainsKey(Player2Connection))
                return null;
            return NetworkServer.connections[Player2Connection].identity;
        }
    }

    public bool IsPlayer1Authenticated = false;
    public bool IsPlayer2Authenticated = false;

    public static new NetworkManagerKnockout singleton => NetworkManager.singleton as NetworkManagerKnockout;

    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        
        if (IsPlayer1Authenticated && IsPlayer2Authenticated) {
            Debug.Log("Both player authenticated but one left. Settings a winner for other");

            if (conn.connectionId == Player1Connection) {
                StartCoroutine(Global.QuitWithPlayer2Winner());
                return;
            }
            if (conn.connectionId == Player2Connection) {
                StartCoroutine(Global.QuitWithPlayer1Winner());
                return;
            }
        }
        else {
            StartCoroutine(Global.QuitWithMatchCancel());
            return;
        }

        if (conn.identity.netId == Player1Object.netId) {
            Debug.Log("Player 1 Disconnected!");
            IsPlayer1Authenticated = false;
            Player1Connection = -1;
        }
        if (conn.identity.netId == Player2Object.netId) {
            Debug.Log("Player 2 Disconnected!");
            IsPlayer2Authenticated = false;
            Player2Connection = -1;
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
    }

    //public override void OnServerChangeScene(string newSceneName) {
    //    Debug.Log("Changing scene");
    //    totalPlayersChangingScene = NetworkServer.connections.Count;
    //    readyPlayers = 0;
    //    base.OnServerChangeScene(newSceneName);
    //}

    //public override void OnServerReady(NetworkConnectionToClient conn) {
    //    readyPlayers++;
    //    Debug.Log($"Another client ready conn={conn.connectionId} {readyPlayers} {totalPlayersChangingScene}");
    //    base.OnServerReady(conn);
    //}
}
