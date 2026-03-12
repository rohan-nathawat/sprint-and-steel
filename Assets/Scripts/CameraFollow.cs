using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float damping;

    // ---- shake presets ----
    [Header("Camera Shake")]
    public float damageMagnitude  = 0.35f;
    public float damageDuration   = 0.25f;
    public float killMagnitude    = 0.18f;
    public float killDuration     = 0.18f;
    public float attackMagnitude  = 0.06f;
    public float attackDuration   = 0.1f;

    private static CameraFollow _instance;

    private float _shakeMag;
    private float _shakeDur;
    private float _shakeTimer;

    private Vector3 _velocity = Vector3.zero;

    void Awake()
    {
        _instance = this;
    }

    void OnEnable()
    {
        PlayerHealth.OnPlayerDamaged    += OnPlayerDamaged;
        EnemyHealth.OnEnemyDied         += OnEnemyDied;
        PlayerCombat.OnPlayerAttacked   += OnPlayerAttacked;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDamaged    -= OnPlayerDamaged;
        EnemyHealth.OnEnemyDied         -= OnEnemyDied;
        PlayerCombat.OnPlayerAttacked   -= OnPlayerAttacked;
    }

    void Update()
    {
        Vector3 movePosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, movePosition, ref _velocity, damping);

        if (_shakeTimer > 0f)
        {
            float remaining = _shakeTimer / _shakeDur; // 1 → 0 falloff
            Vector2 rand = Random.insideUnitCircle * (_shakeMag * remaining);
            transform.position += new Vector3(rand.x, rand.y, 0f);
            _shakeTimer -= Time.deltaTime;
        }
    }

    // Call from anywhere: CameraFollow.Shake(mag, dur)
    public static void Shake(float magnitude, float duration)
    {
        if (_instance == null) return;
        // Allow a stronger shake to override a weaker one; never shorten an ongoing shake
        if (magnitude >= _instance._shakeMag || duration > _instance._shakeTimer)
        {
            _instance._shakeMag   = magnitude;
            _instance._shakeDur   = duration;
            _instance._shakeTimer = duration;
        }
    }

    // ---- event handlers ----
    private void OnPlayerDamaged()               => Shake(damageMagnitude, damageDuration);
    private void OnEnemyDied(EnemyHealth _)      => Shake(killMagnitude,   killDuration);
    private void OnPlayerAttacked()              => Shake(attackMagnitude, attackDuration);
}
