using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static event Action<PlayerHealth> OnPlayerDied;
    public static event Action OnPlayerDamaged;

    [Header("Health")]
    public int maxHealth = 5;
    public float invulnerabilityTime = 0.25f;

    [Header("Regeneration")]
    public bool enableRegen = true;
    [Min(0f)] public float regenDelayAfterDamage = 3f;
    [Min(0f)] public float regenPerSecond = 0.5f;

    [Header("Hit Audio")]
    public AudioSource audioSource;
    public AudioClip hitSfx;
    [Range(0f, 1f)] public float hitSfxVolume = 1f;

    public int CurrentHealth { get; private set; }
    public float CurrentHealth01
    {
        get
        {
            float max = Mathf.Max(1f, maxHealth);
            return Mathf.Clamp01(_currentHealthFloat / max);
        }
    }

    private float invulnerabilityTimer;
    private float regenDelayTimer;
    private float _currentHealthFloat;
    private PlayerKnockback knockback;

    void Awake()
    {
        _currentHealthFloat = maxHealth;
        CurrentHealth = maxHealth;
        knockback = GetComponent<PlayerKnockback>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (hitSfx != null)
            hitSfx.LoadAudioData();

        Debug.Log($"Player Health: {CurrentHealth}/{maxHealth}");
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;

        if (!enableRegen)
            return;

        if (CurrentHealth <= 0 || _currentHealthFloat >= maxHealth)
            return;

        if (regenDelayTimer > 0f)
        {
            regenDelayTimer -= Time.deltaTime;
            return;
        }

        _currentHealthFloat = Mathf.Min(maxHealth, _currentHealthFloat + regenPerSecond * Time.deltaTime);
        CurrentHealth = Mathf.Clamp(Mathf.CeilToInt(_currentHealthFloat), 0, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector2.zero);
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (damage <= 0)
            return;

        if (invulnerabilityTimer > 0f)
            return;

        TryPlayHitSfx();
        _currentHealthFloat = Mathf.Max(0f, _currentHealthFloat - damage);
        CurrentHealth = Mathf.Clamp(Mathf.CeilToInt(_currentHealthFloat), 0, maxHealth);
        invulnerabilityTimer = invulnerabilityTime;
        regenDelayTimer = regenDelayAfterDamage;
        OnPlayerDamaged?.Invoke();
        Debug.Log($"Player took {damage} damage. Health: {CurrentHealth}/{maxHealth}");

        if (knockback != null)
        {
            Vector2 direction = hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : Vector2.zero;
            knockback.Knockback(direction);
        }

        if (CurrentHealth <= 0)
            Die();
    }

    private void TryPlayHitSfx()
    {
        if (hitSfx == null || audioSource == null)
            return;

        audioSource.PlayOneShot(hitSfx, hitSfxVolume);
    }

    private void Die()
    {
        OnPlayerDied?.Invoke(this);
        Debug.Log("Player died.");
        Scene activeScene = SceneManager.GetActiveScene();
        Time.timeScale = 0f;
        SceneTransitionManager.FreezePlayerInput(gameObject);
        SceneTransitionManager.LoadScene(activeScene.name);
    }
}
