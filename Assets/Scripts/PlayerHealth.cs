using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public static event Action<PlayerHealth> OnPlayerDied;
    public static event Action OnPlayerDamaged;

    [Header("Health")]
    public int maxHealth = 5;
    public float invulnerabilityTime = 0.25f;

    [Header("Hit Audio")]
    public AudioSource audioSource;
    public AudioClip hitSfx;
    [Range(0f, 1f)] public float hitSfxVolume = 1f;

    public int CurrentHealth { get; private set; }

    private float invulnerabilityTimer;
    private PlayerKnockback knockback;

    void Awake()
    {
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
        CurrentHealth -= damage;
        invulnerabilityTimer = invulnerabilityTime;
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
        Destroy(gameObject);
    }
}
