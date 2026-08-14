using UnityEngine;

public static class GameSettings
{
    public static string Difficulty
    {
        get => PlayerPrefs.GetString("difficulty", "medium");
        set { PlayerPrefs.SetString("difficulty", value); PlayerPrefs.Save(); }
    }

    public static bool IsBasic => Difficulty == "basic";
}
