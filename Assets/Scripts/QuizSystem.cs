using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// ── Question model ────────────────────────────────────────────────────────────

public enum QuestionType { MCFrToEs, MCEsToFr, FreeFrToEs, FreeEsToFr }

public class QuizQuestion
{
    public CapturedWord  word;
    public QuestionType  type;
    public List<string>  options      = new List<string>(); // 4 entries for MC
    public int           correctIndex;                       // index of correct answer in options

    public string GetPrompt()
    {
        switch (type)
        {
            case QuestionType.MCFrToEs:
            case QuestionType.FreeFrToEs: return "¿Qué significa...?";
            default:                      return "¿Cómo se dice en français...?";
        }
    }

    public string GetWordDisplay()
    {
        switch (type)
        {
            case QuestionType.MCFrToEs:
            case QuestionType.FreeFrToEs: return word.french;
            default:                      return word.spanish;
        }
    }

    public string GetCorrectAnswer()
    {
        switch (type)
        {
            case QuestionType.MCFrToEs:
            case QuestionType.FreeFrToEs: return word.spanish;
            default:                      return word.french;
        }
    }

    public bool CheckAnswer(string answer) =>
        string.Equals(answer.Trim(), GetCorrectAnswer().Trim(),
                      System.StringComparison.OrdinalIgnoreCase);
}

// ── QuizSystem ────────────────────────────────────────────────────────────────

public class QuizSystem : MonoBehaviour
{
    public static QuizSystem Instance { get; private set; }

    public int CurrentStreak { get; private set; }

    static readonly string[] SkipScenes = { "MainMenu", "IntroVideo", "Citas", "Salida" };

    // Fallback distractors when the player has few captured words
    static readonly (string fr, string es)[] FallbackPool =
    {
        ("maison","casa"),    ("eau","agua"),      ("pain","pan"),
        ("mer","mar"),        ("rue","calle"),      ("ami","amigo"),
        ("travail","trabajo"),("vin","vino"),        ("fromage","queso"),
        ("poisson","pescado"),("bateau","barco"),    ("port","puerto"),
        ("marché","mercado"), ("soleil","sol"),      ("nuit","noche"),
        ("matin","mañana"),   ("soir","tarde"),      ("femme","mujer"),
        ("homme","hombre"),   ("enfant","niño"),     ("livre","libro"),
        ("table","mesa"),     ("chaise","silla"),    ("fenêtre","ventana"),
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (System.Array.IndexOf(SkipScenes, scene.name) >= 0) return;

        var wordSystem = FindObjectOfType<WordSystem>();
        if (wordSystem == null || wordSystem.capturedWords.Count == 0) return;

        // Tick down all review counters
        foreach (var w in wordSystem.capturedWords)
            w.sceneChangesUntilReview = Mathf.Max(0, w.sceneChangesUntilReview - 1);

        var due = wordSystem.capturedWords.Where(w => w.sceneChangesUntilReview == 0).ToList();
        if (due.Count == 0) return;

        var questions = BuildQuestions(due, wordSystem.capturedWords);
        QuizUI.Instance?.Show(questions);
    }

    List<QuizQuestion> BuildQuestions(List<CapturedWord> due, List<CapturedWord> all)
    {
        bool isBasic    = GameSettings.IsBasic;
        var result      = new List<QuizQuestion>();
        var shuffled    = due.OrderBy(_ => Random.value).ToList();
        var mcTypes     = new[] { QuestionType.MCFrToEs, QuestionType.MCEsToFr };
        var allTypes    = new[] { QuestionType.MCFrToEs, QuestionType.MCEsToFr,
                                  QuestionType.FreeFrToEs, QuestionType.FreeEsToFr };

        for (int i = 0; i < 3; i++)
        {
            var word = shuffled[i % shuffled.Count];
            var type = isBasic
                ? mcTypes[Random.Range(0, mcTypes.Length)]
                : allTypes[Random.Range(0, allTypes.Length)];
            var q = new QuizQuestion { word = word, type = type };

            bool isMC = type == QuestionType.MCFrToEs || type == QuestionType.MCEsToFr;
            if (isMC) BuildMCOptions(q, all, isBasic ? 2 : 4);

            result.Add(q);
        }
        return result;
    }

    void BuildMCOptions(QuizQuestion q, List<CapturedWord> all, int totalOptions = 4)
    {
        bool esDir      = q.type == QuestionType.MCFrToEs;
        string correct  = q.GetCorrectAnswer();
        int needed      = totalOptions - 1;
        var distractors = new List<string>();

        foreach (var w in all.OrderBy(_ => Random.value))
        {
            if (distractors.Count >= needed) break;
            string cand = esDir ? w.spanish : w.french;
            if (cand != correct && !distractors.Contains(cand))
                distractors.Add(cand);
        }

        foreach (var (fr, es) in FallbackPool.OrderBy(_ => Random.value))
        {
            if (distractors.Count >= needed) break;
            string cand = esDir ? es : fr;
            if (cand != correct && !distractors.Contains(cand))
                distractors.Add(cand);
        }

        q.options      = distractors.Take(needed).ToList();
        q.correctIndex = Random.Range(0, totalOptions);
        q.options.Insert(q.correctIndex, correct);
    }

    public void OnAnswerGiven(QuizQuestion q, bool correct)
    {
        if (correct)
        {
            CurrentStreak++;
            q.word.reviewInterval          = Mathf.Min(q.word.reviewInterval * 2, 30);
            q.word.sceneChangesUntilReview = q.word.reviewInterval;
        }
        else
        {
            CurrentStreak                  = 0;
            q.word.reviewInterval          = 2;
            q.word.sceneChangesUntilReview = 2;
        }
    }

    public void OnQuizComplete(int correctCount)
    {
        var resources = FindObjectOfType<ResourceSystem>();
        if (resources == null) return;
        if      (correctCount >= 3) resources.money += 15;
        else if (correctCount == 2) resources.money += 7;
    }
}
