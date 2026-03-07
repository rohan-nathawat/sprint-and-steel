using System.Collections;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    public Rigidbody2D rb;

    public float knockbackSpeed = 12f;
    public float knockbackDuration = 0.2f;
    public AnimationCurve knockbackCurve;
    public bool isKnockbackActive = false;

    Coroutine knockbackRoutine;

    public void Knockback(Vector2 direction)
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction));
    }

    IEnumerator KnockbackRoutine(Vector2 direction)
    {
        float elapsedTime = 0f;
        while (elapsedTime < knockbackDuration)
        {
            isKnockbackActive = true;
            float knockbackProgress = elapsedTime / knockbackDuration;
            float currentKnockbackSpeed = knockbackCurve.Evaluate(knockbackProgress) * knockbackSpeed;
            rb.linearVelocity = direction * currentKnockbackSpeed;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isKnockbackActive = false;
    }
}