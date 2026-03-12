using UnityEngine;

/// <summary>
/// Spawns sprite afterimages behind the player to create a hot-pink → magenta → transparent
/// boxy trail. Uses a pre-allocated object pool — zero GC allocations after Awake.
/// Attach to the same GameObject as PlayerMovement.
/// </summary>
public class PlayerTrail : MonoBehaviour
{
    [Header("Trail Timing")]
    [Tooltip("How often a ghost image is spawned (seconds).")]
    public float spawnInterval = 0.04f;

    [Tooltip("How long each ghost lives before fully fading out (seconds). 0.15–0.25 recommended.")]
    public float trailLifetime = 0.2f;

    [Header("Trail Shape")]
    [Tooltip("Multiplier applied to the ghost's scale at the end of its life. Keeps it boxy while tapering.")]
    [Range(0.1f, 1f)]
    public float endScaleMultiplier = 0.35f;

    [Header("Trail Colors")]
    [Tooltip("Color at spawn — hot pink.")]
    public Color colorStart = new Color(1f, 0.08f, 0.58f, 0.9f);

    [Tooltip("Color at mid-life — magenta.")]
    public Color colorMid = new Color(0.85f, 0f, 1f, 0.5f);

    [Tooltip("Color at end of life — fully transparent magenta.")]
    public Color colorEnd = new Color(0.85f, 0f, 1f, 0f);

    // --- pool internals ---
    private struct GhostState
    {
        public float elapsed;
        public Vector3 startScale;
        public bool active;
    }

    // Pool size: enough to cover trailLifetime / spawnInterval with headroom
    private const int PoolSize = 16;

    private SpriteRenderer[] _poolSR;
    private Transform[]      _poolTR;
    private GhostState[]     _states;

    private SpriteRenderer _playerSR;
    private Rigidbody2D    _rb;
    private float          _spawnTimer;
    private Vector3        _baseScale;

    void Awake()
    {
        _playerSR  = GetComponent<SpriteRenderer>();
        _rb        = GetComponent<Rigidbody2D>();
        _baseScale = transform.localScale;

        // Pre-allocate the pool
        _poolSR = new SpriteRenderer[PoolSize];
        _poolTR = new Transform[PoolSize];
        _states = new GhostState[PoolSize];

        for (int i = 0; i < PoolSize; i++)
        {
            var go = new GameObject("TrailGhost");
            go.SetActive(false);
            _poolTR[i] = go.transform;
            _poolSR[i] = go.AddComponent<SpriteRenderer>();
            _states[i]  = default;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Tick all active ghosts — no coroutines, no allocations
        for (int i = 0; i < PoolSize; i++)
        {
            if (!_states[i].active) continue;

            _states[i].elapsed += dt;
            float elapsed = _states[i].elapsed;

            if (elapsed >= trailLifetime)
            {
                _states[i].active = false;
                _poolTR[i].gameObject.SetActive(false);
                continue;
            }

            float t = elapsed / trailLifetime;

            Color c = t < 0.5f
                ? Color.Lerp(colorStart, colorMid, t * 2f)
                : Color.Lerp(colorMid, colorEnd, (t - 0.5f) * 2f);

            _poolSR[i].color = c;
            _poolTR[i].localScale = Vector3.Lerp(_states[i].startScale, _states[i].startScale * endScaleMultiplier, t);
        }

        // Spawn new ghost on interval while moving
        _spawnTimer -= dt;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;
            if (_rb.linearVelocity.sqrMagnitude > 0.01f)
                ActivateGhost();
        }
    }

    void ActivateGhost()
    {
        if (_playerSR == null || _playerSR.sprite == null) return;

        // Find the first inactive slot
        for (int i = 0; i < PoolSize; i++)
        {
            if (_states[i].active) continue;

            Transform tr = _poolTR[i];
            SpriteRenderer sr = _poolSR[i];

            tr.SetPositionAndRotation(transform.position, transform.rotation);
            Vector3 scale = transform.localScale;
            tr.localScale = scale;

            sr.sprite          = _playerSR.sprite;
            sr.flipX           = _playerSR.flipX;
            sr.flipY           = _playerSR.flipY;
            sr.sortingLayerID  = _playerSR.sortingLayerID;
            sr.sortingOrder    = _playerSR.sortingOrder - 1;
            sr.color           = colorStart;

            _states[i].elapsed    = 0f;
            _states[i].startScale = scale;
            _states[i].active     = true;

            tr.gameObject.SetActive(true);
            return;
        }
        // Pool exhausted — silently skip this frame (raise PoolSize if needed)
    }
}
