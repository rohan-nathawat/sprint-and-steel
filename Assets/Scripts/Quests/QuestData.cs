using UnityEngine;

// ─────────────────────────────────────────────────────────────
//  QuestData.cs
//  Create quest assets via:
//  Right-click in Project > Create > Sprint & Steel > Quest
// ─────────────────────────────────────────────────────────────

public enum QuestDifficulty { Easy, Medium, Hard }
public enum QuestStatus     { Locked, Unlocked }
public enum QuestObjectiveType { NoObjective, DefeatEnemies }

[CreateAssetMenu(menuName = "Sprint & Steel/Quest", fileName = "Quest_New")]
public class QuestData : ScriptableObject
{
    [Header("Info")]
    public string        questName    = "New Quest";
    public string        location     = "Unknown";
    public QuestDifficulty difficulty = QuestDifficulty.Easy;

    [Header("State")]
    public QuestStatus   status       = QuestStatus.Unlocked;

    [Header("Scene")]
    [Tooltip("Exact name of the Unity scene to load when quest is accepted.")]
    public string        sceneName    = "";

    [Header("Objective")]
    public QuestObjectiveType objectiveType = QuestObjectiveType.DefeatEnemies;
    [Min(1)] public int objectiveTarget = 8;

    [Header("Rewards")]
    [Min(0)] public int rewardCurrency = 100;

    [Header("Return")]
    [Tooltip("Scene loaded after quest completion.")]
    public string hubSceneName = "Hub";

    [Header("Description (optional)")]
    [TextArea(2,4)]
    public string        description  = "";
}
