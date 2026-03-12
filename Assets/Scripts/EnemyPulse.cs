using UnityEngine;

/// <summary>
/// Makes an enemy's sprite pulsate between white and blue.
/// Attach to the same GameObject as EnemyFollow.
/// </summary>
public class EnemyPulse : MonoBehaviour
{
    [Header("Pulse Colors")]
    public Color colorA = Color.white;
    public Color colorB = new Color(0.2f, 0.5f, 1f, 1f); // bright blue

    [Header("Pulse Speed")]
    [Tooltip("Full white-to-blue-to-white cycles per second.")]
    public float frequency = 1.4f;

    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // PingPong gives a smooth back-and-forth 0→1→0 wave
        float t = Mathf.PingPong(Time.time * frequency, 1f);
        _sr.color = Color.Lerp(colorA, colorB, t);
    }
}
