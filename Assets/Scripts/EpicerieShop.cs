using UnityEngine;

// Place on an NPC/trigger inside the Épicerie scene.
// Player presses E to open the shop dialogue. Buttons are provided by DialogueSystem's
// acceptJobButton (Acheter) and declineJobButton (Partir).
public class EpicerieShop : MonoBehaviour
{
    [Header("Prices (in francs)")]
    [SerializeField] private int foodPrice   = 8;
    [SerializeField] private int waterPrice  = 5;
    [SerializeField] private float foodRestore  = 0.35f;
    [SerializeField] private float waterRestore = 0.35f;

    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private ResourceSystem  resources;
    private InteractIndicator indicator;

    private ShopItem pendingItem;

    private enum ShopItem { Food, Water }

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
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        string text =
            $"Épicerie Moreau\n\n" +
            $"[Pain + légumes]   {foodPrice} francs  (+{Mathf.RoundToInt(foodRestore*100)}% nourriture)\n" +
            $"[Eau fraîche]      {waterPrice} francs  (+{Mathf.RoundToInt(waterRestore*100)}% eau)\n\n" +
            $"Que désires-tu ?";

        dialogueSystem.OpenShopMenu(text,
            buyFoodLabel:  $"Pain ({foodPrice}f)",
            buyWaterLabel: $"Eau ({waterPrice}f)",
            onBuyFood:  BuyFood,
            onBuyWater: BuyWater,
            onLeave:    () => dialogueSystem.Close());
    }

    private void BuyFood()
    {
        if (resources == null) return;
        bool ok = resources.SpendMoney(foodPrice);
        string msg = ok
            ? $"Tu achètes du pain. (+{Mathf.RoundToInt(foodRestore*100)}% nourriture)"
            : "Tu n'as pas assez de francs.";
        if (ok) resources.Recover(foodRestore, 0f, 0f);
        dialogueSystem.ShowShopResult(msg);
    }

    private void BuyWater()
    {
        if (resources == null) return;
        bool ok = resources.SpendMoney(waterPrice);
        string msg = ok
            ? $"Tu achètes de l'eau fraîche. (+{Mathf.RoundToInt(waterRestore*100)}% eau)"
            : "Tu n'as pas assez de francs.";
        if (ok) resources.Recover(0f, waterRestore, 0f);
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
