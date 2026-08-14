using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

// Comparación tolerante de texto para validar que el jugador leyó en voz alta
// la frase francesa objetivo — no es puntuación fonética real, solo distancia
// de edición sobre el texto ya transcrito por STT (ver BeginnerTurnController).
public static class TextMatcher
{
    const float Threshold = 0.75f; // recalibrar aquí tras probar en dispositivo real

    public static bool IsCloseEnough(string spoken, string target)
    {
        string a = Normalize(spoken);
        string b = Normalize(target);
        if (a.Length == 0 || b.Length == 0) return false;

        int dist = Levenshtein(a, b);
        float sim = 1f - (float)dist / Mathf.Max(a.Length, b.Length);
        return sim >= Threshold;
    }

    static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";

        s = s.ToLowerInvariant().Trim();
        s = s.Normalize(NormalizationForm.FormD);
        s = new string(s.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        s = Regex.Replace(s, @"[^\w\s]", "");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        return s;
    }

    static int Levenshtein(string a, string b)
    {
        int[,] d = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) d[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Mathf.Min(
                    d[i - 1, j] + 1,
                    d[i, j - 1] + 1,
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[a.Length, b.Length];
    }
}
