using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

[AddComponentMenu("Network/ Authenticators/Wagr Authenticator")]
public class WagrAuthenticator: NetworkAuthenticator
{
    [Header("Server Side Client Tokens")]
    public string Player1Token = null;
    public string Player2Token = null;

    [Header("Cleint Side Token")]
    public string LocalToken = null;

    readonly HashSet<NetworkConnectionToClient> connectionsPendingDisconnect = new HashSet<NetworkConnectionToClient>();

    #region Messages

    public struct AuthRequestMessage : NetworkMessage {
        public string authToken; // user identification token from backend
    }

    public struct AuthResponseMessage : NetworkMessage {
        public byte code;
        public byte playerIndex;
        public string message;
    }

    #endregion

    #region Server

#if UNITY_SERVER
    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer() {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from StopServer to reset the Authenticator
    /// <para>Server message handlers should be unregistered in this method.</para>
    /// </summary>
    public override void OnStopServer() {
        // unregister the handler for the authentication request
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    /// <summary>
    /// Called on server from OnServerConnectInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn) {
        // do nothing...wait for AuthRequestMessage from client
    }

    /// <summary>
    /// Called on server when the client's AuthRequestMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg) {
        Debug.Log($"Authentication Request: {msg.authToken}");

        if (connectionsPendingDisconnect.Contains(conn)) return;

        if (string.IsNullOrEmpty(Player1Token) || string.IsNullOrEmpty(Player2Token)) {
            Debug.LogError("Player Tokens are not set!!!");

            connectionsPendingDisconnect.Add(conn);

            // create and send msg to client so it knows to disconnect
            AuthResponseMessage authResponseMessage = new AuthResponseMessage {
                code = 200,
                message = "Invalid Credentials"
            };

            conn.Send(authResponseMessage);

            // must set NetworkConnection isAuthenticated = false
            conn.isAuthenticated = false;

            // disconnect the client after 1 second so that response message gets delivered
            StartCoroutine(DelayedDisconnect(conn, 1f));

            return;
        }


        if ((!NetworkManagerKnockout.singleton.IsPlayer1Authenticated && msg.authToken == Player1Token)) {
            // create and send msg to client so it knows to proceed
            AuthResponseMessage authResponseMessage = new AuthResponseMessage {
                code = 100,
                playerIndex = 1,
                message = "Success"
            };

            Debug.Log($"Authentication request with token {msg.authToken} accepted as Player 1.");

            NetworkManagerKnockout.singleton.Player1Connection = conn.connectionId;
            conn.Send(authResponseMessage);
            NetworkManagerKnockout.singleton.IsPlayer1Authenticated = true;

            ServerAccept(conn);
        }
        else if (!NetworkManagerKnockout.singleton.IsPlayer2Authenticated && msg.authToken == Player2Token) {
            // create and send msg to client so it knows to proceed
            AuthResponseMessage authResponseMessage = new AuthResponseMessage {
                code = 100,
                playerIndex = 2,
                message = "Success"
            };

            Debug.Log($"Authentication request with token {msg.authToken} accepted as Player 2");

            NetworkManagerKnockout.singleton.Player2Connection = conn.connectionId;
            conn.Send(authResponseMessage);
            NetworkManagerKnockout.singleton.IsPlayer2Authenticated = true;

            ServerAccept(conn);
        }
        else {
            connectionsPendingDisconnect.Add(conn);

            // create and send msg to client so it knows to disconnect
            AuthResponseMessage authResponseMessage = new AuthResponseMessage {
                code = 200,
                message = "Invalid Credentials"
            };

            Debug.Log($"Authentication request with token {msg.authToken} denied");

            conn.Send(authResponseMessage);

            // must set NetworkConnection isAuthenticated = false
            conn.isAuthenticated = false;

            // disconnect the client after 1 second so that response message gets delivered
            StartCoroutine(DelayedDisconnect(conn, 1f));
        }
    }

    IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime) {
        yield return new WaitForSeconds(waitTime);

        // Reject the unsuccessful authentication
        ServerReject(conn);

        yield return null;

        // remove conn from pending connections
        connectionsPendingDisconnect.Remove(conn);
    }
#endif
    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient() {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Called on client from StopClient to reset the Authenticator
    /// <para>Client message handlers should be unregistered in this method.</para>
    /// </summary>
    public override void OnStopClient() {
        // unregister the handler for the authentication response
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    /// <summary>
    /// Called on client from OnClientConnectInternal when a client needs to authenticate
    /// </summary>
    public override void OnClientAuthenticate() {
        AuthRequestMessage authRequestMessage = new AuthRequestMessage {
            authToken = LocalToken,
        };


        Debug.Log($"Sending local token \"{LocalToken}\" to server");
        NetworkClient.Send(authRequestMessage);
    }

    /// <summary>
    /// Called on client when the server's AuthResponseMessage arrives
    /// </summary>
    /// <param name="msg">The message payload</param>
    public void OnAuthResponseMessage(AuthResponseMessage msg) {
        if (msg.code == 100) {
            //Debug.Log($"Authentication Response: {msg.message}");

            Global.localPlayerIndex = msg.playerIndex;
            // Authentication has been accepted
            ClientAccept();
        }
        else {
            Debug.LogError($"Authentication Response: {msg.message}");

            // Authentication has been rejected
            ClientReject();
        }
    }

    #endregion
}
