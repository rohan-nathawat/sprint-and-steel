using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public static class MainMenuSetup
{
    const float BOARD_W = 720f;
    const float BOARD_H = 460f;
    const float ROW_W = 420f;
    const float ROW_H = 72f;
    const float ROW_GAP = 18f;

    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    static readonly Color ColBg = C(0.1f, 0.1f, 0.12f, 0.96f);
    static readonly Color ColBorder = C(0.42f, 0.42f, 0.46f, 1f);
    static readonly Color ColHighlight = C(1f, 1f, 1f, 0.1f);
    static readonly Color ColTitle = C(0.92f, 0.92f, 0.95f, 1f);
    static readonly Color ColHint = C(1f, 1f, 1f, 0.45f);

    [MenuItem("Tools/Sprint & Steel/Build Main Menu")]
    public static void Build()
    {
        var existing = GameObject.Find("MainMenu_Canvas");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var canvasGO = new GameObject("MainMenu_Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        var menu = canvasGO.AddComponent<MainMenuUI>();

        var root = Rect("Root", canvasGO.transform);
        Stretch(root, 0, 0, 0, 0);

        var board = Rect("Board", root.transform);
        Pin(board, new Vector2(0.5f, 0.5f), new Vector2(BOARD_W, BOARD_H), Vector2.zero);
        board.AddComponent<Image>().color = ColBg;
        Outline(board, ColBorder);

        var title = Rect("Title", board.transform);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.76f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = new Vector2(24f, 0f);
        titleRT.offsetMax = new Vector2(-24f, -12f);

        var titleTMP = title.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "SPRINT & STEEL";
        titleTMP.fontSize = 50f;
        titleTMP.characterSpacing = 6f;
        titleTMP.color = ColTitle;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.raycastTarget = false;

        var optionsRoot = Rect("Options", board.transform);
        var optionsRT = optionsRoot.GetComponent<RectTransform>();
        optionsRT.anchorMin = new Vector2(0.5f, 0.36f);
        optionsRT.anchorMax = new Vector2(0.5f, 0.36f);
        optionsRT.pivot = new Vector2(0.5f, 0.5f);
        optionsRT.sizeDelta = new Vector2(ROW_W, (ROW_H * 2f) + ROW_GAP);

        var highlight = Rect("SelectionHighlight", optionsRoot.transform);
        var highlightRT = highlight.GetComponent<RectTransform>();
        highlightRT.anchorMin = new Vector2(0.5f, 0.5f);
        highlightRT.anchorMax = new Vector2(0.5f, 0.5f);
        highlightRT.pivot = new Vector2(0.5f, 0.5f);
        highlightRT.sizeDelta = new Vector2(ROW_W, ROW_H);
        highlightRT.anchoredPosition = new Vector2(0f, (ROW_H + ROW_GAP) * 0.5f);
        highlight.AddComponent<Image>().color = ColHighlight;
        Outline(highlight, C(1f, 1f, 1f, 0.2f));

        RectTransform[] rows = new RectTransform[2];
        TextMeshProUGUI[] options = new TextMeshProUGUI[2];

        BuildOptionRow(optionsRoot.transform, "NewGame", "New Game", (ROW_H + ROW_GAP) * 0.5f, out rows[0], out options[0]);
        BuildOptionRow(optionsRoot.transform, "Continue", "Continue", -(ROW_H + ROW_GAP) * 0.5f, out rows[1], out options[1]);

        var hint = Rect("Hint", board.transform);
        var hintRT = hint.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0f, 0f);
        hintRT.anchorMax = new Vector2(1f, 0.22f);
        hintRT.offsetMin = new Vector2(24f, 12f);
        hintRT.offsetMax = new Vector2(-24f, 0f);

        var hintTMP = hint.AddComponent<TextMeshProUGUI>();
        hintTMP.text = "[↑/↓] Navigate    [ENTER] Select";
        hintTMP.fontSize = 22f;
        hintTMP.color = ColHint;
        hintTMP.alignment = TextAlignmentOptions.Center;
        hintTMP.raycastTarget = false;

        menu.root = root;
        menu.optionRows = rows;
        menu.optionTexts = options;
        menu.selectionHighlight = highlightRT;
        menu.hintText = hintTMP;

        Selection.activeGameObject = canvasGO;
        Debug.Log("[Sprint & Steel] Main Menu built. Set scene names on MainMenuUI for New Game and Continue.");
    }

    static void BuildOptionRow(Transform parent, string name, string label, float y, out RectTransform rowRT, out TextMeshProUGUI text)
    {
        var row = Rect(name, parent);
        rowRT = row.GetComponent<RectTransform>();
        rowRT.anchorMin = new Vector2(0.5f, 0.5f);
        rowRT.anchorMax = new Vector2(0.5f, 0.5f);
        rowRT.pivot = new Vector2(0.5f, 0.5f);
        rowRT.anchoredPosition = new Vector2(0f, y);
        rowRT.sizeDelta = new Vector2(ROW_W, ROW_H);

        var rowBg = row.AddComponent<Image>();
        rowBg.color = C(1f, 1f, 1f, 0.03f);

        var txt = Rect("Label", row.transform);
        Stretch(txt, 0, 0, 0, 0);
        text = txt.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 38f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = C(1f, 1f, 1f, 0.55f);
        text.raycastTarget = false;
    }

    static GameObject Rect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void Pin(GameObject go, Vector2 anchor, Vector2 size, Vector2 pos)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    static void Stretch(GameObject go, float l, float b, float r, float t)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(r, t);
    }

    static void Outline(GameObject go, Color col)
    {
        var outline = go.AddComponent<Outline>();
        outline.effectColor = col;
        outline.effectDistance = new Vector2(1f, -1f);
    }
}
#endif
