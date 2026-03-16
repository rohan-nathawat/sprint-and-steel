using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  DialogueSetup.cs
//  Tools > Sprint & Steel > Build Dialogue Box
// ─────────────────────────────────────────────────────────────
#if UNITY_EDITOR
using UnityEditor;

public static class DialogueSetupMenu
{
    const float BOX_HEIGHT    = 140f;  // taller
    const float BOX_BOTTOM    = 40f;
    const float PADDING       = 16f;
    const float PORTRAIT_SIZE = 52f;
    const float GAP           = 14f;

    [MenuItem("Tools/Sprint & Steel/Build Dialogue Box")]
    public static void BuildDialogue()
    {
        // ── Canvas ──────────────────────────────────────────
        GameObject canvasGO = new GameObject("Dialogue_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        DialogueManager mgr = canvasGO.AddComponent<DialogueManager>();

        // ── Box — narrower (25% margins each side) ───────────
        GameObject box = MakeRect("DialogueBox", canvasGO.transform);
        RectTransform boxRT = box.GetComponent<RectTransform>();
        boxRT.anchorMin        = new Vector2(0.25f, 0f); // narrower than before
        boxRT.anchorMax        = new Vector2(0.75f, 0f);
        boxRT.pivot            = new Vector2(0.5f, 0f);
        boxRT.anchoredPosition = new Vector2(0f, BOX_BOTTOM);
        boxRT.sizeDelta        = new Vector2(0f, BOX_HEIGHT);
        box.AddComponent<Image>().color = new Color(0.039f, 0.110f, 0.102f, 0.96f);

        // 1px teal border
        GameObject border = MakeRect("Border", box.transform);
        SetStretch(border, 0, 0, 0, 0);
        border.AddComponent<Image>().color = new Color(0.361f, 0.878f, 0.753f, 0.4f);
        GameObject borderFill = MakeRect("BorderFill", border.transform);
        SetStretch(borderFill, 1, 1, -1, -1);
        borderFill.AddComponent<Image>().color = new Color(0.039f, 0.110f, 0.102f, 0.96f);

        // Corner brackets
        AddCorner("TL", box.transform, new Vector2(0,1), new Vector2( 2,-2));
        AddCorner("TR", box.transform, new Vector2(1,1), new Vector2(-7,-2));
        AddCorner("BL", box.transform, new Vector2(0,0), new Vector2( 2, 7));
        AddCorner("BR", box.transform, new Vector2(1,0), new Vector2(-7, 7));

        // ── Portrait ─────────────────────────────────────────
        GameObject portrait = MakeRect("NPC_Portrait", box.transform);
        RectTransform pRT = portrait.GetComponent<RectTransform>();
        pRT.anchorMin        = new Vector2(0f, 0.5f);
        pRT.anchorMax        = new Vector2(0f, 0.5f);
        pRT.pivot            = new Vector2(0f, 0.5f);
        pRT.anchoredPosition = new Vector2(PADDING, 0f);
        pRT.sizeDelta        = new Vector2(PORTRAIT_SIZE, PORTRAIT_SIZE);
        Image portImg = portrait.AddComponent<Image>();
        portImg.color = new Color(0.361f, 0.878f, 0.753f);

        // Portrait inner dark square
        GameObject inner = MakeRect("Portrait_Inner", portrait.transform);
        SetStretch(inner, 0.2f, 0.2f, -0.4f, -0.4f);
        Image innerImg = inner.AddComponent<Image>();
        innerImg.color = new Color(0.039f, 0.110f, 0.102f, 0.6f);
        mgr.portraitInner = innerImg;

        // ── NPC Name ─────────────────────────────────────────
        float textLeft  = PADDING + PORTRAIT_SIZE + GAP;
        float textRight = PADDING;

        GameObject nameGO = MakeRect("NPC_Name", box.transform);
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 1f);
        nameRT.anchorMax = new Vector2(1f, 1f);
        nameRT.pivot     = new Vector2(0f, 1f);
        nameRT.offsetMin = new Vector2(textLeft,  -PADDING - 22f);
        nameRT.offsetMax = new Vector2(-textRight, -PADDING);
        TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text             = "NPC";
        nameTMP.fontSize         = 20f;   // 2x
        nameTMP.color            = new Color(0.361f, 0.878f, 0.753f);
        nameTMP.characterSpacing = 2f;
        nameTMP.raycastTarget    = false;
        mgr.npcNameText = nameTMP;

        // ── Body Text ────────────────────────────────────────
        GameObject bodyGO = MakeRect("Dialogue_Body", box.transform);
        RectTransform bodyRT = bodyGO.GetComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0f, 0f);
        bodyRT.anchorMax = new Vector2(1f, 1f);
        bodyRT.offsetMin = new Vector2(textLeft,  PADDING + 22f);
        bodyRT.offsetMax = new Vector2(-textRight, -(PADDING + 24f));
        TextMeshProUGUI bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyTMP.text          = "";
        bodyTMP.fontSize      = 20f;   // 2x
        bodyTMP.color         = new Color(1f, 1f, 1f, 0.82f);
        bodyTMP.lineSpacing   = 4f;
        bodyTMP.raycastTarget = false;
        mgr.dialogueBodyText = bodyTMP;

        // ── Key Hints ────────────────────────────────────────
        AddHint("Hint_Continue", box.transform, new Vector2(-PADDING - 90f, PADDING), "[E] Continue");
        AddHint("Hint_Close",    box.transform, new Vector2(-PADDING,       PADDING), "[ESC] Close");

        // ── Finish ───────────────────────────────────────────
        mgr.dialogueBox = box;
        Selection.activeGameObject = canvasGO;
        Debug.Log("[Sprint & Steel] Dialogue Canvas built.");
    }

    // ── Helpers ──────────────────────────────────────────────

    static GameObject MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetStretch(GameObject go, float left, float bottom, float right, float top)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left,  bottom);
        rt.offsetMax = new Vector2(right, top);
    }

    static void AddCorner(string id, Transform parent, Vector2 anchor, Vector2 offset)
    {
        var go = MakeRect("Corner_" + id, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchor;
        rt.anchorMax        = anchor;
        rt.pivot            = anchor;
        rt.anchoredPosition = offset;
        rt.sizeDelta        = new Vector2(5f, 5f);
        var img = go.AddComponent<Image>();
        img.color         = new Color(0.361f, 0.878f, 0.753f);
        img.raycastTarget = false;
    }

    static void AddHint(string name, Transform parent, Vector2 offset, string label)
    {
        var go = MakeRect(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(1f, 0f);
        rt.anchorMax        = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.anchoredPosition = offset;
        rt.sizeDelta        = new Vector2(90f, 18f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = label;
        tmp.fontSize      = 16f;   // 2x
        tmp.color         = new Color(1f, 1f, 1f, 0.3f);
        tmp.alignment     = TextAlignmentOptions.Right;
        tmp.raycastTarget = false;
    }
}
#endif
