using UnityEngine;

public class BakerNPC : MonoBehaviour
{
    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private BakerShop bakerShop;
    private InteractIndicator indicator;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        bakerShop = GetComponent<BakerShop>();
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
            dialogueSystem.OpenBakerChat(bakerShop);
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