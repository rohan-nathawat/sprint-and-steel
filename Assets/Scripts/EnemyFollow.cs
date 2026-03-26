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

    public LayerMask obstacleLayer;   
    public float avoidanceDistance = 1f;
    public float turnSpeed = 5f;    

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
            rb.linearVelocity = Vector2.zero;
            return; 
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

        
        if(distance <= attackRadius) 
        {
            // animator.SetTrigger("Attack");
        }

        // State behavior
        switch(state)
        {
            case EnemyState.Idle:
                rb.linearVelocity = Vector2.zero;
                // animator.SetBool("IsMoving", false);
                break;

            case EnemyState.Chase:
                ChasePlayer();
                // animator.SetBool("IsMoving", true);
                break;
        }
    }

    private void ChasePlayer()
    {
        Vector2 targetDirection = (player.position - transform.position).normalized;
        Vector2 currentDirection = rb.linearVelocity.normalized;

        if (currentDirection == Vector2.zero) currentDirection = targetDirection;

        Vector2 desiredDirection = Vector2.Lerp(currentDirection, targetDirection, Time.deltaTime * turnSpeed);
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDirection, avoidanceDistance, obstacleLayer);
        if (hit.collider != null)
        {
            
            Vector2 avoidDirection = Vector2.Perpendicular(desiredDirection);

            if (Physics2D.Raycast(transform.position, avoidDirection, avoidanceDistance, obstacleLayer).collider == null)
            {
                desiredDirection = avoidDirection;
            }
  
            else if (Physics2D.Raycast(transform.position, -avoidDirection, avoidanceDistance, obstacleLayer).collider == null)
            {
                desiredDirection = -avoidDirection;
            }
            else
            {
                desiredDirection = -desiredDirection;
            }
        }
        
        rb.linearVelocity = desiredDirection * moveSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}