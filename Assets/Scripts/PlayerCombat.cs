using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public InputActionReference attackAction;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 2.4f;
    public LayerMask enemyLayer;

    public float attackCooldown = 0.12f; // FAST
    private float lastAttackTime;

    public int damage = 1;

    [Header("Slash VFX")]
    public GameObject slashEffectPrefab;
    public Transform slashSpawnPoint;
    public float slashEffectLifetime = 0.2f;
    public bool parentSlashToPlayer = true;

    private void OnEnable()
    {
        attackAction.action.performed += OnAttack;
        attackAction.action.Enable();
    }

    private void OnDisable()
    {
        attackAction.action.performed -= OnAttack;
        attackAction.action.Disable();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        Attack();
    }

    private void Attack()
    {
        SpawnSlashEffect();

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in hits)
        {
            Vector2 direction = (enemy.transform.position - transform.position).normalized;

            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, direction);
            }
        }
    }

    private void SpawnSlashEffect()
    {
        if (slashEffectPrefab == null)
            return;

        Transform spawnTransform = slashSpawnPoint != null ? slashSpawnPoint : transform;
        Vector3 spawnPosition = spawnTransform.position;

        Vector2 aimDirection = (attackPoint.position - transform.position);
        if (aimDirection.sqrMagnitude < 0.0001f)
            aimDirection = Vector2.right;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        Transform parent = parentSlashToPlayer ? spawnTransform : null;
        GameObject slash = Instantiate(slashEffectPrefab, spawnPosition, rotation, parent);
        Destroy(slash, slashEffectLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}