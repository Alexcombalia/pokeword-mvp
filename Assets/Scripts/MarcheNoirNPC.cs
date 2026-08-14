using UnityEngine;

// Black market vendor in Le Panier — moral choice.
// Food costs fewer francs than the Épicerie but causes moral stress (small energy drain).
// Accept button = buy contraband. Decline button = refuse.
public class MarcheNoirNPC : MonoBehaviour
{
    [SerializeField] private int   contrabandPrice = 4;
    [SerializeField] private float foodRestore     = 0.30f;
    [SerializeField] private float energyPenalty   = 0.08f;

    private bool playerInRange = false;
    private DialogueSystem   dialogueSystem;
    private ResourceSystem   resources;
    private InteractIndicator indicator;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        resources      = FindObjectOfType<ResourceSystem>();
        indicator      = GetComponent<InteractIndicator>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null || dialogueSystem.IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed)
        {
            indicator?.Hide();
            OpenOffer();
        }
    }

    private void OpenOffer()
    {
        string text =
            $"Psst... tu veux manger ?\n" +
            $"J'ai de la [nourriture|comida] sans [ticket|cupón].\n" +
            $"{contrabandPrice} francs. Pas un mot à la [police|policía].";

        dialogueSystem.OpenShopMenu(
            text,
            buyFoodLabel:  $"Acheter ({contrabandPrice}f)",
            buyWaterLabel: "Refuser",
            onBuyFood:  BuyContraband,
            onBuyWater: () => dialogueSystem.Close(),
            onLeave:    () => dialogueSystem.Close()
        );
    }

    private void BuyContraband()
    {
        if (resources == null) return;
        bool ok = resources.SpendMoney(contrabandPrice);
        string msg;
        if (ok)
        {
            resources.Recover(foodRestore, 0f, -energyPenalty);
            msg = "Tu prends la [nourriture|comida] en silence.\n" +
                  "Tu manges, mais tu te sens mal à l'aise.";
        }
        else
        {
            msg = "Tu n'as pas assez de [francs|francos].";
        }
        dialogueSystem.ShowShopResult(msg);
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
