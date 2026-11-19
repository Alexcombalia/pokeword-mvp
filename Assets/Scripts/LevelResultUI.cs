using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Pantalla de resultado al terminar un nivel: 3 estrellas independientes,
// estado del objetivo, tiempo, aviso de pista usada (-1 estrella).
public class LevelResultUI : MonoBehaviour
{
    public static LevelResultUI Instance { get; private set; }

    static readonly Color BgDeep   = new Color(0.012f, 0.031f, 0.059f, 0.94f);
    static readonly Color NavyMid  = new Color(0.027f, 0.067f, 0.118f, 0.98f);
    static readonly Color Gold     = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color GoldDim  = new Color(1.000f, 0.820f, 0.400f, 0.25f);
    static readonly Color Cream    = new Color(0.941f, 0.902f, 0.827f, 1.00f);
    static readonly Color CreamDim = new Color(0.941f, 0.902f, 0.827f, 0.60f);
    static readonly Color Fail     = new Color(0.85f,  0.35f,  0.30f,  1f);
    static readonly Color TextDark = new Color(0.012f, 0.031f, 0.059f, 1f);

    static readonly Color LineOk   = new Color(0.35f, 0.75f, 0.40f, 1f);
    static readonly Color LineFail = new Color(0.85f, 0.45f, 0.40f, 1f);

    GameObject root;
    TMP_Text   titleText;
    Image[]    starIcons = new Image[3];
    TMP_Text   statusText;
    TMP_Text   objectiveLineText;
    TMP_Text   wordsLineText;
    TMP_Text   speedLineText;
    TMP_Text   hintText;
    TMP_Text   repsText;   // repeticiones de habla y escucha
    GameObject epiloguePanel;   // carta de cierre del caso, antes de las estrellas
    TMP_Text   epilogueText;
    GameObject nextBtnGO;
    TMP_Text   nextBtnLabel;
    TMP_Text   retryBtnLabel;
    TMP_Text   menuBtnLabel;

