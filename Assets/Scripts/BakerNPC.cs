using UnityEngine;

public class BakerNPC : MonoBehaviour
{
    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private BakerShop bakerShop;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        bakerShop = GetComponent<BakerShop>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null) return;

        if (!dialogueSystem.IsOpen() && Input.GetKeyDown(KeyCode.E))
        {
            dialogueSystem.OpenBakerChat(bakerShop);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}