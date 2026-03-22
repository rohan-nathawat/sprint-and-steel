using UnityEngine;

// ─────────────────────────────────────────────────────────────
//  TutorialEnd.cs
//  Attach to your endpoint GameObject (the one with the
//  BoxCollider2D already on it — make sure Is Trigger is ticked)
// ─────────────────────────────────────────────────────────────
public class TutorialEnd : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Exact name of your Hub scene in Build Settings")]
    public string hubSceneName = "Hub";

    [Header("Settings")]
    public string playerTag = "Player";

    bool _triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;

        SceneTransitionManager.FreezePlayerInput(other.gameObject);

        SceneTransitionManager.LoadScene(hubSceneName);
    }
}
