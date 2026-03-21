using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  CinematicTextSetup.cs
//  Tools > Sprint & Steel > Build Cinematic Text Canvas
// ─────────────────────────────────────────────────────────────
#if UNITY_EDITOR
using UnityEditor;

public static class CinematicTextSetup
{
    [MenuItem("Tools/Sprint & Steel/Build Cinematic Text Canvas")]
    public static void Build()
    {
        // ── Canvas ──────────────────────────────────────────
        var canvasGO = new GameObject("Cinematic_Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // above everything
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var ct = canvasGO.AddComponent<CinematicText>();

        // ── Text object — centred, lower third like BOTW ────
        var textGO = new GameObject("CinematicLabel");
        textGO.transform.SetParent(canvasGO.transform, false);
        var rt = textGO.AddComponent<RectTransform>();

        // Position in lower third of screen
        rt.anchorMin        = new Vector2(0.2f, 0.15f);
        rt.anchorMax        = new Vector2(0.8f, 0.35f);
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text            = "";
        tmp.fontSize        = 28f;
        tmp.color           = new Color(1f, 1f, 1f, 0f); // start invisible
        tmp.alignment       = TextAlignmentOptions.MidlineLeft;
        tmp.lineSpacing     = 8f;
        tmp.raycastTarget   = false;
        // Soft italic for cinematic feel — toggle off if you prefer upright
        tmp.fontStyle       = FontStyles.Italic;

        // ── Wire up ──────────────────────────────────────────
        ct.cinematicLabel = tmp;

        // Pre-populate a couple of example lines so the array
        // isn't empty when the developer opens the Inspector
        ct.lines = new CinematicLine[]
        {
            new CinematicLine
            {
                text             = "We need to prepare you to protect us.",
                delayBeforeFade  = 1.2f,
                fadeInDuration   = 1.5f,
                holdDuration     = 3.5f,
                fadeOutDuration  = 1.2f
            },
            new CinematicLine
            {
                text             = "This training ground will teach you everything.",
                delayBeforeFade  = 0.8f,
                fadeInDuration   = 1.5f,
                holdDuration     = 3.5f,
                fadeOutDuration  = 1.2f
            }
        };

        Selection.activeGameObject = canvasGO;
        Debug.Log("[Sprint & Steel] Cinematic Canvas built. Drag your HUD canvases into the Huds To Hide array on CinematicText.");
    }
}
#endif
