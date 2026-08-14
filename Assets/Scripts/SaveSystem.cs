using System.Collections.Generic;
using UnityEngine;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.IO;
#endif

[System.Serializable]
public class SavedWord
{
    public string french;
    public string spanish;
    public string context;
    public int    reviewInterval;
    public int    sceneChangesUntilReview;
}

[System.Serializable]
public class SaveData
{
    public float food;
    public float water;
    public float energy;
    public int money;
    public int day;
    public int missionStep;
    public SavedWord[] words;
}

// Separate from SaveData on purpose: level results are permanent progress
// records, not a snapshot of the current playthrough (food/water/day), so
// they get their own small store instead of widening every Save() call site.
[System.Serializable]
public class LevelResultsData
{
    public List<LevelResult> results = new List<LevelResult>();
}

public static class SaveSystem
{
    private const string PlayerPrefsKey = "SaveData";
    private const string LevelResultsPrefsKey = "LevelResults";
#if !UNITY_WEBGL || UNITY_EDITOR
    private static string path => Application.persistentDataPath + "/save.json";
    private static string levelResultsPath => Application.persistentDataPath + "/level_results.json";
#endif

    public static void SaveLevelResults(Dictionary<string, LevelResult> results)
    {
        var data = new LevelResultsData();
        foreach (var kvp in results) data.results.Add(kvp.Value);
        string json = JsonUtility.ToJson(data);

#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString(LevelResultsPrefsKey, json);
        PlayerPrefs.Save();
#else
        File.WriteAllText(levelResultsPath, json);
#endif
    }

    public static Dictionary<string, LevelResult> LoadLevelResults()
    {
        var dict = new Dictionary<string, LevelResult>();
        string json = null;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (PlayerPrefs.HasKey(LevelResultsPrefsKey)) json = PlayerPrefs.GetString(LevelResultsPrefsKey);
#else
        if (File.Exists(levelResultsPath)) json = File.ReadAllText(levelResultsPath);
#endif

        if (string.IsNullOrEmpty(json)) return dict;

        var data = JsonUtility.FromJson<LevelResultsData>(json);
        if (data?.results != null)
            foreach (var r in data.results)
                if (!string.IsNullOrEmpty(r.difficulty)) // descarta resultados guardados antes de este arreglo
                    dict[$"{r.levelId}:{r.difficulty}"] = r;

        return dict;
    }

    public static void Save(ResourceSystem resources, WordSystem wordSystem, int day)
    {
        SavedWord[] savedWords = new SavedWord[0];

        if (wordSystem != null && wordSystem.capturedWords != null)
        {
            savedWords = new SavedWord[wordSystem.capturedWords.Count];

            for (int i = 0; i < wordSystem.capturedWords.Count; i++)
            {
                var cw = wordSystem.capturedWords[i];
                savedWords[i] = new SavedWord
                {
                    french                  = cw.french,
                    spanish                 = cw.spanish,
                    context                 = cw.context,
                    reviewInterval          = cw.reviewInterval,
                    sceneChangesUntilReview = cw.sceneChangesUntilReview
                };
            }
        }

        int missionStep = MissionSystem.Instance != null ? MissionSystem.Instance.missionStep : 0;

        SaveData data = new SaveData
        {
            food  = resources.food,
            water = resources.water,
            energy = resources.energy,
            money = resources.money,
            day   = day,
            missionStep = missionStep,
            words = savedWords
        };

        string json = JsonUtility.ToJson(data, true);

#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
#else
        File.WriteAllText(path, json);
        Debug.Log("Game saved at: " + path);
#endif
    }

    public static SaveData Load()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return null;
        string json = PlayerPrefs.GetString(PlayerPrefsKey);
#else
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
#endif
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteSave()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
#else
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save deleted.");
        }
#endif
    }
}
