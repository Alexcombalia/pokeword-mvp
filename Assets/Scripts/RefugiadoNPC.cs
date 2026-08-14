using System.Collections.Generic;
using UnityEngine;

// Spanish Republican refugee in the port — scripted only (no AI).
// Teaches urgency vocabulary through emotional, story-driven dialogue.
public class RefugiadoNPC : MonoBehaviour
{
    private static readonly List<List<string>> conversations = new List<List<string>>
    {
        new List<string>
        {
            "Por favor... s'il vous plaît. Avez-vous de l'[eau|agua] ?",
            "Je suis arrivé de [Barcelone|Barcelona] il y a trois [jours|días].",
            "Ma [famille|familia]... elle est encore en [Espagne|España].",
            "Je cherche du [travail|trabajo]. N'importe quoi."
        },
        new List<string>
        {
            "Attention à la [police|policía]. Ils demandent les [papiers|papeles].",
            "Si tu n'as pas de [documents|documentos]... ils t'emmènent.",
            "Moi, j'ai [peur|miedo] chaque [jour|día].",
            "Mais je ne peux pas [partir|salir]. Je n'ai nulle part où aller."
        },
        new List<string>
        {
            "Tu es [gentil|amable] de me parler.",
            "Ici, beaucoup de [gens|gente] font semblant de ne pas nous [voir|ver].",
            "Nous sommes des [réfugiés|refugiados]. Pas des [criminels|criminales].",
            "Merci... [merci|gracias]."
        }
    };

    private int convIndex = 0;
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
            var conv = conversations[convIndex % conversations.Count];
            dialogueSystem.Open(new List<string>(conv));
            convIndex++;
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
