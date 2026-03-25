using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CurrencyWallet
{
    const string CurrencyKey = "PlayerCurrency";

    public static int Current => PlayerPrefs.GetInt(CurrencyKey, 0);

    public static void Add(int amount)
    {
        if (amount <= 0)
            return;

        PlayerPrefs.SetInt(CurrencyKey, Current + amount);
        PlayerPrefs.Save();
    }

    public static void Set(int amount)
    {
        PlayerPrefs.SetInt(CurrencyKey, Mathf.Max(0, amount));
        PlayerPrefs.Save();
    }

    public static void Reset()
    {
        Set(0);
    }

    public static bool CanAfford(int amount)
    {
        if (amount <= 0)
            return true;

        return Current >= amount;
    }

    public static bool TrySpend(int amount)
    {
        if (amount <= 0)
            return true;

        if (!CanAfford(amount))
            return false;

        PlayerPrefs.SetInt(CurrencyKey, Current - amount);
        PlayerPrefs.Save();
        return true;
    }
}

public class QuestRuntimeManager : MonoBehaviour
{
    static QuestRuntimeManager _instance;

    QuestData _activeQuest;
    int _objectiveProgress;
    bool _completionTriggered;

    GameObject _runtimeCanvas;
    TMP_Text _objectiveText;
    GameObject _completionPopup;
    TMP_Text _completionText;

    public static void BeginQuest(QuestData quest)
    {
        if (quest == null)
            return;

        EnsureInstance().BeginQuestInternal(quest);
    }

    static QuestRuntimeManager EnsureInstance()
    {
        if (_instance != null)
            return _instance;

        GameObject go = new GameObject("QuestRuntimeManager");
        _instance = go.AddComponent<QuestRuntimeManager>();
        DontDestroyOnLoad(go);
        return _instance;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        EnemyHealth.OnEnemyDied += HandleEnemyDied;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        EnemyHealth.OnEnemyDied -= HandleEnemyDied;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void BeginQuestInternal(QuestData quest)
    {
        _activeQuest = quest;
        _objectiveProgress = 0;
        _completionTriggered = false;

        if (string.IsNullOrEmpty(_activeQuest.sceneName))
        {
            Debug.LogWarning($"[QuestRuntime] '{_activeQuest.questName}' has no sceneName set.");
            return;
        }

        SceneTransitionManager.LoadScene(_activeQuest.sceneName);
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_activeQuest == null)
            return;

        bool inQuestScene = scene.name == _activeQuest.sceneName;
        if (inQuestScene)
        {
            if (_activeQuest.objectiveType == QuestObjectiveType.NoObjective)
            {
                _activeQuest = null;
                _objectiveProgress = 0;
                _completionTriggered = false;
                DestroyRuntimeUI();
                return;
            }

            EnsureRuntimeUI();
            RefreshObjectiveText();
            if (_completionPopup != null)
                _completionPopup.SetActive(false);

            return;
        }

        DestroyRuntimeUI();
    }

    void HandleEnemyDied(EnemyHealth _)
    {
        if (_activeQuest == null || _completionTriggered)
            return;

        if (SceneManager.GetActiveScene().name != _activeQuest.sceneName)
            return;

        if (_activeQuest.objectiveType != QuestObjectiveType.DefeatEnemies)
            return;

        _objectiveProgress++;
        RefreshObjectiveText();

        if (_objectiveProgress >= Mathf.Max(1, _activeQuest.objectiveTarget))
            CompleteQuest();
    }

    void CompleteQuest()
    {
        if (_activeQuest == null || _completionTriggered)
            return;

        _completionTriggered = true;

        int reward = Mathf.Max(0, _activeQuest.rewardCurrency);
        CurrencyWallet.Add(reward);

        ShowCompletionPopup(reward);
        StartCoroutine(ReturnToHubAfterDelay());
    }

