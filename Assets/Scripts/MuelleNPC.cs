using System.Collections.Generic;
using UnityEngine;

// Dock worker at the Muelle — scripted opener then AI chat (marin personality).
public class MuelleNPC : MonoBehaviour
{
    [Header("NPC Config")]
    [SerializeField] private string npcId       = "muelle_marin_01";
    [SerializeField] private string displayName  = "Antoine";

    private bool playerInRange = false;
    private DialogueSystem      dialogueSystem;
    private GenericNPCChatClient chatClient;
    private InteractIndicator   indicator;

    private static readonly List<string> opener = new List<string>
    {
        "Hé ! Attention au [bateau|barco], tu vas tomber dans l'[eau|agua] !",
        "Je m'appelle Antoine. Je [travaille|trabajo] sur ces [quais|muelles] depuis dix ans.",
        "Tu veux savoir quelque chose sur le [port|puerto] ?"
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
                dialogueSystem.OpenGenericChat(npcId, "marin", displayName, "male", chatClient, transform);
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
