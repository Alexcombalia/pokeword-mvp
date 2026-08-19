using UnityEngine;

public static class GameSettings
{
    // Dificultad = cuánto andamiaje lleva el jugador (traducción, lectura en
    // voz alta, guía fonética). NO tiene nada que ver con qué idioma aprende.
    public static string Difficulty
    {
        get => PlayerPrefs.GetString("difficulty", "medium");
        set { PlayerPrefs.SetString("difficulty", value); PlayerPrefs.Save(); }
    }

    public static bool IsBasic => Difficulty == "basic";

    // ── Idioma que el jugador APRENDE ────────────────────────────────────
    //
    // "fr" (francés, el original) o "en" (inglés). El idioma de apoyo sigue
    // siendo el español en los dos casos: cambia lo que hablan los NPCs, no
    // la interfaz. Por eso el modo débutant casi no se entera del cambio —
    // ya muestra siempre la mitad española de sus textos.
    //
    // Se elige al abrir el juego y se guarda. Todo lo que dependa del idioma
    // cuelga de aquí: los expedientes del servidor (sufijo de clave, ver
    // DossierSuffix), las voces TTS, el reconocimiento de voz y las palabras
    // objetivo del nivel.
    public const string LangFrench  = "fr";
    public const string LangEnglish = "en";
    public const string LangItalian = "it";

    public static string TargetLanguage
    {
        get => PlayerPrefs.GetString("target_language", LangFrench);
        set { PlayerPrefs.SetString("target_language", value); PlayerPrefs.Save(); }
    }

    public static bool IsEnglish => TargetLanguage == LangEnglish;
    public static bool IsItalian => TargetLanguage == LangItalian;

    // Sufijo que se le añade a la clave de expediente para pedirle al
    // servidor la versión en este idioma: "cased1_nadia" en francés,
    // "cased1_nadia_en" en inglés.
    //
    // Por convención y no por tabla: así añadir italiano no obliga a tocar
    // el cliente, solo a escribir los expedientes "_it" en el servidor.
    // El francés no lleva sufijo porque es el idioma con el que se construyó
    // el juego y renombrar sus claves habría roto los dos casos ya hechos.
    public static string DossierSuffix =>
        TargetLanguage == LangFrench ? "" : "_" + TargetLanguage;

    // Nombre del idioma para la interfaz, en español.
    public static string TargetLanguageName =>
        TargetLanguage == LangEnglish ? "inglés" : TargetLanguage == LangItalian ? "italiano" : "francés";
}
