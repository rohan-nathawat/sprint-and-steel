using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector2 direction = Vector2.right;
    private int damage = 1;
    private float speed = 6f;
    private float lifetime = 2f;
    private LayerMask playerLayer;

    public void Initialize(Vector2 shootDirection, int projectileDamage, float projectileSpeed, float projectileLifetime, LayerMask hitLayer)
    {
        direction = shootDirection.normalized;
        damage = projectileDamage;
        speed = projectileSpeed;
        lifetime = projectileLifetime;
        playerLayer = hitLayer;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage, direction);

        Destroy(gameObject);
    }
}
