using UnityEngine;

public class Auberge : MonoBehaviour
{
    [SerializeField] private int sleepCost = 10;
    [SerializeField] private float energyRestore = 1f;

    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private ResourceSystem resources;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        resources = FindObjectOfType<ResourceSystem>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null) return;
        if (dialogueSystem.IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            dialogueSystem.OpenSleep(this, sleepCost);
        }
    }

    public bool Sleep()
    {
        if (resources == null) return false;

        bool paid = resources.SpendMoney(sleepCost);
        if (!paid) return false;

        resources.energy = energyRestore;

        var daySystem = FindObjectOfType<DaySystem>();
        if (daySystem != null)
        {
            daySystem.NextDay();
            daySystem.SaveDay(resources);
        }

        Debug.Log("Nuevo día iniciado.");
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}