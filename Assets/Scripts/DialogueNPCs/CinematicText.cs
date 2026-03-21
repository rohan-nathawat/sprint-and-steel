using System.Collections;
using UnityEngine;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  CinematicText.cs
//  Attach to your Cinematic_Canvas.
//  Set up each line + its trigger in the Inspector.
// ─────────────────────────────────────────────────────────────

[System.Serializable]
public class CinematicLine
{
    [TextArea(2, 4)]
    public string text;
    public float delayBeforeFade  = 1f;   // pause before text appears
    public float fadeInDuration   = 1.5f;
    public float holdDuration     = 3.5f;
    public float fadeOutDuration  = 1.2f;
}

public class CinematicText : MonoBehaviour
{
    [Header("Text Object")]
    public TextMeshProUGUI cinematicLabel;

    [Header("Lines — each one triggered by its own TriggerZone")]
    public CinematicLine[] lines;

    [Header("HUD Canvases to hide while text is showing")]
    public Canvas[] hudsToHide;

    // Called by CinematicTrigger when player enters zone
    public void PlayLine(int index)
    {
        if (index < 0 || index >= lines.Length) return;
        StartCoroutine(RunLine(lines[index]));
    }

    IEnumerator RunLine(CinematicLine line)
    {
        SetHUDs(false);
        SetAlpha(0f);
        cinematicLabel.text = line.text;

        // Delay before fade in
        yield return new WaitForSeconds(line.delayBeforeFade);

        // Fade in
        yield return StartCoroutine(Fade(0f, 1f, line.fadeInDuration));

        // Hold
        yield return new WaitForSeconds(line.holdDuration);

        // Fade out
        yield return StartCoroutine(Fade(1f, 0f, line.fadeOutDuration));

        SetHUDs(true);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        Color c = cinematicLabel.color;
        c.a = a;
        cinematicLabel.color = c;
    }

    void SetHUDs(bool visible)
    {
        foreach (Canvas hud in hudsToHide)
            if (hud != null) hud.enabled = visible;
    }
}
