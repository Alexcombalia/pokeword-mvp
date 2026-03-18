using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DynamicWordHighlighter
{
    public static string ProcessText(string input, out Dictionary<string, (string fr, string es)> capturableWords)
    {
        Dictionary<string, (string fr, string es)> tempCapturable = new Dictionary<string, (string fr, string es)>();

        if (string.IsNullOrEmpty(input))
        {
            capturableWords = tempCapturable;
            return input;
        }

        var db = DynamicWordDatabase.Instance;
        if (db == null)
        {
            capturableWords = tempCapturable;
            return input;
        }

        string pattern = @"\b[\wÀ-ÿ']+\b";

        string result = Regex.Replace(input, pattern, match =>
        {
            string originalWord = match.Value;
            string cleanWord = originalWord.ToLower();

            if (db.TryGetTranslation(cleanWord, out string translation))
            {
                if (!tempCapturable.ContainsKey(cleanWord))
                {
                    tempCapturable[cleanWord] = (cleanWord, translation);
                }

                return $"<link=\"{cleanWord}\"><color=yellow>{originalWord}</color></link>";
            }

            return originalWord;
        });

        capturableWords = tempCapturable;
        return result;
    }
}