    LevelDefinition pendingRetry;
    LevelDefinition pendingNext;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        root.SetActive(false);
    }

    public void Show(LevelDefinition level, LevelResult result, bool success,
                      bool starObjective, bool starWords, bool starSpeed, bool hintUsed, bool skipPenalty = false)
    {
        var dialogue = FindObjectOfType<DialogueSystem>();
        if (dialogue != null && dialogue.IsOpen()) dialogue.Close();

        pendingRetry = level;

        gameObject.SetActive(true);
        root.SetActive(true);

        titleText.text = level.title;

        for (int i = 0; i < 3; i++)
            starIcons[i].color = i < result.stars ? Gold : GoldDim;

        bool es = GameSettings.IsBasic;
        bool detectiveWin = level.detectiveMode && success;

        // "Se acabó el tiempo" no vale para un caso: en modo detective el
        // tiempo no hace perder nunca. Si un caso se pierde es por haber
        // acusado a quien no era.
        bool accusedWrong = level.detectiveMode && !success;

        statusText.text  = success
            ? (level.detectiveMode
                ? (LangText.Pick("Caso cerrado", "Affaire classée", "Case closed", "Caso chiuso", "Cas tancat"))
                : (LangText.Pick("¡Objetivo conseguido!", "Objectif réussi !", "Objective complete!", "Obiettivo raggiunto!", "Objectiu aconseguit!")))
            : accusedWrong
                ? (LangText.Pick("Acusaste a un inocente.", "Vous avez accusé un innocent.", "You accused an innocent person.", "Hai accusato un innocente.", "Has acusat un innocent."))
                : (LangText.Pick("Se acabó el tiempo — inténtalo de nuevo.", "Temps écoulé — réessaie.", "Time is up — try again.", "Tempo scaduto — riprova.", "S'ha acabat el temps — torna-ho a provar."));
        statusText.color = success ? Gold : Fail;

        // El epílogo cierra la historia en los dos desenlaces: el bueno al
        // resolverlo, y el amargo al mandar a la cárcel a quien no era. Perder
        // un caso tiene que contarse, no despacharse con una pantalla de fallo.
        string epilogue = detectiveWin ? level.DisplayEpilogue
                        : accusedWrong ? level.DisplayEpilogueFail
                        : null;
        if (!string.IsNullOrEmpty(epilogue))
        {
            epilogueText.text = epilogue;
            epiloguePanel.SetActive(true);
            epiloguePanel.transform.SetAsLastSibling(); // por delante de la tarjeta
        }
        else
        {
            epiloguePanel.SetActive(false);
        }

        // Por qué se ganó o no cada estrella, en concreto — no solo el
        // icono relleno/vacío, sino la razón (tiempo exacto, palabras que
        // faltaron), para que quede claro qué hacer mejor la próxima vez.
        string elapsedTime = TimeText.Elapsed(result.timeSeconds);
        string starTime    = TimeText.Threshold(level.starTimeSeconds);

        // Las repeticiones de habla y escucha. Desde el 30/08/2026 el objetivo
        // del juego no es aprender el idioma sino entrenar listening y
        // speaking, y esto es lo que mide esa afirmación — el único número que
        // el jugador no consigue en ninguna otra app.
        var lm = LevelManager.Instance;
        int spokenWords = lm != null ? lm.WordsSpoken : 0;
        int heardWords  = lm != null ? lm.WordsHeard  : 0;
        string spokenTime = TimeText.Elapsed(lm != null ? lm.SecondsSpoken : 0f);
        string heardTime  = TimeText.Elapsed(lm != null ? lm.SecondsHeard  : 0f);
        string lng = GameSettings.TargetLanguageName;

        // En modo detective las tres estrellas significan otra cosa: resolver
        // el caso, romper al mentiroso y el cronómetro. Y nunca se pierde por
        // tiempo, así que el texto de "no llegaste a tiempo" no aplica.
        bool detective = level.detectiveMode;

        objectiveLineText.text = starObjective
            ? (detective ? (LangText.Pick("Caso resuelto", "Affaire résolue", "Case solved", "Caso risolto", "Cas resolt"))
                         : (LangText.Pick("Objetivo conseguido", "Objectif atteint", "Objective reached", "Obiettivo raggiunto", "Objectiu aconseguit")))
            : (detective ? (LangText.Pick("El comisario no cerró el caso", "Le commissaire n'a pas classé l'affaire", "The inspector did not close the case", "Il commissario non ha chiuso il caso", "El comissari no va tancar el cas"))
                         : (LangText.Pick("No conseguiste el objetivo a tiempo", "Objectif non atteint à temps", "Objective not reached in time", "Obiettivo non raggiunto in tempo", "No has aconseguit l'objectiu a temps")));
        objectiveLineText.color = starObjective ? LineOk : LineFail;

        // La 2ª estrella NO es siempre la del careo: solo lo es en los niveles
        // que declaran un secondStarCheckpoint (los avanzados). En los casos
        // débutant sigue siendo la de vocabulario.
        //
        // Bastaba con estar en modo detective para enseñar el texto del
        // mentiroso, así que un jugador del caso débutant que había hecho
        // confesar a Piotr leía "no llegaste a romper al mentiroso" y perdía la
        // estrella por otra razón — la pantalla le mentía sobre su propia
        // partida, que es de los fallos que peor sientan.
        if (detective && !string.IsNullOrEmpty(level.secondStarCheckpoint))
        {
            wordsLineText.text = starWords
                ? (LangText.Pick("Rompiste al mentiroso", "Tu as fait craquer le menteur", "You broke the liar", "Hai fatto crollare il bugiardo", "Has fet caure el mentider"))
                : (LangText.Pick("No llegaste a romper al mentiroso", "Tu n'as pas fait craquer le menteur", "You did not break the liar", "Non hai fatto crollare il bugiardo", "No has arribat a fer caure el mentider"));
        }
        else
        {
            // 2ª estrella de HABLA (sustituye a la de vocabulario, 30/08/2026).
            int target = level.spokenWordsTarget;
            wordsLineText.text = starWords
                ? LangText.Pick($"Hablaste {spokenWords} palabras en voz alta",
                                $"Tu as dit {spokenWords} mots à voix haute",
                                $"You spoke {spokenWords} words out loud",
                                $"Hai detto {spokenWords} parole ad alta voce",
                                $"Vas dir {spokenWords} paraules en veu alta")
                : LangText.Pick($"Hablaste {spokenWords} palabras (hacían falta {target})",
                                $"Tu as dit {spokenWords} mots (il en fallait {target})",
                                $"You spoke {spokenWords} words (you needed {target})",
                                $"Hai detto {spokenWords} parole (ne servivano {target})",
                                $"Vas dir {spokenWords} paraules (en calien {target})");
        }
        wordsLineText.color = starWords ? LineOk : LineFail;

        if (detective)
        {
            speedLineText.text = starSpeed
                ? LangText.Pick($"Resuelto en {elapsedTime} (menos de {starTime})", $"Résolu en {elapsedTime} (moins de {starTime})",
                                $"Solved in {elapsedTime} (under {starTime})",      $"Risolto in {elapsedTime} (meno di {starTime})",
                                $"Resolt en {elapsedTime} (menys de {starTime})")
                : LangText.Pick($"Tardaste {elapsedTime} (necesitabas menos de {starTime})", $"{elapsedTime} (il fallait moins de {starTime})",
                                $"You took {elapsedTime} (you needed under {starTime})",     $"Ci hai messo {elapsedTime} (serviva meno di {starTime})");
        }
        else
        {
            speedLineText.text = success
                ? (starSpeed
                    ? LangText.Pick($"Lo hiciste en {elapsedTime} (menos de {starTime})", $"Fait en {elapsedTime} (moins de {starTime})",
                                    $"Done in {elapsedTime} (under {starTime})",          $"Fatto in {elapsedTime} (meno di {starTime})",
                                    $"Fet en {elapsedTime} (menys de {starTime})")
                    : LangText.Pick($"Tardaste {elapsedTime} (necesitabas menos de {starTime})", $"{elapsedTime} (il fallait moins de {starTime})",
                                    $"You took {elapsedTime} (you needed under {starTime})",     $"Ci hai messo {elapsedTime} (serviva meno di {starTime})"))
                : (LangText.Pick("No hubo estrella de velocidad — no llegaste a tiempo", "Pas d'étoile de vitesse — temps écoulé", "No speed star — you ran out of time", "Niente stella di velocità — tempo scaduto", "No hi ha estrella de velocitat — no has arribat a temps"));
        }
        speedLineText.color = starSpeed ? LineOk : LineFail;

        // Mismo aviso que la pista, una línea por muleta usada — para que
        // quede claro por qué se perdió cada estrella deducida.
        repsText.text =
            LangText.Pick($"Has hablado {spokenWords} palabras en {lng}  ·  {spokenTime}",
                          $"Tu as dit {spokenWords} mots  ·  {spokenTime}",
                          $"You spoke {spokenWords} words  ·  {spokenTime}",
                          $"Hai detto {spokenWords} parole  ·  {spokenTime}",
                          $"Has dit {spokenWords} paraules  ·  {spokenTime}")
            + "\n" +
            LangText.Pick($"Has escuchado {heardWords} palabras  ·  {heardTime}",
                          $"Tu en as écouté {heardWords}  ·  {heardTime}",
                          $"You listened to {heardWords} words  ·  {heardTime}",
                          $"Ne hai ascoltate {heardWords}  ·  {heardTime}",
                          $"N'has escoltat {heardWords}  ·  {heardTime}");

        string hintLine = hintUsed ? (LangText.Pick("Pista usada (-1 estrella)", "Indice utilisé (-1 étoile)", "Hint used (-1 star)", "Indizio usato (-1 stella)", "Pista usada (-1 estrella)")) : "";
        string skipLine = skipPenalty ? (LangText.Pick("Saltos usados (-1 estrella)", "Indices sautés (-1 étoile)", "Skips used (-1 star)", "Salti usati (-1 stella)", "Salts usats (-1 estrella)")) : "";
        hintText.text = string.Join("\n", new[] { hintLine, skipLine }.Where(s => !string.IsNullOrEmpty(s)));

        // Botón de siguiente nivel: solo si existe (1 estrella mínima
        // desbloquea el siguiente, regla ya cerrada en el diseño). Con solo
        // el Nivel 1 construido todavía no aparece — es normal, no un bug.
        pendingNext = (result.stars >= 1) ? LevelDatabase.Get(level.id + 1) : null;
        nextBtnGO.SetActive(pendingNext != null);
        if (pendingNext != null)
            nextBtnLabel.text = LangText.Pick("SIGUIENTE NIVEL >", "NIVEAU SUIVANT >", "NEXT LEVEL >", "LIVELLO SUCCESSIVO >", "NIVELL SEGÜENT >");

        retryBtnLabel.text = LangText.Pick("REINTENTAR", "RÉESSAYER", "RETRY", "RIPROVA", "TORNAR-HO A PROVAR");
        menuBtnLabel.text  = LangText.Pick("Menú de niveles", "Menu des niveaux", "Level menu", "Menu dei livelli", "Menú de nivells");
    }

    void Retry()
    {
        root.SetActive(false);
        gameObject.SetActive(false);
        if (pendingRetry != null) LevelManager.Instance?.StartLevel(pendingRetry.id);
    }

    void NextLevel()
    {
        root.SetActive(false);
        gameObject.SetActive(false);
        if (pendingNext != null) LevelManager.Instance?.StartLevel(pendingNext.id);
    }

    void BackToMenu()
    {
        root.SetActive(false);
        gameObject.SetActive(false);
        LevelManager.Instance?.ExitLevel();

        // Antes esto solo mostraba el menú ENCIMA de la escena de Puerto,
        // que seguía viva debajo — por eso el botón "Volver" del menú de
        // niveles revelaba Puerto sin ninguna interfaz. Cargar MainMenu de
        // verdad cierra el nivel del todo, no solo lo tapa.
        SceneManager.LoadScene("MainMenu");
        LevelSelectUI.Instance?.Show();
    }

    void BuildUI()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 260; // above LevelHUD (120) / base HUD; dialogue (400) is always closed before Show() runs

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        root = new GameObject("Root", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(transform, false);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero; rootRT.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = BgDeep;

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.anchoredPosition = Vector2.zero;
        cardRT.sizeDelta = new Vector2(600, 900);   // +120: hueco para el bloque de repeticiones
        card.GetComponent<Image>().color = NavyMid;

        titleText = MakeLabel(card.transform, "Title", "", new Vector2(0, 330), new Vector2(520, 70),
                               30, Gold, TextAlignmentOptions.Center, FontStyles.Bold);
        titleText.enableWordWrapping = true;

        var starSprite = StarIcon.GetSprite();
        for (int i = 0; i < 3; i++)
        {
            var starGO = new GameObject("Star" + i, typeof(RectTransform), typeof(Image));
            starGO.transform.SetParent(card.transform, false);
            var sRT = starGO.GetComponent<RectTransform>();
            sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f, 0.5f);
            sRT.pivot     = new Vector2(0.5f, 0.5f);
            sRT.anchoredPosition = new Vector2(-100 + i * 100, 235);
            sRT.sizeDelta = new Vector2(80, 80);
            var img = starGO.GetComponent<Image>();
            img.sprite = starSprite;
            img.color  = GoldDim;
            starIcons[i] = img;
        }

        statusText = MakeLabel(card.transform, "Status", "", new Vector2(0, 155), new Vector2(540, 50),
                                27, Gold, TextAlignmentOptions.Center, FontStyles.Bold);

        // 3 líneas — por qué se ganó o no cada estrella, en concreto.
        objectiveLineText = MakeLabel(card.transform, "LineObjective", "", new Vector2(0, 100), new Vector2(540, 36),
                                       21, Cream, TextAlignmentOptions.Center, FontStyles.Normal);
        wordsLineText     = MakeLabel(card.transform, "LineWords", "", new Vector2(0, 66), new Vector2(540, 36),
                                       21, Cream, TextAlignmentOptions.Center, FontStyles.Normal);
        speedLineText     = MakeLabel(card.transform, "LineSpeed", "", new Vector2(0, 32), new Vector2(540, 36),
                                       21, Cream, TextAlignmentOptions.Center, FontStyles.Normal);

        hintText   = MakeLabel(card.transform, "Hint", "", new Vector2(0, -6), new Vector2(520, 32),
                                18, CreamDim, TextAlignmentOptions.Center, FontStyles.Italic);

        // ── Repeticiones de habla y escucha ──
        // Separado de las tres líneas de estrella y con su propio recuadro: no
        // es una condición que se gane o se pierda, es LO QUE HAS HECHO. Es el
        // número que sostiene la tesis del juego (entrenar speaking y
        // listening) y el que se le enseña a un evaluador.
        var repsBg = new GameObject("RepsBg", typeof(RectTransform), typeof(Image));
        repsBg.transform.SetParent(card.transform, false);
        var rbRT = repsBg.GetComponent<RectTransform>();
        rbRT.anchorMin = rbRT.anchorMax = new Vector2(0.5f, 0.5f);
        rbRT.pivot     = new Vector2(0.5f, 0.5f);
        rbRT.anchoredPosition = new Vector2(0, -68);
        rbRT.sizeDelta = new Vector2(540, 88);
        var rbImg = repsBg.GetComponent<Image>();
        rbImg.sprite = RoundedRectSprite.GetSprite();
        rbImg.type   = Image.Type.Sliced;
        rbImg.color  = new Color(0.051f, 0.122f, 0.208f, 1f);
        rbImg.raycastTarget = false;

        repsText = MakeLabel(repsBg.transform, "RepsText", "", Vector2.zero, new Vector2(520, 88),
                              20, Gold, TextAlignmentOptions.Center, FontStyles.Bold);
        repsText.lineSpacing = 8f;

        // ── Carta de cierre del caso ──
        // Va por delante de la tarjeta de resultado, a pantalla completa, en
        // vez de meterse dentro: la tarjeta ya va justa de sitio, y un caso de
        // misterio se cierra mejor con una pausa a solas con el texto que con
        // tres líneas apretadas encima de los botones.
        epiloguePanel = new GameObject("Epilogue", typeof(RectTransform), typeof(Image));
        epiloguePanel.transform.SetParent(root.transform, false);
        var epRT = epiloguePanel.GetComponent<RectTransform>();
        epRT.anchorMin = Vector2.zero; epRT.anchorMax = Vector2.one;
        epRT.offsetMin = Vector2.zero; epRT.offsetMax = Vector2.zero;
        epiloguePanel.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.10f, 0.97f);

        epilogueText = MakeLabel(epiloguePanel.transform, "EpilogueText", "", new Vector2(0, 90), new Vector2(760, 620),
                                  27, Cream, TextAlignmentOptions.TopLeft, FontStyles.Normal);
        epilogueText.enableWordWrapping = true;
        epilogueText.lineSpacing = 12f;

        MakeBtn(epiloguePanel.transform, "BtnEpilogue", "SUITE", new Vector2(0, -360), new Vector2(360, 68),
                Gold, TextDark, 25, FontStyles.Bold, () => epiloguePanel.SetActive(false));
        epiloguePanel.SetActive(false);

        nextBtnGO = MakeBtn(card.transform, "BtnNext", "NIVEAU SUIVANT >", new Vector2(0, -165), new Vector2(420, 68),
                Gold, TextDark, 25, FontStyles.Bold, NextLevel);
        nextBtnLabel = nextBtnGO.GetComponentInChildren<TMP_Text>();
        nextBtnGO.SetActive(false);

        var retryBtnGO = MakeBtn(card.transform, "BtnRetry", "RÉESSAYER", new Vector2(0, -255), new Vector2(400, 62),
                new Color(0.051f, 0.122f, 0.208f, 1f), Gold, 25, FontStyles.Bold, Retry);
        retryBtnLabel = retryBtnGO.GetComponentInChildren<TMP_Text>();

        var menuBtnGO = MakeBtn(card.transform, "BtnMenu", "Menu des niveaux", new Vector2(0, -335), new Vector2(360, 54),
                Color.clear, CreamDim, 21, FontStyles.Normal, BackToMenu);
        menuBtnLabel = menuBtnGO.GetComponentInChildren<TMP_Text>();
    }

    TMP_Text MakeLabel(Transform parent, string name, string text, Vector2 pos, Vector2 size,
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
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = color;
        tmp.alignment = align; tmp.fontStyle = style;
        return tmp;
    }

    GameObject MakeBtn(Transform parent, string name, string label, Vector2 pos, Vector2 size,
                 Color bgColor, Color textColor, float fontSize, FontStyles style,
                 UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = bgColor;

        var lbl = new GameObject("Label", typeof(RectTransform));
        lbl.transform.SetParent(go.transform, false);
        var lRT = lbl.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.anchoredPosition = Vector2.zero; lRT.sizeDelta = Vector2.zero;
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = style;
        tmp.raycastTarget = false;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        return go;
    }
}
