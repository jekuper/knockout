using Mirror;
using UnityEngine;

public class ServerToAllClientsComm : NetworkBehaviour {
    public static ServerToAllClientsComm Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStartClient() {
        base.OnStartClient();
    }

    [ClientRpc]
    public void RpcSendGameResult(int code) {
        Global.GameFinishCode = code;
    }

    [ClientRpc]
    public void RpcSendUsernames(string username1, string username2) {
        Debug.LogError("RECEIVED RPC");
        Global.Player1Name = username1;
        Global.Player2Name = username2;
    }
}
