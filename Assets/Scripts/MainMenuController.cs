using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    // ── Palette ───────────────────────────────────────────────────────────
    static readonly Color BgDeep    = new Color(0.012f, 0.031f, 0.059f, 1.00f);
    static readonly Color NavyMid   = new Color(0.027f, 0.067f, 0.118f, 0.96f);
    static readonly Color NavyLight = new Color(0.051f, 0.122f, 0.208f, 1.00f);
    static readonly Color Gold      = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color GoldDim   = new Color(1.000f, 0.820f, 0.400f, 0.60f);
    static readonly Color Cream     = new Color(0.941f, 0.902f, 0.827f, 1.00f);
    static readonly Color CreamDim  = new Color(0.941f, 0.902f, 0.827f, 0.55f);
    static readonly Color BtnPrimary = new Color(0.85f, 0.68f, 0.25f, 1.00f); // darker gold for bg

    void Awake() => BuildUI();

    // ── Game logic ────────────────────────────────────────────────────────

    // Both modes go straight to the menú de niveles — only the difficulty
    // differs (see LevelSelectUI / redesign). There is no separate "story"
    // entry point anymore.
    public void PlayAdvanced()
    {
        GameSettings.Difficulty = "medium";
        LevelSelectUI.Instance?.Show();
    }

    public void PlayBeginner()
    {
        GameSettings.Difficulty = "basic";
        LevelSelectUI.Instance?.Show();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResetGame()
    {
        SaveSystem.DeleteSave();
        if (TutorialSystem.Instance != null)
            TutorialSystem.Instance.ResetTutorial();
        else
        {
            PlayerPrefs.DeleteKey("tutorialDone");
            PlayerPrefs.Save();
        }

        var ws = FindObjectOfType<WordSystem>();
        if (ws != null) ws.ResetWords();

        var ls = FindObjectOfType<LevelSystem>();
        if (ls != null) ls.ResetLevel();

        var ms = FindObjectOfType<MissionSystem>();
        if (ms != null) ms.ResetMission();

        var rs = FindObjectOfType<ResourceSystem>();
        if (rs != null) rs.ResetResources();

        var ds = FindObjectOfType<DaySystem>();
        if (ds != null) ds.ResetDay();

        // CoachMarks ("aprender haciendo") solo se muestran una vez por
        // jugador (PlayerPrefs) — hay que limpiarlas aquí también, si no,
        // probar el tutorial de nuevo requiere borrar PlayerPrefs a mano.
        PlayerPrefs.DeleteKey("coach_timer");
        PlayerPrefs.DeleteKey("coach_word_panel");
        PlayerPrefs.DeleteKey("coach_mic");
        PlayerPrefs.DeleteKey("guided_tutorial_done");
        PlayerPrefs.DeleteKey("guided_tutorial_beginner_turn_done");
        PlayerPrefs.DeleteKey("guided_tutorial_scene_crossing_done");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");
    }

    // ── UI builder ────────────────────────────────────────────────────────

    GameObject langScreen;
    TMP_Text   taglineTxt;
    TMP_Text   advanceBtnTxt, debutantBtnTxt, resetBtnTxt, quitBtnTxt;

    // Rótulos del menú en el idioma que se aprende. El menú del juego siempre
    // estuvo en francés como parte de la ambientación; ahora hay dos idiomas.
    void ApplyMenuLanguage()
    {
        // Mismo criterio que el resto del juego: LangText.Pick decide, y aquí
        // el primer argumento (español) no se usa nunca — el menú va siempre en
        // el idioma que se aprende. Se pasa igual para no inventar otra regla.
        string P(string fr, string en, string it, string ca) =>
            GameSettings.IsCatalan ? ca : GameSettings.IsItalian ? it : GameSettings.IsEnglish ? en : fr;

        if (taglineTxt      != null) taglineTxt.text      = P("Apprends le français en enquêtant", "Aprende inglés investigando", "Aprende italiano investigando", "Aprèn català investigant");
        if (advanceBtnTxt   != null) advanceBtnTxt.text   = P("MODE AVANCÉ",   "ADVANCED MODE", "MODO AVANZATO", "MODE AVANÇAT");
        if (debutantBtnTxt  != null) debutantBtnTxt.text  = P("MODE DÉBUTANT", "BEGINNER MODE", "MODO PRINCIPIANTE", "MODE PRINCIPIANT");
        if (resetBtnTxt     != null) resetBtnTxt.text     = P("Recommencer depuis le début", "Start from the beginning", "Ricomincia da capo", "Tornar a començar");
        if (quitBtnTxt      != null) quitBtnTxt.text      = P("Quitter", "Quit", "Esci", "Sortir");
    }

    // Primera pantalla al abrir el juego: qué idioma se quiere aprender.
    //
    // Lo que cambia NO es la interfaz —esa sigue en español— sino lo que
    // hablan los NPCs. Por eso la pregunta es "¿qué idioma quieres aprender?"
    // y no "¿en qué idioma quieres jugar?": son cosas distintas y confundirlas
    // haría que un jugador esperase el menú en inglés.
    //
    // Aparece SIEMPRE, no solo la primera vez. Mientras esto se esté testeando
    // interesa poder cambiar de idioma sin borrar la partida; cuando el juego
    // salga, basta con saltarla si ya hay idioma guardado.
    void BuildLanguageScreen(Transform root)
    {
        langScreen = Rect("LangScreen", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        langScreen.GetComponent<Image>().color = BgDeep;

        MakeLabel("LangTitle", langScreen.transform, "L I N G U A   N O I R",
            new Vector2(0, 260), new Vector2(900, 90),
            44f, Gold, TextAlignmentOptions.Center, FontStyles.Bold);

        MakeLabel("LangQuestion", langScreen.transform, "¿Qué idioma quieres aprender?",
            new Vector2(0, 120), new Vector2(900, 50),
            28f, Cream, TextAlignmentOptions.Center, FontStyles.Normal);

        // Cuatro botones no caben con el alto y el paso de tres: bajan a 72 de
        // alto y 82 de paso, y la nota de abajo se aparta. Con el reparto viejo
        // el cuarto botón se comía la nota.
        MakeBtn("BtnFrench", langScreen.transform, "FRANCÉS",
            new Vector2(0, 50), new Vector2(440, 72),
            Gold, BgDeep, 28f, FontStyles.Bold, () => ChooseLanguage(GameSettings.LangFrench), bordered: true);

        MakeBtn("BtnEnglish", langScreen.transform, "INGLÉS",
            new Vector2(0, -32), new Vector2(440, 72),
            NavyLight, Gold, 28f, FontStyles.Bold, () => ChooseLanguage(GameSettings.LangEnglish));

        MakeBtn("BtnItalian", langScreen.transform, "ITALIANO",
            new Vector2(0, -114), new Vector2(440, 72),
            NavyLight, Gold, 28f, FontStyles.Bold, () => ChooseLanguage(GameSettings.LangItalian));

        // "CATALÀ" y no "CATALÁN": en la pantalla donde eliges qué idioma
        // aprender, cada lengua se nombra a sí misma.
        MakeBtn("BtnCatalan", langScreen.transform, "CATALÀ",
            new Vector2(0, -196), new Vector2(440, 72),
            NavyLight, Gold, 28f, FontStyles.Bold, () => ChooseLanguage(GameSettings.LangCatalan));

        MakeLabel("LangNote", langScreen.transform,
            "El juego está en español. Cambia el idioma que hablan\nlos personajes con los que investigas.",
            new Vector2(0, -300), new Vector2(860, 70),
            17f, CreamDim, TextAlignmentOptions.Center, FontStyles.Italic);
    }

    void ChooseLanguage(string lang)
    {
        GameSettings.TargetLanguage = lang;
        ApplyMenuLanguage();   // el menú de detrás ya estaba construido: hay que reescribirlo
        langScreen.SetActive(false);
    }

    void BuildUI()
    {
        var cvGO = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var cv   = cvGO.GetComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 100;

        var scaler                = cvGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;

        Transform root = cvGO.transform;

        // ── Full-screen background ────────────────────────────────────────
        var bg = Rect("BG", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bg.GetComponent<Image>().color = BgDeep;

        // ── Decorative horizontal band (center, behind title) ─────────────
        var band = Anchored("Band", root, new Vector2(0.5f, 0.54f), new Vector2(1080, 320));
        band.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.10f, 0.85f);

        // ── Title block ───────────────────────────────────────────────────
        var titleY = 310f; // px above center (portrait: more vertical space)

        // Top separator line
        MakeLine("LineTop", root, new Vector2(0, titleY + 95), new Vector2(460, 1.5f), GoldDim);

        // Location
        MakeLabel("Location", root, "M A R S E I L L E  ·  1 9 4 1",
            new Vector2(0, titleY + 62), new Vector2(700, 36),
            20f, CreamDim, TextAlignmentOptions.Center, FontStyles.Normal);

        // Game title. «Pokeword» (Pokémon + capturar palabras) se retiró el
        // 24/08/2026: describía un juego de vocabulario que ya no existe.
        //
        // El cuerpo baja de 80 a 62 porque el nombre nuevo es más largo — con
        // el espaciado, "L I N G U A   N O I R" son 21 caracteres frente a los
        // 17 de antes, y a 80 se salía de los 900 de ancho.
        MakeLabel("Title", root, "L I N G U A   N O I R",
            new Vector2(0, titleY), new Vector2(900, 110),
            62f, Gold, TextAlignmentOptions.Center, FontStyles.Bold);

        // Tagline. Era "en vivant" cuando el juego iba de sobrevivir recién
        // llegado a Marsella; desde el 18/08/2026 el juego es exclusivamente
        // de resolver misterios y el jugador es un inspector de policía, así que
        // lo que se aprende hablando ya no es vivir, es investigar.
        MakeLabel("Tagline", root, "",   // lo rellena ApplyMenuLanguage
            new Vector2(0, titleY - 72), new Vector2(700, 36),
            20f, CreamDim, TextAlignmentOptions.Center, FontStyles.Italic);
        taglineTxt = root.Find("Tagline")?.GetComponent<TMP_Text>();

        // Bottom separator line
        MakeLine("LineBot", root, new Vector2(0, titleY - 104), new Vector2(460, 1.5f), GoldDim);

        // ── Button stack ──────────────────────────────────────────────────
        // Both modes lead straight to the menú de niveles (LevelSelectUI) —
        // only the difficulty differs. No separate NIVEAUX button needed.
        float baseY = -400f; // portrait: buttons well below center

        // Los rótulos van en el IDIOMA QUE SE APRENDE, no en español: era así
        // desde siempre (el menú estaba en francés) y es parte de la
        // ambientación. Lo que cambia es que ahora hay dos idiomas posibles.
        //
        // Reset y Quitar van más grandes que antes (18→22 y 16→20, con sus
        // cajas ensanchadas): "Recommencer depuis le début" no se leía entero.
        MakeBtn("BtnAvance", root, "MODE AVANCÉ",
            new Vector2(0, baseY + 150), new Vector2(440, 78),
            Gold, BgDeep, 32f, FontStyles.Bold, PlayAdvanced, bordered: true);

        MakeBtn("BtnDebutant", root, "MODE DÉBUTANT",
            new Vector2(0, baseY + 60), new Vector2(440, 70),
            NavyLight, Gold, 28f, FontStyles.Bold, PlayBeginner);

        MakeBtn("BtnReset", root, "Recommencer depuis le début",
            new Vector2(0, baseY - 22), new Vector2(480, 54),
            Color.clear, CreamDim, 22f, FontStyles.Normal, ResetGame);

        MakeBtn("BtnQuit", root, "Quitter",
            new Vector2(0, baseY - 96), new Vector2(260, 44),
            Color.clear, CreamDim, 20f, FontStyles.Normal, QuitGame);

        // El menú se construye en Awake, ANTES de que el jugador elija idioma
        // en la primera pantalla — así que sus rótulos nacen con el idioma de
        // la partida anterior. Se recogen aquí y se reescriben al elegir.
        advanceBtnTxt  = root.Find("BtnAvance/Label")?.GetComponent<TMP_Text>();
        debutantBtnTxt = root.Find("BtnDebutant/Label")?.GetComponent<TMP_Text>();
        resetBtnTxt    = root.Find("BtnReset/Label")?.GetComponent<TMP_Text>();
        quitBtnTxt     = root.Find("BtnQuit/Label")?.GetComponent<TMP_Text>();
        ApplyMenuLanguage();

        // ── Pantalla de elección de idioma ────────────────────────────────
        // Va por delante del menú, a pantalla completa. Se construye al final
        // para quedar la última en jerarquía y taparlo todo sin tocar
        // sortingOrder.
        BuildLanguageScreen(root);

        // ── Footer ────────────────────────────────────────────────────────
        var footer = new GameObject("Footer", typeof(RectTransform));
        footer.transform.SetParent(root, false);
        var fRT = footer.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0.5f, 0f);
        fRT.anchorMax = new Vector2(0.5f, 0f);
        fRT.pivot     = new Vector2(0.5f, 0f);
        fRT.anchoredPosition = new Vector2(0, 24);
        fRT.sizeDelta = new Vector2(700, 28);
        var ftxt = footer.AddComponent<TextMeshProUGUI>();
        ftxt.text      = "Lingua Noir  ·  Marseille, 1941  ·  v0.1";
        ftxt.fontSize  = 14f;
        ftxt.color     = CreamDim;
        ftxt.alignment = TextAlignmentOptions.Center;
        ftxt.fontStyle = FontStyles.Italic;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    GameObject Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
                    Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return go;
    }

    GameObject Anchored(string name, Transform parent, Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
        return go;
    }

    void MakeLine(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
    }

    void MakeLabel(string name, Transform parent, string text, Vector2 pos, Vector2 size,
                   float fontSize, Color color, TextAlignmentOptions align, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = align;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TMPro.TextOverflowModes.Overflow;
    }

    void MakeBtn(string name, Transform parent, string label, Vector2 pos, Vector2 size,
                 Color bgColor, Color textColor, float fontSize, FontStyles style,
                 UnityEngine.Events.UnityAction onClick, bool bordered = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img   = go.GetComponent<Image>();
        img.color = bgColor;

        if (bordered)
        {
            // Thin gold border via an outline child
            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(go.transform, false);
            var bRT = border.GetComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = new Vector2(-2, -2); bRT.offsetMax = new Vector2(2, 2);
            border.GetComponent<Image>().color = Gold;
            border.transform.SetAsFirstSibling();
        }

        var lbl    = new GameObject("Label", typeof(RectTransform));
        lbl.transform.SetParent(go.transform, false);
        var lRT = lbl.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.anchoredPosition = Vector2.zero; lRT.sizeDelta = Vector2.zero;

        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.color     = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cols = btn.colors;
        cols.normalColor      = bgColor;
        cols.highlightedColor = new Color(bgColor.r + 0.08f, bgColor.g + 0.08f, bgColor.b + 0.08f, 1f);
        cols.pressedColor     = new Color(bgColor.r - 0.08f, bgColor.g - 0.08f, bgColor.b - 0.08f, 1f);
        btn.colors = cols;
        btn.onClick.AddListener(onClick);
    }
}
