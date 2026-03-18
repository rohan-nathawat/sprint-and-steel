using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  QuestRowUI.cs
//  Placed on each quest row prefab.
//  QuestBoardUI calls Setup() and SetSelected() on it.
// ─────────────────────────────────────────────────────────────
public class QuestRowUI : MonoBehaviour
{
    [Header("References")]
    public Image           rowBackground;
    public Image           statusDot;        // coloured square left of name (hidden when locked)
    public GameObject      lockIcon;         // shown when locked
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI locationText;
    public Image[]         difficultyPips;   // array of 3 pip images
    public TextMeshProUGUI statusText;

    // ── Colours ──────────────────────────────────────────────
    static readonly Color ColTeal    = new Color(0.361f, 0.878f, 0.753f);
    static readonly Color ColAmber   = new Color(0.878f, 0.722f, 0.361f);
    static readonly Color ColPink    = new Color(0.878f, 0.361f, 0.478f);
    static readonly Color ColEasy    = new Color(0.361f, 0.878f, 0.753f);
    static readonly Color ColMedium  = new Color(0.878f, 0.722f, 0.361f);
    static readonly Color ColHard    = new Color(0.878f, 0.361f, 0.478f);
    static readonly Color ColDimPip  = new Color(1f, 1f, 1f, 0.12f);
    static readonly Color ColDimText = new Color(1f, 1f, 1f, 0.28f);
    static readonly Color ColRowNorm = new Color(1f, 1f, 1f, 0.03f);
    static readonly Color ColRowSel  = new Color(0.361f, 0.878f, 0.753f, 0.07f);
    static readonly Color ColRowLock = new Color(0f, 0f, 0f, 0.20f);
    static readonly Color ColBorderNorm = new Color(1f, 1f, 1f, 0.06f);
    static readonly Color ColBorderSel  = new Color(0.361f, 0.878f, 0.753f, 0.30f);
    static readonly Color ColBorderLock = new Color(1f, 1f, 1f, 0.03f);

    QuestData _data;

    // ─────────────────────────────────────────────────────────

    public void Setup(QuestData data)
    {
        _data = data;
        bool locked = data.status == QuestStatus.Locked;

        // Lock icon / status dot
        if (lockIcon)  lockIcon.SetActive(locked);
        if (statusDot) statusDot.gameObject.SetActive(!locked);

        // Dot colour
        if (!locked && statusDot)
        {
            statusDot.color = data.difficulty == QuestDifficulty.Hard ? ColPink
                            : ColTeal;
        }

        // Quest name
        if (questNameText)
        {
            questNameText.text  = data.questName;
            questNameText.color = locked ? ColDimText : new Color(1f, 1f, 1f, 0.82f);
        }

        // Location
        if (locationText)
            locationText.color = locked ? new Color(1f,1f,1f,0.18f) : new Color(1f,1f,1f,0.4f);
        if (locationText) locationText.text = data.location;

        // Difficulty pips
        int pipCount = data.difficulty == QuestDifficulty.Easy   ? 1
                     : data.difficulty == QuestDifficulty.Medium  ? 2
                     : 3;
        Color pipCol = data.difficulty == QuestDifficulty.Easy   ? ColEasy
                     : data.difficulty == QuestDifficulty.Medium  ? ColMedium
                     : ColHard;

        for (int i = 0; i < difficultyPips.Length; i++)
        {
            if (difficultyPips[i] == null) continue;
            bool active = i < pipCount;
            difficultyPips[i].color = locked ? ColDimPip
                                    : active ? pipCol
                                    : new Color(pipCol.r, pipCol.g, pipCol.b, 0.15f);
        }

        // Status label
        if (statusText)
        {
            statusText.text  = locked ? "Locked" : "Unlocked";
            statusText.color = locked ? new Color(1f,1f,1f,0.2f) : ColTeal;
        }

        // Row opacity
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg) cg.alpha = locked ? 0.55f : 1f;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (_data == null) return;
        bool locked = _data.status == QuestStatus.Locked;

        // Background
        if (rowBackground)
            rowBackground.color = locked ? ColRowLock
                                : selected ? ColRowSel
                                : ColRowNorm;

        // Border via Outline component
        Outline outline = GetComponent<Outline>();
        if (outline)
            outline.effectColor = locked ? ColBorderLock
                                : selected ? ColBorderSel
                                : ColBorderNorm;
    }
}
