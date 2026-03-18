using UnityEngine;

public class ResourceSystem : MonoBehaviour
{
    [Header("Current Resources (0..1)")]
    [Range(0f, 1f)] public float food = 0.9f;
    [Range(0f, 1f)] public float water = 0.9f;
    [Range(0f, 1f)] public float energy = 0.9f;

    [Header("Money")]
    public int money = 50;

    [Header("Drain Speeds (per second)")]
    public float foodDrainPerSecond = 0.0005f;
    public float waterDrainPerSecond = 0.0005f;
    public float energyDrainPerSecond = 0.0005f;

    private void Update()
    {
        float dt = Time.deltaTime;

        food = Mathf.Clamp01(food - foodDrainPerSecond * dt);
        water = Mathf.Clamp01(water - waterDrainPerSecond * dt);
        energy = Mathf.Clamp01(energy - energyDrainPerSecond * dt);
    }

    public void DrainOnConversation()
    {
        food = Mathf.Clamp01(food - 0.02f);
        water = Mathf.Clamp01(water - 0.02f);
        energy = Mathf.Clamp01(energy - 0.03f);
    }

    public void DrainOnSceneChange()
    {
        food = Mathf.Clamp01(food - 0.01f);
        water = Mathf.Clamp01(water - 0.01f);
        energy = Mathf.Clamp01(energy - 0.01f);
    }

    public void Recover(float foodAmount, float waterAmount, float energyAmount)
    {
        food = Mathf.Clamp01(food + foodAmount);
        water = Mathf.Clamp01(water + waterAmount);
        energy = Mathf.Clamp01(energy + energyAmount);
    }

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (money < amount)
            return false;

        money -= amount;
        return true;
    }
}