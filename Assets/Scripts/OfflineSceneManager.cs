using TMPro;
using UnityEngine;

public class OfflineSceneManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI message;

    private void Start() {
        switch (Global.GameFinishCode) {
            case 1: // first player won
                if (Global.localPlayerIndex == 1) {
                    DisplayWin();
                }
                else {
                    DisplayLose();
                }
                break;
            case 2: // second player won
                if (Global.localPlayerIndex == 2) {
                    DisplayWin();
                }
                else {
                    DisplayLose();
                }
                break;
            case 0: // match cancelled
                DisplayCancel();
                break;
            case -1: // server did not send results
                DisplayError();
                break;
            default:
                break;
        }
    }

    public void DisplayWin() {
        message.text = "YOU WON!\nYou can close the game now.";
    }
    public void DisplayLose() {
        message.text = "YOU LOST!\nYou can close the game now.";
    }
    public void DisplayCancel() {
        message.text = "Match Cancelled.\nYou can close the game now.";
    }
    public void DisplayError() {
        message.text = "Unknown Error occured.\nPlease reach out to the admin.";
    }
}
