using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // Drag your player here
    public Rigidbody2D rb;            // Enemy Rigidbody2D
    public Animator animator;         // Optional animator for idle/chase

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float detectionRadius = 5f;
    public float attackRadius = 1f;   // Optional: distance to trigger attack
    public float knockbackDuration = 0.2f;
    public float knockbackForce = 5f;
    public EnemyKnockback knockbackScript;

    [Header("Knockback Recovery")]
    public float postKnockbackPause = 0.2f;

    public LayerMask obstacleLayer;    // Layer for obstacles to avoid
    public float avoidanceDistance = 1f; // Distance to check for obstacles
    public float turnSpeed = 5f;      // How quickly the enemy turns

    private enum EnemyState { Idle, Chase, Knockback }
    private EnemyState state = EnemyState.Idle;

    private Vector2 knockbackDir;
    private float knockbackRecoveryTimer = 0f;

    void Update()
    {
        if(player == null) return;

        // Handle knockback timer
        if(knockbackScript != null && knockbackScript.isKnockbackActive) 
        {
            knockbackRecoveryTimer = postKnockbackPause;
            rb.linearVelocity = Vector2.zero; // Stop AI movement
            return; // Skip chasing / idle logic
        }

        // Pause briefly after knockback before resuming follow behavior.
        if (knockbackRecoveryTimer > 0f)
        {
            knockbackRecoveryTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // State switching
        if(distance <= detectionRadius) state = EnemyState.Chase;
        else state = EnemyState.Idle;

        // Optional: switch to attack if within attack radius
        if(distance <= attackRadius) 
        {
            // Here you can trigger attack animation / logic
            // e.g., animator.SetTrigger("Attack");
        }

        // State behavior
        switch(state)
        {
            case EnemyState.Idle:
                rb.linearVelocity = Vector2.zero;
                // Optional: play idle animation
                // animator.SetBool("IsMoving", false);
                break;

            case EnemyState.Chase:
                ChasePlayer();
                // Optional: play moving animation
                // animator.SetBool("IsMoving", true);
                break;
        }
    }

    private void ChasePlayer()
    {
        Vector2 targetDirection = (player.position - transform.position).normalized;
        Vector2 currentDirection = rb.linearVelocity.normalized;
        
        // If not moving, start with target direction
        if (currentDirection == Vector2.zero) currentDirection = targetDirection;
        
        // Smoothly interpolate towards target direction
        Vector2 desiredDirection = Vector2.Lerp(currentDirection, targetDirection, Time.deltaTime * turnSpeed);
        
        // Check for obstacles ahead
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDirection, avoidanceDistance, obstacleLayer);
        if (hit.collider != null)
        {
            // Obstacle detected, try to avoid by turning
            Vector2 avoidDirection = Vector2.Perpendicular(desiredDirection);
            
            // Check left side
            if (Physics2D.Raycast(transform.position, avoidDirection, avoidanceDistance, obstacleLayer).collider == null)
            {
                desiredDirection = avoidDirection;
            }
            // Check right side
            else if (Physics2D.Raycast(transform.position, -avoidDirection, avoidanceDistance, obstacleLayer).collider == null)
            {
                desiredDirection = -avoidDirection;
            }
            else
            {
                // Both sides blocked, stop or reverse
                desiredDirection = -desiredDirection;
            }
        }
        
        rb.linearVelocity = desiredDirection * moveSpeed;
    }

    // Call this from your player attack

    // Optional: visualize detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}