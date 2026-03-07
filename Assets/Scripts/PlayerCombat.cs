using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Input & Animation")]
    public InputActionReference attackAction;
    public Animator animator;
    public Transform slashVisual; // Slash animation object to rotate (optional)
    
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 2.4f;
    public float attackPointOffset = 1f;
    public float attackCooldown = 0.5f;
    public int damage = 1;
    public LayerMask enemyLayer;

    [Header("Slash VFX")]
    public GameObject slashEffectPrefab;
    public Transform slashSpawnPoint;
    public float slashEffectLifetime = 0.2f;
    public bool parentSlashToPlayer = true;

    private PlayerMovement playerMovement;
    private Vector2 lastAttackDirection = Vector2.right;
    private float nextAttackTime;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        UpdateAttackPointPosition();
        HandleAttackInput();
    }

    private void OnEnable()
    {
        if (attackAction?.action != null)
            attackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (attackAction?.action != null)
            attackAction.action.Disable();
    }

    private void HandleAttackInput()
    {
        if (attackAction?.action == null || !attackAction.action.WasPressedThisFrame())
            return;

        if (Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UpdateAttackPointPosition()
    {
        if (attackPoint == null || playerMovement == null)
            return;

        Vector2 moveDirection = playerMovement.GetMovementDirection;
        
        if (moveDirection.sqrMagnitude > 0.0001f)
            lastAttackDirection = moveDirection.normalized;

        attackPoint.position = transform.position + (Vector3)(lastAttackDirection * attackPointOffset);
    }

    private void Attack()
    {
        TriggerAnimation();
        SpawnSlashEffect();
        DealDamage();
    }

    private void TriggerAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetFloat("AttackX", lastAttackDirection.x);
            animator.SetFloat("AttackY", lastAttackDirection.y);
            StartCoroutine(ResetAttackBool());
        }

        // Rotate slash visual to face attack direction
        if (slashVisual != null)
        {
            float angle = Mathf.Atan2(lastAttackDirection.y, lastAttackDirection.x) * Mathf.Rad2Deg;
            slashVisual.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private IEnumerator ResetAttackBool()
    {
        yield return null;
        animator?.SetBool("IsAttacking", false);
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health != null)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                health.TakeDamage(damage, direction);
            }
        }
    }

    private void SpawnSlashEffect()
    {
        if (slashEffectPrefab == null)
            return;

        Transform spawnTransform = slashSpawnPoint != null ? slashSpawnPoint : transform;
        float angle = Mathf.Atan2(lastAttackDirection.y, lastAttackDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        Transform parent = parentSlashToPlayer ? spawnTransform : null;

        GameObject slash = Instantiate(slashEffectPrefab, spawnTransform.position, rotation, parent);
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