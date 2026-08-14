using System.Collections.Generic;
using UnityEngine;

// Washwoman in Le Panier — scripted opener, then switches to AI chat (lavandera personality).
public class LavandieraNPC : MonoBehaviour
{
    [Header("NPC Config")]
    [SerializeField] private string npcId      = "lavandera_01";
    [SerializeField] private string displayName = "Madame Colette";

    private bool playerInRange = false;
    private DialogueSystem     dialogueSystem;
    private GenericNPCChatClient chatClient;
    private InteractIndicator  indicator;

    private static readonly List<string> opener = new List<string>
    {
        "Ah, un [étranger|extranjero] ! Bienvenue dans le [quartier|barrio].",
        "Je suis Colette. Je [lave|lavo] le [linge|ropa] de tout le voisinage.",
        "Tu as des [questions|preguntas] ? Je sais tout ce qui se passe ici !"
    };

    private bool openerShown = false;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        chatClient     = FindObjectOfType<GenericNPCChatClient>();
        indicator      = GetComponent<InteractIndicator>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null || dialogueSystem.IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed)
        {
            indicator?.Hide();
            if (!openerShown)
            {
                openerShown = true;
                dialogueSystem.Open(new List<string>(opener));
            }
            else
            {
                dialogueSystem.OpenGenericChat(npcId, "lavandera", displayName, "female", chatClient, transform);
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
