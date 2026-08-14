using System.Collections.Generic;
using UnityEngine;

// Inspector: assign this to the port officer NPC in OficinaPortuaria scene.
// Scripted (no AI) — fixed lines to avoid hallucination on a key story beat.
public class OficinaPortuariaNPC : MonoBehaviour
{
    private static readonly List<string> lines = new List<string>
    {
        "Inspecteur Gaston, [bureau|oficina] du port.",
        "Tu cherches quelqu'un ? Un journaliste, tu dis...",
        "Henry Beaumont — oui, je connais ce nom.",
        "Il traîne au [marché|mercado], du côté du Bar Dragón Verde.",
        "Fais attention, les [rues|calles] ne sont pas sûres.",
        "Bonne [chance|suerte]."
    };

    private bool playerInRange = false;
    private bool hasSpoken     = false;
    private DialogueSystem  dialogueSystem;
    private InteractIndicator indicator;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        indicator      = GetComponent<InteractIndicator>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null || dialogueSystem.IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed)
        {
            indicator?.Hide();
            dialogueSystem.Open(new List<string>(lines));

            if (!hasSpoken)
            {
                hasSpoken = true;
                MissionSystem.Instance?.AdvanceTo(1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        indicator?.Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        indicator?.Hide();
    }
}
