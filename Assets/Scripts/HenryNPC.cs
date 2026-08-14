using UnityEngine;

public class HenryNPC : MonoBehaviour
{
    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private InteractIndicator indicator;
    private bool hasSpoken = false;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        indicator = GetComponent<InteractIndicator>();
        if (GetComponent<YSorter>() == null)
            gameObject.AddComponent<YSorter>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null) return;

        if (!dialogueSystem.IsOpen() && (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed))
        {
            indicator?.Hide();
            dialogueSystem.OpenHenryChat();

            if (!hasSpoken)
            {
                hasSpoken = true;
                MissionSystem.Instance?.AdvanceTo(2);
            }
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
