using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────
//  DialogueManager.cs
//  Attach to the DialogueCanvas GameObject.
//  Call Show() from your NPC script to trigger dialogue.
// ─────────────────────────────────────────────────────────────
public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public GameObject dialogueBox;
    public Image      portraitInner;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueBodyText;

    [Header("Typewriter")]
    public float typewriterSpeed = 0.03f;   // seconds per character

    // ── Colours ─────────────────────────────────────────────
    static readonly Color ColTeal    = new Color(0.361f, 0.878f, 0.753f); // #5CE0C0
    static readonly Color ColTextOn  = new Color(1f, 1f, 1f, 0.82f);

    bool   _isTyping   = false;
    bool   _skipTyping = false;
    string _fullText   = "";
    Coroutine _typeRoutine;

    /// <summary>True while the typewriter coroutine is still running.</summary>
    public bool IsTyping => _isTyping;
    public bool IsOpen => dialogueBox != null && dialogueBox.activeSelf;

    // ─────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Show a single line of dialogue.
    /// Call with: DialogueManager.Instance.Show("Merchant", Color.cyan, "Your text here");
    /// </summary>
    public void Show(string npcName, Color portraitColor, string text)
    {
        dialogueBox.SetActive(true);

        npcNameText.text    = npcName.ToUpper();
        if (portraitInner != null) portraitInner.color = portraitColor;
        _fullText           = text;

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(Typewrite(text));
    }

    /// <summary>Hide the dialogue box.</summary>
    public void Hide()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        dialogueBox.SetActive(false);
        _isTyping = false;
    }

    // ─────────────────────────────────────────────────────────
    //  Input — call this from your Player input handler
    //  (bound to E / Space / interact button)
    // ─────────────────────────────────────────────────────────
    public void OnContinuePressed()
    {
        if (_isTyping)
        {
            // Skip to end of current line instantly
            _skipTyping = true;
        }
        else
        {
            Hide();
        }
    }

    // ─────────────────────────────────────────────────────────
    //  Typewriter coroutine
    // ─────────────────────────────────────────────────────────
    IEnumerator Typewrite(string text)
    {
        _isTyping   = true;
        _skipTyping = false;
        dialogueBodyText.text = "";

        foreach (char c in text)
        {
            if (_skipTyping) break;
            dialogueBodyText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // Show full text + blinking cursor tag
        dialogueBodyText.text = text + "<color=#00000000>|</color>";
        _isTyping   = false;
        _skipTyping = false;
    }

    // ─────────────────────────────────────────────────────────
    //  Singleton (optional but convenient)
    // ─────────────────────────────────────────────────────────
    public static DialogueManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        dialogueBox.SetActive(false);
    }
}