    IEnumerator ReturnToHubAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2.25f);

        string returnScene = string.IsNullOrEmpty(_activeQuest.hubSceneName)
            ? "Hub"
            : _activeQuest.hubSceneName;

        _activeQuest = null;
        _objectiveProgress = 0;
        _completionTriggered = false;
        DestroyRuntimeUI();

        SceneTransitionManager.LoadScene(returnScene);
    }

    void EnsureRuntimeUI()
    {
        if (_runtimeCanvas != null)
            return;

        _runtimeCanvas = new GameObject("QuestRuntimeUI");
        DontDestroyOnLoad(_runtimeCanvas);

        Canvas canvas = _runtimeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

        CanvasScaler scaler = _runtimeCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        _runtimeCanvas.AddComponent<GraphicRaycaster>();

        GameObject objectivePanel = new GameObject("ObjectivePanel");
        objectivePanel.transform.SetParent(_runtimeCanvas.transform, false);
        RectTransform opRt = objectivePanel.AddComponent<RectTransform>();
        opRt.anchorMin = new Vector2(1f, 1f);
        opRt.anchorMax = new Vector2(1f, 1f);
        opRt.pivot = new Vector2(1f, 1f);
        opRt.anchoredPosition = new Vector2(-28f, -28f);
        opRt.sizeDelta = new Vector2(520f, 88f);

        Image opBg = objectivePanel.AddComponent<Image>();
        opBg.color = new Color(0f, 0f, 0f, 0.58f);

        GameObject objectiveTextObj = new GameObject("ObjectiveText");
        objectiveTextObj.transform.SetParent(objectivePanel.transform, false);
        RectTransform otRt = objectiveTextObj.AddComponent<RectTransform>();
        otRt.anchorMin = Vector2.zero;
        otRt.anchorMax = Vector2.one;
        otRt.offsetMin = new Vector2(14f, 8f);
        otRt.offsetMax = new Vector2(-14f, -8f);

        _objectiveText = objectiveTextObj.AddComponent<TextMeshProUGUI>();
        _objectiveText.fontSize = 26f;
        _objectiveText.color = new Color(0.93f, 0.94f, 0.95f, 1f);
        _objectiveText.alignment = TextAlignmentOptions.TopRight;
        _objectiveText.enableWordWrapping = true;

        _completionPopup = new GameObject("QuestCompletePopup");
        _completionPopup.transform.SetParent(_runtimeCanvas.transform, false);
        RectTransform cpRt = _completionPopup.AddComponent<RectTransform>();
        cpRt.anchorMin = new Vector2(0.5f, 0.5f);
        cpRt.anchorMax = new Vector2(0.5f, 0.5f);
        cpRt.pivot = new Vector2(0.5f, 0.5f);
        cpRt.anchoredPosition = Vector2.zero;
        cpRt.sizeDelta = new Vector2(560f, 180f);

        Image cpBg = _completionPopup.AddComponent<Image>();
        cpBg.color = new Color(0f, 0f, 0f, 0.86f);

        GameObject completionTextObj = new GameObject("CompletionText");
        completionTextObj.transform.SetParent(_completionPopup.transform, false);
        RectTransform ctRt = completionTextObj.AddComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.offsetMin = new Vector2(18f, 14f);
        ctRt.offsetMax = new Vector2(-18f, -14f);

        _completionText = completionTextObj.AddComponent<TextMeshProUGUI>();
        _completionText.fontSize = 34f;
        _completionText.color = new Color(0.361f, 0.878f, 0.753f, 1f);
        _completionText.alignment = TextAlignmentOptions.Center;
        _completionText.enableWordWrapping = true;

        _completionPopup.SetActive(false);
    }

    void RefreshObjectiveText()
    {
        if (_objectiveText == null || _activeQuest == null)
            return;

        int target = Mathf.Max(1, _activeQuest.objectiveTarget);
        string objectiveLine = _activeQuest.objectiveType switch
        {
            QuestObjectiveType.NoObjective => "No objective",
            QuestObjectiveType.DefeatEnemies => $"Defeat enemies: {_objectiveProgress}/{target}",
            _ => $"Progress: {_objectiveProgress}/{target}",
        };

        _objectiveText.text = $"QUEST OBJECTIVE\n{objectiveLine}";
    }

    void ShowCompletionPopup(int reward)
    {
        if (_completionPopup == null || _completionText == null || _activeQuest == null)
            return;

        _completionText.text = $"Quest Complete!\n{_activeQuest.questName}\n+{reward} Currency";
        _completionPopup.SetActive(true);
    }

    void DestroyRuntimeUI()
    {
        if (_runtimeCanvas != null)
            Destroy(_runtimeCanvas);

        _runtimeCanvas = null;
        _objectiveText = null;
        _completionPopup = null;
        _completionText = null;
    }
}
