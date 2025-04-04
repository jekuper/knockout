using Mirror;
using UnityEngine;

public class ServerSharedInfo : NetworkBehaviour
{
    [SyncVar()]
    public string Player1Name = "";
    [SyncVar()]
    public string Player2Name = "";

    public static ServerSharedInfo Instance;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Multiple Server Shared Info objects.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        if (NetworkServer.active) {
            Player1Name = Global.Player1Name;
            Player2Name = Global.Player2Name;
        }
    }
}
