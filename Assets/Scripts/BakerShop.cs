using UnityEngine;

public class BakerShop : MonoBehaviour
{
    [SerializeField] private int breadCost = 5;
    [SerializeField] private float foodRestore = 0.3f;

    private ResourceSystem resources;

    private void Start()
    {
        resources = FindObjectOfType<ResourceSystem>();
    }

    public bool CanBuyBread()
    {
        return resources != null && resources.money >= breadCost;
    }

    public bool BuyBread()
    {
        if (resources == null) return false;

        bool paid = resources.SpendMoney(breadCost);

        if (paid)
        {
            resources.Recover(foodRestore, 0f, 0f);
            Debug.Log("Has comprado pan. +comida");
            return true;
        }

        Debug.Log("No tienes suficiente dinero.");
        return false;
    }

    public int GetBreadCost()
    {
        return breadCost;
    }
}