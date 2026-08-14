using UnityEngine;

public class JacquesNPC : MonoBehaviour
{
    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private JacquesChatClient chatClient;
    private InteractIndicator indicator;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        chatClient = GetComponent<JacquesChatClient>();
        indicator = GetComponent<InteractIndicator>();
        if (GetComponent<YSorter>() == null)
            gameObject.AddComponent<YSorter>();
    }

    private void Update()
    {
        if (!playerInRange || dialogueSystem == null || chatClient == null) return;

        if (!dialogueSystem.IsOpen() && (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed))
        {
            indicator?.Hide();
            dialogueSystem.OpenJacquesChat(chatClient, transform);

            if (LevelManager.Instance != null && LevelManager.Instance.IsActive)
                CoachMarks.Instance?.ShowOnce("mic",
                    GameSettings.IsBasic
                        ? "Toca aquí y habla en francés"
                        : "Touche ici et parle en français !",
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 500), new Vector2(340, 70));
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
