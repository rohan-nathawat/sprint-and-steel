using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public static event System.Action OnPlayerAttacked;

    [Header("Input & Animation")]
    public InputActionReference attackAction;
    public Animator animator;
    public Transform slashVisual;
    public float attackAnimationDuration = 0.15f;
    
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

    [Header("Attack Audio")]
    public AudioSource audioSource;
    public AudioClip attackSfx;
    [Range(0f, 1f)] public float attackSfxVolume = 1f;

    private PlayerMovement playerMovement;
    private Vector2 lastAttackDirection = Vector2.right;
    private float nextAttackTime;
    private GameObject activeSlashEffect;
    private bool isAttackAnimationPlaying;
    private Coroutine attackAnimationRoutine;

    private Vector2 slashAttackDirection;
    private Vector3 slashLocalOffset;
    private Vector3 slashAttackWorldOffset;
    private Quaternion slashRotation;
    private Coroutine followSlashRoutine;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (attackSfx != null)
            attackSfx.LoadAudioData();
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
        if (attackAction?.action == null)
            return;

        if (!attackAction.action.WasPressedThisFrame())
            return;

        TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        if (isAttackAnimationPlaying)
            return;

        nextAttackTime = Time.time + attackCooldown;

        TryPlayAttackSfx();
        Attack();
    }

    private void UpdateAttackPointPosition()
    {
        if (attackPoint == null || playerMovement == null)
            return;

        Vector2 moveDirection = playerMovement.GetMovementDirection;
        
        if (!isAttackAnimationPlaying && moveDirection.sqrMagnitude > 0.0001f)
            lastAttackDirection = moveDirection.normalized;

        attackPoint.position = transform.position + (Vector3)(lastAttackDirection * attackPointOffset);
    }

    private void Attack()
    {
        OnPlayerAttacked?.Invoke();
        TriggerAnimation();
        SpawnSlashEffect();
        DealDamage();
    }

    private void TryPlayAttackSfx()
    {
        if (attackSfx == null || audioSource == null)
            return;

        audioSource.PlayOneShot(attackSfx, attackSfxVolume);
    }

    private void TriggerAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetFloat("AttackX", lastAttackDirection.x);
            animator.SetFloat("AttackY", lastAttackDirection.y);

            if (attackAnimationRoutine != null)
                StopCoroutine(attackAnimationRoutine);

            attackAnimationRoutine = StartCoroutine(ResetAttackBool());
        }

        if (slashVisual != null)
        {
            float angle = Mathf.Atan2(lastAttackDirection.y, lastAttackDirection.x) * Mathf.Rad2Deg;
            slashVisual.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private IEnumerator ResetAttackBool()
    {
        isAttackAnimationPlaying = true;
        yield return new WaitForSeconds(attackAnimationDuration);
        
        if (followSlashRoutine != null)
        {
            StopCoroutine(followSlashRoutine);
            followSlashRoutine = null;
        }
        
        if (activeSlashEffect != null)
            activeSlashEffect.transform.position = transform.position + slashAttackWorldOffset;
        
        animator?.SetBool("IsAttacking", false);
        isAttackAnimationPlaying = false;
        attackAnimationRoutine = null;
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

        if (activeSlashEffect != null)
            return;

        slashAttackDirection = lastAttackDirection;
        
        float angle = Mathf.Atan2(slashAttackDirection.y, slashAttackDirection.x) * Mathf.Rad2Deg;
        slashRotation = Quaternion.Euler(0f, 0f, angle);

        slashLocalOffset = (Vector3)(slashAttackDirection * attackPointOffset);
        slashAttackWorldOffset = slashLocalOffset;
        Vector3 worldSpawnPosition = transform.position + slashLocalOffset;

        activeSlashEffect = Instantiate(slashEffectPrefab, worldSpawnPosition, slashRotation, null);

        if (parentSlashToPlayer)
        {
            if (followSlashRoutine != null)
                StopCoroutine(followSlashRoutine);
            followSlashRoutine = StartCoroutine(FollowPlayerWithFixedLocalOffset());
        }

        StartCoroutine(ClearSlashEffectAfterLifetime());
    }

    private IEnumerator FollowPlayerWithFixedLocalOffset()
    {
        while (activeSlashEffect != null && isAttackAnimationPlaying)
        {
            activeSlashEffect.transform.position = transform.position + slashAttackWorldOffset;
            activeSlashEffect.transform.rotation = slashRotation;
            yield return null;
        }
    }

    private IEnumerator ClearSlashEffectAfterLifetime()
    {
        if (activeSlashEffect == null)
            yield break;

        Destroy(activeSlashEffect, slashEffectLifetime);
        yield return new WaitForSeconds(slashEffectLifetime);
        
        if (followSlashRoutine != null)
        {
            StopCoroutine(followSlashRoutine);
            followSlashRoutine = null;
        }
        
        activeSlashEffect = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void ForceStopCombatVisuals()
    {
        if (attackAnimationRoutine != null)
        {
            StopCoroutine(attackAnimationRoutine);
            attackAnimationRoutine = null;
        }

        if (followSlashRoutine != null)
        {
            StopCoroutine(followSlashRoutine);
            followSlashRoutine = null;
        }

        isAttackAnimationPlaying = false;
        animator?.SetBool("IsAttacking", false);

        if (activeSlashEffect != null)
            Destroy(activeSlashEffect);

        activeSlashEffect = null;

        if (attackAction?.action != null)
            attackAction.action.Disable();
    }
}