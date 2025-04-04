using Mirror;
using System;
using System.Collections;
using UnityEngine;

public static class Global {
    public static float MaxPuckStrength = 3f; // Maximum allowed drag strength
    public static int localPlayerIndex = -1;

    public static string Player1Name;
    public static string Player2Name;

    public static int GameFinishCode = -1;



    public static IEnumerator QuitWithPlayer1Winner() {
        Debug.Log("Quit player 1 win");
        try {
            ServerToAllClientsComm.Instance.RpcSendGameResult(1);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
        yield return new WaitForSecondsRealtime(3f);
        Application.Quit(1001); // first player win
    }
    public static IEnumerator QuitWithPlayer2Winner() {
        Debug.Log("Quit player 2 win");
        try {
            ServerToAllClientsComm.Instance.RpcSendGameResult(2);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
        yield return new WaitForSecondsRealtime(3f);
        Application.Quit(1002); // first player win
    }
    public static IEnumerator QuitWithMatchCancel() {
        Debug.Log("Quit match cancel");
        try {
            ServerToAllClientsComm.Instance.RpcSendGameResult(0);
        } catch (Exception e){
            Debug.LogException(e);
        }
        yield return new WaitForSecondsRealtime(3f);
        Application.Quit(1000); // if the match cancelled
    }
}
