using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public static event Action<EnemyHealth> OnEnemyDied;

    public int health = 1;

    [Header("Death VFX")]
    public ParticleSystem deathParticlesPrefab;

    [Header("Hit Audio")]
    public AudioSource audioSource;
    public AudioClip hitSfx;
    [Range(0f, 1f)] public float hitSfxVolume = 1f;

    [Header("Death Audio")]
    public AudioClip deathSfx;
    [Range(0f, 1f)] public float deathSfxVolume = 1f;

    EnemyKnockback kb;
    bool isDead;

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
        if (isDead)
            return;

        TryPlayHitSfx();
        health -= damage;
        if (kb != null)
            kb.Knockback(hitDirection);

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        OnEnemyDied?.Invoke(this);
        PlayDeathSfx();
        PlayDeathParticles();
        Destroy(gameObject);
    }

    private void TryPlayHitSfx()
    {
        if (hitSfx == null || audioSource == null)
            return;

        audioSource.PlayOneShot(hitSfx, hitSfxVolume);
    }

    private void PlayDeathParticles()
    {
        if (deathParticlesPrefab == null)
            return;

        ParticleSystem particles = Instantiate(
            deathParticlesPrefab,
            transform.position,
            deathParticlesPrefab.transform.rotation
        );

        particles.Play();

        var main = particles.main;
        float totalLifetime = main.duration;
        if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
            totalLifetime += main.startLifetime.constantMax;
        else
            totalLifetime += main.startLifetime.constant;

        Destroy(particles.gameObject, totalLifetime);
    }

    private void PlayDeathSfx()
    {
        if (deathSfx == null)
            return;

        GameObject deathAudioObject = new GameObject("EnemyDeathSfx");
        deathAudioObject.transform.position = transform.position;

        AudioSource deathAudioSource = deathAudioObject.AddComponent<AudioSource>();
        deathAudioSource.clip = deathSfx;
        deathAudioSource.volume = deathSfxVolume;
        deathAudioSource.spatialBlend = 0f;
        deathAudioSource.Play();

        Destroy(deathAudioObject, deathSfx.length);
    }
}