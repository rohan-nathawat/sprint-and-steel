using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Rigidbody2D rb;
    public EnemyFollow followScript;
    public LayerMask playerLayer;

    [Header("Shared Attack")]
    public int damage = 1;
    public float attackRange = 1.4f;
    public float attackCooldown = 1f;
    public float attackWindup = 0.15f;

    private float nextAttackTime;
    private bool isAttacking;

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (followScript == null)
            followScript = GetComponent<EnemyFollow>();

        Debug.Log($"{gameObject.name} EnemyCombat initialized. AttackRange: {attackRange}, PlayerLayer: {playerLayer.value}");
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning($"{gameObject.name}: No player reference!");
            return;
        }

        if (isAttacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange)
            return;

        Debug.Log($"{gameObject.name} starting attack! Distance: {distance:F2}");
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (followScript != null)
            followScript.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (attackWindup > 0f)
            yield return new WaitForSeconds(attackWindup);

        DoMediumAttack();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        nextAttackTime = Time.time + attackCooldown;

        if (followScript != null)
            followScript.enabled = true;

        isAttacking = false;
    }

    private void DoMediumAttack()
    {
        TryDamagePlayerInRadius(attackRange);
    }

    private void TryDamagePlayerInRadius(float radius)
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, playerLayer);
        if (hit == null)
        {
            return; // Silenced - too spammy for tank charge
        }

        Debug.Log($"{gameObject.name}: FOUND PLAYER COLLIDER '{hit.name}'! Checking for PlayerHealth...");
        
        PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Vector2 hitDirection = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            Debug.Log($"{gameObject.name}: Calling TakeDamage({damage}) on player...");
            playerHealth.TakeDamage(damage, hitDirection);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Hit collider '{hit.name}' has NO PlayerHealth component!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
