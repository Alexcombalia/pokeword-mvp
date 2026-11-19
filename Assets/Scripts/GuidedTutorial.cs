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

    GameObject wordChip;        // la palabra de prueba que hay que tocar
    TMP_Text   wordChipLabel;
    bool       wordChipTapped;

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

        skipTxt.text = LangText.Pick("Saltar X", "Passer X", "Skip X", "Salta X", "Saltar X");
        SetVisible(true);

        // ── Paso 1 — moverse (joystick, abajo a la derecha) ──
        ShowStep(
            LangText.Pick("Desliza aquí para moverte", "Glisse ici pour te déplacer", "Slide here to move", "Scorri qui per muoverti", "Llisca aquí per moure't"),
            new Vector2(1f, 0f), new Vector2(-180, 480), new Vector2(380, 150),
            new Vector2(-180, 360), vertical: true);
        yield return WaitForMovement();

        // ── Paso 2 — capturar una palabra ──
        //
        // La palabra sale del propio nivel (`ActiveTargetWords[0]`), así que
        // está en el idioma que se está aprendiendo y funciona en los cuatro
        // sin escribir contenido nuevo.
        //
        // Y se captura DE VERDAD, no es una simulación: por eso el paso
        // siguiente tiene sentido — al abrir el cuaderno hay algo dentro. Un
        // cuaderno vacío no enseña qué es un cuaderno.
        //
        // Tiene que ser una palabra de mentira y no una del diálogo porque
        // aquí todavía no se ha hablado con nadie: la conversación es el
        // último paso del tutorial.
        yield return ShowTapWordStep();

        // ── Paso 3 — cuaderno de palabras (botón P, pos (140,420)) ──
        ShowStep(
            LangText.Pick("Ahora pulsa aquí", "Maintenant, appuie ici", "Now tap here", "Ora tocca qui", "Ara toca aquí"),
            new Vector2(0f, 0f), new Vector2(320, 780), new Vector2(420, 150),
            new Vector2(140, 560), vertical: true);
        yield return WaitForPokeWorldOpen();

        // Con el cuaderno ya abierto y la palabra dentro, se explica qué es.
        // Antes esto se decía ANTES de abrirlo, hablando de una lista que el
        // jugador todavía no había visto.
        ShowCard(LangText.Pick(
            "Aquí tienes las palabras que has capturado, con su traducción.\nVuelve cuando quieras: no se pierden.",
            "Voici les mots que tu as capturés, avec leur traduction.\nReviens quand tu veux : ils ne se perdent pas.",
            "Here are the words you have captured, with their translation.\nCome back whenever you like — they are not lost.",
            "Ecco le parole che hai catturato, con la traduzione.\nTorna quando vuoi: non si perdono.",
            "Aquí tens les paraules que has capturat, amb la traducció.\nTorna quan vulguis: no es perden."),
            dim: false);
        // Abajo del todo y sin velo: la lista está detrás y no se puede tapar
        // ni atenuar — es lo que el jugador tiene que mirar.
        cardGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -620);
        yield return new WaitForSecondsRealtime(5f);

        // ── Paso 4 — cerrar el cuaderno (botón X, en MobileControlsSetup: pos (-80,420)) ──
        ShowStep(
            LangText.Pick("Pulsa aquí para cerrar el cuaderno de palabras", "Appuie ici pour fermer le carnet de mots", "Tap here to close the word notebook", "Tocca qui per chiudere il taccuino", "Toca aquí per tancar la llibreta de paraules"),
            new Vector2(1f, 0f), new Vector2(-260, 780), new Vector2(420, 150),
            new Vector2(-80, 560), vertical: true);
        yield return WaitForPokeWorldClose();

        var lvl = LevelManager.Instance?.CurrentLevel;

        // ── Paso 5 — cómo se puntúa ──
        //
        // Va ANTES del paso de la pista, que dice "te cuesta una estrella":
        // esa frase no significa nada para quien todavía no sabe que hay
        // estrellas ni cuántas. Y sin esto el jugador cree que las tres cosas
        // del brief son obligatorias y que dejarse una es fallar el nivel.
        ShowCard(LangText.Pick(
            "Cada nivel se puede acabar con 1, 2 o 3 estrellas.\nLa primera es resolver el caso. Las otras dos son retos extra: puedes dejarlas pasar.",
            "Chaque niveau peut se finir avec 1, 2 ou 3 étoiles.\nLa première, c'est résoudre l'affaire. Les deux autres sont des défis en plus : tu peux les laisser.",
            "Every level can end with 1, 2 or 3 stars.\nThe first is solving the case. The other two are extra challenges — you can let them go.",
            "Ogni livello può finire con 1, 2 o 3 stelle.\nLa prima è risolvere il caso. Le altre due sono sfide in più: puoi lasciarle perdere.",
            "Cada nivell es pot acabar amb 1, 2 o 3 estrelles.\nLa primera és resoldre el cas. Les altres dues són reptes extra: les pots deixar passar."));
        yield return new WaitForSecondsRealtime(6.5f);

        // ── Paso 6 — el botón de pista (LevelHUD, esquina inferior
        // izquierda: pos (24,28) con pivote (0,0), o sea centro en (144,60)) ──
        //
        // Solo si el nivel tiene pistas. Va ANTES de "hablar" porque ese paso
        // termina con el diálogo abierto tapando la pantalla, y ya no habría
        // dónde enseñar esto.
        //
        // NO espera a que se pulse: usar una pista cuesta una estrella, y
        // obligar al jugador a gastarla para pasar el tutorial sería cobrarle
        // por aprender dónde está el botón.
        if (lvl != null && lvl.hintAvailable && lvl.hints != null && lvl.hints.Count > 0)
        {
            ShowStep(
                LangText.Pick("¿Atascado? Aquí tienes una pista.\nTe cuesta una estrella.",
                              "Bloqué ? Ici, un indice.\nÇa coûte une étoile.",
                              "Stuck? Here is a hint.\nIt costs you a star.",
                              "Bloccato? Qui trovi un indizio.\nTi costa una stella.",
                              "Encallat? Aquí tens una pista.\nEt costa una estrella."),
                new Vector2(0f, 0f), new Vector2(330, 340), new Vector2(440, 170),
                new Vector2(144, 150), vertical: true);
            yield return new WaitForSecondsRealtime(4.5f);
        }

        // ── Paso 7 — cómo se cierra un caso ──
        //
        // El único paso que no señala un botón: explica una regla del juego,
        // no la interfaz. Hace falta porque resolver el caso mentalmente no lo
        // cierra — el agente no detiene a nadie hasta que le dan un nombre, y
        // nada en pantalla dice que haya que ir a buscarle. Sin esto el
        // jugador sabe quién fue y se queda dando vueltas por el salón.
        if (lvl != null && lvl.detectiveMode)
        {
            ShowCard(LangText.Pick(
                "Cuando sepas quién fue, busca al agente de policía y revélale lo que has descubierto.\nÉl hará la detención.",
                "Quand tu sauras qui c'est, va voir l'agent de police et dis-lui ce que tu as découvert.\nC'est lui qui arrête.",
                "When you know who did it, find the police officer and tell him what you found out.\nHe makes the arrest.",
                "Quando saprai chi è stato, cerca l'agente di polizia e digli quello che hai scoperto.\nSarà lui ad arrestarlo.",
                "Quan sàpigues qui va ser, busca l'agent de policia i revela-li el que has descobert.\nEll farà la detenció."));
            yield return new WaitForSecondsRealtime(6f);
        }

        // ── Paso 8 — hablar (botón E, en MobileControlsSetup: pos (140,260)) ──
        ShowStep(
            LangText.Pick("Acércate a alguien y pulsa aquí para hablar", "Approche-toi de quelqu'un et appuie ici pour parler", "Walk up to someone and tap here to talk", "Avvicinati a qualcuno e tocca qui per parlare", "Acosta't a algú i toca aquí per parlar"),
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
        skipTxt.text = LangText.Pick("Saltar X", "Passer X", "Skip X", "Salta X", "Saltar X");
        SetVisible(true);

        // El idioma que se aprende va interpolado, no escrito: este mismo
        // panel se usa para francés, inglés e italiano, y decía "en francés"
        // en los tres.
        string tgt = GameSettings.TargetLanguageName;
        ShowStep(
            LangText.Pick(
                $"Esta es la traducción de lo que dijiste. Algunas palabras clave pueden estar ocultas — ¡acuérdate de ellas! Graba tu voz diciéndola en {tgt}.",
                "Voici la traduction de ce que tu as dit. Certains mots clés peuvent être cachés — souviens-t'en ! Enregistre ta voix en le disant en français.",
                "This is the translation of what you said. Some key words may be hidden — remember them! Record your voice saying it in English.",
                "Questa è la traduzione di ciò che hai detto. Alcune parole chiave possono essere nascoste — ricordale! Registra la tua voce dicendola in italiano.",
                "Aquesta és la traducció del que has dit. Algunes paraules clau poden estar amagades — recorda-les! Grava la teva veu dient-la en català."),
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

        skipTxt.text = LangText.Pick("Saltar X", "Passer X", "Skip X", "Salta X", "Saltar X");
        SetVisible(true);
        cardText.text = LangText.Pick(
            "En este nivel puedes cambiar de zona — acércate al borde y pulsa E",
            "Dans ce niveau tu peux changer de zone — approche-toi du bord et appuie sur E",
            "In this level you can change area — go to the edge and press E",
            "In questo livello puoi cambiare zona — avvicinati al bordo e premi E",
            "En aquest nivell pots canviar de zona — acosta't a la vora i prem E");

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
        if (wordChip != null) wordChip.SetActive(false);
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
        if (dimGO != null) dimGO.SetActive(true);   // por si el paso anterior lo quitó
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

    // ── Palabra de prueba ────────────────────────────────────────────────
    //
    // Enseña la mecánica de capturar tocando una palabra, que es la única que
    // el jugador no descubre solo: las demás (moverse, hablar, el cuaderno)
    // tienen un botón visible, y esta no — son palabras dentro de una frase.
    //
    // Espera al toque de verdad, como el resto de pasos del tutorial: no
    // avanza por tiempo.
    IEnumerator ShowTapWordStep()
    {
        var lvl   = LevelManager.Instance?.CurrentLevel;
        var words = lvl?.ActiveTargetWords;
        if (words == null || words.Count == 0) yield break;   // nivel sin vocabulario: se salta

        string target  = words[0].french;    // "french" es el nombre viejo del campo: es el idioma OBJETIVO
        string spanish = words[0].spanish;

        ShowCard(LangText.Pick(
            "Las palabras en dorado se pueden capturar.\nToca esta para probar:",
            "Les mots en doré peuvent être capturés.\nTouche celui-ci pour essayer :",
            "Words in gold can be captured.\nTap this one to try:",
            "Le parole in oro si possono catturare.\nTocca questa per provare:",
            "Les paraules en daurat es poden capturar.\nToca aquesta per provar:"));

        wordChip.SetActive(true);
        wordChipLabel.text = target;
        wordChipTapped = false;

        // La flecha señala la palabra, no un botón de la interfaz.
        arrowRT.anchorMin = arrowRT.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRT.pivot     = new Vector2(0.5f, 0.5f);
        arrowGlyph.text   = "↓";
        arrowBasePos      = new Vector2(0, -60);
        StopCoroutine(nameof(BounceArrow));
        StartCoroutine(nameof(BounceArrow));

        while (!wordChipTapped) yield return null;

        // Captura real: entra en el cuaderno y sale la tarjeta de traducción
        // de siempre, la misma que verá en el juego.
        FindObjectOfType<WordSystem>()?.Capture(target, spanish);

        wordChip.SetActive(false);

        // La tarjeta de traducción se dibuja en 410 y el tutorial en 470, así
        // que quedaría detrás del velo. Se aparta el tutorial entero mientras
        // se ve: es la recompensa del paso y tiene que verse limpia.
        SetVisible(false);
        yield return new WaitForSecondsRealtime(2.4f);
        SetVisible(true);
    }

    // Paso sin flecha, para lo que no está en ningún botón. La tarjeta va al
    // centro y el glifo se vacía: dejar la flecha apuntando a una esquina
    // cualquiera haría creer que allí hay algo que pulsar.
    // dim=false deja ver lo que hay DEBAJO sin atenuar. Hace falta para el
    // paso del cuaderno: decir "aquí están tus palabras" mientras la lista se
    // ve apagada al 55% es justo lo contrario de lo que se quiere.
    void ShowCard(string text, bool dim = true)
    {
        if (dimGO != null) dimGO.SetActive(dim);
        cardText.text = text;

        var cardRT = cardGO.GetComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.anchoredPosition = new Vector2(0, 120);
        cardRT.sizeDelta = new Vector2(640, 240);

        StopCoroutine(nameof(BounceArrow));
        arrowGlyph.text = "";
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

        // Palabra de prueba, en el centro y bajo la tarjeta. Se pinta con los
        // mismos colores que una palabra capturable del diálogo para que el
        // jugador reconozca el patrón después, dentro del juego.
        wordChip = new GameObject("WordChip", typeof(RectTransform), typeof(Image));
        wordChip.transform.SetParent(root.transform, false);
        var chipRT = wordChip.GetComponent<RectTransform>();
        chipRT.anchorMin = chipRT.anchorMax = new Vector2(0.5f, 0.5f);
        chipRT.pivot     = new Vector2(0.5f, 0.5f);
        chipRT.anchoredPosition = new Vector2(0, -160);
        chipRT.sizeDelta = new Vector2(420, 110);
        var chipImg = wordChip.GetComponent<Image>();
        chipImg.sprite = RoundedRectSprite.GetSprite();
        chipImg.type   = Image.Type.Sliced;
        chipImg.color  = new Color(0.051f, 0.122f, 0.208f, 0.98f);

        var chipLblGO = new GameObject("Label", typeof(RectTransform));
        chipLblGO.transform.SetParent(wordChip.transform, false);
        var clRT = chipLblGO.GetComponent<RectTransform>();
        clRT.anchorMin = Vector2.zero; clRT.anchorMax = Vector2.one;
        clRT.offsetMin = Vector2.zero; clRT.offsetMax = Vector2.zero;
        wordChipLabel = chipLblGO.AddComponent<TextMeshProUGUI>();
        wordChipLabel.fontSize  = 44;
        wordChipLabel.color     = Gold;
        wordChipLabel.alignment = TextAlignmentOptions.Center;
        wordChipLabel.fontStyle = FontStyles.Bold;
        wordChipLabel.raycastTarget = false;

        var chipBtn = wordChip.AddComponent<Button>();
        chipBtn.targetGraphic = chipImg;
        chipBtn.onClick.AddListener(() => wordChipTapped = true);
        wordChip.SetActive(false);

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
