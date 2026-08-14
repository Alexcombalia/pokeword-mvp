using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

// Oculta progresivamente palabras clave de la frase francesa que el jugador
// debe leer en voz alta (modo débutant) — solo vocabulario objetivo del nivel
// (targetWords), nunca artículos/partículas, con una proporción que escala
// con el id del nivel. Ver BeginnerTurnController.
public static class PhraseMasker
{
    static readonly Regex WordPattern = new Regex(@"\b[\wà-ÿÀ-Ÿ']+\b");

    public static string Mask(string phrase, List<TargetWord> targetWords, int levelId)
    {
        if (string.IsNullOrEmpty(phrase) || targetWords == null || targetWords.Count == 0)
            return phrase;

        // Nivel 1 ≈ 5% oculto, nivel 5 ≈ 65% oculto.
        float p = Mathf.Lerp(0.05f, 0.65f, Mathf.Clamp01((levelId - 1) / 4f));
        var targetSet = new HashSet<string>(targetWords.Select(w => w.french.ToLowerInvariant()));

        return WordPattern.Replace(phrase, m =>
        {
            string w = m.Value.ToLowerInvariant();
            if (!targetSet.Contains(w)) return m.Value; // solo vocabulario clave es candidato
            if (Random.value >= p) return m.Value;
            return new string('▬', Mathf.Clamp(m.Value.Length, 3, 6));
        });
    }
}
