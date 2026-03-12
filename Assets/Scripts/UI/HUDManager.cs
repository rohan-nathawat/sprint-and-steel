using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  HUDManager.cs
//  Attach to a Canvas GameObject in your scene.
//  Assign all references in the Inspector.
// ─────────────────────────────────────────────────────────────
public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    public Image healthBarFill;
    public TextMeshProUGUI healthPercentText;

    [Header("Sprint")]
    public Image sprintBarFill;
    public TextMeshProUGUI sprintStatusText;     // "READY" / "CHARGING"
    public Image sprintIconSquare;               // small coloured square

    [Header("Attack")]
    public Image attackBarFill;
    public TextMeshProUGUI attackStatusText;     // "READY" / "CHARGING"
    public Image attackIconSquare;

    [Header("Top Right")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI killsText;

    [Header("Auto Bind")]
    public bool autoBindSceneSystems = true;
    public float autoBindRetryInterval = 1f;

    // ── Colours (match the mockup) ──────────────────────────
    static readonly Color ColHealth  = new Color(0.878f, 0.361f, 0.478f); // #E05C7A
    static readonly Color ColSprint  = new Color(0.361f, 0.878f, 0.753f); // #5CE0C0
    static readonly Color ColAttack  = new Color(0.878f, 0.722f, 0.361f); // #E0B85C
    static readonly Color ColDim     = new Color(1f, 1f, 1f, 0.25f);
    static readonly Color ColReadyAtk= new Color(0.878f, 0.722f, 0.361f);

    // ── Internal state ──────────────────────────────────────
    float _currentHealth = 1f;   // 0–1
    float _currentSprint = 1f;   // 0–1
    float _currentAttack = 0.4f; // 0–1
    int   _kills = 0;
    float _timeRemaining = 180f; // seconds

    PlayerHealth _playerHealth;
    PlayerMovement _playerMovement;
    PlayerCombat _playerCombat;
    TimedRunManager _timedRunManager;
    float _nextAutoBindTime;

    void Awake()
    {
        AutoWireHudReferences();
        ApplyThemeColors();
    }

    void Start()
    {
        TryBindSceneSystems(true);
        RefreshFromGameplay();
    }

    void Update()
    {
        if (autoBindSceneSystems && Time.unscaledTime >= _nextAutoBindTime)
        {
            TryBindSceneSystems();
            _nextAutoBindTime = Time.unscaledTime + Mathf.Max(0.2f, autoBindRetryInterval);
        }

        RefreshFromGameplay();
    }

    // ─────────────────────────────────────────────────────────
    //  Public API — call these from your Player / GameManager
    // ─────────────────────────────────────────────────────────

    /// <summary>Update health bar. value = 0..1</summary>
    public void SetHealth(float value)
    {
        _currentHealth = Mathf.Clamp01(value);
        SetBarVisual(healthBarFill, _currentHealth);
        if (healthPercentText)
        {
            healthPercentText.text = Mathf.RoundToInt(_currentHealth * 100f) + "%";
            healthPercentText.color = ColHealth;
        }
    }

    /// <summary>Update sprint meter. value = 0..1</summary>
    public void SetSprint(float value)
    {
        _currentSprint = Mathf.Clamp01(value);
        SetBarVisual(sprintBarFill, _currentSprint);
        RefreshStatusLabel(sprintStatusText, sprintIconSquare, _currentSprint, ColSprint, "READY");
    }

    /// <summary>Update attack meter. value = 0..1</summary>
    public void SetAttack(float value)
    {
        _currentAttack = Mathf.Clamp01(value);
        SetBarVisual(attackBarFill, _currentAttack);
        RefreshStatusLabel(attackStatusText, attackIconSquare, _currentAttack, ColReadyAtk, "READY");
    }

    /// <summary>Set remaining time in seconds.</summary>
    public void SetTime(float seconds)
    {
        _timeRemaining = Mathf.Max(0, seconds);
        if (timerText)
        {
            int totalSeconds = Mathf.CeilToInt(_timeRemaining);
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;
            timerText.text = string.Format("{0:00}:{1:00}", m, s);
        }
    }

    /// <summary>Add kills count.</summary>
    public void SetKills(int kills)
    {
        _kills = kills;
        if (killsText) killsText.text = "Kills: " + kills;
    }

    // ─────────────────────────────────────────────────────────
    //  Internal helpers
    // ─────────────────────────────────────────────────────────

    void RefreshStatusLabel(TextMeshProUGUI label, Image icon, float value, Color readyColor, string readyWord)
    {
        bool ready = value >= 1f;
        if (label)
        {
            label.text  = ready ? readyWord : "CHARGING";
            label.color = ready ? readyColor : ColDim;
        }
        if (icon)
            icon.color = ready ? readyColor : ColDim;
    }

    void SetBarVisual(Image fillImage, float normalizedValue)
    {
        if (fillImage == null)
            return;

        float value = Mathf.Clamp01(normalizedValue);
        
        RectTransform rt = fillImage.rectTransform;
        if (rt == null)
            return;

        Vector2 anchorMin = rt.anchorMin;
        Vector2 anchorMax = rt.anchorMax;
        anchorMin.x = 0f;
        anchorMax.x = value;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;

        rt.offsetMin = new Vector2(0f, rt.offsetMin.y);
        rt.offsetMax = new Vector2(0f, rt.offsetMax.y);
    }

    void TryBindSceneSystems(bool force = false)
    {
        if (!autoBindSceneSystems)
            return;

        if (force || _playerHealth == null)
        {
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (_playerHealth == null)
            {
                GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
                if (playerByTag != null)
                    _playerHealth = playerByTag.GetComponent<PlayerHealth>();
            }
        }

        if (force || _playerMovement == null)
        {
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (_playerMovement == null)
            {
                GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
                if (playerByTag != null)
                    _playerMovement = playerByTag.GetComponent<PlayerMovement>();
            }
        }

        if (force || _playerCombat == null)
        {
            _playerCombat = FindFirstObjectByType<PlayerCombat>();
            if (_playerCombat == null)
            {
                GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
                if (playerByTag != null)
                    _playerCombat = playerByTag.GetComponent<PlayerCombat>();
            }
        }

        if (force || _timedRunManager == null)
            _timedRunManager = FindFirstObjectByType<TimedRunManager>();
    }

    void AutoWireHudReferences()
    {
        if (healthBarFill == null)
            healthBarFill = FindImageByName("Bar_Health_Fill");

        if (sprintBarFill == null)
            sprintBarFill = FindImageByName("Bar_Sprint_Fill");

        if (attackBarFill == null)
            attackBarFill = FindImageByName("Bar_Attack_Fill");

        if (sprintIconSquare == null)
            sprintIconSquare = FindImageByName("Icon", "Bar_Sprint_Row");

        if (attackIconSquare == null)
            attackIconSquare = FindImageByName("Icon", "Bar_Attack_Row");

        if (healthPercentText == null)
            healthPercentText = FindTextByName("Pct_Health");

        if (sprintStatusText == null)
            sprintStatusText = FindTextByName("Status_Sprint");

        if (attackStatusText == null)
            attackStatusText = FindTextByName("Status_Attack");

        if (timerText == null)
            timerText = FindTextByName("Text_Timer");

        if (killsText == null)
            killsText = FindTextByName("Text_Kills");
    }

    Image FindImageByName(string objectName, string parentName = null)
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null)
                continue;

            if (image.gameObject.name != objectName)
                continue;

            if (!string.IsNullOrEmpty(parentName))
            {
                Transform p = image.transform.parent;
                if (p == null || p.name != parentName)
                    continue;
            }

            return image;
        }

        return null;
    }

    TextMeshProUGUI FindTextByName(string objectName)
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text != null && text.gameObject.name == objectName)
                return text;
        }

        return null;
    }

    void RefreshFromGameplay()
    {
        if (_playerHealth != null)
        {
            float max = Mathf.Max(1f, _playerHealth.maxHealth);
            SetHealth(_playerHealth.CurrentHealth / max);
        }

        if (_playerMovement != null)
            SetSprint(_playerMovement.DashCharge01);

        if (_playerCombat != null)
            SetAttack(_playerCombat.AttackCharge01);

        if (_timedRunManager != null)
        {
            SetTime(_timedRunManager.TimeRemaining);
            SetKills(_timedRunManager.KillCount);
        }
    }

    void ApplyThemeColors()
    {
        if (healthBarFill) healthBarFill.color = ColHealth;
        if (sprintBarFill) sprintBarFill.color = ColSprint;
        if (attackBarFill) attackBarFill.color = ColAttack;
        if (sprintIconSquare) sprintIconSquare.color = ColSprint;
        if (attackIconSquare) attackIconSquare.color = ColDim;
    }

    // ─────────────────────────────────────────────────────────
    //  Demo: drive values in Play Mode without a player script
    // ─────────────────────────────────────────────────────────
#if UNITY_EDITOR
    [Header("── Editor Preview (remove in production) ──")]
    [Range(0,1)] public float previewHealth  = 0.65f;
    [Range(0,1)] public float previewSprint  = 1f;
    [Range(0,1)] public float previewAttack  = 0.4f;
    public int   previewKills = 3;
    public float previewTime  = 167f;

    void OnValidate()
    {
        ApplyThemeColors();

        if (Application.isPlaying)
            return;

        SetHealth(previewHealth);
        SetSprint(previewSprint);
        SetAttack(previewAttack);
        SetKills(previewKills);
        SetTime(previewTime);
    }
#endif
}
