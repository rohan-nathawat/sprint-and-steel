using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float dashCooldownTimer = 0f;

    private Rigidbody2D rb;
    public InputActionReference movementAction;
    private Vector2 movement;
    public AnimationCurve dashCurve;

    private bool isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isDashingInput = Keyboard.current.leftShiftKey.isPressed;

        if (movement.sqrMagnitude > 1f) movement = movement.normalized;
        rb.linearVelocity = movement * moveSpeed;

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        if (isDashingInput && !isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(Dash(movement));
            dashCooldownTimer = dashCooldown;
        }
    }

    public void Movement(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            float dashProgress = elapsedTime / dashDuration;
            float currentDashSpeed = dashCurve.Evaluate(dashProgress) * dashSpeed;
            rb.linearVelocity = direction * currentDashSpeed;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
