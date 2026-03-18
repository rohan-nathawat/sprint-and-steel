using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  QuestBoardSetup.cs
//  Tools > Sprint & Steel > Build Quest Board
//  Uses ONLY manual RectTransform anchors. No layout groups.
// ─────────────────────────────────────────────────────────────
#if UNITY_EDITOR
using UnityEditor;

public static class QuestBoardSetup
{
    // Board dimensions (reference resolution 1920x1080)
    const float BW       = 860f;   // board width
    const float BH       = 460f;   // board height
    const float HDR_H    = 52f;    // header height
    const float COL_H    = 26f;    // column header row height
    const float FOOT_H   = 38f;    // footer height
    const float ROW_H    = 46f;    // each quest row height
    const float ROW_GAP  = 5f;     // gap between rows
    const float PAD      = 16f;    // inner padding

    // Column x positions (0–1 within board)
    const float COL_LOC  = 0.42f;
    const float COL_DIFF = 0.70f;
    const float COL_STAT = 0.87f;

    static Color C(float r, float g, float b, float a = 1f) => new Color(r,g,b,a);
    static readonly Color ColBoardBG  = C(0.102f, 0.071f, 0.031f, 0.97f);
    static readonly Color ColHeaderBG = C(0.165f, 0.102f, 0.039f);
    static readonly Color ColBorder   = C(0.353f, 0.227f, 0.094f);
    static readonly Color ColTeal     = C(0.361f, 0.878f, 0.753f);
    static readonly Color ColAmber    = C(0.878f, 0.722f, 0.361f);

