using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CapturedWord
{
    public string french;
    public string spanish;

    public CapturedWord(string fr, string es)
    {
        french = fr;
        spanish = es;
    }
}

public class WordSystem : MonoBehaviour
{
    public List<CapturedWord> capturedWords = new List<CapturedWord>();

    public bool IsCaptured(string frenchWord)
    {
        foreach (var w in capturedWords)
        {
            if (w.french == frenchWord) return true;
        }
        return false;
    }

    public void Capture(string frenchWord, string spanishTranslation)
    {
        if (IsCaptured(frenchWord)) return;

        capturedWords.Add(new CapturedWord(frenchWord, spanishTranslation));
        Debug.Log($"[Pokéworld] Capturada: {frenchWord} = {spanishTranslation}");
    }
}
