using System.Collections.Generic;
using UnityEngine;

// Vichy police officer in the Plaza — blocks player, demands papers in French.
// Correct answer: player types "oui" / "voilà" / "mes papiers" (or similar).
// Wrong answer or ignoring: energy drain + warning.
// After first successful interaction, policeman ignores player for the rest of the session.
public class PoliceVichyNPC : MonoBehaviour
{
    [SerializeField] private float energyPenaltyWrong  = 0.12f;
    [SerializeField] private float energyPenaltyIgnore = 0.06f;

    // Accepted answer keywords (any match = pass)
    private static readonly string[] validAnswers =
        { "oui", "voilà", "voila", "papier", "papiers", "bien sûr", "bien sur", "monsieur" };

    private bool playerInRange  = false;
    private bool sessionCleared = false; // police leaves player alone after passing once
    private bool awaitingAnswer = false;

    private DialogueSystem   dialogueSystem;
    private ResourceSystem   resources;
    private InteractIndicator indicator;

    private static readonly List<string> challengeLines = new List<string>
    {
        "Halte ! [Police|Policía] nationale.\nVos [papiers|documentos], s'il vous plaît !"
    };

    private static readonly List<string> passLines = new List<string>
    {
        "Très bien. Circulez.",
        "Bonne [journée|día]... et restez visible."
    };

    private static readonly List<string> failLines = new List<string>
    {
        "Vous refusez de coopérer ?\nC'est noté. Demi-tour !",
        "Vous [perdez|pierde] du temps et de l'[énergie|energía]."
    };

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        resources      = FindObjectOfType<ResourceSystem>();
        indicator      = GetComponent<InteractIndicator>();
    }

    private void Update()
    {
        if (sessionCleared) return;
        if (!playerInRange) return;
        if (dialogueSystem == null) return;

        // Intercept: player enters zone, police challenges immediately (no E press)
        // But if dialogue is already open and player is in input mode, check for answer
        if (!dialogueSystem.IsOpen() && !awaitingAnswer)
        {
            TriggerChallenge();
        }
    }

    private void TriggerChallenge()
    {
        awaitingAnswer = true;
        dialogueSystem.OpenPoliceChallenge(
            challengeLine: challengeLines[0],
            onAnswer:      OnPlayerAnswer,
            onTimeout:     OnPlayerIgnored
        );
    }

    private void OnPlayerAnswer(string answer)
    {
        answer = answer.Trim().ToLower();
        bool passed = false;
        foreach (var kw in validAnswers)
            if (answer.Contains(kw)) { passed = true; break; }

        awaitingAnswer = false;

        if (passed)
        {
            sessionCleared = true;
            dialogueSystem.Open(new List<string>(passLines));
        }
        else
        {
            if (resources != null)
                resources.Recover(0f, 0f, -energyPenaltyWrong);
            dialogueSystem.Open(new List<string>(failLines));
        }
    }

    private void OnPlayerIgnored()
    {
        awaitingAnswer = false;
        if (resources != null)
            resources.Recover(0f, 0f, -energyPenaltyIgnore);
        dialogueSystem.Open(new List<string>(failLines));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        awaitingAnswer = false;
    }
}
