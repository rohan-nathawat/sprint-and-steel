using UnityEngine;
using UnityEngine.InputSystem;

// ─────────────────────────────────────────────────────────────
//  NPC.cs
//  Attach to any NPC GameObject.
//  Player triggers dialogue by pressing E while in range.
// ─────────────────────────────────────────────────────────────
public class NPC : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName       = "Merchant";
    public Color  portraitColor = new Color(0.361f, 0.878f, 0.753f); // teal

    [TextArea(2, 6)]
    public string[] dialogueLines = {
        "The enemies are getting faster. Upgrade your sprint before the next room.",
        "Come back when you have more kills. I might have something for you."
    };

    int  _lineIndex    = 0;
    bool _playerInRange = false;

    void Update()
    {
        if (!_playerInRange) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            Interact();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    void Interact()
    {
        var dm = DialogueManager.Instance;

        // If the typewriter is still running, skip to end of the current line
        if (dm.IsTyping) { dm.OnContinuePressed(); return; }

        if (_lineIndex < dialogueLines.Length)
        {
            dm.Show(npcName, portraitColor, dialogueLines[_lineIndex]);
            _lineIndex++;
        }
        else
        {
            Close();
        }
    }

    void Close()
    {
        DialogueManager.Instance.Hide();
        _lineIndex = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            Close();
        }
    }
}
