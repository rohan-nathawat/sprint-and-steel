using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 1;
    EnemyKnockback kb;

    void Awake() {
        kb = GetComponent<EnemyKnockback>();
    }

    // call from player code, passing the push direction
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        health -= damage;
        if (kb != null)
            kb.Knockback(hitDirection);

        if (health <= 0)
           Destroy(gameObject);
    }
}