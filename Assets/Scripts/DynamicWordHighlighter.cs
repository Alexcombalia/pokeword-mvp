using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class DynamicWordHighlighter
{
    // Matches [french|spanish] tags inserted by the server
    private static readonly Regex tagPattern = new Regex(@"\[([^\|\]]+)\|([^\]]+)\]");

    // Matches plain French words for database fallback
    private static readonly Regex wordPattern = new Regex(@"\b[\wà-ÿÀ-Ÿ']+\b");

    public static string ProcessText(string input, out Dictionary<string, (string fr, string es)> capturableWords)
    {
        var capturable = new Dictionary<string, (string fr, string es)>();

        if (string.IsNullOrEmpty(input))
        {
            capturableWords = capturable;
            return input;
        }

        // Pass 1 — extract server-tagged words [french|spanish]
        string result = tagPattern.Replace(input, match =>
        {
            string fr = match.Groups[1].Value.Trim().ToLower();
            string es = match.Groups[2].Value.Trim();
            string display = match.Groups[1].Value.Trim();

            if (!capturable.ContainsKey(fr))
                capturable[fr] = (fr, es);

            // Azul tinta (#1F5C8A, mismo tono que SpeechCaptionUI.SpanishInk),
            // no el dorado pálido #FFD166 pensado para el panel de chat azul
            // marino antiguo — sobre el pergamino claro actual ese dorado casi
            // no se distinguía del papel (contraste casi nulo), y encima el
            // cuerpo del texto del NPC ya es dorado oscuro (FrenchInk), así que
            // un dorado más claro tampoco se distinguía de la frase alrededor.
            return $"<link=\"{fr}\"><u><color=#1F5C8A>{display}</color></u></link>";
        });

        // Pass 2 — fallback: highlight words found in the local database
        var db = DynamicWordDatabase.Instance;
        if (db != null)
        {
            result = wordPattern.Replace(result, match =>
            {
                string original = match.Value;
                string clean = original.ToLower();

                // Skip if already inside a TMPro link tag
                if (capturable.ContainsKey(clean)) return original;

                if (db.TryGetTranslation(clean, out string translation))
                {
                    capturable[clean] = (clean, translation);
                    return $"<link=\"{clean}\"><u><color=#1F5C8A>{original}</color></u></link>";
                }

                return original;
            });
        }

        capturableWords = capturable;
        return result;
    }
}
