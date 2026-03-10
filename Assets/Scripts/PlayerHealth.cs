using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public float invulnerabilityTime = 0.25f;

    public int CurrentHealth { get; private set; }

    private float invulnerabilityTimer;
    private PlayerKnockback knockback;

    void Awake()
    {
        CurrentHealth = maxHealth;
        knockback = GetComponent<PlayerKnockback>();
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

        CurrentHealth -= damage;
        invulnerabilityTimer = invulnerabilityTime;
        Debug.Log($"Player took {damage} damage. Health: {CurrentHealth}/{maxHealth}");

        if (knockback != null)
        {
            Vector2 direction = hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : Vector2.zero;
            knockback.Knockback(direction);
        }

        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Player died.");
        Destroy(gameObject);
    }
}