    [MenuItem("Tools/Sprint & Steel/Build Quest Board")]
    public static void Build()
    {
        // ── Canvas ──────────────────────────────────────────
        var canvasGO = new GameObject("QuestBoard_Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var ui = canvasGO.AddComponent<QuestBoardUI>();

        // ── Board panel — centred ────────────────────────────
        var board = Rect("BoardRoot", canvasGO.transform);
        Pin(board, 0.5f, 0.5f, BW, BH, 0, 0);
        board.AddComponent<Image>().color = ColBoardBG;
        Outline(board, ColBorder);
        Nail(board.transform, new Vector2(0,1), new Vector2( 8,-8));
        Nail(board.transform, new Vector2(1,1), new Vector2(-8,-8));
        Nail(board.transform, new Vector2(0,0), new Vector2( 8, 8));
        Nail(board.transform, new Vector2(1,0), new Vector2(-8, 8));

        // ── Header ───────────────────────────────────────────
        var header = Rect("Header", board.transform);
        Edge(header, top: true, height: HDR_H);
        header.AddComponent<Image>().color = ColHeaderBG;

        var titleGO = Rect("Title", header.transform);
        Stretch(titleGO, PAD, 0, -PAD, 0);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text             = "Quest Board";
        titleTMP.fontSize         = 22f;
        titleTMP.color            = ColAmber;
        titleTMP.characterSpacing = 3f;
        titleTMP.alignment        = TextAlignmentOptions.MidlineLeft;
        titleTMP.raycastTarget    = false;

        // Header bottom divider
        var div = Rect("HeaderDiv", board.transform);
        var divRT = div.GetComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0,1); divRT.anchorMax = new Vector2(1,1);
        divRT.pivot     = new Vector2(0.5f,1);
        divRT.offsetMin = new Vector2(0, -(HDR_H+1)); divRT.offsetMax = Vector2.zero;
        div.AddComponent<Image>().color = ColBorder;

        // ── Column headers ───────────────────────────────────
        float chTop = -(HDR_H + 2);
        float chBot = -(HDR_H + 2 + COL_H);
        ColHeader(board.transform, "Quest",      0f,       COL_LOC,  chTop, chBot, TextAlignmentOptions.MidlineLeft);
        ColHeader(board.transform, "Location",   COL_LOC,  COL_DIFF, chTop, chBot, TextAlignmentOptions.MidlineLeft);
        ColHeader(board.transform, "Difficulty", COL_DIFF, COL_STAT, chTop, chBot, TextAlignmentOptions.MidlineLeft);
        ColHeader(board.transform, "Status",     COL_STAT, 1f,       chTop, chBot, TextAlignmentOptions.MidlineRight);

        // Column header divider
        var cdiv = Rect("ColDiv", board.transform);
        var cdivRT = cdiv.GetComponent<RectTransform>();
        cdivRT.anchorMin = new Vector2(0,1); cdivRT.anchorMax = new Vector2(1,1);
        cdivRT.pivot     = new Vector2(0.5f,1);
        cdivRT.offsetMin = new Vector2(0, chBot-1); cdivRT.offsetMax = new Vector2(0, chBot);
        cdiv.AddComponent<Image>().color = C(1,1,1,0.07f);

        // ── Quest rows ───────────────────────────────────────
        // Rows start below column headers, above footer
        float rowsTop = HDR_H + 2 + COL_H + 4;  // px from top of board
        float rowsBot = FOOT_H + 4;              // px from bottom of board

        // Row container (just a rect — NO layout group)
        var rowContainer = Rect("RowContainer", board.transform);
        var rcRT = rowContainer.GetComponent<RectTransform>();
        rcRT.anchorMin = Vector2.zero; rcRT.anchorMax = Vector2.one;
        rcRT.offsetMin = new Vector2(PAD, rowsBot);
        rcRT.offsetMax = new Vector2(-PAD, -rowsTop);

        // ── Footer ───────────────────────────────────────────
        var footer = Rect("Footer", board.transform);
        Edge(footer, top: false, height: FOOT_H);
        footer.AddComponent<Image>().color = C(0,0,0,0.15f);

        var footDiv = Rect("FooterDiv", board.transform);
        var fdRT = footDiv.GetComponent<RectTransform>();
        fdRT.anchorMin = new Vector2(0,0); fdRT.anchorMax = new Vector2(1,0);
        fdRT.pivot     = new Vector2(0.5f,0);
        fdRT.offsetMin = new Vector2(0, FOOT_H); fdRT.offsetMax = new Vector2(0, FOOT_H+1);
        footDiv.AddComponent<Image>().color = ColBorder;

        HintLabel(footer.transform, "[ENTER] Accept", new Vector2(-PAD - 120f, 0));
        HintLabel(footer.transform, "[ESC] Close",    new Vector2(-PAD,        0));

        // ── Row prefab (not placed in scene, just built) ─────
        var rowPrefab = BuildRowPrefab();

        // ── Confirm popup ────────────────────────────────────
        var overlay = Rect("ConfirmOverlay", board.transform);
        Stretch(overlay, 0, 0, 0, 0);
        overlay.AddComponent<Image>().color = C(0,0,0,0.65f);

        var popup = Rect("ConfirmPanel", overlay.transform);
        Pin(popup, 0.5f, 0.5f, 500f, 190f, 0, 0);
        popup.AddComponent<Image>().color = C(0.05f,0.08f,0.06f,0.97f);
        Outline(popup, ColTeal * C(1,1,1,0.4f));

        var popHead = Rect("PopupHeading", popup.transform);
        var phRT = popHead.GetComponent<RectTransform>();
        phRT.anchorMin = new Vector2(0,1); phRT.anchorMax = new Vector2(1,1);
        phRT.pivot     = new Vector2(0.5f,1);
        phRT.offsetMin = new Vector2(PAD, -58f); phRT.offsetMax = new Vector2(-PAD, -10f);
        var phTMP = popHead.AddComponent<TextMeshProUGUI>();
        phTMP.text      = "Accept Quest?";
        phTMP.fontSize  = 20f;
        phTMP.color     = ColTeal;
        phTMP.alignment = TextAlignmentOptions.Center;
        phTMP.raycastTarget = false;

        var popQName = Rect("PopupQuestName", popup.transform);
        var pqRT = popQName.GetComponent<RectTransform>();
        pqRT.anchorMin = new Vector2(0,1); pqRT.anchorMax = new Vector2(1,1);
        pqRT.pivot     = new Vector2(0.5f,1);
        pqRT.offsetMin = new Vector2(PAD, -96f); pqRT.offsetMax = new Vector2(-PAD, -58f);
        var pqTMP = popQName.AddComponent<TextMeshProUGUI>();
        pqTMP.text      = "";
        pqTMP.fontSize  = 16f;
        pqTMP.color     = C(1,1,1,0.7f);
        pqTMP.alignment = TextAlignmentOptions.Center;
        pqTMP.raycastTarget = false;

        // Yes button
        var yesGO = Rect("YesBtn", popup.transform);
        var ybRT  = yesGO.GetComponent<RectTransform>();
        ybRT.anchorMin = new Vector2(0.08f,0); ybRT.anchorMax = new Vector2(0.46f,0);
        ybRT.pivot     = new Vector2(0.5f,0);
        ybRT.offsetMin = new Vector2(0, 14f); ybRT.offsetMax = new Vector2(0, 50f);
        yesGO.AddComponent<Image>().color = C(0.361f,0.878f,0.753f,0.15f);
        Outline(yesGO, C(0.361f,0.878f,0.753f,0.4f));
        var yesBtn = yesGO.AddComponent<Button>();
        var yesLbl = Rect("Label", yesGO.transform);
        Stretch(yesLbl, 0,0,0,0);
        var yesTMP = yesLbl.AddComponent<TextMeshProUGUI>();
        yesTMP.text      = "Yes, Accept";
        yesTMP.fontSize  = 15f;
        yesTMP.color     = ColTeal;
        yesTMP.alignment = TextAlignmentOptions.Center;
        yesTMP.raycastTarget = false;

        // No button
        var noGO = Rect("NoBtn", popup.transform);
        var nbRT = noGO.GetComponent<RectTransform>();
        nbRT.anchorMin = new Vector2(0.54f,0); nbRT.anchorMax = new Vector2(0.92f,0);
        nbRT.pivot     = new Vector2(0.5f,0);
        nbRT.offsetMin = new Vector2(0, 14f); nbRT.offsetMax = new Vector2(0, 50f);
        noGO.AddComponent<Image>().color = C(1,1,1,0.05f);
        Outline(noGO, C(1,1,1,0.2f));
        var noBtn = noGO.AddComponent<Button>();
        var noLbl = Rect("Label", noGO.transform);
        Stretch(noLbl, 0,0,0,0);
        var noTMP = noLbl.AddComponent<TextMeshProUGUI>();
        noTMP.text      = "No, Go Back";
        noTMP.fontSize  = 15f;
        noTMP.color     = C(1,1,1,0.45f);
        noTMP.alignment = TextAlignmentOptions.Center;
        noTMP.raycastTarget = false;

        // ── Wire up QuestBoardUI ─────────────────────────────
        ui.boardRoot        = board;
        ui.rowContainer     = rowContainer.transform;
        ui.rowPrefab        = rowPrefab;
        ui.confirmPopup     = overlay;
        ui.confirmQuestName = pqTMP;
        ui.confirmYesBtn    = yesBtn;
        ui.confirmNoBtn     = noBtn;

        Selection.activeGameObject = canvasGO;
        Debug.Log("[Sprint & Steel] Quest Board built. Assign QuestData assets to QuestBoardUI.quests in the Inspector.");
    }

