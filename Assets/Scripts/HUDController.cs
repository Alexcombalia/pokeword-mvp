using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Fill Images")]
    public Image foodFill;
    public Image waterFill;
    public Image energyFill;

    [Header("Money UI")]
    public TMP_Text moneyText;

    private ResourceSystem resources;

    private void Start()
    {
        resources = FindObjectOfType<ResourceSystem>();
        if (resources == null)
        {
            Debug.LogError("No se encontró ResourceSystem en la escena.");
        }
    }

    private void Update()
    {
        if (resources == null) return;

        foodFill.fillAmount = resources.food;
        waterFill.fillAmount = resources.water;
        energyFill.fillAmount = resources.energy;

        if (moneyText != null)
        {
            moneyText.text = "$" + resources.money.ToString();
        }
    }
}