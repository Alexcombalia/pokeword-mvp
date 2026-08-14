using UnityEngine;
using TMPro;

public class DaySystem : MonoBehaviour
{
    public int currentDay = 1;

    private TMP_Text dayText;

    private void Start()
    {
        dayText = GameObject.Find("DayText")?.GetComponent<TMP_Text>();

        LoadDay();
        UpdateUI();
    }

    public void NextDay()
    {
        currentDay++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (dayText == null)
        {
            dayText = GameObject.Find("DayText")?.GetComponent<TMP_Text>();
        }

        if (dayText != null)
            dayText.text = "Jour " + currentDay;
    }

    public void SaveDay(ResourceSystem resources)
    {
        var wordSystem = FindObjectOfType<WordSystem>();
        SaveSystem.Save(resources, wordSystem, currentDay);
    }

    public void LoadDay()
    {
        var data = SaveSystem.Load();
        if (data != null)
        {
            currentDay = data.day;
        }
    }
    public void ResetDay()
    {
        currentDay = 1;
        UpdateUI();
    }
}