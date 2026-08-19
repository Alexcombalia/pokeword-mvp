using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Turno de 3 pasos del modo débutant: el jugador ya habló en español (el
// servidor tradujo eso a playerMessageTranslation, ver DialogueSystem) y,
// antes de revelar lo que responde el NPC, debe LEER esa traducción en voz
// alta en francés — con algunas palabras clave ocultas (PhraseMasker) y
// comparación tolerante de lo que dijo (TextMatcher). Sin penalización por
// fallar la lectura: reintento libre, y tras 3 fallos aparece un botón para
// saltar ESE turno (LevelManager.UseSkip cuenta los saltos de todo el nivel;
// 3+ saltos en el mismo nivel sí penaliza -1 estrella, ver LevelManager).
//
// Overlay a pantalla completa y opaco a propósito: deja inequívoco que este
// es un paso aparte de la conversación normal (cumple el requisito de que
// "dilo en español / léelo en francés / dilo en francés" se vean como 3
// momentos distintos), y bloquea el raycast hacia el botón de micro de
// DialogueSystem debajo — nunca coexisten dos grabaciones de audio a la vez.
public class BeginnerTurnController : MonoBehaviour
{
    public static BeginnerTurnController Instance { get; private set; }

    // Para que GuidedTutorial pueda esperar (polling) a que el jugador
    // termine de verdad este turno (por éxito o por saltar), en vez de solo
    // el primer toque — mismo patrón que PokeWorldController.IsOpen().
    public bool IsOpen => root != null && root.activeSelf;

    // Para que GuidedTutorial pueda quitarse de encima en cuanto el jugador
    // empieza a grabar — si se queda hasta que el turno entero termine, la
    // tarjeta del tutorial tapa la frase que el jugador necesita leer.
    public bool IsRecording => recorder != null && recorder.IsRecording;

    static readonly Color GoldInk     = new Color(0.612f, 0.420f, 0.039f, 1f); // #9C6B0A
    static readonly Color InkBrown    = new Color(0.235f, 0.161f, 0.098f, 1f); // #3C291A
    // La tinta oscura no se leía bien sobre el pergamino (mismo problema ya
    // corregido en WordCapturePopup.esText) — la frase que hay que leer en
    // voz alta es lo más importante de esta pantalla, así que aquí se
    // prioriza contraste máximo con blanco cálido en vez de tinta oscura.
    static readonly Color PhraseHighlight = new Color(0.98f, 0.96f, 0.90f, 1f);
    static readonly Color FeedbackBad = new Color(0.55f, 0.15f, 0.10f, 1f);
    static readonly Color FeedbackOk  = new Color(0.12f, 0.45f, 0.18f, 1f);
    static readonly Color RecordIdle  = new Color(0.051f, 0.122f, 0.208f, 0.92f);
    static readonly Color RecordingA  = new Color(0.75f, 0.1f, 0.1f, 0.95f);
    static readonly Color RecordingB  = new Color(1f, 0.25f, 0.25f, 0.95f);

    const int MaxAttemptsBeforeSkip = 3;

    GameObject root;
    TMP_Text   headerText;
    TMP_Text   phraseText;
    TMP_Text   phoneticText;
    TMP_Text   attemptText;
    TMP_Text   feedbackText;
    GameObject recordBtnGO;
    TMP_Text   recordBtnLabel;
    Image      recordBtnImg;
    GameObject skipBtnGO;
    TMP_Text   skipBtnLabel;
    GameObject playBtnGO;
    TMP_Text   playBtnLabel;
    Image      playBtnImg;
    Image      playBtnIcon;

    VoiceRecorder recorder;
    PhraseTTSClient ttsClient;
    string targetPhrase;
    int attemptCount;
    bool everOpened;
    float recordStartTime;
    Action<Dictionary<string, (string fr, string es)>> onDone;

    // Cachea el audio del turno actual — pulsar el botón varias veces para
    // la misma frase no debe volver a pedirlo al servidor cada vez.
    string cachedAudioBase64;
    string cachedForPhrase;
    bool fetchingAudio;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        recorder = gameObject.AddComponent<VoiceRecorder>();
        recorder.OnRecordingStarted += OnRecordingConfirmed;
        recorder.OnRecordingFailed  += _ => HandleAttemptFailed(technical: true);

        ttsClient = gameObject.AddComponent<PhraseTTSClient>();

        BuildUI();
        root.SetActive(false);
    }

    void Update()
    {
        if (root == null || !root.activeSelf) return;
        if (recorder != null && recorder.IsRecording)
        {
            float elapsed = Time.time - recordStartTime;
            recordBtnLabel.text = $"{elapsed:F0}s";
            recordBtnImg.color  = Color.Lerp(RecordingA, RecordingB, Mathf.PingPong(Time.time * 2f, 1f));
        }
    }

    // phonetic: guía de pronunciación en español (ver playerMessagePhonetic
    // en el servidor) — se muestra desde el primer intento, no es una "vía
    // de escape" que haya que ganarse como el botón Saltar, es una ayuda
    // para aprender a decirlo bien. Null/"" (el servidor no siempre la manda)
    // la oculta sin más, nunca bloquea el turno.
    public void RunTurn(string phrase, string phonetic, Action<Dictionary<string, (string fr, string es)>> onDoneCallback)
    {
        targetPhrase = phrase;
        onDone       = onDoneCallback;
        attemptCount = 0;

        var level       = LevelManager.Instance?.CurrentLevel;
        int levelId      = level?.id ?? 1;
        var targetWords  = level?.ActiveTargetWords ?? new List<TargetWord>();
        string masked    = PhraseMasker.Mask(phrase, targetWords, levelId);

        bool es = GameSettings.IsBasic;
        // Decía "EN FRANCÉS" a fuego, incluso jugando en inglés o italiano — y es
        // la instrucción más importante de la pantalla, la que dice qué hacer.
        headerText.text = es
            ? "AHORA LÉELO EN " + GameSettings.TargetLanguageName.ToUpper()
            : "MAINTENANT LIS-LE EN FRANÇAIS";
        phraseText.text = masked;

        if (!string.IsNullOrEmpty(phonetic))
        {
            phoneticText.text = es ? $"Se pronuncia: {phonetic}" : $"Ça se prononce : {phonetic}";
            phoneticText.gameObject.SetActive(true);
        }
        else
        {
            phoneticText.gameObject.SetActive(false);
        }

        attemptText.text = "";
        feedbackText.text = "";
        skipBtnGO.SetActive(false);
        ResetRecordButton();

        cachedAudioBase64 = null;
        cachedForPhrase   = null;
        fetchingAudio     = false;
        ResetPlayButton();

        root.SetActive(true);

        if (!everOpened)
        {
            everOpened = true;
            GuidedTutorial.Instance?.ExplainBeginnerTurnOnce();
        }
    }

    void OnRecordButtonClicked()
    {
        if (recorder == null) return;

        if (!recorder.IsRecording)
        {
            recorder.StartRecording();
        }
        else
        {
            recordBtnLabel.text = "...";
            recordBtnImg.color  = new Color(0.2f, 0.2f, 0.2f, 0.85f);
            // languageHint "fr": aquí no hay ambigüedad — el jugador está
            // leyendo una frase francesa concreta, así que se fija el
            // idioma sin alternativa (evita que un francés con acento se
            // detecte como español y salga transcrito irreconocible).
            recorder.StopAndTranscribe(
                onResult: HandleTranscription,
                onError: _ => HandleAttemptFailed(technical: true),
                languageHint: GameSettings.TargetLanguage
            );
        }
    }

    // Botón "▶" — reproduce cómo suena la frase objetivo en francés (TTS del
    // servidor, /voice/synthesize), independiente de la grabación del
    // jugador. Cachea el audio para no volver a pedirlo si se pulsa varias
    // veces seguidas en el mismo turno (ver cachedAudioBase64).
    void OnPlayPronunciationClicked()
    {
        if (fetchingAudio) return;

        if (cachedAudioBase64 != null && cachedForPhrase == targetPhrase)
        {
            NPCVoicePlayer.Instance?.PlayVoice(cachedAudioBase64);
            return;
        }

        fetchingAudio = true;
        ShowPlayButtonLoading();
        string phraseForAudio = targetPhrase;
        StartCoroutine(ttsClient.FetchAudio(phraseForAudio,
            onAudioBase64: audio =>
            {
                fetchingAudio = false;
                ResetPlayButton();
                // La frase pudo cambiar (nuevo turno) mientras llegaba la
                // respuesta — no cachear/reproducir un audio que ya no toca.
                if (phraseForAudio != targetPhrase) return;
                cachedAudioBase64 = audio;
                cachedForPhrase   = phraseForAudio;
                NPCVoicePlayer.Instance?.PlayVoice(audio);
            },
            onError: _ =>
            {
                fetchingAudio = false;
                ResetPlayButton();
            }));
    }

    void ResetPlayButton()
    {
        if (playBtnLabel != null) { playBtnLabel.text = ""; playBtnLabel.gameObject.SetActive(false); }
        if (playBtnIcon  != null) playBtnIcon.gameObject.SetActive(true);
    }

    void ShowPlayButtonLoading()
    {
        if (playBtnIcon  != null) playBtnIcon.gameObject.SetActive(false);
        if (playBtnLabel != null) { playBtnLabel.text = "..."; playBtnLabel.gameObject.SetActive(true); }
    }

    void HandleTranscription(string spokenText)
    {
        bool es = GameSettings.IsBasic;
        if (TextMatcher.IsCloseEnough(spokenText, targetPhrase))
        {
            feedbackText.color = FeedbackOk;
            feedbackText.text  = es ? "¡Bien dicho!" : "Bien dit !";
            FinishTurn();
        }
        else
        {
            HandleAttemptFailed(technical: false);
        }
    }

    // technical=true: fallo de grabación (audio muy corto, sin permiso de
    // micro, sin transcripción del servidor). SIEMPRE cuenta como intento
    // igual que un fallo por pronunciación — antes los fallos técnicos no
    // sumaban, y si el reconocimiento de voz fallaba varias veces seguidas
    // (mala conexión, ruido, servidor lento) el botón Saltar nunca llegaba a
    // aparecer y el jugador se quedaba bloqueado sin ninguna salida. Es más
    // importante garantizar una salida siempre disponible que "proteger" al
    // jugador de perder un intento por un fallo que no es culpa suya — el
    // mensaje sigue distinguiendo el motivo, pero el contador no.
    void HandleAttemptFailed(bool technical)
    {
        bool es = GameSettings.IsBasic;
        ResetRecordButton();

        attemptCount++;
        attemptText.text = es ? $"Intento {attemptCount}/{MaxAttemptsBeforeSkip}" : $"Essai {attemptCount}/{MaxAttemptsBeforeSkip}";

        feedbackText.color = FeedbackBad;
        feedbackText.text = technical
            ? (es ? "No se ha oído bien — inténtalo otra vez" : "Mal entendu — réessaie")
            : (es ? "Inténtalo de nuevo" : "Réessaie");

        if (attemptCount >= MaxAttemptsBeforeSkip)
        {
            RefreshSkipLabel();
            skipBtnGO.SetActive(true);
        }
    }

    // Saltar es gratis las dos primeras veces del nivel y a la tercera cuesta
    // una estrella (LevelManager.SkipCount >= 3, ver EndLevel). El botón decía
    // solo "SALTAR", así que el jugador perdía una estrella sin que nadie se
    // lo hubiera advertido — y el botón de pista, que cuesta lo mismo, sí lo
    // avisa en su etiqueta. Se avisa cuando el SIGUIENTE salto es el que cobra.
    void RefreshSkipLabel()
    {
        if (skipBtnLabel == null) return;
        bool es = GameSettings.IsBasic;
        int used = LevelManager.Instance != null ? LevelManager.Instance.SkipCount : 0;
        bool nextOneCosts = used >= LevelManager.SkipPenaltyThreshold - 1;

        skipBtnLabel.text = nextOneCosts
            ? (es ? "SALTAR  (-1 estrella)" : "PASSER  (-1 étoile)")
            : (es ? "SALTAR" : "PASSER");
    }

    void OnSkipClicked()
    {
        LevelManager.Instance?.UseSkip();
        FinishTurn();
    }

    void FinishTurn()
    {
        DynamicWordHighlighter.ProcessText(targetPhrase, out var capturable);
        root.SetActive(false);
        onDone?.Invoke(capturable);
    }

    void ResetRecordButton()
    {
        if (recordBtnLabel != null) recordBtnLabel.text  = GameSettings.IsBasic ? "GRABAR" : "ENREGISTRER";
        if (recordBtnImg   != null) recordBtnImg.color   = RecordIdle;
    }

    void OnRecordingConfirmed()
    {
        recordStartTime = Time.time;
        recordBtnLabel.text = "0s";
        recordBtnImg.color  = RecordingA;
    }

    void BuildUI()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        // Por encima de SpeechCaptionUI (405) / DialogueSystem (400) — este
        // panel tapa la conversación mientras dura el gate de lectura. Por
        // debajo de CoachMarks (450) / GuidedTutorial (470) para que el
        // tutorial pueda seguir señalando por encima si hace falta.
        canvas.sortingOrder = 420;

        var scaler                 = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        root = new GameObject("Root", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(transform, false);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero; rootRT.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0.012f, 0.031f, 0.059f, 0.94f);

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(620, 760);
        var cardImg = card.GetComponent<Image>();
        cardImg.sprite = ParchmentSprite.GetPaper(); cardImg.type = Image.Type.Sliced; cardImg.color = Color.white;

        var borderGO = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(card.transform, false);
        borderGO.transform.SetAsFirstSibling();
        var bRT = borderGO.GetComponent<RectTransform>();
        bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
        bRT.offsetMin = new Vector2(-6, -6); bRT.offsetMax = new Vector2(6, 6);
        var bImg = borderGO.GetComponent<Image>();
        bImg.sprite = ParchmentSprite.GetInk(); bImg.type = Image.Type.Sliced; bImg.color = Color.white;

        headerText = MakeLabel(card.transform, "Header", "",   // lo pone RunTurn según el idioma
            new Vector2(0, 300), new Vector2(560, 60), 26, GoldInk, TextAlignmentOptions.Center, FontStyles.Bold);

        phraseText = MakeLabel(card.transform, "Phrase", "",
            new Vector2(0, 200), new Vector2(560, 110), 32, PhraseHighlight, TextAlignmentOptions.Center, FontStyles.Bold);
        phraseText.enableWordWrapping = true;
        phraseText.enableAutoSizing   = true;
        phraseText.fontSizeMin = 20; phraseText.fontSizeMax = 32;

        // Botón "▶" — escucha cómo suena la frase en francés (TTS del
        // servidor, independiente de la grabación del jugador). Fila propia,
        // centrada, entre la frase y la guía de pronunciación — antes era una
        // insignia pequeña en la esquina y apenas se veía.
        playBtnGO = new GameObject("PlayBtn", typeof(RectTransform), typeof(Image));
        playBtnGO.transform.SetParent(card.transform, false);
        var pbRT = playBtnGO.GetComponent<RectTransform>();
        pbRT.anchorMin = pbRT.anchorMax = new Vector2(0.5f, 0.5f);
        pbRT.pivot     = new Vector2(0.5f, 0.5f);
        pbRT.anchoredPosition = new Vector2(0, 105);
        pbRT.sizeDelta = new Vector2(84, 84);
        playBtnImg = playBtnGO.GetComponent<Image>();
        playBtnImg.color = RecordIdle;

        var pbIconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        pbIconGO.transform.SetParent(playBtnGO.transform, false);
        var pbIconRT = pbIconGO.GetComponent<RectTransform>();
        pbIconRT.anchorMin = Vector2.zero; pbIconRT.anchorMax = Vector2.one;
        pbIconRT.offsetMin = new Vector2(22, 18); pbIconRT.offsetMax = new Vector2(-18, -18);
        playBtnIcon = pbIconGO.GetComponent<Image>();
        playBtnIcon.sprite = PlayIcon.GetSprite();
        playBtnIcon.color  = PhraseHighlight;
        playBtnIcon.raycastTarget = false;

        var pbLblGO = new GameObject("Label", typeof(RectTransform));
        pbLblGO.transform.SetParent(playBtnGO.transform, false);
        var pbLblRT = pbLblGO.GetComponent<RectTransform>();
        pbLblRT.anchorMin = Vector2.zero; pbLblRT.anchorMax = Vector2.one;
        pbLblRT.anchoredPosition = Vector2.zero; pbLblRT.sizeDelta = Vector2.zero;
        playBtnLabel = pbLblGO.AddComponent<TextMeshProUGUI>();
        playBtnLabel.fontSize  = 26;
        playBtnLabel.color     = PhraseHighlight;
        playBtnLabel.alignment = TextAlignmentOptions.Center;
        playBtnLabel.fontStyle = FontStyles.Bold;
        playBtnLabel.raycastTarget = false;
        pbLblGO.SetActive(false);

        var playBtn = playBtnGO.AddComponent<Button>();
        playBtn.targetGraphic = playBtnImg;
        playBtn.onClick.AddListener(OnPlayPronunciationClicked);

        // Guía de pronunciación en español (playerMessagePhonetic) — desde
        // el primer intento, no se gana con fallos como el botón Saltar (ver
        // RunTurn). Se oculta sola si el servidor no la mandó esta vez.
        // Tamaño grande a propósito: casi tan importante de leer como la
        // propia frase francesa, con 20px quedaba demasiado pequeña.
        phoneticText = MakeLabel(card.transform, "Phonetic", "",
            new Vector2(0, 15), new Vector2(560, 70), 28, PhraseHighlight, TextAlignmentOptions.Center, FontStyles.Italic);
        phoneticText.enableWordWrapping = true;
        phoneticText.enableAutoSizing   = true;
        phoneticText.fontSizeMin = 20; phoneticText.fontSizeMax = 28;

        // Botón GRABAR — mismo patrón toca-graba/toca-para que
        // DialogueSystem.OnMicButtonClicked, pero con un VoiceRecorder
        // propio (nunca dos grabaciones activas a la vez, ver comentario de
        // clase sobre el overlay opaco bloqueando el raycast de abajo).
        recordBtnGO = new GameObject("RecordBtn", typeof(RectTransform), typeof(Image));
        recordBtnGO.transform.SetParent(card.transform, false);
        var rbRT = recordBtnGO.GetComponent<RectTransform>();
        rbRT.anchorMin = rbRT.anchorMax = new Vector2(0.5f, 0.5f);
        rbRT.pivot     = new Vector2(0.5f, 0.5f);
        rbRT.anchoredPosition = new Vector2(0, -70);
        rbRT.sizeDelta = new Vector2(220, 90);
        recordBtnImg = recordBtnGO.GetComponent<Image>();
        recordBtnImg.color = RecordIdle;
        var recLbl = new GameObject("Label", typeof(RectTransform));
        recLbl.transform.SetParent(recordBtnGO.transform, false);
        var rlRT = recLbl.GetComponent<RectTransform>();
        rlRT.anchorMin = Vector2.zero; rlRT.anchorMax = Vector2.one;
        rlRT.anchoredPosition = Vector2.zero; rlRT.sizeDelta = Vector2.zero;
        recordBtnLabel = recLbl.AddComponent<TextMeshProUGUI>();
        recordBtnLabel.text      = "GRABAR";
        recordBtnLabel.fontSize  = 26;
        recordBtnLabel.color     = Color.white;
        recordBtnLabel.alignment = TextAlignmentOptions.Center;
        recordBtnLabel.fontStyle = FontStyles.Bold;
        recordBtnLabel.raycastTarget = false;
        var recordBtn = recordBtnGO.AddComponent<Button>();
        recordBtn.targetGraphic = recordBtnImg;
        recordBtn.onClick.AddListener(OnRecordButtonClicked);

        attemptText = MakeLabel(card.transform, "Attempt", "",
            new Vector2(0, -145), new Vector2(400, 34), 20, InkBrown, TextAlignmentOptions.Center, FontStyles.Italic);

        feedbackText = MakeLabel(card.transform, "Feedback", "",
            new Vector2(0, -185), new Vector2(540, 40), 22, InkBrown, TextAlignmentOptions.Center, FontStyles.Bold);

        // Botón Saltar — oculto hasta el 3er fallo (ver HandleAttemptFailed).
        // No sustituye a Grabar: ambos quedan visibles, el jugador decide si
        // sigue intentando o salta este turno concreto.
        skipBtnGO = new GameObject("SkipBtn", typeof(RectTransform), typeof(Image));
        skipBtnGO.transform.SetParent(card.transform, false);
        var skRT = skipBtnGO.GetComponent<RectTransform>();
        skRT.anchorMin = skRT.anchorMax = new Vector2(0.5f, 0.5f);
        skRT.pivot     = new Vector2(0.5f, 0.5f);
        skRT.anchoredPosition = new Vector2(0, -270);
        skRT.sizeDelta = new Vector2(240, 60);
        var skImg = skipBtnGO.GetComponent<Image>();
        skImg.color = new Color(0.5f, 0.15f, 0.1f, 0.9f);
        var skLbl = new GameObject("Label", typeof(RectTransform));
        skLbl.transform.SetParent(skipBtnGO.transform, false);
        var slRT = skLbl.GetComponent<RectTransform>();
        slRT.anchorMin = Vector2.zero; slRT.anchorMax = Vector2.one;
        slRT.anchoredPosition = Vector2.zero; slRT.sizeDelta = Vector2.zero;
        var skTxt = skLbl.AddComponent<TextMeshProUGUI>();
        skTxt.text      = GameSettings.IsBasic ? "SALTAR" : "PASSER";
        skipBtnLabel    = skTxt;
        skTxt.fontSize  = 24;
        skTxt.color     = Color.white;
        skTxt.alignment = TextAlignmentOptions.Center;
        skTxt.fontStyle = FontStyles.Bold;
        skTxt.raycastTarget = false;
        var skipBtn = skipBtnGO.AddComponent<Button>();
        skipBtn.targetGraphic = skImg;
        skipBtn.onClick.AddListener(OnSkipClicked);
        skipBtnGO.SetActive(false);
    }

    static TMP_Text MakeLabel(Transform parent, string name, string text, Vector2 pos, Vector2 size,
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
}
