using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  QuestBoardUI.cs
//  Attach to QuestBoard_Canvas.
//  Assign QuestData assets in the Inspector.
// ─────────────────────────────────────────────────────────────
public class QuestBoardUI : MonoBehaviour
{
    public static bool IsAnyBoardOpen { get; private set; }

    [Header("Quest Data — assign in Inspector")]
    public List<QuestData> quests = new List<QuestData>();

    [Header("References (auto-assigned by QuestBoardSetup)")]
    public GameObject      boardRoot;
    public Transform       rowContainer;
    public GameObject      rowPrefab;
    public GameObject      confirmPopup;
    public TextMeshProUGUI confirmQuestName;
    public Button          confirmYesBtn;
    public Button          confirmNoBtn;

    const float ROW_H   = 46f;
    const float ROW_GAP = 5f;

    List<QuestRowUI> _rows        = new List<QuestRowUI>();
    int              _selectedIdx = 0;
    bool             _popupOpen   = false;
    QuestData        _pendingQuest;

    static readonly Color ColTeal  = new Color(0.361f, 0.878f, 0.753f);
    static readonly Color ColAmber = new Color(0.878f, 0.722f, 0.361f);

    // ─────────────────────────────────────────────────────────

    void Start()
    {
        BuildRows();
        boardRoot.SetActive(false);
        confirmPopup.SetActive(false);
        IsAnyBoardOpen = false;
        confirmYesBtn.onClick.AddListener(OnConfirmYes);
        confirmNoBtn.onClick.AddListener(OnConfirmNo);
    }

    void OnDisable()
    {
        IsAnyBoardOpen = false;
    }

    void Update()
    {
        if (_popupOpen)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                OnConfirmYes();
            if (Input.GetKeyDown(KeyCode.Escape))
                OnConfirmNo();
            return;
        }

        if (!boardRoot.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)   || Input.GetKeyDown(KeyCode.W)) MoveSelection(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) MoveSelection( 1);
        if (Input.GetKeyDown(KeyCode.Return)     || Input.GetKeyDown(KeyCode.KeypadEnter)) TryAccept();
        if (Input.GetKeyDown(KeyCode.Escape))    Hide();
    }

    // ─────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────

    public void Show()
    {
        boardRoot.SetActive(true);
        IsAnyBoardOpen = true;
        _selectedIdx = FirstSelectableIndex();
        RefreshAllRows();
    }

    public void Hide()
    {
        boardRoot.SetActive(false);
        IsAnyBoardOpen = false;
        confirmPopup.SetActive(false);
        _popupOpen = false;
    }

    // ─────────────────────────────────────────────────────────
    //  Row building — manual positioning, no layout groups
    // ─────────────────────────────────────────────────────────

    void BuildRows()
    {
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);
        _rows.Clear();

        for (int i = 0; i < quests.Count; i++)
        {
            var go  = Instantiate(rowPrefab, rowContainer);
            var rt  = go.GetComponent<RectTransform>();

            // Position each row manually from the top of the container
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(0, -(i * (ROW_H + ROW_GAP)));
            rt.sizeDelta        = new Vector2(0, ROW_H);

            var row = go.GetComponent<QuestRowUI>();
            row.Setup(quests[i]);
            _rows.Add(row);
        }

        RefreshAllRows();
    }

    void RefreshAllRows()
    {
        for (int i = 0; i < _rows.Count; i++)
            _rows[i].SetSelected(i == _selectedIdx && quests[i].status != QuestStatus.Locked);
    }

    void MoveSelection(int dir)
    {
        int count = quests.Count;
        int next  = _selectedIdx;
        for (int attempt = 0; attempt < count; attempt++)
        {
            next = (next + dir + count) % count;
            if (quests[next].status != QuestStatus.Locked) break;
        }
        _selectedIdx = next;
        RefreshAllRows();
    }

    int FirstSelectableIndex()
    {
        for (int i = 0; i < quests.Count; i++)
            if (quests[i].status != QuestStatus.Locked) return i;
        return 0;
    }

    // ─────────────────────────────────────────────────────────
    //  Accept flow
    // ─────────────────────────────────────────────────────────

    void TryAccept()
    {
        if (_selectedIdx < 0 || _selectedIdx >= quests.Count) return;
        var q = quests[_selectedIdx];
        if (q.status != QuestStatus.Unlocked) return;

        _pendingQuest          = q;
        confirmQuestName.text  = q.questName.ToUpper();
        confirmPopup.SetActive(true);
        _popupOpen = true;
    }

    void OnConfirmYes()
    {
        if (_pendingQuest == null) return;
        confirmPopup.SetActive(false);
        _popupOpen = false;

        QuestRuntimeManager.BeginQuest(_pendingQuest);

        Hide();
    }

    void OnConfirmNo()
    {
        confirmPopup.SetActive(false);
        _popupOpen    = false;
        _pendingQuest = null;
    }
}
