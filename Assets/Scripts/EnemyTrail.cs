using UnityEngine;

/// <summary>
/// Spawns sprite afterimages behind an enemy — white → blue → transparent.
/// Uses a pre-allocated object pool (zero GC after Awake).
/// Attach to the same GameObject as EnemyFollow.
/// </summary>
public class EnemyTrail : MonoBehaviour
{
    [Header("Trail Timing")]
    public float spawnInterval = 0.05f;
    public float trailLifetime  = 0.15f;

    [Header("Trail Shape")]
    [Range(0.1f, 1f)]
    public float endScaleMultiplier = 0.4f;

    [Header("Trail Colors")]
    public Color colorStart = new Color(1f,   1f,   1f,   0.85f); // white
    public Color colorMid   = new Color(0.2f, 0.5f, 1f,   0.45f); // bright blue
    public Color colorEnd   = new Color(0.1f, 0.3f, 1f,   0f);    // transparent blue

    // ---- pool internals ----
    private struct GhostState
    {
        public float   elapsed;
        public Vector3 startScale;
        public bool    active;
    }

    private const int PoolSize = 12;

    private SpriteRenderer[] _poolSR;
    private Transform[]      _poolTR;
    private GhostState[]     _states;

    private SpriteRenderer _enemySR;
    private Rigidbody2D    _rb;
    private float          _spawnTimer;

    void Awake()
    {
        _enemySR = GetComponent<SpriteRenderer>();
        _rb      = GetComponent<Rigidbody2D>();

        _poolSR = new SpriteRenderer[PoolSize];
        _poolTR = new Transform[PoolSize];
        _states = new GhostState[PoolSize];

        for (int i = 0; i < PoolSize; i++)
        {
            var go = new GameObject("EnemyTrailGhost");
            go.SetActive(false);
            _poolTR[i] = go.transform;
            _poolSR[i] = go.AddComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

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
                : Color.Lerp(colorMid,   colorEnd,  (t - 0.5f) * 2f);

            _poolSR[i].color        = c;
            _poolTR[i].localScale   = Vector3.Lerp(_states[i].startScale, _states[i].startScale * endScaleMultiplier, t);
        }

        _spawnTimer -= dt;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;
            if (_rb.linearVelocity.sqrMagnitude > 0.01f)
                ActivateGhost();
        }
    }

    void OnDestroy()
    {
        // Enemy was killed — destroy all pooled ghost objects so nothing lingers
        for (int i = 0; i < PoolSize; i++)
        {
            if (_poolTR[i] != null)
                Destroy(_poolTR[i].gameObject);
        }
    }

    void ActivateGhost()
    {
        if (_enemySR == null || _enemySR.sprite == null) return;

        for (int i = 0; i < PoolSize; i++)
        {
            if (_states[i].active) continue;

            Transform tr = _poolTR[i];
            SpriteRenderer sr = _poolSR[i];

            tr.SetPositionAndRotation(transform.position, transform.rotation);
            Vector3 scale = transform.localScale;
            tr.localScale = scale;

            sr.sprite         = _enemySR.sprite;
            sr.flipX          = _enemySR.flipX;
            sr.flipY          = _enemySR.flipY;
            sr.sortingLayerID = _enemySR.sortingLayerID;
            sr.sortingOrder   = _enemySR.sortingOrder - 1;
            sr.color          = colorStart;

            _states[i].elapsed    = 0f;
            _states[i].startScale = scale;
            _states[i].active     = true;

            tr.gameObject.SetActive(true);
            return;
        }
    }
}
