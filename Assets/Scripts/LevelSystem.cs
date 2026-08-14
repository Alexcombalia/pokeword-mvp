using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }

    // Total words needed to reach each level (index = level number)
    private static readonly int[] thresholds = { 0, 5, 13, 25, 40, 58, 80, 105, 125, 145, 165 };
    public static int MaxLevel => thresholds.Length - 1; // 10

    // Minimum level required to enter each scene (unlisted scenes = 0, always accessible)
    private static readonly Dictionary<string, int> sceneRequirements = new Dictionary<string, int>
    {
        // Nivel 1 — 5 palabras
        { "Auberge",          1 },
        { "Epicerie",         1 },
        { "Pescaderia",       1 },
        { "OficinaPortuaria", 1 },
        // Nivel 2 — 13 palabras
        { "LaCanebiere",      2 },
        { "CafeTerraza",      2 },
        // Nivel 3 — 25 palabras
        { "VillaFrancois",    3 },
        // Nivel 4 — 40 palabras
        { "Centro",           4 },
    };

    private static readonly string[] levelNames =
    {
        "",            // 0 unused
        "Débutant",    // 1
        "Débutant",    // 2
        "Voyageur",    // 3
        "Voyageur",    // 4
        "Curieux",     // 5
        "Curieux",     // 6
        "Habitant",    // 7
        "Habitant",    // 8
        "Marsellais",  // 9
        "Marsellais",  // 10
    };

    private TMP_Text levelText;
    private TMP_Text progressText;

    private int lastLevel = -1;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        FindUIElements();
        RefreshUI();
    }

    private void Update()
    {
        if (levelText == null) FindUIElements();

        int level = CurrentLevel();
        if (level != lastLevel)
        {
            if (lastLevel >= 0 && level > lastLevel) Haptics.Medium();
            lastLevel = level;
            RefreshUI();
        }
    }

    private void FindUIElements()
    {
        // FindObjectsOfType(true) includes inactive objects — needed because HUD starts hidden
        foreach (var t in FindObjectsOfType<TMP_Text>(true))
        {
            if (t.name == "LevelText")         levelText    = t;
            if (t.name == "LevelProgressText") progressText = t;
        }
    }

    private void RefreshUI()
    {
        if (levelText == null) FindUIElements();

        int level = CurrentLevel();
        int wordCount = WordCount();
        int nextThreshold = WordsForNextLevel(level);

        if (levelText != null)
            levelText.text = $"Niveau {level} · {GetLevelName(level)}";

        if (progressText != null)
        {
            if (level < MaxLevel)
                progressText.text = $"{wordCount} / {nextThreshold} mots";
            else
                progressText.text = "Niveau max";
        }
    }

    // ── Public API ────────────────────────────────────────────────

    public int CurrentLevel()
    {
        return LevelForCount(WordCount());
    }

    public static int LevelForCount(int wordCount)
    {
        int level = 0;
        for (int i = 1; i < thresholds.Length; i++)
        {
            if (wordCount >= thresholds[i]) level = i;
            else break;
        }
        return level;
    }

    public static int WordsForLevel(int level)
    {
        if (level <= 0 || level >= thresholds.Length) return 0;
        return thresholds[level];
    }

    public static int WordsForNextLevel(int level)
    {
        int next = level + 1;
        if (next >= thresholds.Length) return thresholds[thresholds.Length - 1];
        return thresholds[next];
    }

    public static string GetLevelName(int level)
    {
        if (level < 1 || level >= levelNames.Length) return "";
        return levelNames[level];
    }

    public static int RequiredLevel(string sceneName)
    {
        return sceneRequirements.TryGetValue(sceneName, out int req) ? req : 0;
    }

    public bool CanAccess(string sceneName)
    {
        return CurrentLevel() >= RequiredLevel(sceneName);
    }

    public void NotifyWordCaptured()
    {
        RefreshUI();
    }

    public void ResetLevel()
    {
        RefreshUI();
    }

    private int WordCount()
    {
        var ws = FindObjectOfType<WordSystem>();
        return ws != null ? ws.capturedWords.Count : 0;
    }
}
