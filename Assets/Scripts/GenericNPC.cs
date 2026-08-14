using UnityEngine;

public class GenericNPC : MonoBehaviour
{
    public enum NpcGender { Female, Male }

    [Header("NPC Identity")]
    [Tooltip("Unique ID for this NPC — used to keep separate conversation memory")]
    public string npcId = "npc_01";

    [Tooltip("Personality type: passant | vieux | jeune | mefiant | lavandera | marin | bourgeois | refugiado | anciano | vendedor | estibador | capitan | pescador | barman | posadero | nino | mujer_elegante | policia_patrulla | cliente_bar | vecino")]
    public string personalityId = "passant";

    [Tooltip("Male or Female — determines the voice used")]
    public NpcGender gender = NpcGender.Female;

    [Header("Display Name (shown in dialogue)")]
    public string displayName = "Inconnu";

    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private GenericNPCChatClient chatClient;
    private InteractIndicator indicator;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        chatClient = GetComponent<GenericNPCChatClient>();
        if (chatClient == null)
            chatClient = gameObject.AddComponent<GenericNPCChatClient>();
        indicator = GetComponent<InteractIndicator>();
        if (GetComponent<YSorter>() == null)
            gameObject.AddComponent<YSorter>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null || chatClient == null) return;

        if (!dialogueSystem.IsOpen() && (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed))
        {
            indicator?.Hide();
            string genderStr = gender == NpcGender.Male ? "male" : "female";
            dialogueSystem.OpenGenericChat(npcId, personalityId, displayName, genderStr, chatClient, transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        indicator?.Show();
        Haptics.Light();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        indicator?.Hide();
    }
}
