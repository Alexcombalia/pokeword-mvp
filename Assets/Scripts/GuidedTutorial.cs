using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Tutorial "aprender haciendo" forzado — se ejecuta UNA sola vez, la primera
// vez que se entra a cualquier nivel. A diferencia de CoachMarks (avisos
// puntuales con temporizador), aquí cada paso ESPERA a que el jugador haga
// la acción real (mover el joystick, pulsar E, pulsar P) antes de avanzar —
// nunca se puede perder por ir "demasiado rápido" porque no hay timer que
// lo cierre solo. Patrón: pantalla atenuada + flecha animada + texto, como
// en el onboarding de la mayoría de juegos móviles.
public class GuidedTutorial : MonoBehaviour
{
    public static GuidedTutorial Instance { get; private set; }

    static readonly Color DimBg  = new Color(0f, 0f, 0f, 0.55f);
    static readonly Color CardBg = new Color(0.051f, 0.122f, 0.208f, 0.97f);
    static readonly Color Gold   = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color Cream  = new Color(0.941f, 0.902f, 0.827f, 1.00f);

    GameObject root;
    GameObject dimGO;
    GameObject cardGO;
    TMP_Text   cardText;
    RectTransform arrowRT;
    TMP_Text   arrowGlyph;
    Vector2    arrowBasePos;
    TMP_Text   skipTxt;

    public bool IsRunning { get; private set; }