    // ─────────────────────────────────────────────────────────
    //  Row prefab — built as a standalone GO, not in scene
    // ─────────────────────────────────────────────────────────
    static GameObject BuildRowPrefab()
    {
        var row = new GameObject("QuestRow_Prefab");
        row.AddComponent<RectTransform>().sizeDelta = new Vector2(0, ROW_H);
        row.AddComponent<CanvasGroup>();
        var rowBg = row.AddComponent<Image>();
        rowBg.color = C(1,1,1,0.03f);
        var rowUI = row.AddComponent<QuestRowUI>();
        rowUI.rowBackground = rowBg;

        // Status dot
        var dot = new GameObject("StatusDot");
        dot.transform.SetParent(row.transform, false);
        var dotRT = dot.AddComponent<RectTransform>();
        dotRT.anchorMin = new Vector2(0,0.5f); dotRT.anchorMax = new Vector2(0,0.5f);
        dotRT.pivot     = new Vector2(0,0.5f);
        dotRT.anchoredPosition = new Vector2(2, 0);
        dotRT.sizeDelta = new Vector2(8, 8);
        rowUI.statusDot = dot.AddComponent<Image>();
        rowUI.statusDot.color = ColTeal;

        // Lock icon
        var lk = new GameObject("LockIcon");
        lk.transform.SetParent(row.transform, false);
        var lkRT = lk.AddComponent<RectTransform>();
        lkRT.anchorMin = new Vector2(0,0.5f); lkRT.anchorMax = new Vector2(0,0.5f);
        lkRT.pivot     = new Vector2(0,0.5f);
        lkRT.anchoredPosition = new Vector2(2, 0);
        lkRT.sizeDelta = new Vector2(8, 10);
        lk.AddComponent<Image>().color = C(1,1,1,0.2f);
        rowUI.lockIcon = lk;

        // Quest name
        var nameGO = new GameObject("QuestName");
        nameGO.transform.SetParent(row.transform, false);
        var nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0,0); nameRT.anchorMax = new Vector2(COL_LOC,1);
        nameRT.offsetMin = new Vector2(14,0); nameRT.offsetMax = new Vector2(-2,0);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text      = "Quest";
        nameTMP.fontSize  = 16f;
        nameTMP.color     = C(1,1,1,0.82f);
        nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        nameTMP.raycastTarget = false;
        rowUI.questNameText = nameTMP;

