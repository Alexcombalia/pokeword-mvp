using System.Collections.Generic;
using UnityEngine;

// Fish market vendor — scripted lines, rich food/sea vocabulary.
// The [french|spanish] tags are processed by DynamicWordHighlighter so the player can tap to capture.
public class PescaderiaNPC : MonoBehaviour
{
    private static readonly List<string> lines = new List<string>
    {
        "Bienvenue ! Je vends le meilleur [poisson|pescado] de Marseille !",
        "Ce matin j'ai pêché des [sardines|sardinas] et du [thon|atún].",
        "Le [marché|mercado] du [port|puerto] est ouvert chaque [matin|mañana].",
        "Tu veux du [pain|pan] aussi ? Va chez le boulanger !",
        "La [mer|mar] nous nourrit. Dieu merci.",
        "Bonne [journée|día] !"
    };

    private bool playerInRange = false;
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
