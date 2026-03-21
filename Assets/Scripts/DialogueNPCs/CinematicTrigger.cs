using UnityEngine;

// ─────────────────────────────────────────────────────────────
//  CinematicTrigger.cs
//  Place one of these on each trigger zone in your scene.
//  Set which line index it fires in the Inspector.
//  Add a BoxCollider2D set to Is Trigger on the same object.
// ─────────────────────────────────────────────────────────────
public class CinematicTrigger : MonoBehaviour
{
    [Header("Which line to play (matches index in CinematicText.lines)")]
    public int lineIndex = 0;

    [Header("References")]
    public CinematicText cinematicText;

    [Header("Settings")]
    public string playerTag   = "Player";
    public bool   triggerOnce = true;     // prevent replaying if player walks back

    bool _triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        cinematicText.PlayLine(lineIndex);
    }
}
