using System.Collections.Generic;
using UnityEngine;

public class DynamicWordDatabase : MonoBehaviour
{
    public static DynamicWordDatabase Instance;

    private Dictionary<string, string> wordMap = new Dictionary<string, string>()
    {
        { "bonjour", "hola" },
        { "merci", "gracias" },
        { "pain", "pan" },
        { "eau", "agua" },
        { "baguette", "baguette" },
        { "pomme", "manzana" },
        { "lait", "leche" },
        { "fromage", "queso" },
        { "farine", "harina" },
        { "beurre", "mantequilla" }
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool TryGetTranslation(string frenchWord, out string spanish)
    {
        return wordMap.TryGetValue(frenchWord.ToLower(), out spanish);
    }

    public Dictionary<string, string> GetAllWords()
    {
        return wordMap;
    }
}