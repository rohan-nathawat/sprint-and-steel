using System.Collections;
using UnityEngine;

// ─────────────────────────────────────────────────────────────
//  DiagonalGrid.cs
//  Attach to an empty GameObject in your scene.
//  Generates a diagonal pulsating grid using simple sprites.
//  No custom shader required.
// ─────────────────────────────────────────────────────────────
public class DiagonalGrid : MonoBehaviour
{
    [Header("Grid Size")]
    public int   columns       = 30;      // how many cells across
    public int   rows          = 20;      // how many cells tall
    public float cellSize      = 1f;      // world units per cell

    [Header("Visuals")]
    public Color baseColor     = new Color(0.05f, 0.12f, 0.09f, 1f);
    public Color lineColor     = new Color(0.15f, 0.30f, 0.22f, 1f);
    [Range(0.01f, 0.3f)]
    public float lineThickness = 0.05f;

    [Header("Pulse")]
    [Range(0f, 1f)]
    public float pulseStrength = 0.5f;
    public float pulseSpeed    = 1.2f;
    public float waveFrequency = 0.5f;
    [Min(1f)] public float pulseUpdatesPerSecond = 20f;

    [Header("Sorting")]
    public string sortingLayer = "Default";
    public int    orderInLayer = -10;

    // ── Internal ─────────────────────────────────────────────
    SpriteRenderer[,] _hLines;   // horizontal lines of grid
    SpriteRenderer[,] _vLines;   // vertical lines of grid
    Sprite            _whiteSprite;
    Vector2[,]        _hPulsePositions;
    Vector2[,]        _vPulsePositions;
    float             _nextPulseUpdateTime;

    void Start()
    {
        _whiteSprite = CreateWhiteSprite();
        BuildGrid();
    }

    void Update()
    {
        if (pulseStrength <= 0f)
            return;

        float interval = 1f / Mathf.Max(1f, pulseUpdatesPerSecond);
        if (Time.time < _nextPulseUpdateTime)
            return;

        _nextPulseUpdateTime = Time.time + interval;
        PulseGrid();
    }

    // ─────────────────────────────────────────────────────────
    //  Build
    // ─────────────────────────────────────────────────────────

    void BuildGrid()
    {
        _hLines = new SpriteRenderer[columns, rows + 1];
        _vLines = new SpriteRenderer[columns + 1, rows];
        _hPulsePositions = new Vector2[columns, rows + 1];
        _vPulsePositions = new Vector2[columns + 1, rows];

        float totalW = columns * cellSize;
        float totalH = rows    * cellSize;
        float startX = transform.position.x - totalW * 0.5f;
        float startY = transform.position.y - totalH * 0.5f;

        // Horizontal lines
        for (int x = 0; x < columns; x++)
        for (int y = 0; y <= rows;   y++)
        {
            float wx = startX + x * cellSize + cellSize * 0.5f;
            float wy = startY + y * cellSize;
            _hLines[x, y] = MakeLine(
                new Vector3(wx, wy, 0),
                new Vector3(cellSize, lineThickness, 1),
                0f
            );
            _hPulsePositions[x, y] = new Vector2(wx, wy);
        }

        // Vertical lines
        for (int x = 0; x <= columns; x++)
        for (int y = 0; y < rows;     y++)
        {
            float wx = startX + x * cellSize;
            float wy = startY + y * cellSize + cellSize * 0.5f;
            _vLines[x, y] = MakeLine(
                new Vector3(wx, wy, 0),
                new Vector3(lineThickness, cellSize, 1),
                0f
            );
            _vPulsePositions[x, y] = new Vector2(wx, wy);
        }

        // Rotate everything 45 degrees around the centre
        foreach (Transform child in transform)
            child.RotateAround(transform.position, Vector3.forward, 45f);
    }

    SpriteRenderer MakeLine(Vector3 pos, Vector3 scale, float rot)
    {
        var go  = new GameObject("GridLine");
        go.transform.SetParent(transform);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        go.transform.localRotation = Quaternion.Euler(0, 0, rot);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = _whiteSprite;
        sr.color        = lineColor;
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder     = orderInLayer;
        return sr;
    }

    // ─────────────────────────────────────────────────────────
    //  Pulse
    // ─────────────────────────────────────────────────────────

    void PulseGrid()
    {
        if (_hLines == null || _vLines == null)
            return;

        for (int x = 0; x < columns; x++)
        for (int y = 0; y <= rows;   y++)
        {
            if (_hLines[x, y] == null) continue;
            float wx  = _hPulsePositions[x, y].x;
            float wy  = _hPulsePositions[x, y].y;
            float brightness = GetPulse(wx, wy);
            _hLines[x, y].color = Color.Lerp(lineColor, Color.white, brightness * pulseStrength);
        }

        for (int x = 0; x <= columns; x++)
        for (int y = 0; y < rows;     y++)
        {
            if (_vLines[x, y] == null) continue;
            float wx  = _vPulsePositions[x, y].x;
            float wy  = _vPulsePositions[x, y].y;
            float brightness = GetPulse(wx, wy);
            _vLines[x, y].color = Color.Lerp(lineColor, Color.white, brightness * pulseStrength);
        }
    }

    float GetPulse(float wx, float wy)
    {
        float diag = (wx + wy) * waveFrequency;
        return Mathf.Sin(diag - Time.time * pulseSpeed) * 0.5f + 0.5f;
    }

    // ─────────────────────────────────────────────────────────
    //  White sprite (generated in code — no texture needed)
    // ─────────────────────────────────────────────────────────

    Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }
}
