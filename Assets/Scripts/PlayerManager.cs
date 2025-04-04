using Mirror;
using UnityEngine;

public class PlayerManager : NetworkBehaviour {

    [Command]
    public void CmdRegisterDragData(uint puckNetId, Vector2 direction, float strength) {
        // Save the action for the turn
        GameManagerKnockout.Instance.StorePlayerAction(netIdentity, puckNetId, direction, strength);
    }

    [TargetRpc]
    public void TargetPuckChargeSaved(NetworkConnectionToClient target, uint puckNetId, Vector2 chargeSaved) {
        PuckManager puck = NetworkClient.spawned[puckNetId].GetComponent<PuckManager>();
        puck.LockCharge(chargeSaved);
    }
}
