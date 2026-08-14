using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [Header("Dialogue Lines (French)")]
    [TextArea(2, 6)]
    public List<string> dialogueLines = new List<string>();

    [Header("Capturable Words")]
    public List<CapturableWordEntry> capturableWords = new List<CapturableWordEntry>();

    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null) return;
        if (dialogueSystem.IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            var dict = new Dictionary<string, (string fr, string es)>();
            foreach (var w in capturableWords)
            {
                if (w == null) continue;
                if (string.IsNullOrEmpty(w.linkId)) continue;
                dict[w.linkId.ToLower()] = (w.french, w.spanish);
            }

            dialogueSystem.SetCapturableWords(dict);
            dialogueSystem.Open(dialogueLines);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        Haptics.Light();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}
