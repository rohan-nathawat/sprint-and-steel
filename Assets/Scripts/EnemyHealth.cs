using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 1;

    [Header("Hit Audio")]
    public AudioSource audioSource;
    public AudioClip hitSfx;
    [Range(0f, 1f)] public float hitSfxVolume = 1f;

    EnemyKnockback kb;

    void Awake() {
        kb = GetComponent<EnemyKnockback>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (hitSfx != null)
            hitSfx.LoadAudioData();
    }

    // call from player code, passing the push direction
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        TryPlayHitSfx();
        health -= damage;
        if (kb != null)
            kb.Knockback(hitDirection);

        if (health <= 0)
           Destroy(gameObject);
    }

    private void TryPlayHitSfx()
    {
        if (hitSfx == null || audioSource == null)
            return;

        audioSource.PlayOneShot(hitSfx, hitSfxVolume);
    }
}