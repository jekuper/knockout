using UnityEngine;
using System;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username1;
    [SerializeField] private TextMeshProUGUI username2;
    [SerializeField] private TextMeshProUGUI timer;

    private void Start() {
        username1.text = Global.username1;
        username2.text = Global.username2;
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
