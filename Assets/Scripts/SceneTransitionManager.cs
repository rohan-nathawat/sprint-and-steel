using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    static SceneTransitionManager _instance;

    [Header("Fade")]
    [Range(0.05f, 5f)] public float fadeOutDuration = 1.8f;
    [Range(0f, 5f)] public float holdBlackBeforeLoadDuration = 1f;
    [Range(0f, 5f)] public float holdBlackAfterLoadDuration = 0.75f;
    [Range(0f, 5f)] public float fadeInDuration = 1.8f;
    public Color fadeColor = Color.black;

    [Header("Audio")]
    public bool fadeMusic = true;
    public bool fadeOnlyLoopingSources = true;

    [Header("Fallback")]
    public bool autoFadeOnExternalSceneLoad = true;

    CanvasGroup _fadeCanvasGroup;
    Image _fadeImage;
    bool _isTransitioning;
    float _currentMusicMultiplier = 1f;
    readonly List<AudioSource> _fadingSources = new List<AudioSource>();
    readonly List<float> _fadingSourceStartVolumes = new List<float>();
    PlayerInput _frozenPlayerInput;
    PlayerMovement _frozenPlayerMovement;
    PlayerCombat _frozenPlayerCombat;
    bool _frozenPlayerInputWasEnabled;
    bool _frozenPlayerMovementWasEnabled;
    bool _frozenPlayerCombatWasEnabled;

    public static bool IsTransitioning => _instance != null && _instance._isTransitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        EnsureInstance().StartCoroutine(EnsureInstance().LoadSceneRoutine(sceneName));
    }

    public static void FreezePlayerInput(GameObject playerRoot)
    {
        if (playerRoot == null)
            return;

        EnsureInstance().FreezePlayerInputInternal(playerRoot);
    }

    static SceneTransitionManager EnsureInstance()
    {
        if (_instance != null)
            return _instance;

        GameObject go = new GameObject("SceneTransitionManager");
        _instance = go.AddComponent<SceneTransitionManager>();
        DontDestroyOnLoad(go);
        _instance.EnsureFadeUI();
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
        EnsureFadeUI();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (_isTransitioning)
            yield break;

        _isTransitioning = true;
        EnsureFadeUI();

        if (_fadeCanvasGroup != null)
        {
            _fadeImage.color = fadeColor;
            CaptureMusicSourcesForFade();
            yield return FadeVisualAndMusic(1f, 0f, fadeOutDuration);
            if (holdBlackBeforeLoadDuration > 0f)
                yield return new WaitForSecondsRealtime(holdBlackBeforeLoadDuration);
        }

        yield return null;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            RestoreFrozenPlayerInput();
            _isTransitioning = false;
            yield break;
        }

        loadOperation.allowSceneActivation = false;
        while (loadOperation.progress < 0.9f)
            yield return null;

        loadOperation.allowSceneActivation = true;
        while (!loadOperation.isDone)
            yield return null;

        EnsureFadeUI();
        _fadeImage.color = fadeColor;
        _fadeCanvasGroup.alpha = 1f;

        if (holdBlackAfterLoadDuration > 0f)
            yield return new WaitForSecondsRealtime(holdBlackAfterLoadDuration);

        CaptureMusicSourcesForFade();
        ApplyMusicMultiplier(0f);
        yield return FadeVisualAndMusic(0f, 1f, fadeInDuration);

        RestoreFrozenPlayerInput();
        _isTransitioning = false;
    }

    void EnsureFadeUI()
    {
        if (_fadeCanvasGroup != null)
            return;

        GameObject canvasGo = new GameObject("SceneTransitionCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject fadeGo = new GameObject("Fade");
        fadeGo.transform.SetParent(canvasGo.transform, false);

        RectTransform rt = fadeGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _fadeImage = fadeGo.AddComponent<Image>();
        _fadeImage.color = fadeColor;

        _fadeCanvasGroup = fadeGo.AddComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 0f;
        _fadeCanvasGroup.blocksRaycasts = false;
        _fadeCanvasGroup.interactable = false;
    }

    IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float startAlpha = _fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        _fadeCanvasGroup.alpha = targetAlpha;
    }

    void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        if (_isTransitioning || !autoFadeOnExternalSceneLoad)
            return;

        EnsureFadeUI();
        StartCoroutine(FadeInAfterExternalLoad());
    }

    IEnumerator FadeInAfterExternalLoad()
    {
        _isTransitioning = true;

        _fadeImage.color = fadeColor;
        _fadeCanvasGroup.alpha = 1f;

        if (holdBlackAfterLoadDuration > 0f)
            yield return new WaitForSecondsRealtime(holdBlackAfterLoadDuration);

        CaptureMusicSourcesForFade();
        ApplyMusicMultiplier(0f);
        yield return FadeVisualAndMusic(0f, 1f, fadeInDuration);

        RestoreFrozenPlayerInput();
        _isTransitioning = false;
    }

    void FreezePlayerInputInternal(GameObject playerRoot)
    {
        if (playerRoot == null)
            return;

        Rigidbody2D rb = playerRoot.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        _frozenPlayerInput = playerRoot.GetComponent<PlayerInput>();
        _frozenPlayerMovement = playerRoot.GetComponent<PlayerMovement>();
        _frozenPlayerCombat = playerRoot.GetComponent<PlayerCombat>();

        if (_frozenPlayerInput != null)
        {
            _frozenPlayerInputWasEnabled = _frozenPlayerInput.enabled;
            _frozenPlayerInput.enabled = false;
        }

        if (_frozenPlayerMovement != null)
        {
            _frozenPlayerMovementWasEnabled = _frozenPlayerMovement.enabled;
            _frozenPlayerMovement.enabled = false;
        }

        if (_frozenPlayerCombat != null)
        {
            _frozenPlayerCombatWasEnabled = _frozenPlayerCombat.enabled;
            _frozenPlayerCombat.enabled = false;
        }
    }

    void RestoreFrozenPlayerInput()
    {
        if (_frozenPlayerInput != null)
            _frozenPlayerInput.enabled = _frozenPlayerInputWasEnabled;

        if (_frozenPlayerMovement != null)
            _frozenPlayerMovement.enabled = _frozenPlayerMovementWasEnabled;

        if (_frozenPlayerCombat != null)
            _frozenPlayerCombat.enabled = _frozenPlayerCombatWasEnabled;

        _frozenPlayerInput = null;
        _frozenPlayerMovement = null;
        _frozenPlayerCombat = null;
        _frozenPlayerInputWasEnabled = false;
        _frozenPlayerMovementWasEnabled = false;
        _frozenPlayerCombatWasEnabled = false;
    }

    void CaptureMusicSourcesForFade()
    {
        _fadingSources.Clear();
        _fadingSourceStartVolumes.Clear();

        if (!fadeMusic)
            return;

        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allSources)
        {
            if (source == null || !source.enabled || source.clip == null)
                continue;

            if (fadeOnlyLoopingSources && !source.loop)
                continue;

            _fadingSources.Add(source);
            _fadingSourceStartVolumes.Add(source.volume);
        }
    }

    IEnumerator FadeVisualAndMusic(float targetCanvasAlpha, float targetMusicMultiplier, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float startAlpha = _fadeCanvasGroup.alpha;
        float startMusicMultiplier = _currentMusicMultiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetCanvasAlpha, t);

            if (fadeMusic)
            {
                float currentMusicMultiplier = Mathf.Lerp(startMusicMultiplier, targetMusicMultiplier, t);
                ApplyMusicMultiplier(currentMusicMultiplier);
            }

            yield return null;
        }

        _fadeCanvasGroup.alpha = targetCanvasAlpha;
        if (fadeMusic)
            ApplyMusicMultiplier(targetMusicMultiplier);
    }

    void ApplyMusicMultiplier(float multiplier)
    {
        _currentMusicMultiplier = multiplier;

        for (int i = 0; i < _fadingSources.Count; i++)
        {
            AudioSource source = _fadingSources[i];
            if (source == null)
                continue;

            source.volume = _fadingSourceStartVolumes[i] * multiplier;
        }
    }
}