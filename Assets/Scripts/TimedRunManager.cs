using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TimedRunManager : MonoBehaviour
{
    [Header("Run Settings")]
    [Min(1f)] public float runDurationSeconds = 180f;
    public bool freezeTimeOnEnd = true;
    public bool disablePlayerControlsOnEnd = true;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text killCountText;
    public TMP_Text endMessageText;
    public string timeUpMessage = "Time's Up!";
    public string playerDiedMessage = "You Died!";

    private float timeRemaining;
    private int killCount;
    private bool runEnded;

    public float TimeRemaining => timeRemaining;
    public int KillCount => killCount;
    public float RunDuration => runDurationSeconds;
    public bool RunEnded => runEnded;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyDied += HandleEnemyDied;
        PlayerHealth.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyDied -= HandleEnemyDied;
        PlayerHealth.OnPlayerDied -= HandlePlayerDied;
    }

    private void Start()
    {
        timeRemaining = runDurationSeconds;
        killCount = 0;
        runEnded = false;

        if (endMessageText != null)
            endMessageText.gameObject.SetActive(false);

        RefreshUI();
    }

    private void Update()
    {
        if (runEnded)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndRun(timeUpMessage);
            return;
        }

        RefreshTimerUI();
    }

    private void HandleEnemyDied(EnemyHealth _)
    {
        if (runEnded)
            return;

        killCount++;
        RefreshKillCountUI();
    }

    private void HandlePlayerDied(PlayerHealth _)
    {
        if (runEnded)
            return;

        EndRun(playerDiedMessage);
    }

    private void EndRun(string reasonMessage)
    {
        runEnded = true;
        RefreshUI();

        if (endMessageText != null)
        {
            endMessageText.gameObject.SetActive(true);
            endMessageText.text = reasonMessage + "\nKills: " + killCount;
        }

        if (disablePlayerControlsOnEnd)
            FreezePlayerControls();

        if (freezeTimeOnEnd)
            Time.timeScale = 0f;
    }

    private void FreezePlayerControls()
    {
        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
        if (combat != null)
        {
            combat.ForceStopCombatVisuals();
            combat.enabled = false;
        }

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = false;

        Rigidbody2D playerBody = FindFirstObjectByType<Rigidbody2D>();
        if (playerBody != null && playerBody.GetComponent<PlayerHealth>() != null)
            playerBody.linearVelocity = Vector2.zero;
    }

    private void RefreshUI()
    {
        RefreshTimerUI();
        RefreshKillCountUI();
    }

    private void RefreshTimerUI()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void RefreshKillCountUI()
    {
        if (killCountText == null)
            return;

        killCountText.text = "Kills: " + killCount;
    }
}
