using UnityEngine;
using System;
using TMPro;
using Mirror;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username1;
    [SerializeField] private TextMeshProUGUI username2;
    [SerializeField] private TextMeshProUGUI timer;

    private void Start() {
        if (NetworkClient.active) {
            username1.text = Global.Player1Name;
            username2.text = Global.Player2Name;
        }
    }

    private void Update() {
        if (GameManagerKnockout.Instance != null) {
            if (GameManagerKnockout.Instance.CurrentState == GameManagerKnockout.GameState.WaitingForInput) {
                timer.text = GameManagerKnockout.Instance.Timer.ToString("F1");
            }
            else {
                timer.text = "";
            }
        }
    }
}
