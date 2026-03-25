using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("Interaction Prompt")]
    [Min(0f)] public float promptHeight = 1.35f;
    public Color promptColor = new Color(0.93f, 0.94f, 0.95f, 1f);

    [Header("Idle Motion")]
    public bool enableIdleBob = true;
    [Min(0f)] public float bobHeight = 0.03f;
    [Min(0f)] public float bobSpeed = 1.6f;
    [Min(0f)] public float swayAmount = 0.012f;
    [Min(0f)] public float swaySpeed = 1.1f;
    public bool useRandomStartOffset = true;

    [TextArea(2, 6)]
    public string[] dialogueLines = {
        "The enemies are getting faster. Upgrade your sprint before the next room.",
        "Come back when you have more kills. I might have something for you."
    };

    int  _lineIndex    = 0;
    bool _playerInRange = false;
    Vector3 _startLocalPosition;
    float _bobTimeOffset;
    float _motionTime;
    TextMeshPro _interactionPrompt;

    void Start()
    {
        _startLocalPosition = transform.localPosition;
        _bobTimeOffset = useRandomStartOffset ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        EnsureInteractionPrompt();
        UpdateInteractionPrompt();
    }

    void Update()
    {
        if (!_playerInRange) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            Interact();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    void LateUpdate()
    {
        bool dialogueIsOpen = DialogueManager.Instance != null && DialogueManager.Instance.IsOpen;

        if (!enableIdleBob)
        {
            transform.localPosition = _startLocalPosition;
            return;
        }

        if (dialogueIsOpen)
            return;

        _motionTime += Time.deltaTime;
        float yOffset = Mathf.Sin((_motionTime * bobSpeed) + _bobTimeOffset) * bobHeight;
        float xOffset = Mathf.Sin((_motionTime * swaySpeed) + _bobTimeOffset + 1.3f) * swayAmount;
        transform.localPosition = _startLocalPosition + new Vector3(xOffset, yOffset, 0f);
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

    void EnsureInteractionPrompt()
    {
        if (_interactionPrompt != null)
            return;

        Transform existing = transform.Find("InteractionPrompt");
        GameObject promptGO = existing != null ? existing.gameObject : new GameObject("InteractionPrompt");
        promptGO.transform.SetParent(transform, false);
        promptGO.transform.localPosition = new Vector3(0f, promptHeight, 0f);
        promptGO.transform.localRotation = Quaternion.identity;
        promptGO.transform.localScale = Vector3.one * 0.45f;

        _interactionPrompt = promptGO.GetComponent<TextMeshPro>();
        if (_interactionPrompt == null)
            _interactionPrompt = promptGO.AddComponent<TextMeshPro>();

        _interactionPrompt.alignment = TextAlignmentOptions.Center;
        _interactionPrompt.fontSize = 6f;
        _interactionPrompt.color = promptColor;
        _interactionPrompt.enableWordWrapping = false;
        _interactionPrompt.outlineWidth = 0.2f;
        _interactionPrompt.outlineColor = new Color(0f, 0f, 0f, 0.8f);
        _interactionPrompt.raycastTarget = false;
    }

    void UpdateInteractionPrompt()
    {
        if (_interactionPrompt == null)
            return;

        _interactionPrompt.transform.localPosition = new Vector3(0f, promptHeight, 0f);
        _interactionPrompt.color = promptColor;
        _interactionPrompt.text = $"{npcName}\n[E] Interact";
        _interactionPrompt.gameObject.SetActive(_playerInRange);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            UpdateInteractionPrompt();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            UpdateInteractionPrompt();
            Close();
        }
    }
}
