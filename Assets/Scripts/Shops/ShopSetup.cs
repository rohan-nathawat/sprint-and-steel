using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

public static class ShopSetup
{
    const float SCALE = 1.5f;
    const float BW = 900f * SCALE;
    const float BH = 560f * SCALE;
    const float PAD = 16f * SCALE;
    const float HEADER_H = 54f * SCALE;
    const float FOOTER_H = 44f * SCALE;
    const float ROW_H = 84f * SCALE;
    const float ROW_GAP = 8f * SCALE;

    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);
    static readonly Color ColBoardBG = C(0.14f, 0.14f, 0.16f, 0.97f);
    static readonly Color ColHeaderBG = C(0.21f, 0.21f, 0.24f);
    static readonly Color ColBorder = C(0.42f, 0.42f, 0.46f);
    static readonly Color ColHeaderText = C(0.92f, 0.92f, 0.92f);
    static readonly Color ColAccent = C(0.75f, 0.8f, 0.88f);

    [MenuItem("Tools/Sprint & Steel/Build Shop")]
    public static void Build()
    {
        var canvasGO = new GameObject("Shop_Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 35;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var ui = canvasGO.AddComponent<ShopUI>();

        var board = Rect("BoardRoot", canvasGO.transform);
        Pin(board, 0.5f, 0.5f, BW, BH, 0, 0);
        board.AddComponent<Image>().color = ColBoardBG;
        Outline(board, ColBorder);
        Nail(board.transform, new Vector2(0, 1), new Vector2(8, -8));
        Nail(board.transform, new Vector2(1, 1), new Vector2(-8, -8));
        Nail(board.transform, new Vector2(0, 0), new Vector2(8, 8));
        Nail(board.transform, new Vector2(1, 0), new Vector2(-8, 8));

        var header = Rect("Header", board.transform);
        Edge(header, top: true, height: HEADER_H);
        header.AddComponent<Image>().color = ColHeaderBG;

        var titleGO = Rect("Title", header.transform);
        Stretch(titleGO, PAD, 0, -PAD, 0);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Shopkeeper";
        titleTMP.fontSize = 22f * SCALE;
        titleTMP.color = ColHeaderText;
        titleTMP.characterSpacing = 3f;
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
        titleTMP.raycastTarget = false;

        var currencyGO = Rect("Currency", header.transform);
        var curRT = currencyGO.GetComponent<RectTransform>();
        curRT.anchorMin = new Vector2(0.7f, 0);
        curRT.anchorMax = new Vector2(1f, 1f);
        curRT.offsetMin = new Vector2(0, 0);
        curRT.offsetMax = new Vector2(-PAD, 0);
        var currencyTMP = currencyGO.AddComponent<TextMeshProUGUI>();
        currencyTMP.text = "$0";
        currencyTMP.fontSize = 22f * SCALE;
        currencyTMP.color = ColAccent;
        currencyTMP.alignment = TextAlignmentOptions.MidlineRight;
        currencyTMP.raycastTarget = false;

        var headerDiv = Rect("HeaderDiv", board.transform);
        var hdRT = headerDiv.GetComponent<RectTransform>();
        hdRT.anchorMin = new Vector2(0, 1);
        hdRT.anchorMax = new Vector2(1, 1);
        hdRT.pivot = new Vector2(0.5f, 1);
        hdRT.offsetMin = new Vector2(0, -(HEADER_H + 1));
        hdRT.offsetMax = Vector2.zero;
        headerDiv.AddComponent<Image>().color = ColBorder;

        var footer = Rect("Footer", board.transform);
        Edge(footer, top: false, height: FOOTER_H);
        footer.AddComponent<Image>().color = C(0, 0, 0, 0.15f);

        var statusGO = Rect("StatusText", footer.transform);
        var statusRT = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0, 0f);
        statusRT.anchorMax = new Vector2(0.65f, 1f);
        statusRT.offsetMin = new Vector2(PAD, 0);
        statusRT.offsetMax = new Vector2(-8f, 0);
        var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
        statusTMP.text = "";
        statusTMP.fontSize = 14f * SCALE;
        statusTMP.color = ColAccent;
        statusTMP.alignment = TextAlignmentOptions.MidlineLeft;
        statusTMP.raycastTarget = false;

        HintLabel(footer.transform, "[W/S or ↑/↓] Select", new Vector2(-PAD - (360f * SCALE), 0));
        HintLabel(footer.transform, "[ENTER] Buy", new Vector2(-PAD - (170f * SCALE), 0));
        HintLabel(footer.transform, "[X] Close", new Vector2(-PAD, 0));

        var rowsRoot = Rect("RowsRoot", board.transform);
        var rrRT = rowsRoot.GetComponent<RectTransform>();
        rrRT.anchorMin = Vector2.zero;
        rrRT.anchorMax = Vector2.one;
        rrRT.offsetMin = new Vector2(PAD, FOOTER_H + (10f * SCALE));
        rrRT.offsetMax = new Vector2(-PAD, -(HEADER_H + (12f * SCALE)));

        var titleTexts = new TextMeshProUGUI[ShopUpgrades.Count];
        var levelTexts = new TextMeshProUGUI[ShopUpgrades.Count];
        var descTexts = new TextMeshProUGUI[ShopUpgrades.Count];
        var costTexts = new TextMeshProUGUI[ShopUpgrades.Count];
        var buyButtons = new Button[ShopUpgrades.Count];
        var rowImages = new Image[ShopUpgrades.Count];

        for (int i = 0; i < ShopUpgrades.Count; i++)
        {
            float y = -(i * (ROW_H + ROW_GAP));

            var row = Rect("Row_" + i, rowsRoot.transform);
            var rowRT = row.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0, 1);
            rowRT.anchorMax = new Vector2(1, 1);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.sizeDelta = new Vector2(0, ROW_H);
            rowRT.anchoredPosition = new Vector2(0, y);

            var rowImg = row.AddComponent<Image>();
            rowImg.color = C(1f, 1f, 1f, 0.045f);
            Outline(row, C(1f, 1f, 1f, 0.12f));
            rowImages[i] = rowImg;

            var nameGO = Rect("Title", row.transform);
            var nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0.48f);
            nameRT.anchorMax = new Vector2(0.45f, 1f);
            nameRT.offsetMin = new Vector2(12f * SCALE, 0f);
            nameRT.offsetMax = new Vector2(-6f * SCALE, -8f * SCALE);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "Upgrade";
            nameTMP.fontSize = 17f * SCALE;
            nameTMP.color = ColHeaderText;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.raycastTarget = false;
            titleTexts[i] = nameTMP;

            var lvGO = Rect("Level", row.transform);
            var lvRT = lvGO.GetComponent<RectTransform>();
            lvRT.anchorMin = new Vector2(0f, 0f);
            lvRT.anchorMax = new Vector2(0.45f, 0.5f);
            lvRT.offsetMin = new Vector2(12f * SCALE, 8f * SCALE);
            lvRT.offsetMax = new Vector2(-6f * SCALE, 0f);
            var lvTMP = lvGO.AddComponent<TextMeshProUGUI>();
            lvTMP.text = "Lv 0/8";
            lvTMP.fontSize = 13f * SCALE;
            lvTMP.color = C(1f, 1f, 1f, 0.6f);
            lvTMP.alignment = TextAlignmentOptions.MidlineLeft;
            lvTMP.raycastTarget = false;
            levelTexts[i] = lvTMP;

            var descGO = Rect("Description", row.transform);
            var descRT = descGO.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.45f, 0f);
            descRT.anchorMax = new Vector2(0.77f, 1f);
            descRT.offsetMin = new Vector2(4f * SCALE, 10f * SCALE);
            descRT.offsetMax = new Vector2(-6f * SCALE, -10f * SCALE);
            var descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text = "Description";
            descTMP.fontSize = 13f * SCALE;
            descTMP.color = C(1f, 1f, 1f, 0.55f);
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.raycastTarget = false;
            descTMP.textWrappingMode = TextWrappingModes.Normal;
            descTexts[i] = descTMP;

            var buyGO = Rect("BuyButton", row.transform);
            var buyRT = buyGO.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.78f, 0.18f);
            buyRT.anchorMax = new Vector2(0.98f, 0.82f);
            buyRT.offsetMin = Vector2.zero;
            buyRT.offsetMax = Vector2.zero;
            var buyImg = buyGO.AddComponent<Image>();
            buyImg.color = C(0.72f, 0.72f, 0.76f, 0.18f);
            Outline(buyGO, C(0.84f, 0.84f, 0.9f, 0.35f));
            var buyBtn = buyGO.AddComponent<Button>();
            buyButtons[i] = buyBtn;

            var costGO = Rect("Cost", buyGO.transform);
            Stretch(costGO, 0, 0, 0, 0);
            var costTMP = costGO.AddComponent<TextMeshProUGUI>();
            costTMP.text = "$0";
            costTMP.fontSize = 16f * SCALE;
            costTMP.color = ColAccent;
            costTMP.alignment = TextAlignmentOptions.Center;
            costTMP.raycastTarget = false;
            costTexts[i] = costTMP;
        }

        ui.boardRoot = board;
        ui.currencyText = currencyTMP;
        ui.statusText = statusTMP;
        ui.titleTexts = titleTexts;
        ui.levelTexts = levelTexts;
        ui.descriptionTexts = descTexts;
        ui.costTexts = costTexts;
        ui.buyButtons = buyButtons;
        ui.rowBackgrounds = rowImages;

        Selection.activeGameObject = canvasGO;
        Debug.Log("[Sprint & Steel] Shop UI built. Add ShopkeeperNPC to an NPC trigger and interact with E.");
    }

    static GameObject Rect(string name, Transform parent)
    {
        var go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void Pin(GameObject go, float ax, float ay, float w, float h, float ox, float oy)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(ax, ay);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(ox, oy);
    }

    static void Stretch(GameObject go, float l, float b, float r, float t)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(r, t);
    }

    static void Edge(GameObject go, bool top, float height)
    {
        var rt = go.GetComponent<RectTransform>();
        if (top)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(0, -height);
            rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = new Vector2(0, height);
        }
    }

    static void Outline(GameObject go, Color col)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = col;
        o.effectDistance = new Vector2(1, -1);
    }

    static void Nail(Transform parent, Vector2 anchor, Vector2 offset)
    {
        var go = new GameObject("Nail");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(8f * SCALE, 8f * SCALE);
        go.AddComponent<Image>().color = C(0.75f, 0.75f, 0.78f);
    }

    static void HintLabel(Transform parent, string text, Vector2 offset)
    {
        var go = Rect("Hint", parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(220f * SCALE, 30f * SCALE);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 13f * SCALE;
        tmp.color = C(1, 1, 1, 0.25f);
        tmp.alignment = TextAlignmentOptions.MidlineRight;
        tmp.raycastTarget = false;
    }
}
#endif