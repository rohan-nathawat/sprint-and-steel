using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Directional Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite upLeftSprite;
    public Sprite upRightSprite;
    public Sprite downLeftSprite;
    public Sprite downRightSprite;

    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    [Header("Dash Audio")]
    public AudioSource audioSource;
    public AudioClip dashSfx;
    [Range(0f, 1f)] public float dashSfxVolume = 1f;
    private float dashCooldownTimer = 0f;

    public float DashCharge01
    {
        get
        {
            if (dashCooldown <= 0f)
                return 1f;

            return Mathf.Clamp01(1f - (dashCooldownTimer / dashCooldown));
        }
    }

    private Rigidbody2D rb;
    private PlayerKnockback playerKnockback;
    public InputActionReference movementAction;
    private Vector2 movement;
    public AnimationCurve dashCurve;
    
    public Vector2 GetMovementDirection => movement;

    private bool isDashing;
    private Vector2 lastFacingDirection = Vector2.down;
    private HashSet<EnemyHealth> enemiesHitThisDash = new HashSet<EnemyHealth>();

    [Header("Dash Collision")]
    public LayerMask enemyLayer;
    public float dashCollisionRadius = 1.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerKnockback = GetComponent<PlayerKnockback>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (dashSfx != null)
            dashSfx.LoadAudioData();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerKnockback != null && playerKnockback.isKnockbackActive)
        {
            UpdateDirectionSprite();
            return;
        }

        bool isDashingInput = Keyboard.current.leftShiftKey.isPressed;

        if (movement.sqrMagnitude > 1f) movement = movement.normalized;
        rb.linearVelocity = movement * moveSpeed;
        UpdateDirectionSprite();

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        if (isDashingInput && !isDashing && dashCooldownTimer <= 0)
        {
            TryPlayDashSfx();
            StartCoroutine(Dash(movement));
            dashCooldownTimer = dashCooldown;
        }
    }

    private void UpdateDirectionSprite()
    {
        if (spriteRenderer == null)
            return;

        Vector2 direction = movement;
        if (direction.sqrMagnitude > 0.0001f)
            lastFacingDirection = direction.normalized;
        else
            direction = lastFacingDirection;

        Sprite targetSprite = GetSpriteForDirection(direction);
        if (targetSprite != null)
            spriteRenderer.sprite = targetSprite;
    }

    private Sprite GetSpriteForDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0f)
            angle += 360f;

        if (angle >= 337.5f || angle < 22.5f)
            return rightSprite;
        if (angle < 67.5f)
            return upRightSprite;
        if (angle < 112.5f)
            return upSprite;
        if (angle < 157.5f)
            return upLeftSprite;
        if (angle < 202.5f)
            return leftSprite;
        if (angle < 247.5f)
            return downLeftSprite;
        if (angle < 292.5f)
            return downSprite;

        return downRightSprite;
    }

    public void Movement(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    private void TryPlayDashSfx()
    {
        if (dashSfx == null || audioSource == null)
            return;

        audioSource.PlayOneShot(dashSfx, dashSfxVolume);
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        enemiesHitThisDash.Clear();
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            if (playerKnockback != null && playerKnockback.isKnockbackActive)
                break;

            float dashProgress = elapsedTime / dashDuration;
            float currentDashSpeed = dashCurve.Evaluate(dashProgress) * dashSpeed;
            rb.linearVelocity = direction * currentDashSpeed;

            CheckDashCollisions();

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private void CheckDashCollisions()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashCollisionRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health == null || enemiesHitThisDash.Contains(health))
                continue;

            enemiesHitThisDash.Add(health);
            Vector2 direction = (hit.transform.position - transform.position).normalized;
            health.TakeDamage(999, direction);
        }
    }
}
