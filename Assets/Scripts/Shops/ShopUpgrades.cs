using UnityEngine;
using UnityEngine.SceneManagement;

public enum ShopUpgradeType
{
    HealthRegenDelayReduction = 0,
    SprintDelayReduction = 1,
    AttackDelayReduction = 2,
    DamageIncrease = 3,
    DamageRadiusIncrease = 4,
}

public struct ShopUpgradeDefinition
{
    public ShopUpgradeType type;
    public string title;
    public string description;
    public int maxLevel;
    public int baseCost;
    public int costStep;
}

public static class ShopUpgrades
{
    const string UpgradeKeyPrefix = "ShopUpgrade_";

    static readonly ShopUpgradeDefinition[] Definitions =
    {
        new ShopUpgradeDefinition
        {
            type = ShopUpgradeType.HealthRegenDelayReduction,
            title = "Health Regen Delay",
            description = "Start healing sooner after taking damage.",
            maxLevel = 5,
            baseCost = 50,
            costStep = 20,
        },
        new ShopUpgradeDefinition
        {
            type = ShopUpgradeType.SprintDelayReduction,
            title = "Sprint Delay",
            description = "Reduce dash cooldown between sprints.",
            maxLevel = 7,
            baseCost = 60,
            costStep = 25,
        },
        new ShopUpgradeDefinition
        {
            type = ShopUpgradeType.AttackDelayReduction,
            title = "Attack Delay",
            description = "Reduce time between consecutive attacks.",
            maxLevel = 3,
            baseCost = 60,
            costStep = 30,
        },
        new ShopUpgradeDefinition
        {
            type = ShopUpgradeType.DamageIncrease,
            title = "Damage",
            description = "Increase damage dealt per hit.",
            maxLevel = 5,
            baseCost = 70,
            costStep = 35,
        },
        new ShopUpgradeDefinition
        {
            type = ShopUpgradeType.DamageRadiusIncrease,
            title = "Damage Radius",
            description = "Increase sword hit radius.",
            maxLevel = 5,
            baseCost = 60,
            costStep = 30,
        },
    };

    public static int Count => Definitions.Length;

    public static ShopUpgradeDefinition GetDefinition(int index)
    {
        index = Mathf.Clamp(index, 0, Definitions.Length - 1);
        return Definitions[index];
    }

    public static ShopUpgradeDefinition GetDefinition(ShopUpgradeType type)
    {
        return Definitions[(int)type];
    }

    public static int GetLevel(ShopUpgradeType type)
    {
        var def = GetDefinition(type);
        int saved = PlayerPrefs.GetInt(UpgradeKeyPrefix + type, 0);
        return Mathf.Clamp(saved, 0, def.maxLevel);
    }

    public static int GetCost(ShopUpgradeType type)
    {
        var def = GetDefinition(type);
        int level = GetLevel(type);
        if (level >= def.maxLevel)
            return 0;

        return def.baseCost + (def.costStep * level);
    }

    public static bool IsMaxed(ShopUpgradeType type)
    {
        var def = GetDefinition(type);
        return GetLevel(type) >= def.maxLevel;
    }

    public static void ResetAll()
    {
        for (int i = 0; i < Definitions.Length; i++)
            PlayerPrefs.DeleteKey(UpgradeKeyPrefix + Definitions[i].type);

        PlayerPrefs.Save();

        if (Application.isPlaying)
            ShopUpgradeRuntime.ApplyToCurrentPlayer();
    }

    public static bool TryPurchase(ShopUpgradeType type, out string statusMessage)
    {
        var def = GetDefinition(type);
        int level = GetLevel(type);

        if (level >= def.maxLevel)
        {
            statusMessage = "Already max level.";
            return false;
        }

        int cost = GetCost(type);
        if (!CurrencyWallet.TrySpend(cost))
        {
            statusMessage = "Not enough currency.";
            return false;
        }

        int newLevel = level + 1;
        PlayerPrefs.SetInt(UpgradeKeyPrefix + type, newLevel);
        PlayerPrefs.Save();

        ShopUpgradeRuntime.ApplyToCurrentPlayer();

        statusMessage = $"{def.title} upgraded to Lv {newLevel}.";
        return true;
    }
}

public class PlayerShopStats : MonoBehaviour
{
    PlayerHealth _playerHealth;
    PlayerMovement _playerMovement;
    PlayerCombat _playerCombat;

    float _baseRegenDelay;
    float _baseSprintDelay;
    float _baseAttackDelay;
    int _baseDamage;
    float _baseAttackRadius;

    bool _hasBaseValues;

    void Awake()
    {
        CacheBaseValues();
    }

    public void ApplyUpgradesFromSave()
    {
        CacheBaseValues();
        if (!_hasBaseValues)
            return;

        int regenLevel = ShopUpgrades.GetLevel(ShopUpgradeType.HealthRegenDelayReduction);
        int sprintLevel = ShopUpgrades.GetLevel(ShopUpgradeType.SprintDelayReduction);
        int attackDelayLevel = ShopUpgrades.GetLevel(ShopUpgradeType.AttackDelayReduction);
        int damageLevel = ShopUpgrades.GetLevel(ShopUpgradeType.DamageIncrease);
        int radiusLevel = ShopUpgrades.GetLevel(ShopUpgradeType.DamageRadiusIncrease);

        _playerHealth.regenDelayAfterDamage = Mathf.Max(0.25f, _baseRegenDelay - (regenLevel * 0.2f));
        _playerMovement.dashCooldown = Mathf.Max(0.1f, _baseSprintDelay - (sprintLevel * 0.08f));
        _playerCombat.attackCooldown = Mathf.Max(0.05f, _baseAttackDelay - (attackDelayLevel * 0.05f));
        _playerCombat.damage = _baseDamage + damageLevel;
        _playerCombat.attackRange = _baseAttackRadius + (radiusLevel * 0.15f);
    }

    void CacheBaseValues()
    {
        if (_hasBaseValues)
            return;

        _playerHealth = GetComponent<PlayerHealth>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();

        if (_playerHealth == null || _playerMovement == null || _playerCombat == null)
            return;

        _baseRegenDelay = _playerHealth.regenDelayAfterDamage;
        _baseSprintDelay = _playerMovement.dashCooldown;
        _baseAttackDelay = _playerCombat.attackCooldown;
        _baseDamage = _playerCombat.damage;
        _baseAttackRadius = _playerCombat.attackRange;
        _hasBaseValues = true;
    }
}

public class ShopUpgradeRuntime : MonoBehaviour
{
    static ShopUpgradeRuntime _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstance();
    }

    static ShopUpgradeRuntime EnsureInstance()
    {
        if (_instance != null)
            return _instance;

        GameObject go = new GameObject("ShopUpgradeRuntime");
        _instance = go.AddComponent<ShopUpgradeRuntime>();
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
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void Start()
    {
        ApplyToCurrentPlayer();
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToCurrentPlayer();
    }

    public static void ApplyToCurrentPlayer()
    {
        EnsureInstance();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        PlayerShopStats stats = player.GetComponent<PlayerShopStats>();
        if (stats == null)
            stats = player.AddComponent<PlayerShopStats>();

        stats.ApplyUpgradesFromSave();
    }
}