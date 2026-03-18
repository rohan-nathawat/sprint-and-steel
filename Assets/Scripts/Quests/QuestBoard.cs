using UnityEngine;

// ─────────────────────────────────────────────────────────────
//  QuestBoard.cs
//  Attach to your Quest Board GameObject in the scene.
//  Requires a Collider2D set to Is Trigger on the same object
//  (or a child) to detect when the player is nearby.
// ─────────────────────────────────────────────────────────────
public class QuestBoard : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How close the player needs to be to interact.")]
    public float interactRange = 1.5f;

    [Tooltip("Tag on your player GameObject.")]
    public string playerTag = "Player";

    [Header("Prompt (optional)")]
    [Tooltip("A world-space UI prompt like '[E] Read Board'. Leave empty to skip.")]
    public GameObject interactPrompt;

    bool _playerInRange = false;

    // ─────────────────────────────────────────────────────────

    void Start()
    {
        if (interactPrompt)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (!_playerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            QuestBoardUI ui = FindObjectOfType<QuestBoardUI>();
            if (ui == null)
            {
                Debug.LogWarning("[QuestBoard] No QuestBoardUI found in scene.");
                return;
            }

            // Toggle: open if closed, close if open
            if (ui.boardRoot.activeSelf)
                ui.Hide();
            else
                ui.Show();
        }
    }

    // ── Trigger detection ────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInRange = true;
        if (interactPrompt) interactPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInRange = false;
        if (interactPrompt) interactPrompt.SetActive(false);

        // Close the board if player walks away
        QuestBoardUI ui = FindObjectOfType<QuestBoardUI>();
        if (ui != null) ui.Hide();
    }
}
