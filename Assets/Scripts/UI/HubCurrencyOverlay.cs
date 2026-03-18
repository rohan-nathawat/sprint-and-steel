using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HubCurrencyOverlay : MonoBehaviour
{
    static HubCurrencyOverlay _instance;

    [SerializeField] string hubSceneName = "Hub";

    GameObject _overlayCanvas;
    TextMeshProUGUI _currencyText;
    bool _hudStyleApplied;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("HubCurrencyOverlay");
        _instance = go.AddComponent<HubCurrencyOverlay>();
        DontDestroyOnLoad(go);
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
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void Start()
    {
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void Update()
    {
        if (_overlayCanvas != null && _overlayCanvas.activeSelf)
        {
            if (!_hudStyleApplied)
                _hudStyleApplied = TryApplyHudTextPreset();

            RefreshCurrencyText();
        }
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool inHub = scene.name == hubSceneName;
        if (!inHub)
        {
            if (_overlayCanvas != null)
                _overlayCanvas.SetActive(false);
            return;
        }

        EnsureOverlayUI();
        _overlayCanvas.SetActive(true);
        _hudStyleApplied = TryApplyHudTextPreset();
        RefreshCurrencyText();
    }

    void EnsureOverlayUI()
    {
        if (_overlayCanvas != null)
            return;

        _overlayCanvas = new GameObject("HubCurrencyCanvas");
        DontDestroyOnLoad(_overlayCanvas);

        Canvas canvas = _overlayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 130;

        CanvasScaler scaler = _overlayCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObj = new GameObject("CurrencyText");
        textObj.transform.SetParent(_overlayCanvas.transform, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-36f, -36f);
        rt.sizeDelta = new Vector2(360f, 48f);

        _currencyText = textObj.AddComponent<TextMeshProUGUI>();
        _currencyText.fontSize = 30f;
        _currencyText.alignment = TextAlignmentOptions.TopRight;
        _currencyText.color = new Color(0.878f, 0.722f, 0.361f, 1f);
        _currencyText.textWrappingMode = TextWrappingModes.NoWrap;

        _hudStyleApplied = TryApplyHudTextPreset();
    }

    bool TryApplyHudTextPreset()
    {
        if (_currencyText == null)
            return false;

        HUDManager hud = FindFirstObjectByType<HUDManager>();
        if (hud == null)
            return false;

        TextMeshProUGUI source = hud.attackStatusText;
        if (source == null)
            source = hud.sprintStatusText;
        if (source == null)
            source = hud.healthPercentText;
        if (source == null)
            return false;

        _currencyText.font = source.font;
        _currencyText.fontSharedMaterial = source.fontSharedMaterial;
        _currencyText.fontSize = source.fontSize + 12;
        _currencyText.fontStyle = source.fontStyle;
        _currencyText.characterSpacing = source.characterSpacing;
        _currencyText.wordSpacing = source.wordSpacing;
        _currencyText.lineSpacing = source.lineSpacing;
        _currencyText.paragraphSpacing = source.paragraphSpacing;
        _currencyText.overflowMode = source.overflowMode;
        _currencyText.isOrthographic = source.isOrthographic;
        _currencyText.richText = source.richText;

        _currencyText.alignment = TextAlignmentOptions.TopRight;
        _currencyText.textWrappingMode = TextWrappingModes.NoWrap;
        _currencyText.color = new Color(0.878f, 0.722f, 0.361f, 1f);

        return true;
    }

    void RefreshCurrencyText()
    {
        if (_currencyText == null)
            return;

        _currencyText.text = $"${CurrencyWallet.Current}";
    }
}
