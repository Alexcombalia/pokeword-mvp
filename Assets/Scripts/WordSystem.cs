using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CapturedWord
{
    public string french;
    public string spanish;
    public string context;               // NPC sentence where the word appeared
    public int    reviewInterval;        // scene changes between reviews (doubles on correct answer)
    public int    sceneChangesUntilReview; // countdown to next review

    public CapturedWord(string fr, string es, string ctx = "")
    {
        french  = fr; spanish = es; context = ctx;
        reviewInterval          = 3;
        sceneChangesUntilReview = 3;
    }
}


public class WordSystem : MonoBehaviour
{
    public List<CapturedWord> capturedWords = new List<CapturedWord>();

    private void Start()
    {
        var data = SaveSystem.Load();
        LoadFromSave(data);
    }

    public bool IsCaptured(string frenchWord)
    {
        foreach (var w in capturedWords)
        {
            if (w.french == frenchWord) return true;
        }
        return false;
    }

    public void Capture(string frenchWord, string spanishTranslation, string context = "")
    {
        // Se avisa SIEMPRE, esté o no ya en el cuaderno permanente — para
        // la estrella de palabras del nivel actual cuenta "la tocaste en
        // este intento", no si ya la sabías de antes (ver LevelManager).
        LevelManager.Instance?.NotifyWordCaptured(frenchWord);

        if (!IsCaptured(frenchWord))
        {
            var word = new CapturedWord(frenchWord, spanishTranslation, context);
            if (GameSettings.IsBasic)
            {
                word.reviewInterval          = 5;
                word.sceneChangesUntilReview = 5;
            }
            capturedWords.Add(word);

            Haptics.Medium();
            Debug.Log($"[Pokéworld] Capturada: {frenchWord} = {spanishTranslation}");
            if (LevelSystem.Instance != null) LevelSystem.Instance.NotifyWordCaptured();
        }

        // La tarjeta con la traducción se muestra SIEMPRE al tocar la palabra,
        // ya esté en el cuaderno o no — antes solo salía la primera vez, y al
        // volver a tocar una palabra ya conocida no pasaba nada visible (se
        // podía confundir con un fallo).
        WordCapturePopup.Instance?.Show(frenchWord, spanishTranslation, context);
    }

    public void LoadFromSave(SaveData data)
    {
        capturedWords.Clear();

        if (data == null || data.words == null)
            return;

        foreach (var savedWord in data.words)
        {
            if (savedWord == null) continue;

            var w = new CapturedWord(savedWord.french, savedWord.spanish, savedWord.context ?? "");
            // Backwards compat: old saves have reviewInterval == 0
            if (savedWord.reviewInterval > 0)
            {
                w.reviewInterval          = savedWord.reviewInterval;
                w.sceneChangesUntilReview = savedWord.sceneChangesUntilReview;
            }
            capturedWords.Add(w);
        }
    }

    public void ResetWords()
    {
        capturedWords.Clear();
    }
}
