using UnityEngine;
using UnityEngine.SceneManagement;

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        SceneManager.LoadScene(hubSceneName);
    }
}
