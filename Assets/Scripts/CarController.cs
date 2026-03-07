using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference movementAction;
    [SerializeField] private InputActionReference handbrakeAction;
    [SerializeField] private InputActionReference nitroAction;
    [SerializeField] private bool allowKeyboardFallback = true;
    [SerializeField] private float inputDeadZone = 0.08f;
    [SerializeField] private float throttleResponse = 9f;
    [SerializeField] private float steeringResponse = 15f;

    [Header("Engine")]
    [SerializeField] private float engineForce = 46f;
    [SerializeField] private float reverseEngineForce = 24f;
    [SerializeField] private float maxForwardSpeed = 23f;
    [SerializeField] private float maxReverseSpeed = 9f;
    [SerializeField, Range(0.1f, 1f)] private float topSpeedEngineFactor = 0.25f;

    [Header("Braking")]
    [SerializeField] private float brakeForce = 90f;
    [SerializeField] private float reverseBrakeForce = 75f;
    [SerializeField] private float coastingBrake = 11f;
    [SerializeField] private float handbrakeLongitudinalBrake = 42f;

    [Header("Steering")]
    [SerializeField] private float steerStrength = 235f;
    [SerializeField] private float lowSpeedSteer = 1f;
    [SerializeField] private float highSpeedSteer = 0.33f;
    [SerializeField] private float speedForHighSteerReduction = 22f;
    [SerializeField] private float minimumSteerAtLowSpeed = 0.28f;
    [SerializeField] private float reverseSteerMultiplier = -0.75f;

    [Header("Grip & Drift")]
    [SerializeField, Range(0f, 1f)] private float baseLateralGrip = 0.88f;
    [SerializeField, Range(0f, 1f)] private float driftLateralGrip = 0.4f;
    [SerializeField, Range(0f, 1f)] private float handbrakeLateralGrip = 0.14f;
    [SerializeField] private float minSpeedForDrift = 4f;
    [SerializeField] private float driftBuildRate = 4.5f;
    [SerializeField] private float driftReleaseRate = 2.5f;
    [SerializeField] private float driftSteerInfluence = 0.95f;
    [SerializeField] private float handbrakeYawTorque = 200f;
    [SerializeField] private float handbrakeEntryKick = 1.5f;
    [SerializeField] private float handbrakeTurnMultiplier = 1.2f;
    [SerializeField] private float tractionRecoverStrength = 2.3f;

    [Header("Aero / Damping")]
    [SerializeField] private float normalLinearDamping = 0.7f;
    [SerializeField] private float handbrakeLinearDamping = 0.35f;
    [SerializeField] private float angularDamping = 8f;
    [SerializeField] private float downforce = 1.8f;

    [Header("Realistic Direction Changes")]
    [SerializeField] private float gearShiftSpeedThreshold = 0.8f;
    [SerializeField] private float driveToReverseDelay = 0.22f;
    [SerializeField] private float reverseToDriveDelay = 0.14f;

    [Header("Nitrous")]
    [SerializeField] private float nitroCapacity = 5f;
    [SerializeField] private float nitroDrainPerSecond = 1.15f;
    [SerializeField] private float nitroRechargePerSecond = 0.8f;
    [SerializeField] private float nitroExtraForce = 40f;
    [SerializeField] private float nitroTopSpeedBonus = 8f;
    [SerializeField] private float nitroSteerPenalty = 0.88f;
    [SerializeField] private float nitroMinThrottle = 0.2f;

    private enum GearState
    {
        Drive,
        Reverse
    }

    private Vector2 moveInput;
    private float throttleInput;
    private float steeringInput;
    private float throttleSmoothed;
    private float steeringSmoothed;

    private bool handbrakeInput;
    private bool handbrakePressedThisFrame;
    private bool nitroInput;
    private bool nitroActive;

    private GearState gear = GearState.Drive;
    private float reverseRequestTimer;
    private float driveRequestTimer;
    private float nitroRemaining;
    private float driftAmount;

    public float NitroNormalized => nitroCapacity <= 0.01f ? 0f : nitroRemaining / nitroCapacity;
    public bool IsNitroActive => nitroActive;
    public float DriftAmount => driftAmount;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        nitroRemaining = nitroCapacity;
    }

    private void OnEnable()
    {
        EnableAction(movementAction);
        EnableAction(handbrakeAction);
        EnableAction(nitroAction);
    }

    private void OnDisable()
    {
        DisableAction(movementAction);
        DisableAction(handbrakeAction);
        DisableAction(nitroAction);
    }

    private void Update()
    {
        ReadInput();
        SmoothInput();
        UpdateGearState();
        UpdateNitroState();
    }

    private void FixedUpdate()
    {
        ApplyEngineAndBraking();
        ApplySteering();
        ApplyDriftAndGrip();
        ApplyDampingAndAero();
        ClampTopSpeed();
    }

    private void ReadInput()
    {
        if (movementAction != null && movementAction.action != null)
            moveInput = movementAction.action.ReadValue<Vector2>();

        throttleInput = Mathf.Abs(moveInput.y) < inputDeadZone ? 0f : Mathf.Clamp(moveInput.y, -1f, 1f);
        steeringInput = Mathf.Abs(moveInput.x) < inputDeadZone ? 0f : Mathf.Clamp(moveInput.x, -1f, 1f);

        bool previousHandbrake = handbrakeInput;
        handbrakeInput = IsPressed(handbrakeAction);
        nitroInput = IsPressed(nitroAction);

        if (allowKeyboardFallback && Keyboard.current != null)
        {
            handbrakeInput |= Keyboard.current.spaceKey.isPressed;
            nitroInput |= Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        }

        handbrakePressedThisFrame = handbrakeInput && !previousHandbrake;
    }

    private void SmoothInput()
    {
        throttleSmoothed = Mathf.MoveTowards(throttleSmoothed, throttleInput, throttleResponse * Time.deltaTime);
        steeringSmoothed = Mathf.MoveTowards(steeringSmoothed, steeringInput, steeringResponse * Time.deltaTime);
    }

    private void UpdateGearState()
    {
        float forwardSpeed = GetForwardSpeed();
        bool wantsReverse = throttleInput < -0.1f;
        bool wantsDrive = throttleInput > 0.1f;

        if (wantsReverse)
        {
            reverseRequestTimer = forwardSpeed <= gearShiftSpeedThreshold ? reverseRequestTimer + Time.deltaTime : 0f;
            driveRequestTimer = 0f;

            if (reverseRequestTimer >= driveToReverseDelay)
                gear = GearState.Reverse;
        }
        else if (wantsDrive)
        {
            driveRequestTimer = forwardSpeed >= -gearShiftSpeedThreshold ? driveRequestTimer + Time.deltaTime : 0f;
            reverseRequestTimer = 0f;

            if (driveRequestTimer >= reverseToDriveDelay)
                gear = GearState.Drive;
        }
        else
        {
            reverseRequestTimer = 0f;
            driveRequestTimer = 0f;
        }
    }

    private void UpdateNitroState()
    {
        bool canUseNitro = nitroRemaining > 0f && throttleSmoothed > nitroMinThrottle && GetForwardSpeed() > 1f;
        nitroActive = nitroInput && canUseNitro;

        if (nitroActive)
            nitroRemaining = Mathf.Max(0f, nitroRemaining - nitroDrainPerSecond * Time.deltaTime);
        else
            nitroRemaining = Mathf.Min(nitroCapacity, nitroRemaining + nitroRechargePerSecond * Time.deltaTime);
    }

    private void ApplyEngineAndBraking()
    {
        Vector2 forward = transform.up;
        float forwardSpeed = GetForwardSpeed();
        float absForwardSpeed = Mathf.Abs(forwardSpeed);

        if (gear == GearState.Drive)
        {
            if (throttleSmoothed > 0.01f)
            {
                float forceScale = GetEngineScale(absForwardSpeed, maxForwardSpeed);
                float nitroForce = nitroActive ? nitroExtraForce : 0f;
                rb.AddForce(forward * throttleSmoothed * (engineForce * forceScale + nitroForce), ForceMode2D.Force);
            }
            else if (throttleSmoothed < -0.01f)
            {
                ApplyLongitudinalBrake(brakeForce);
            }
        }
        else
        {
            if (throttleSmoothed < -0.01f)
            {
                float forceScale = GetEngineScale(absForwardSpeed, maxReverseSpeed);
                rb.AddForce(-forward * -throttleSmoothed * reverseEngineForce * forceScale, ForceMode2D.Force);
            }
            else if (throttleSmoothed > 0.01f)
            {
                ApplyLongitudinalBrake(reverseBrakeForce);
            }
        }

        if (Mathf.Abs(throttleSmoothed) < 0.01f && Mathf.Abs(forwardSpeed) > 0.03f)
            ApplyLongitudinalBrake(coastingBrake);

        if (handbrakeInput)
            ApplyLongitudinalBrake(handbrakeLongitudinalBrake);
    }

    private void ApplySteering()
    {
        if (Mathf.Abs(steeringSmoothed) < 0.001f)
            return;

        float speed = rb.linearVelocity.magnitude;
        float speed01 = Mathf.Clamp01(speed / Mathf.Max(speedForHighSteerReduction, 0.01f));
        float steerBySpeed = Mathf.Lerp(lowSpeedSteer, highSpeedSteer, speed01);
        steerBySpeed = Mathf.Max(steerBySpeed, minimumSteerAtLowSpeed);

        float directionMultiplier = GetForwardSpeed() >= 0f ? 1f : reverseSteerMultiplier;
        float driftTurnBonus = handbrakeInput ? handbrakeTurnMultiplier : 1f;
        float nitroSteerMultiplier = nitroActive ? nitroSteerPenalty : 1f;

        float turnRate = -steeringSmoothed * steerStrength * steerBySpeed * directionMultiplier * driftTurnBonus * nitroSteerMultiplier;
        rb.MoveRotation(rb.rotation + turnRate * Time.fixedDeltaTime);
    }

    private void ApplyDriftAndGrip()
    {
        Vector2 forward = transform.up;
        Vector2 right = transform.right;

        float speed = rb.linearVelocity.magnitude;
        float speed01 = Mathf.Clamp01(speed / Mathf.Max(minSpeedForDrift, 0.01f));
        float steerSlip = Mathf.Abs(steeringSmoothed) * driftSteerInfluence * speed01;
        float throttleSlip = Mathf.Max(0f, throttleSmoothed) * 0.35f * speed01;
        float desiredDrift = Mathf.Clamp01(steerSlip + throttleSlip + (handbrakeInput ? 1f : 0f));

        float driftRate = desiredDrift > driftAmount ? driftBuildRate : driftReleaseRate;
        driftAmount = Mathf.MoveTowards(driftAmount, desiredDrift, driftRate * Time.fixedDeltaTime);

        float sideSpeed = Vector2.Dot(rb.linearVelocity, right);
        float targetGrip = handbrakeInput
            ? handbrakeLateralGrip
            : Mathf.Lerp(baseLateralGrip, driftLateralGrip, driftAmount);

        float correctedSideSpeed = sideSpeed * targetGrip;
        float forwardSpeed = Vector2.Dot(rb.linearVelocity, forward);
        Vector2 targetVelocity = (forward * forwardSpeed) + (right * correctedSideSpeed);

        float gripLerp = Mathf.Lerp(14f, 4f, driftAmount);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, gripLerp * Time.fixedDeltaTime);

        float recoverStrength = Mathf.Lerp(tractionRecoverStrength, 0.2f, driftAmount);
        Vector2 recoveredVelocity = (forward * Vector2.Dot(rb.linearVelocity, forward)) + (right * correctedSideSpeed);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, recoveredVelocity, recoverStrength * Time.fixedDeltaTime);

        if (handbrakePressedThisFrame && speed > minSpeedForDrift)
        {
            Vector2 kickDir = -Mathf.Sign(steeringSmoothed == 0f ? 1f : steeringSmoothed) * right;
            rb.AddForce(kickDir * handbrakeEntryKick, ForceMode2D.Impulse);
        }

        if (handbrakeInput && speed > minSpeedForDrift)
            rb.AddTorque(-steeringSmoothed * handbrakeYawTorque, ForceMode2D.Force);
    }

    private void ApplyDampingAndAero()
    {
        rb.linearDamping = handbrakeInput ? handbrakeLinearDamping : normalLinearDamping;
        rb.angularDamping = angularDamping;

        float speed = rb.linearVelocity.magnitude;
        rb.AddForce(-transform.up * speed * downforce, ForceMode2D.Force);
    }

    private void ClampTopSpeed()
    {
        Vector2 forward = transform.up;
        Vector2 right = transform.right;

        float forwardSpeed = Vector2.Dot(rb.linearVelocity, forward);
        float sideSpeed = Vector2.Dot(rb.linearVelocity, right);
        float currentForwardCap = maxForwardSpeed + (nitroActive ? nitroTopSpeedBonus : 0f);
        float clampedForward = Mathf.Clamp(forwardSpeed, -maxReverseSpeed, currentForwardCap);

        rb.linearVelocity = (forward * clampedForward) + (right * sideSpeed);
    }

    private void ApplyLongitudinalBrake(float brakeAmount)
    {
        float forwardSpeed = GetForwardSpeed();
        if (Mathf.Abs(forwardSpeed) < 0.01f)
            return;

        Vector2 forward = transform.up;
        float brakeDir = -Mathf.Sign(forwardSpeed);
        rb.AddForce(forward * brakeDir * brakeAmount, ForceMode2D.Force);
    }

    private float GetForwardSpeed()
    {
        return Vector2.Dot(rb.linearVelocity, transform.up);
    }

    private float GetEngineScale(float absSpeed, float maxSpeed)
    {
        if (maxSpeed <= 0.01f)
            return 1f;

        float t = Mathf.Clamp01(absSpeed / maxSpeed);
        return Mathf.Lerp(1f, topSpeedEngineFactor, t * t);
    }

    private bool IsPressed(InputActionReference actionRef)
    {
        return actionRef != null && actionRef.action != null && actionRef.action.IsPressed();
    }

    private void EnableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
            actionRef.action.Enable();
    }

    private void DisableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
            actionRef.action.Disable();
    }

    // Optional callback hook if you use PlayerInput "Send Messages"/"Unity Events".
    public void Movement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
