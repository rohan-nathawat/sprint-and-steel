using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  HUDSetup.cs
//  Run this ONCE in Editor via  Tools > Sprint & Steel > Build HUD
//  It creates the full Canvas hierarchy so you don't have to
//  manually build it in the Inspector.
// ─────────────────────────────────────────────────────────────
#if UNITY_EDITOR
using UnityEditor;

public static class HUDSetupMenu
{
    const float HealthBarWidth = 320f;
    const float MeterBarWidth = 292f;
    const float MeterIconSize = 24f;
    const float MeterSpacing = 12f;
    const float MeterTextWidth = MeterBarWidth + MeterIconSize + MeterSpacing;

    [MenuItem("Tools/Sprint & Steel/Build HUD")]
    public static void BuildHUD()
    {
        GameObject existingCanvas = GameObject.Find("HUD_Canvas");
        if (existingCanvas != null)
            Object.DestroyImmediate(existingCanvas);

        GameObject canvasGO = new GameObject("HUD_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        HUDManager hud = canvasGO.AddComponent<HUDManager>();

        GameObject topLeft = MakeAnchoredGroup(
            "TopLeft",
            canvasGO.transform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(36f, -36f));

        MakeLabel("LBL_Health", topLeft.transform, "HEALTH", HealthBarWidth);
        var (_, hFill) = MakeBar("Bar_Health", topLeft.transform, new Color(0.878f, 0.361f, 0.478f), 1f, HealthBarWidth);
        hud.healthBarFill = hFill;

        GameObject hPctGO = MakeFixedWidthText("Pct_Health", topLeft.transform, "100%", 20, new Color(0.878f, 0.361f, 0.478f), HealthBarWidth, TextAlignmentOptions.Center);
        hud.healthPercentText = hPctGO.GetComponent<TextMeshProUGUI>();

        GameObject bottomLeft = MakeAnchoredGroup(
            "BottomLeft",
            canvasGO.transform,
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(36f, 36f));

        MakeLabel("LBL_Sprint", bottomLeft.transform, "SPRINT", MeterTextWidth);
        var (_, sFill) = MakeBarWithIcon("Bar_Sprint", bottomLeft.transform,
            new Color(0.361f, 0.878f, 0.753f), 1f, MeterBarWidth, out Image sprintIcon);
        hud.sprintBarFill = sFill;
        hud.sprintIconSquare = sprintIcon;
        GameObject sStatusGO = MakeFixedWidthText("Status_Sprint", bottomLeft.transform, "READY", 18, new Color(0.361f, 0.878f, 0.753f), MeterTextWidth, TextAlignmentOptions.Center);
        hud.sprintStatusText = sStatusGO.GetComponent<TextMeshProUGUI>();

        MakeLabel("LBL_Attack", bottomLeft.transform, "ATTACK", MeterTextWidth);
        var (_, aFill) = MakeBarWithIcon("Bar_Attack", bottomLeft.transform,
            new Color(0.878f, 0.722f, 0.361f), 1f, MeterBarWidth, out Image attackIcon);
        hud.attackBarFill = aFill;
        hud.attackIconSquare = attackIcon;
        GameObject aStatusGO = MakeFixedWidthText("Status_Attack", bottomLeft.transform, "READY", 18, new Color(0.878f, 0.722f, 0.361f), MeterTextWidth, TextAlignmentOptions.Center);
        hud.attackStatusText = aStatusGO.GetComponent<TextMeshProUGUI>();

        hud.SetHealth(1f);
        hud.SetSprint(1f);
        hud.SetAttack(1f);

        Selection.activeGameObject = canvasGO;
        Debug.Log("[Sprint & Steel] HUD rebuilt with health, sprint cooldown, and attack cooldown meters.");
    }

    // ── Helpers ──────────────────────────────────────────────

    static GameObject MakeAnchoredGroup(string name, Transform parent,
        Vector2 anchor, Vector2 pivot, Vector2 offset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
        rt.anchoredPosition = offset;
        VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childForceExpandWidth  = false;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go;
    }

    static void MakeLabel(string name, Transform parent, string text, float width)
    {
        GameObject go = MakeFixedWidthText(name, parent, text, 18, new Color(1f, 1f, 1f, 0.45f), width, TextAlignmentOptions.Center);
        go.GetComponent<TextMeshProUGUI>().characterSpacing = 2f;
    }

    static (Image track, Image fill) MakeBar(string name, Transform parent, Color fillColor, float amount, float width)
    {
        GameObject trackGO = new GameObject(name + "_Track");
        trackGO.transform.SetParent(parent, false);
        RectTransform trt = trackGO.AddComponent<RectTransform>();
        trt.sizeDelta = new Vector2(width, 20);
        Image track = trackGO.AddComponent<Image>();
        track.color = new Color(0, 0, 0, 0.4f);

        GameObject fillGO = new GameObject(name + "_Fill");
        fillGO.transform.SetParent(trackGO.transform, false);
        RectTransform frt = fillGO.AddComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;
        frt.pivot = new Vector2(0f, 0.5f);
        Image fill = fillGO.AddComponent<Image>();
        fill.color = fillColor;
        fill.type  = Image.Type.Filled;
        fill.fillMethod  = Image.FillMethod.Horizontal;
        fill.fillOrigin  = (int)Image.OriginHorizontal.Left;
        fill.fillAmount  = amount;

        return (track, fill);
    }

    static (Image track, Image fill) MakeBarWithIcon(string name, Transform parent,
        Color fillColor, float amount, float width, out Image iconImage)
    {
        GameObject row = new GameObject(name + "_Row");
        row.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = MeterSpacing;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        ContentSizeFitter csf = row.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(row.transform, false);
        RectTransform irt = iconGO.AddComponent<RectTransform>();
        irt.sizeDelta = new Vector2(MeterIconSize, MeterIconSize);
        iconImage = iconGO.AddComponent<Image>();
        iconImage.color = fillColor;

        GameObject trackGO = new GameObject(name + "_Track");
        trackGO.transform.SetParent(row.transform, false);
        RectTransform trt = trackGO.AddComponent<RectTransform>();
        trt.sizeDelta = new Vector2(width, 20);
        Image track = trackGO.AddComponent<Image>();
        track.color = new Color(0, 0, 0, 0.4f);

        GameObject fillGO = new GameObject(name + "_Fill");
        fillGO.transform.SetParent(trackGO.transform, false);
        RectTransform frt = fillGO.AddComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;
        frt.pivot = new Vector2(0f, 0.5f);
        Image fill = fillGO.AddComponent<Image>();
        fill.color = fillColor;
        fill.type  = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = amount;

        return (track, fill);
    }

    static GameObject MakeText(string name, Transform parent, string text, int size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.raycastTarget = false;
        tmp.fontStyle = FontStyles.Bold;
        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
        return go;
    }

    static GameObject MakeFixedWidthText(string name, Transform parent, string text, int size, Color color, float width, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, size + 10f);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = alignment;

        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;

        return go;
    }
}
#endif