        // Location
        var locGO = new GameObject("Location");
        locGO.transform.SetParent(row.transform, false);
        var locRT = locGO.AddComponent<RectTransform>();
        locRT.anchorMin = new Vector2(COL_LOC,0); locRT.anchorMax = new Vector2(COL_DIFF,1);
        locRT.offsetMin = new Vector2(4,0); locRT.offsetMax = new Vector2(-2,0);
        var locTMP = locGO.AddComponent<TextMeshProUGUI>();
        locTMP.text             = "Location";
        locTMP.fontSize         = 14f;
        locTMP.color            = C(1,1,1,0.4f);
        locTMP.characterSpacing = 1f;
        locTMP.alignment        = TextAlignmentOptions.MidlineLeft;
        locTMP.raycastTarget    = false;
        rowUI.locationText = locTMP;

        // Difficulty pips (3 manual squares)
        var pips = new Image[3];
        float pipStart = COL_DIFF;
        float pipW     = (COL_STAT - COL_DIFF) / 3f;
        for (int i = 0; i < 3; i++)
        {
            var pip = new GameObject("Pip" + i);
            pip.transform.SetParent(row.transform, false);
            var pipRT = pip.AddComponent<RectTransform>();
            pipRT.anchorMin = new Vector2(pipStart + i*pipW + 0.01f, 0.3f);
            pipRT.anchorMax = new Vector2(pipStart + i*pipW + pipW - 0.01f, 0.7f);
            pipRT.offsetMin = new Vector2(2,0); pipRT.offsetMax = new Vector2(-2,0);
            pips[i] = pip.AddComponent<Image>();
            pips[i].color = ColTeal;
        }
        rowUI.difficultyPips = pips;

        // Status label
        var statGO = new GameObject("StatusLabel");
        statGO.transform.SetParent(row.transform, false);
        var statRT = statGO.AddComponent<RectTransform>();
        statRT.anchorMin = new Vector2(COL_STAT,0); statRT.anchorMax = new Vector2(1,1);
        statRT.offsetMin = new Vector2(0,0); statRT.offsetMax = new Vector2(-6,0);
        var statTMP = statGO.AddComponent<TextMeshProUGUI>();
        statTMP.text      = "New";
        statTMP.fontSize  = 13f;
        statTMP.color     = ColTeal;
        statTMP.alignment = TextAlignmentOptions.MidlineRight;
        statTMP.raycastTarget = false;
        rowUI.statusText = statTMP;

        return row;
    }

    // ─────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────

    static GameObject Rect(string name, Transform parent)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    // Anchor to centre point with fixed size
    static void Pin(GameObject go, float ax, float ay, float w, float h, float ox, float oy)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot     = new Vector2(ax, ay);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(ox, oy);
    }

    // Stretch to fill parent with offsets
    static void Stretch(GameObject go, float l, float b, float r, float t)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(r, t);
    }

    // Anchor to top or bottom edge with fixed height
    static void Edge(GameObject go, bool top, float height)
    {
        var rt = go.GetComponent<RectTransform>();
        if (top)
        {
            rt.anchorMin = new Vector2(0,1); rt.anchorMax = Vector2.one;
            rt.pivot     = new Vector2(0.5f,1);
            rt.offsetMin = new Vector2(0,-height); rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(1,0);
            rt.pivot     = new Vector2(0.5f,0);
            rt.offsetMin = Vector2.zero; rt.offsetMax = new Vector2(0,height);
        }
    }

    static void Outline(GameObject go, Color col)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor    = col;
        o.effectDistance = new Vector2(1,-1);
    }

    static void Nail(Transform parent, Vector2 anchor, Vector2 offset)
    {
        var go = new GameObject("Nail");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(8,8);
        go.AddComponent<Image>().color = C(0.541f,0.376f,0.125f);
    }

    static void ColHeader(Transform parent, string text,
                           float xMin, float xMax, float yTop, float yBot,
                           TextAlignmentOptions align)
    {
        var go = Rect("CH_" + text, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin,1); rt.anchorMax = new Vector2(xMax,1);
        rt.pivot     = new Vector2(0f,1);
        rt.offsetMin = new Vector2(xMin == 0 ? PAD : 4f, yBot);
        rt.offsetMax = new Vector2(0, yTop);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text             = text;
        tmp.fontSize         = 12f;
        tmp.color            = C(1,1,1,0.25f);
        tmp.characterSpacing = 1.5f;
        tmp.alignment        = align;
        tmp.raycastTarget    = false;
    }

    static void HintLabel(Transform parent, string text, Vector2 offset)
    {
        var go = Rect("Hint", parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1,0.5f); rt.anchorMax = new Vector2(1,0.5f);
        rt.pivot     = new Vector2(1,0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(160f, 30f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 13f;
        tmp.color     = C(1,1,1,0.25f);
        tmp.alignment = TextAlignmentOptions.MidlineRight;
        tmp.raycastTarget = false;
    }
}
#endif
