using Mirror;
using UnityEngine;

public class PuckManager : NetworkBehaviour {
    public int ownerPlayer = -1;
    public GameObject arrowPrefab; // Assign this in the Inspector

    private GameObject arrowInstance;
    private SpriteRenderer arrowSpriteRenderer;
    private Vector2 dragStart;
    private Vector2 dragEnd;
    private bool isDragging = false;

    private bool isLocked = false;

    private void Start() {
        if (arrowPrefab != null) {
            arrowInstance = Instantiate(arrowPrefab);
            arrowSpriteRenderer = arrowInstance.GetComponent<SpriteRenderer>();
            arrowInstance.SetActive(false);
        }
    }

    private void OnMouseDown() {
        if (isLocked || ownerPlayer != Global.localPlayerIndex) return;

        dragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    private void OnMouseDrag() {
        if (isLocked || !isDragging || ownerPlayer != Global.localPlayerIndex) return;

        dragEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVector = dragEnd - dragStart;

        // Update arrow appearance
        arrowInstance.SetActive(true);
        arrowInstance.transform.position = transform.position; // Match puck position
        arrowInstance.transform.right = -dragVector; // Point opposite to drag direction

        // Adjust width based on distance
        float distance = dragVector.magnitude;
        arrowSpriteRenderer.size = new Vector2(distance, arrowSpriteRenderer.size.y);
    }

    private void OnMouseUp() {
        if (isLocked || !isDragging || ownerPlayer != Global.localPlayerIndex) return;

        isDragging = false;
        arrowInstance.SetActive(false);

        Vector2 dragVector = dragEnd - dragStart;
        float strength = Mathf.Min(dragVector.magnitude, Global.MaxPuckStrength);
        Vector2 direction = dragVector.normalized;

        NetworkClient.localPlayer.GetComponent<PlayerManager>().CmdRegisterDragData(netId, direction, strength);
    }



    public void LockCharge(Vector2 charge) {
        isLocked = true;
        arrowInstance.SetActive(true);
        arrowInstance.transform.position = transform.position;
        arrowInstance.transform.right = -charge;

        // Adjust width based on distance
        float distance = charge.magnitude;
        arrowSpriteRenderer.size = new Vector2(distance, arrowSpriteRenderer.size.y);
    }

    public void Unlock() {
        isLocked = false;
        HideArrow();
    }

    public void HideArrow() {
        arrowInstance.SetActive(false);
    }
}
