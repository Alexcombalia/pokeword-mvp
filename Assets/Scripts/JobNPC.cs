using UnityEngine;

public class JobNPC : MonoBehaviour
{
    [Header("Job Data")]
    [SerializeField] private string jobDescription = "Peindre une maison";
    [SerializeField] private float energyCost = 0.2f; // 20%
    [SerializeField] private int moneyReward = 10;

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
            dialogueSystem.OpenJobOffer(this, jobDescription, energyCost, moneyReward);
        }
    }

    public bool CanDoJob()
    {
        return resources != null && resources.energy >= energyCost;
    }

    public bool DoJob()
    {
        if (resources == null) return false;
        if (resources.energy < energyCost) return false;

        resources.energy = Mathf.Clamp01(resources.energy - energyCost);
        resources.AddMoney(moneyReward);

        Debug.Log($"Trabajo completado: {jobDescription}. +{moneyReward}$");
        return true;
    }

    public float GetEnergyCost()
    {
        return energyCost;
    }

    public int GetMoneyReward()
    {
        return moneyReward;
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