    // El botón "Saltar X" es compartido por las tres secuencias (RunSequence,
    // RunBeginnerTurnSequence, RunSceneCrossingSequence) — hay que saber
    // cuál está activa para que saltar cierre la correcta y marque el
    // PlayerPrefs que toca.
    bool runningBeginnerTurnSequence;
    bool runningSceneCrossingSequence;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        SetVisible(false);
    }

    public void RunIfFirstTime()
    {
        if (PlayerPrefs.GetInt("guided_tutorial_done", 0) == 1) return;
        if (IsRunning) return;
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        IsRunning = true;
        // Deja que la escena, el joystick y el jugador terminen de aparecer
        // tras el SceneManager.LoadScene antes de mostrar nada encima.
        yield return new WaitForSecondsRealtime(0.6f);

        bool es = GameSettings.IsBasic;
        skipTxt.text = es ? "Saltar X" : "Passer X";
        SetVisible(true);

        // ── Paso 1 — moverse (joystick, abajo a la derecha) ──
        ShowStep(
            es ? "Desliza aquí para moverte" : "Glisse ici pour te déplacer",
            new Vector2(1f, 0f), new Vector2(-180, 480), new Vector2(380, 150),
            new Vector2(-180, 360), vertical: true);
        yield return WaitForMovement();

        // ── Paso 2 — cuaderno de palabras (botón P, pos (140,420)) ──
        ShowStep(
            es ? "Pulsa aquí para ver las palabras que ya capturaste" : "Appuie ici pour voir les mots que tu as déjà capturés",
            new Vector2(0f, 0f), new Vector2(320, 780), new Vector2(420, 150),
            new Vector2(140, 560), vertical: true);
        yield return WaitForPokeWorldOpen();

        // ── Paso 3 — cerrar el cuaderno (botón X, en MobileControlsSetup: pos (-80,420)) ──
        ShowStep(
            es ? "Pulsa aquí para cerrar el cuaderno de palabras" : "Appuie ici pour fermer le carnet de mots",
            new Vector2(1f, 0f), new Vector2(-260, 780), new Vector2(420, 150),
            new Vector2(-80, 560), vertical: true);
        yield return WaitForPokeWorldClose();

        // ── Paso 4 — el botón de pista (LevelHUD, esquina inferior izquierda,
        // pos (24,28) con pivote (0,0), o sea centro en (144,60)) ──
        //
        // Solo si el nivel tiene pistas. Va ANTES de "hablar" porque ese paso
        // termina con el diálogo abierto tapando la pantalla, y ya no habría
        // dónde enseñar esto.
        //
        // Este paso NO espera a que se pulse: usar una pista cuesta una
        // estrella, y obligar al jugador a gastarla para pasar el tutorial
        // sería cobrarle por aprender dónde está el botón. Se enseña, se dice
        // lo que cuesta, y se sigue.
        var lvl = LevelManager.Instance?.CurrentLevel;
        if (lvl != null && lvl.hintAvailable && lvl.hints != null && lvl.hints.Count > 0)
        {
            ShowStep(
                es ? "¿Atascado? Aquí tienes una pista.\nTe cuesta una estrella." : "Bloqué ? Ici, un indice.\nÇa coûte une étoile.",
                new Vector2(0f, 0f), new Vector2(330, 340), new Vector2(440, 170),
                new Vector2(144, 150), vertical: true);
            yield return new WaitForSecondsRealtime(4.5f);
        }

        // ── Paso 5 — hablar (botón E, en MobileControlsSetup: pos (140,260)) ──
        ShowStep(
            es ? "Acércate a alguien y pulsa aquí para hablar" : "Approche-toi de quelqu'un et appuie ici pour parler",
            new Vector2(0f, 0f), new Vector2(320, 580), new Vector2(420, 150),
            new Vector2(140, 380), vertical: true);
        yield return WaitForDialogueOpen();

        FinishSequence();
    }

    // Secuencia independiente de RunSequence: se dispara por evento (la
    // primera vez que BeginnerTurnController abre su panel), no al entrar al
    // nivel — RunSequence corre ANTES de que exista ninguna respuesta de
    // servidor, no puede señalar un panel que depende de una respuesta
    // async. Solo explica la fase "léelo/dilo" — el botón Saltar es
    // autoexplicativo con su propio label cuando aparece (a los 3 fallos),
    // no existe todavía en el instante en que arranca esta secuencia.
    public void ExplainBeginnerTurnOnce()
    {
        if (PlayerPrefs.GetInt("guided_tutorial_beginner_turn_done", 0) == 1) return;
        if (IsRunning) return;
        StartCoroutine(RunBeginnerTurnSequence());
    }

    IEnumerator RunBeginnerTurnSequence()
    {
        IsRunning = true;
        runningBeginnerTurnSequence = true;
        bool es = GameSettings.IsBasic;
        skipTxt.text = es ? "Saltar X" : "Passer X";
        SetVisible(true);

        ShowStep(
            es
                ? "Esta es la traducción de lo que dijiste. Algunas palabras clave pueden estar ocultas — ¡acuérdate de ellas! Graba tu voz diciéndola en francés."
                : "Voici la traduction de ce que tu as dit. Certains mots clés peuvent être cachés — souviens-t'en ! Enregistre ta voix en le disant en français.",
            new Vector2(0.5f, 0.5f), new Vector2(0, 380), new Vector2(560, 190),
            new Vector2(0, 100), vertical: true);
        // Espera solo a que EMPIECE a grabar, no a que termine el turno entero
        // — si se quedara hasta el final, la tarjeta del tutorial (que
        // apunta al botón GRABAR) tapa la frase que el jugador necesita leer
        // mientras intenta leerla en voz alta.
        yield return WaitForBeginnerTurnRecordStart();

        FinishBeginnerTurnSequence();
    }

    IEnumerator WaitForBeginnerTurnRecordStart()
    {
        while (BeginnerTurnController.Instance == null) yield return null;
        while (!BeginnerTurnController.Instance.IsRecording) yield return null;
    }

    void FinishBeginnerTurnSequence()
    {
        StopAllCoroutines();
        SetVisible(false);
        IsRunning = false;
        runningBeginnerTurnSequence = false;
        PlayerPrefs.SetInt("guided_tutorial_beginner_turn_done", 1);
        PlayerPrefs.Save();
    }

    // Secuencia independiente, disparada desde LevelManager.StartLevel solo
    // para niveles con allowedScenes (cruzan escenas de verdad, p.ej. nivel
    // 4) — en el resto de niveles SceneExit bloquea todo cruce, así que esto
    // no aplicaría. A diferencia de los otros pasos (que señalan un botón
    // fijo de la UI), aquí la flecha sigue la posición REAL en el mundo de
    // la salida — mismo cálculo world-to-screen que ya usa
    // SpeechCaptionUI.PositionBubble para seguir a Jacques.
    public void ExplainSceneCrossingOnce(List<string> allowedScenes, string currentSceneName)
    {
        if (PlayerPrefs.GetInt("guided_tutorial_scene_crossing_done", 0) == 1) return;
        if (IsRunning) return;
        StartCoroutine(RunSceneCrossingSequence(allowedScenes, currentSceneName));
    }

    IEnumerator RunSceneCrossingSequence(List<string> allowedScenes, string currentSceneName)
    {
        IsRunning = true;
        runningSceneCrossingSequence = true;
        // Deja que la escena y sus SceneExit terminen de instanciarse tras
        // el SceneManager.LoadScene antes de buscarlos.
        yield return new WaitForSecondsRealtime(0.6f);

        // Busca la primera salida (de las que este nivel permite cruzar)
        // que NO sea la propia escena de partida — para el nivel 4
        // (Puerto, allowedScenes=[Puerto,Playa,Muelle]) esto encuentra la
        // salida hacia Playa, el primer sitio al que hay que ir.
        SceneExit target = null;
        foreach (var exit in FindObjectsOfType<SceneExit>())
        {
            if (exit.TargetScene != currentSceneName && allowedScenes.Contains(exit.TargetScene))
            {
                target = exit;
                break;
            }
        }

        // Si por lo que sea no se encuentra ninguna (nivel mal configurado,
        // etc.), no bloquear al jugador con un tutorial que señala a nada.
        if (target == null)
        {
            IsRunning = false;
            runningSceneCrossingSequence = false;
            yield break;
        }

        bool es = GameSettings.IsBasic;
        skipTxt.text = es ? "Saltar X" : "Passer X";
        SetVisible(true);
        cardText.text = es
            ? "En este nivel puedes cambiar de zona — acércate al borde y pulsa E"
            : "Dans ce niveau tu peux changer de zone — approche-toi du bord et appuie sur E";

        var cardRT = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(440, 140);

        arrowRT.anchorMin = arrowRT.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRT.pivot     = new Vector2(0.5f, 0.5f);
        arrowGlyph.text   = "↓";

        var canvasRT = GetComponent<RectTransform>();
        var cam = FindObjectOfType<Camera>();

        StopCoroutine(nameof(BounceArrow));
        StartCoroutine(nameof(BounceArrow));

        // Sigue la posición real de la salida en pantalla mientras el
        // jugador siga en la escena de partida — en cuanto cruza (cambia de
        // escena), se acabó, ya lo consiguió (mismo criterio "esperar a la
        // acción real" que el resto de pasos de este tutorial).
        while (SceneManager.GetActiveScene().name == currentSceneName)
        {
            if (cam == null) cam = FindObjectOfType<Camera>();
            if (cam != null && target != null)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(target.transform.position + Vector3.up * 1f);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, null, out Vector2 localPoint);
                arrowBasePos = localPoint + new Vector2(0, 70);
                cardRT.anchoredPosition = localPoint + new Vector2(0, 190);
            }
            yield return null;
        }

        FinishSceneCrossingSequence();
    }

    void FinishSceneCrossingSequence()
    {
        StopAllCoroutines();
        SetVisible(false);
        IsRunning = false;
        runningSceneCrossingSequence = false;
        PlayerPrefs.SetInt("guided_tutorial_scene_crossing_done", 1);
        PlayerPrefs.Save();
    }

    IEnumerator WaitForMovement()
    {
        float sustained = 0f;
        while (sustained < 0.35f)
        {
            sustained = VirtualJoystick.Direction.sqrMagnitude > 0.15f ? sustained + Time.unscaledDeltaTime : 0f;
            yield return null;
        }
    }

    IEnumerator WaitForDialogueOpen()
    {
        DialogueSystem dialogue = null;
        while (dialogue == null || !dialogue.IsOpen())
        {
            if (dialogue == null) dialogue = FindObjectOfType<DialogueSystem>();
            yield return null;
        }
    }

    IEnumerator WaitForPokeWorldOpen()
    {
        PokeWorldController pw = null;
        while (pw == null || !pw.IsOpen())
        {
            if (pw == null) pw = FindObjectOfType<PokeWorldController>();
            yield return null;
        }
    }

    // Se llama justo después de WaitForPokeWorldOpen, así que el cuaderno ya
    // está abierto — solo falta esperar a que el jugador lo cierre de verdad.
    IEnumerator WaitForPokeWorldClose()
    {
        PokeWorldController pw = FindObjectOfType<PokeWorldController>();
        while (pw == null) { yield return null; pw = FindObjectOfType<PokeWorldController>(); }
        while (pw.IsOpen()) yield return null;
    }

    void FinishSequence()
    {
        StopAllCoroutines();
        SetVisible(false);
        IsRunning = false;
        PlayerPrefs.SetInt("guided_tutorial_done", 1);
        PlayerPrefs.Save();
    }

    void SkipCurrentSequence()
    {
        if (runningBeginnerTurnSequence) FinishBeginnerTurnSequence();
        else if (runningSceneCrossingSequence) FinishSceneCrossingSequence();
        else FinishSequence();
    }

    void ShowStep(string text, Vector2 anchor, Vector2 cardPos, Vector2 cardSize, Vector2 arrowPos, bool vertical)
    {
        cardText.text = text;

        var cardRT = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = anchor;
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.anchoredPosition = cardPos;
        cardRT.sizeDelta = cardSize;

        arrowRT.anchorMin = arrowRT.anchorMax = anchor;
        arrowRT.pivot     = new Vector2(0.5f, 0.5f);
        arrowRT.anchoredPosition = arrowPos;
        arrowBasePos = arrowPos;
        // "↓"/"←" (bloque Flechas, U+2190) en vez de "▼"/"◀" (bloque Formas
        // Geométricas) — ese bloque es el mismo de "★"/"🔒", que no existen
        // en la fuente TMP por defecto; las flechas de U+2190 sí parecen
        // estar cubiertas (se usan sin problema en "→" por toda la app).
        arrowGlyph.text = vertical ? "↓" : "←";

        StopCoroutine(nameof(BounceArrow));
        StartCoroutine(nameof(BounceArrow));
    }

    IEnumerator BounceArrow()
    {
        while (true)
        {
            float offset = Mathf.Sin(Time.unscaledTime * 4f) * 14f;
            arrowRT.anchoredPosition = arrowBasePos + new Vector2(0, offset);
            yield return null;
        }
    }

    void SetVisible(bool visible)
    {
        if (root != null) root.SetActive(visible);
    }

    void SkipTutorial()
    {
        SkipCurrentSequence();
    }

    void BuildUI()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 470; // above CoachMarks (450) and DialogueSystem (400)

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        root = new GameObject("Root", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero; rootRT.offsetMax = Vector2.zero;

        // Dim overlay — visual focus only, NEVER blocks touches (the whole
        // point is that the player interacts with the joystick/E/P beneath).
        dimGO = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        dimGO.transform.SetParent(root.transform, false);
        var dimRT = dimGO.GetComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero; dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = Vector2.zero; dimRT.offsetMax = Vector2.zero;
        var dimImg = dimGO.GetComponent<Image>();
        dimImg.color = DimBg;
        dimImg.raycastTarget = false;

        // Card
        cardGO = new GameObject("Card", typeof(RectTransform), typeof(Image));
        cardGO.transform.SetParent(root.transform, false);
        cardGO.GetComponent<Image>().color = CardBg;
        cardGO.GetComponent<Image>().raycastTarget = false;

        var cardTxtGO = new GameObject("Text", typeof(RectTransform));
        cardTxtGO.transform.SetParent(cardGO.transform, false);
        var ctRT = cardTxtGO.GetComponent<RectTransform>();
        ctRT.anchorMin = Vector2.zero; ctRT.anchorMax = Vector2.one;
        ctRT.offsetMin = new Vector2(18, 10); ctRT.offsetMax = new Vector2(-18, -10);
        cardText = cardTxtGO.AddComponent<TextMeshProUGUI>();
        cardText.fontSize  = 27;
        cardText.color     = Cream;
        cardText.alignment = TextAlignmentOptions.Center;
        cardText.fontStyle = FontStyles.Bold;
        cardText.enableWordWrapping = true;
        cardText.raycastTarget = false;
        cardText.enableAutoSizing = true;
        cardText.fontSizeMin = 20;
        cardText.fontSizeMax = 27;

        // Arrow
        var arrowGO = new GameObject("Arrow", typeof(RectTransform));
        arrowGO.transform.SetParent(root.transform, false);
        arrowRT = arrowGO.GetComponent<RectTransform>();
        arrowRT.sizeDelta = new Vector2(80, 80);
        arrowGlyph = arrowGO.AddComponent<TextMeshProUGUI>();
        arrowGlyph.text      = "↓";
        arrowGlyph.fontSize  = 48;
        arrowGlyph.color     = Gold;
        arrowGlyph.alignment = TextAlignmentOptions.Center;
        arrowGlyph.fontStyle = FontStyles.Bold;
        arrowGlyph.raycastTarget = false;

        // Skip button — top-right, always available, never traps the player
        var skipGO = new GameObject("Skip", typeof(RectTransform), typeof(Image));
        skipGO.transform.SetParent(root.transform, false);
        var skipRT = skipGO.GetComponent<RectTransform>();
        skipRT.anchorMin = skipRT.anchorMax = new Vector2(1f, 1f);
        skipRT.pivot     = new Vector2(1f, 1f);
        skipRT.anchoredPosition = new Vector2(-20, -40);
        skipRT.sizeDelta = new Vector2(180, 56);
        skipGO.GetComponent<Image>().color = Color.clear;

        var skipTxtGO = new GameObject("Text", typeof(RectTransform));
        skipTxtGO.transform.SetParent(skipGO.transform, false);
        var stRT = skipTxtGO.GetComponent<RectTransform>();
        stRT.anchorMin = Vector2.zero; stRT.anchorMax = Vector2.one;
        stRT.anchoredPosition = Vector2.zero; stRT.sizeDelta = Vector2.zero;
        skipTxt = skipTxtGO.AddComponent<TextMeshProUGUI>();
        skipTxt.text      = "Passer X";
        skipTxt.fontSize  = 20;
        skipTxt.color     = Cream;
        skipTxt.alignment = TextAlignmentOptions.Right;
        skipTxt.raycastTarget = false;

        var skipBtn = skipGO.AddComponent<Button>();
        skipBtn.targetGraphic = skipGO.GetComponent<Image>();
        skipBtn.onClick.AddListener(SkipTutorial);
    }
}
