using Mirror;
using UnityEngine;

public class WaitSceneManager : MonoBehaviour
{
    bool switched = false;

    private void Update() {
        if (!switched && NetworkManagerKnockout.singleton.IsPlayer1Authenticated && NetworkManagerKnockout.singleton.IsPlayer2Authenticated) {
            NetworkManagerKnockout.singleton.ServerChangeScene("Level");
            switched = true;
        }
    }
}
