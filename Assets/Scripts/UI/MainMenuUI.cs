using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    public GameObject root;
    public TextMeshProUGUI[] optionTexts;
    public RectTransform[] optionRows;
    public RectTransform selectionHighlight;
    public TextMeshProUGUI hintText;

    [Header("Scene Targets")]
    [SerializeField] string newGameScene = "IntroCutscene";
    [SerializeField] string continueScene = "Hub";

    [Header("Continue Save Flag")]
    [SerializeField] string continueSaveKey = "HasSaveData";
    [SerializeField] bool markSaveOnNewGameStart = true;

    [Header("Style")]
    [SerializeField] Color selectedTextColor = new Color(0.95f, 0.95f, 0.98f, 1f);
    [SerializeField] Color unselectedTextColor = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] Color lockedContinueTextColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField, Min(1f)] float selectionLerpSpeed = 14f;

    int _selectedIndex;
    float _targetHighlightY;

    const int NewGameIndex = 0;
    const int ContinueIndex = 1;

    void Start()
    {
        _selectedIndex = 0;
        if (optionRows != null && optionRows.Length > 0 && optionRows[0] != null)
            _targetHighlightY = optionRows[0].anchoredPosition.y;

        RefreshVisuals(immediate: true);
    }

    void Update()
    {
        HandleInput();
        RefreshVisuals(immediate: false);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            MoveSelection(-1);

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            MoveSelection(1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ExecuteSelection();
    }

    void MoveSelection(int direction)
    {
        if (optionTexts == null || optionTexts.Length < 2)
            return;

        _selectedIndex += direction;
        if (_selectedIndex < 0)
            _selectedIndex = optionTexts.Length - 1;
        else if (_selectedIndex >= optionTexts.Length)
            _selectedIndex = 0;

        UpdateHighlightTarget();
    }

    void UpdateHighlightTarget()
    {
        if (optionRows == null || _selectedIndex < 0 || _selectedIndex >= optionRows.Length)
            return;

        RectTransform selectedRow = optionRows[_selectedIndex];
        if (selectedRow == null)
            return;

        _targetHighlightY = selectedRow.anchoredPosition.y;
    }

    void RefreshVisuals(bool immediate)
    {
        bool canContinue = CanContinue();

        if (optionTexts != null)
        {
            for (int i = 0; i < optionTexts.Length; i++)
            {
                if (optionTexts[i] == null)
                    continue;

                if (i == ContinueIndex && !canContinue)
                {
                    optionTexts[i].color = lockedContinueTextColor;
                    continue;
                }

                optionTexts[i].color = i == _selectedIndex ? selectedTextColor : unselectedTextColor;
            }
        }

        if (selectionHighlight != null)
        {
            Vector2 anchored = selectionHighlight.anchoredPosition;
            float y = immediate
                ? _targetHighlightY
                : Mathf.Lerp(anchored.y, _targetHighlightY, 1f - Mathf.Exp(-selectionLerpSpeed * Time.unscaledDeltaTime));

            selectionHighlight.anchoredPosition = new Vector2(anchored.x, y);
        }

        if (hintText != null)
            hintText.text = "[W/S or ↑/↓] Navigate    [ENTER] Select";
    }

    void ExecuteSelection()
    {
        if (_selectedIndex == ContinueIndex && !CanContinue())
            return;

        if (_selectedIndex == NewGameIndex && markSaveOnNewGameStart && !string.IsNullOrWhiteSpace(continueSaveKey))
        {
            PlayerPrefs.SetInt(continueSaveKey, 1);
            PlayerPrefs.Save();
        }

        string targetScene = _selectedIndex == NewGameIndex ? newGameScene : continueScene;
        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogWarning("[MainMenuUI] Target scene is empty. Set scene names in the inspector.");
            return;
        }

        if (SceneTransitionManager.IsTransitioning)
            return;

        SceneTransitionManager.LoadScene(targetScene);
    }

    bool CanContinue()
    {
        if (string.IsNullOrWhiteSpace(continueSaveKey))
            return true;

        return PlayerPrefs.GetInt(continueSaveKey, 0) == 1;
    }
}
