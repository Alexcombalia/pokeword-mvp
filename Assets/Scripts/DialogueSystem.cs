using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [Header("Link Click Handler (on DialogueText)")]
    public TMPLinkClickHandler linkClickHandler;

    private WordSystem wordSystem;

    // Diccionario de capturas: linkId -> (french, spanish)
    private Dictionary<string, (string fr, string es)> currentCapturable = new Dictionary<string, (string fr, string es)>();

    public GameObject dialoguePanel;     // DialoguePanel
    public TMP_Text dialogueText;        // DialogueText
    public TMP_InputField dialogueInput;
    public Button sendButton;
    public Button acceptJobButton;
    public Button declineJobButton;
    public Button buyBreadButton;

    private Queue<string> lines = new Queue<string>();
    private bool isOpen = false;
    private ResourceSystem resources;
    [Header("Chat UI (optional)")]
    private BakerChatClient bakerClient;
    private bool chatMode = false;
    private BakerShop currentBakerShop;
    private JobNPC currentJobNPC;
    private Auberge currentAuberge;
    private HenryChatClient henryClient;
    private bool henryMode = false;
    private JacquesChatClient jacquesClient;
    private bool jacquesMode = false;
    private GenericNPCChatClient genericClient;
    private string genericNpcId;
    private string genericPersonalityId;
    private string genericDisplayName;
    private string genericGender;
    private bool genericMode = false;
    private DetectiveChatClient detectiveClient;
    private PhraseTTSClient greetingTTS;
    private string detectiveDisplayName;
    private bool detectiveMode = false;
    private bool shopMode    = false;

    // Modo "guiones sobre cada personaje" (SpeechCaptionUI) — activo para
    // Jacques y NPCs genéricos, que son los que usa el flujo de niveles.
    // Los modos viejos del RPG (panadero, trabajos, dormir, policía, tienda)
    // se quedan con el panel de chat original: no forman parte del flujo de
    // niveles actual, tocarlos ahora no aporta y cuesta tiempo real.
    private bool captionMode = false;

    private Button inlineCloseBtn;
    private Button nextBtn;
    private TMP_Text nextBtnLabel;

    private VoiceRecorder voiceRecorder;
    private Button micButton;
    private Image micBtnImg;
    private TMP_Text micBtnLabel;
    private float recordStartTime;
    private static readonly Color MicRecordingColorA = new Color(0.75f, 0.1f, 0.1f, 0.95f);
    private static readonly Color MicRecordingColorB = new Color(1f, 0.25f, 0.25f, 0.95f);

    // Keyboard avoidance
    private RectTransform panelRT;
    private Vector2 panelOriginalPos;
    private bool panelPosSaved;
    private bool isKeyboardUp;

    private void Awake()
    {
        resources = FindObjectOfType<ResourceSystem>();
        wordSystem = FindObjectOfType<WordSystem>();
        henryClient = FindObjectOfType<HenryChatClient>();

        if (linkClickHandler != null)
            linkClickHandler.OnLinkClicked += HandleLinkClicked;

        bakerClient = FindObjectOfType<BakerChatClient>();
        if (sendButton != null)
            sendButton.onClick.AddListener(SendChatMessage);

        // Keep dialogue above tutorial (300) and quiz (200) canvases
        var canvas = dialoguePanel != null
            ? dialoguePanel.GetComponentInParent<Canvas>()
            : GetComponent<Canvas>();
        if (canvas != null) canvas.sortingOrder = 400;

        // Letra más grande, forzada por código (sea cual sea el tamaño que
        // tuviera en el prefab) — pensado para que se lea bien en móvil y
        // para gente mayor. Sube también el área de toque de las palabras
        // capturables del chat, que escala con el tamaño de fuente.
        if (dialogueText != null)
        {
            dialogueText.fontSize = 34;
            dialogueText.enableAutoSizing = false;
        }
        if (dialogueInput != null)
        {
            if (dialogueInput.textComponent != null) dialogueInput.textComponent.fontSize = 28;
            if (dialogueInput.placeholder is TMP_Text placeholderTxt)
            {
                placeholderTxt.fontSize = 28;
                // El texto de ayuda del campo vive en el prefab GlobalUI y estaba
                // fijo en francés ("Écris ici..."). Se sobrescribe aquí porque un
                // prefab no puede saber en qué idioma se está jugando.
                placeholderTxt.text = LangText.Pick("Escribe aquí...", "Écris ici...", "Write here...", "Scrivi qui...", "Escriu aquí...");
            }
        }

        BuildCloseButton();
        BuildNextButton();
        voiceRecorder = gameObject.AddComponent<VoiceRecorder>();
        voiceRecorder.OnRecordingStarted += OnRecordingConfirmed;
        voiceRecorder.OnRecordingFailed  += OnRecordingFailed;
        BuildMicButton();

        if (dialogueInput != null)
        {
            dialogueInput.onSubmit.AddListener(_ => { if (chatMode) SendChatMessage(); });
            dialogueInput.onSelect.AddListener(_ => SetKeyboardAvoidance(true));
            dialogueInput.onDeselect.AddListener(_ => SetKeyboardAvoidance(false));
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                               UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Garantiza que al cambiar de escena no quede isOpen atascado en true
        if (isOpen) Close();
    }

    void ShowCloseButton() => inlineCloseBtn?.gameObject.SetActive(true);
    void HideNextButton()  => nextBtn?.gameObject.SetActive(false);

    void EnterCaptionMode()
    {
        captionMode = true;
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        var bg = dialoguePanel != null ? dialoguePanel.GetComponent<Image>() : null;
        if (bg != null) bg.enabled = false;
    }

    void ExitCaptionMode()
    {
        if (!captionMode) return;
        captionMode = false;
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        var bg = dialoguePanel != null ? dialoguePanel.GetComponent<Image>() : null;
        if (bg != null) bg.enabled = true;
        SpeechCaptionUI.Instance?.HideAll();
    }

    void BuildNextButton()
    {
        if (dialoguePanel == null) return;

        var go = new GameObject("NextBtn", typeof(RectTransform));
        go.transform.SetParent(dialoguePanel.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(1f, 0f);
        rt.anchorMax        = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-12f, 12f);
        rt.sizeDelta        = new Vector2(180f, 50f);

        var img   = go.AddComponent<Image>();
        img.color = new Color(1f, 0.82f, 0.4f, 1f); // gold

        var lbl = new GameObject("Label", typeof(RectTransform));
        lbl.transform.SetParent(go.transform, false);
        var lRT = lbl.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.anchoredPosition = Vector2.zero; lRT.sizeDelta = Vector2.zero;

        nextBtnLabel = lbl.AddComponent<TextMeshProUGUI>();
        nextBtnLabel.text      = "Siguiente >";
        nextBtnLabel.fontSize  = 20f;
        nextBtnLabel.color     = new Color(0.012f, 0.031f, 0.059f, 1f);
        nextBtnLabel.alignment = TextAlignmentOptions.Center;
        nextBtnLabel.fontStyle = FontStyles.Bold;
        nextBtnLabel.raycastTarget = false;

        nextBtn = go.AddComponent<Button>();
        nextBtn.targetGraphic = img;
        var cols = nextBtn.colors;
        cols.highlightedColor = new Color(1f, 0.92f, 0.55f, 1f);
        cols.pressedColor     = new Color(0.78f, 0.62f, 0.18f, 1f);
        nextBtn.colors = cols;
        nextBtn.onClick.AddListener(NextLine);
        go.SetActive(false);
    }

    void BuildMicButton()
    {
        if (dialoguePanel == null) return;

        var go = new GameObject("MicBtn", typeof(RectTransform));
        go.transform.SetParent(dialoguePanel.transform, false);

        // Anchored above the DialogueBox (which sits ~305 units tall from the bottom)
        // rather than at the absolute screen corner, and sized to be unmistakable —
        // this is the primary way to talk to an NPC, not a small secondary control.
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 325f);
        rt.sizeDelta        = new Vector2(160f, 160f);

        micBtnImg       = go.AddComponent<Image>();
        micBtnImg.color = new Color(0.051f, 0.122f, 0.208f, 0.92f);

        var lbl = new GameObject("Icon", typeof(RectTransform));
        lbl.transform.SetParent(go.transform, false);
        var lRT = lbl.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.anchoredPosition = Vector2.zero; lRT.sizeDelta = Vector2.zero;

        micBtnLabel           = lbl.AddComponent<TextMeshProUGUI>();
        micBtnLabel.text      = "AUDIO";
        micBtnLabel.fontSize  = 28f;
        micBtnLabel.alignment = TextAlignmentOptions.Center;
        micBtnLabel.raycastTarget = false;

        micButton = go.AddComponent<Button>();
        micButton.targetGraphic = micBtnImg;
        micButton.onClick.AddListener(OnMicButtonClicked);
        go.SetActive(false);
    }

    void OnRecordingConfirmed()
    {
        recordStartTime = Time.time;
        if (micBtnLabel != null) micBtnLabel.text = "0s";
        if (micBtnImg   != null) micBtnImg.color  = MicRecordingColorA;
    }

    void OnRecordingFailed(string reason)
    {
        Debug.LogWarning($"[DialogueSystem] Grabación fallida: {reason}");
        ResetMicButton();
    }

    void OnMicButtonClicked()
    {
        if (voiceRecorder == null) return;

        if (!voiceRecorder.IsRecording)
        {
            // Callar al NPC antes de abrir el micro (ver NPCVoicePlayer.Silence).
            NPCVoicePlayer.Instance?.Silence();
            // UI updates via OnRecordingStarted event once recording actually begins
            voiceRecorder.StartRecording();
        }
        else
        {
            micBtnLabel.text = "...";
            micBtnImg.color  = new Color(0.2f, 0.2f, 0.2f, 0.85f);
            voiceRecorder.StopAndTranscribe(
                onResult: text =>
                {
                    // Repetición de habla en modo avancé. Aquí el jugador habla
                    // directamente en el idioma objetivo, así que lo que cuenta
                    // es lo transcrito.
                    //
                    // En débutant este mismo micrófono se usa para hablar en
                    // ESPAÑOL (la frase se traduce y luego se lee en voz alta),
                    // y eso no es repetición del idioma que se aprende: la
                    // cuenta la lleva BeginnerTurnController al aceptar la
                    // lectura. Por eso el guardia de IsBasic.
                    if (!GameSettings.IsBasic)
                        LevelManager.Instance?.ReportSpoken(text, Time.time - recordStartTime);

                    if (dialogueInput != null)
                    {
                        dialogueInput.text = text;
                        SendChatMessage();
                    }
                    ResetMicButton();
                },
                onError: _ => ResetMicButton()
            );
        }
    }

    void ResetMicButton()
    {
        if (micBtnLabel != null) micBtnLabel.text = "AUDIO";
        if (micBtnImg   != null) micBtnImg.color  = new Color(0.051f, 0.122f, 0.208f, 0.92f);
    }

    void BuildCloseButton()
    {
        if (dialoguePanel == null) return;

        var go = new GameObject("CloseBtn", typeof(RectTransform));
        go.transform.SetParent(dialoguePanel.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(1f, 1f);
        rt.anchorMax        = new Vector2(1f, 1f);
        rt.pivot            = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-8f, -8f);
        rt.sizeDelta        = new Vector2(72f, 72f);

        var img   = go.AddComponent<Image>();
        img.color = new Color(0.10f, 0.05f, 0.05f, 0.90f);

        var lbl = new GameObject("X", typeof(RectTransform));
        lbl.transform.SetParent(go.transform, false);
        var lRT = lbl.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.anchoredPosition = Vector2.zero; lRT.sizeDelta = Vector2.zero;

        var txt = lbl.AddComponent<TextMeshProUGUI>();
        txt.text          = "X";
        txt.fontSize      = 46;
        txt.color         = Color.white;
        txt.alignment     = TextAlignmentOptions.Center;
        txt.fontStyle     = FontStyles.Bold;
        txt.raycastTarget = false;

        inlineCloseBtn = go.AddComponent<Button>();
        inlineCloseBtn.targetGraphic = img;
        var cols = inlineCloseBtn.colors;
        cols.pressedColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        inlineCloseBtn.colors = cols;
        inlineCloseBtn.onClick.AddListener(Close);
        go.SetActive(false);
    }


    private void Update()
    {
        if (!isOpen) return;

        if (chatMode && Input.GetKeyDown(KeyCode.Return))
            SendChatMessage();

        if (!chatMode && Input.GetKeyDown(KeyCode.Space))
            NextLine();

        if (Input.GetKeyDown(KeyCode.Escape) || MobileInput.ClosePressed)
            Close();

        // Mic button mirrors send button visibility
        if (micButton != null && sendButton != null)
            micButton.gameObject.SetActive(sendButton.gameObject.activeSelf);

        // While recording, show elapsed seconds + a pulsing color so it's obvious
        // something is happening even though no live transcription is shown
        if (voiceRecorder != null && voiceRecorder.IsRecording)
        {
            float elapsed = Time.time - recordStartTime;
            if (micBtnLabel != null) micBtnLabel.text = $"{elapsed:F0}s";
            if (micBtnImg   != null) micBtnImg.color  = Color.Lerp(MicRecordingColorA, MicRecordingColorB, Mathf.PingPong(Time.time * 2f, 1f));
        }

        // Fallback poll: onSelect/onDeselect (wired in Awake) is the primary trigger,
        // but if the field was already selected when the panel opened, or focus was
        // lost without a clean onDeselect (e.g. tapping Close while typing), make sure
        // state doesn't get stuck once the keyboard actually goes away.
        if (isKeyboardUp && !TouchScreenKeyboard.visible && (dialogueInput == null || !dialogueInput.isFocused))
            SetKeyboardAvoidance(false);
    }

    // Driven by the input field's onSelect/onDeselect events rather than polling the
    // OS-reported keyboard geometry — TouchScreenKeyboard.area/visible proved
    // unreliable across devices and emulators (BlueStacks included).
    private void SetKeyboardAvoidance(bool up)
    {
        if (up == isKeyboardUp) return;
        if (up && !chatMode) return;
        isKeyboardUp = up;

        if (dialoguePanel == null) return;
        if (panelRT == null) panelRT = dialoguePanel.GetComponent<RectTransform>();
        if (panelRT == null) return;

        if (!panelPosSaved)
        {
            panelOriginalPos = panelRT.anchoredPosition;
            panelPosSaved    = true;
        }

        if (up)
        {
            // Best-effort real keyboard height, falling back to a generous estimate
            // (native keyboards are typically 35-45% of screen height on phones) when
            // the OS hasn't reported anything usable yet.
            float kbH = TouchScreenKeyboard.area.height;
            if (kbH < 50f || kbH > Screen.height * 0.9f) kbH = Screen.height * 0.4f;

            var canvas = dialoguePanel.GetComponentInParent<Canvas>();
            float offset = canvas != null ? kbH / canvas.scaleFactor : kbH;
            panelRT.anchoredPosition = panelOriginalPos + new Vector2(0, offset);
        }
        else
        {
            panelRT.anchoredPosition = panelOriginalPos;
        }
    }

    public void OpenBakerChat()
    {
        gameObject.SetActive(true);
        if (dialoguePanel != null && dialoguePanel.transform.root != null)
            dialoguePanel.transform.root.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen = true;
        chatMode = true;

        if (resources != null) resources.DrainOnConversation();

        if (dialogueText != null)
            dialogueText.text = "Bonjour. Je suis le boulanger.\nAppuie sur B pour acheter du pain.";
        if (dialogueInput != null)
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueInput.text = "";
            // No auto-focus here — activating the field immediately pops the mobile
            // keyboard the instant a chat opens, before the player can read anything.
            // It only opens now when the player deliberately taps the field.
        }

        if (sendButton != null)
            sendButton.gameObject.SetActive(true);

   

        currentCapturable.Clear();

        if (bakerClient != null)
        {
            StartCoroutine(bakerClient.ResetMemory(
                onDone: () =>
                {
                    Debug.Log("Mémoire du boulanger réinitialisée.");
                },
                onError: (err) =>
                {
                    Debug.LogWarning("Erreur reset mémoire boulanger: " + err);
                }
            ));
        }

    }

    // In modo básico, appends the Spanish/French translation Gemini already returned
    // in parentheses right after the line — no-op outside basic mode or when the
    // server didn't send a translation (e.g. medium/full difficulty).
    private static string FormatLine(string speaker, string text, string translation)
    {
        if (GameSettings.IsBasic && !string.IsNullOrEmpty(translation))
            return speaker + ": " + text + " (" + translation + ")";
        return speaker + ": " + text;
    }

    private const string TranslateUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/translate";

    private string chatPlayerLine;
    private string chatNpcLine;
    private int    chatMessageSeq;
    private string pendingPlayerTranslation;

    private void RefreshChatDisplay()
    {
        if (dialogueText != null) dialogueText.text = chatPlayerLine + "\n\n" + chatNpcLine;
    }

    // Fires the moment the player sends a message — separate from the main NPC call
    // because it's a much smaller prompt (~1s vs ~3-4s for the full in-character
    // reply), so the player sees their own translation immediately instead of
    // waiting for the NPC to "finish speaking". `seq` guards against a late response
    // clobbering a newer message if the player sends again before this returns.
    [System.Serializable] private class TranslateRequest { public string text; public string targetLang; }

    private IEnumerator TranslatePlayerMessage(string msg, int seq)
    {
        string body = JsonUtility.ToJson(new TranslateRequest { text = msg, targetLang = GameSettings.TargetLanguage });

        using var request = new UnityWebRequest(TranslateUrl, "POST");
        request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (seq != chatMessageSeq) yield break; // a newer message was sent meanwhile
        if (request.result != UnityWebRequest.Result.Success) yield break;

        var resp = JsonUtility.FromJson<TranslateResponse>(request.downloadHandler.text);
        if (resp == null || !resp.ok || string.IsNullOrEmpty(resp.translation)) yield break;

        pendingPlayerTranslation = resp.translation;
        chatPlayerLine = FormatLine(LangText.Pick("Yo", "Toi", "Me", "Io", "Jo"), msg, pendingPlayerTranslation);
        RefreshChatDisplay();

        // Este endpoint ligero vuelve en ~1s (frente a los ~3-4s de la respuesta
        // completa del NPC), así que en modo guiones el jugador ve la traducción
        // al francés de su propia línea casi al instante en vez de esperar.
        if (captionMode) SpeechCaptionUI.Instance?.SetPlayerTranslation(pendingPlayerTranslation);
    }

    [System.Serializable] private class TranslateResponse { public bool ok; public string translation; }

    private void SendChatMessage()
    {
        if (!chatMode) return;
        if (dialogueInput == null) return;

        string msg = dialogueInput.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        dialogueInput.text = "";
        // No re-focus here — reactivating the field immediately pops the mobile
        // keyboard right back up after the player just closed it (via the native
        // "Done"/Enter action), which read as "my tap didn't work" and needed a
        // confusing second tap. Tapping the field again is how you type another message.
        dialogueInput.DeactivateInputField();

        int seq = ++chatMessageSeq;
        pendingPlayerTranslation = null;
        chatPlayerLine = LangText.Pick("Yo", "Toi", "Me", "Io", "Jo") + ": " + msg;

        // Fires in parallel with the NPC call below — much smaller prompt, returns
        // in ~1s instead of ~3-4s, so the player sees their own translation right
        // away instead of waiting for the NPC to finish "speaking".
        //
        // En modo detective NO se usa. Motivo: la respuesta del NPC ya trae esa
        // misma traducción (playerMessageTranslation) y en el idioma correcto,
        // así que esta llamada solo aportaba llegar antes — a cambio de ser una
        // SEGUNDA fuente de verdad para el mismo texto. Y en cuanto el juego
        // dejó de ser solo francés, las dos fuentes se separaron: esta traducía
        // al francés en partidas inglesas.
        //
        // Se gana coherencia y una llamada menos por turno; se pierde que la
        // traducción aparezca al instante en vez de con la respuesta.
        if (GameSettings.IsBasic && !detectiveMode)
            StartCoroutine(TranslatePlayerMessage(msg, seq));

        // -------------------------
        // GENERIC NPC
        // -------------------------
        if (genericMode)
        {
            SpeechCaptionUI.Instance?.ShowPlayerLine(msg, GameSettings.IsBasic);

            if (genericClient == null)
            {
                SpeechCaptionUI.Instance?.ShowNpcLine(genericDisplayName, "Erreur: client NPC introuvable.", null);
                return;
            }

            StartCoroutine(genericClient.SendMessage(
                msg, genericPersonalityId, genericNpcId, genericGender,
                (reply, replyTranslation, playerMessageTranslation, playerMessagePhonetic, objectiveComplete, checkpointsReached, audioBase64) =>
                {
                    if (seq != chatMessageSeq) return;
                    var processedReply = DynamicWordHighlighter.ProcessText(reply, out var generatedCapturable);
                    currentCapturable.Clear();
                    foreach (var kvp in generatedCapturable)
                        currentCapturable[kvp.Key] = kvp.Value;

                    void Reveal()
                    {
                        SpeechCaptionUI.Instance?.ShowNpcLine(genericDisplayName, processedReply, HandleLinkClicked);
                        SpeechCaptionUI.Instance?.ShowTranslation(GameSettings.IsBasic ? replyTranslation : null);
                        SpeechCaptionUI.Instance?.SetPlayerTranslation(GameSettings.IsBasic ? playerMessageTranslation : null, HandleLinkClicked);
                        // `reply` y no `processedReply`: el segundo lleva ya
                        // las etiquetas de color/enlace de TMP metidas.
                        NPCVoicePlayer.Instance?.PlayVoice(audioBase64, reply);

                        // Retos que ESTE NPC reporta como alcanzados — se
                        // suman al set del nivel (LevelManager), que puede
                        // llenarse desde varios NPCs distintos (nivel 3: uno
                        // por NPC) o desde cualquiera de varios (nivel 2).
                        if (checkpointsReached != null && checkpointsReached.Length > 0)
                            LevelManager.Instance?.ReportCheckpoints(checkpointsReached);
                    }

                    // Débutant + nivel activo + hay algo que leer: retiene la
                    // respuesta hasta que el jugador la "gane" leyéndola en
                    // francés (BeginnerTurnController). Avancé, o si por lo
                    // que sea el servidor no mandó traducción esta vez, se
                    // revela igual que hoy — nunca bloquea la conversación.
                    if (GameSettings.IsBasic && LevelManager.Instance != null && LevelManager.Instance.IsActive
                        && !string.IsNullOrEmpty(playerMessageTranslation))
                    {
                        BeginnerTurnController.Instance?.RunTurn(playerMessageTranslation, playerMessagePhonetic, capturableFromPhrase =>
                        {
                            foreach (var kvp in capturableFromPhrase) currentCapturable[kvp.Key] = kvp.Value;
                            Reveal();
                        });
                    }
                    else
                    {
                        Reveal();
                    }
                },
                (error) =>
                {
                    SpeechCaptionUI.Instance?.ShowNpcLine(genericDisplayName, "Erreur: " + error, null);
                }
            ));
            return;
        }

        // -------------------------
        // NPC DE CASO (modo detective)
        // -------------------------
        // Mismo flujo que el NPC genérico, andamiaje de débutant incluido.
        //
        // Antes esta rama se saltaba BeginnerTurnController dando por hecho
        // que un caso era siempre avanzado. Dejó de ser cierto el 18/08/2026,
        // cuando los casos pasaron a ser TODO el juego y la campaña débutant
        // (Jacques, el puerto, Godofredo) se retiró: ahora hay casos para
        // principiantes, y un principiante necesita el turno de 3 pasos tanto
        // o más en un misterio que en una charla.
        //
        // Regla: el andamiaje lo decide la DIFICULTAD, el misterio lo decide el
        // MODO. Son cosas distintas y no deben volver a mezclarse.
        if (detectiveMode)
        {
            SpeechCaptionUI.Instance?.ShowPlayerLine(msg, GameSettings.IsBasic);

            if (detectiveClient == null)
            {
                SpeechCaptionUI.Instance?.ShowNpcLine(detectiveDisplayName, "Erreur: client NPC introuvable.", null);
                return;
            }

            StartCoroutine(detectiveClient.SendMessage(msg,
                (reply, replyTranslation, playerMessageTranslation, playerMessagePhonetic, objectiveComplete, checkpointsReached, audioBase64) =>
                {
                    if (seq != chatMessageSeq) return;
                    var processed = DynamicWordHighlighter.ProcessText(reply, out var capturable);
                    currentCapturable.Clear();
                    foreach (var kvp in capturable) currentCapturable[kvp.Key] = kvp.Value;

                    void Reveal()
                    {
                        SpeechCaptionUI.Instance?.ShowNpcLine(detectiveDisplayName, processed, HandleLinkClicked);
                        SpeechCaptionUI.Instance?.ShowTranslation(GameSettings.IsBasic ? replyTranslation : null);
                        SpeechCaptionUI.Instance?.SetPlayerTranslation(GameSettings.IsBasic ? playerMessageTranslation : null, HandleLinkClicked);
                        NPCVoicePlayer.Instance?.PlayVoice(audioBase64, reply);

                        // Los hallazgos del caso. LevelManager une lo que
                        // reportan todos los NPCs; el panel verde del HUD se
                        // pinta desde ahí, no desde aquí.
                        if (checkpointsReached != null && checkpointsReached.Length > 0)
                            LevelManager.Instance?.ReportCheckpoints(checkpointsReached);
                    }

                    // Débutant: retiene la respuesta hasta que el jugador la
                    // "gana" leyéndola en francés. Avancé la revela directa.
                    if (GameSettings.IsBasic && LevelManager.Instance != null && LevelManager.Instance.IsActive
                        && !string.IsNullOrEmpty(playerMessageTranslation))
                    {
                        var cancelledClient = detectiveClient;   // capturado: puede cambiar antes del callback
                        BeginnerTurnController.Instance?.RunTurn(playerMessageTranslation, playerMessagePhonetic,
                            capturableFromPhrase =>
                            {
                                foreach (var kvp in capturableFromPhrase) currentCapturable[kvp.Key] = kvp.Value;
                                Reveal();
                            },
                            // Descartar el turno: además de no revelar la respuesta,
                            // se le pide al NPC que olvide el intercambio, o
                            // recordaría algo que para el jugador nunca se dijo.
                            () => { if (cancelledClient != null) StartCoroutine(cancelledClient.UndoLastExchange()); });
                    }
                    else
                    {
                        Reveal();
                    }
                },
                (error) =>
                {
                    SpeechCaptionUI.Instance?.ShowNpcLine(detectiveDisplayName, "Erreur: " + error, null);
                }
            ));
            return;
        }

        // -------------------------
        // JACQUES
        // -------------------------
        if (jacquesMode)
        {
            if (jacquesClient == null) { SpeechCaptionUI.Instance?.ShowNpcLine("Jacques", "Erreur: Jacques introuvable.", null); return; }

            SpeechCaptionUI.Instance?.ShowPlayerLine(msg, GameSettings.IsBasic);
            StartCoroutine(jacquesClient.SendMessage(msg,
                (reply, replyTranslation, playerMessageTranslation, playerMessagePhonetic, objectiveComplete, audioBase64) =>
                {
                    if (seq != chatMessageSeq) return;
                    var processed = DynamicWordHighlighter.ProcessText(reply, out var capturable);
                    currentCapturable.Clear();
                    foreach (var kvp in capturable) currentCapturable[kvp.Key] = kvp.Value;

                    void Reveal()
                    {
                        SpeechCaptionUI.Instance?.ShowNpcLine("Jacques", processed, HandleLinkClicked);
                        SpeechCaptionUI.Instance?.ShowTranslation(GameSettings.IsBasic ? replyTranslation : null);
                        SpeechCaptionUI.Instance?.SetPlayerTranslation(GameSettings.IsBasic ? playerMessageTranslation : null, HandleLinkClicked);
                        NPCVoicePlayer.Instance?.PlayVoice(audioBase64, reply);
                        if (objectiveComplete) LevelManager.Instance?.ReportObjectiveComplete();
                    }

                    if (GameSettings.IsBasic && LevelManager.Instance != null && LevelManager.Instance.IsActive
                        && !string.IsNullOrEmpty(playerMessageTranslation))
                    {
                        BeginnerTurnController.Instance?.RunTurn(playerMessageTranslation, playerMessagePhonetic, capturableFromPhrase =>
                        {
                            foreach (var kvp in capturableFromPhrase) currentCapturable[kvp.Key] = kvp.Value;
                            Reveal();
                        });
                    }
                    else
                    {
                        Reveal();
                    }
                },
                (error) => { SpeechCaptionUI.Instance?.ShowNpcLine("Jacques", "Erreur: " + error, null); }
            ));
            return;
        }

        // -------------------------
        // HENRY
        // -------------------------
        if (henryMode)
        {
            chatNpcLine = "Henry: ...";
            RefreshChatDisplay();

            if (henryClient == null)
            {
                dialogueText.text = "Erreur: client Henry introuvable.";
                return;
            }

            StartCoroutine(henryClient.SendMessage(
                msg,
                (reply, replyTranslation, playerMessageTranslation) =>
                {
                    if (seq != chatMessageSeq) return;
                    var processedReply = DynamicWordHighlighter.ProcessText(reply, out var generatedCapturable);

                    currentCapturable.Clear();
                    foreach (var kvp in generatedCapturable)
                    {
                        currentCapturable[kvp.Key] = kvp.Value;
                    }

                    chatPlayerLine = FormatLine(LangText.Pick("Yo", "Toi", "Me", "Io", "Jo"), msg, pendingPlayerTranslation ?? playerMessageTranslation);
                    chatNpcLine    = FormatLine("Henry", processedReply, replyTranslation);
                    RefreshChatDisplay();
                },
                (error) =>
                {
                    dialogueText.text = "Erreur Henry: " + error;
                }
            ));

            return;
        }

        // -------------------------
        // BAKER
        // -------------------------
        chatNpcLine = "Boulanger: ...";
        RefreshChatDisplay();

        if (bakerClient == null)
        {
            dialogueText.text = "Erreur: client introuvable.";
            return;
        }

        bool autoBuyBread = false;
        bool breadBought = false;

        if (currentBakerShop != null && BakerIntentParser.WantsToBuyBread(msg))
        {
            autoBuyBread = true;
            breadBought = currentBakerShop.BuyBread();
        }

        StartCoroutine(bakerClient.SendMessage(
            msg,
            onReply: (reply, replyTranslation, playerMessageTranslation) =>
            {
                if (seq != chatMessageSeq) return;
                var processedReply = DynamicWordHighlighter.ProcessText(reply, out var generatedCapturable);

                currentCapturable.Clear();
                foreach (var kvp in generatedCapturable)
                {
                    currentCapturable[kvp.Key] = kvp.Value;
                }

                string extra = "";

                if (autoBuyBread)
                {
                    if (breadBought)
                        extra = "\n\n[Boutique] Tu achètes du pain pour 5$.";
                    else
                        extra = "\n\n[Boutique] Tu n'as pas assez d'argent.";
                }

                chatPlayerLine = FormatLine(LangText.Pick("Yo", "Toi", "Me", "Io", "Jo"), msg, pendingPlayerTranslation ?? playerMessageTranslation);
                chatNpcLine    = FormatLine("Boulanger", processedReply, replyTranslation) + extra;
                RefreshChatDisplay();
            },
            onError: (err) =>
            {
                dialogueText.text = "Erreur serveur: " + err;
            }
        ));
    }

    public bool IsOpen()
    {
        // Autocorrección: si el panel no está activo pero isOpen quedó true, resetear
        if (isOpen && (dialoguePanel == null || !dialoguePanel.activeInHierarchy))
        {
            isOpen   = false;
            chatMode = false;
        }
        return isOpen;
    }

    public void Open(List<string> dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Count == 0) return;

        lines.Clear();
        foreach (var l in dialogueLines) lines.Enqueue(l);

        gameObject.SetActive(true);
        if (dialoguePanel != null && dialoguePanel.transform.root != null)
            dialoguePanel.transform.root.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        ShowCloseButton();
        if (nextBtn != null) nextBtn.gameObject.SetActive(true);
        isOpen = true;

        // drenaje por conversación (una vez al abrir)
        if (resources != null) resources.DrainOnConversation();

        NextLine();
    }

    private void NextLine()
    {
        if (lines.Count == 0) { Close(); return; }

        string raw       = lines.Dequeue();
        string processed = DynamicWordHighlighter.ProcessText(raw, out var capturable);

        currentCapturable.Clear();
        foreach (var kvp in capturable) currentCapturable[kvp.Key] = kvp.Value;

        dialogueText.text = processed;

        if (nextBtnLabel != null)
            nextBtnLabel.text = lines.Count == 0 ? "Cerrar" : "Siguiente >";
    }
    private void HandleLinkClicked(string linkId)
    {
        if (!isOpen) return;
        if (wordSystem == null) return;

        string normalized = linkId.ToLower();

        if (currentCapturable.TryGetValue(normalized, out var pair))
        {
            wordSystem.Capture(pair.fr, pair.es);
        }
    }


    public void SetCapturableWords(Dictionary<string, (string fr, string es)> capturable)
    {
        currentCapturable = capturable ?? new Dictionary<string, (string fr, string es)>();
    }
    public void Close()
    {
        // Si el jugador cierra una conversación (Jacques O un NPC genérico)
        // que ya había cumplido el objetivo del nivel, ESE es el momento en
        // que el nivel termina de verdad (no antes) — así puede seguir
        // hablando para intentar capturar más palabras antes de cerrar.
        // (jacquesMode || genericMode): antes solo miraba jacquesMode, así
        // que cerrar la conversación con un NPC genérico tras cumplir el
        // objetivo (niveles 2-4) no llevaba nunca a la pantalla de resultado.
        if ((jacquesMode || genericMode || detectiveMode) && LevelManager.Instance != null && LevelManager.Instance.IsActive
            && LevelManager.Instance.ObjectiveComplete)
        {
            LevelManager.Instance.FinishLevelNow();
        }

        // Si se cierra a media grabación (p.ej. Escape en vez de tocar el
        // botón de micro para pararla), aborta la captura y suelta la
        // música — si no, se quedaba bajada hasta que la grabación
        // expirase sola a los 10s (VoiceRecorder.MaxSeconds).
        voiceRecorder?.CancelRecording();
        ResetMicButton();

        ExitCaptionMode();

        currentCapturable.Clear();
        chatMode = false;

        // Restore panel position if keyboard had pushed it up
        if (panelRT != null && panelPosSaved)
            panelRT.anchoredPosition = panelOriginalPos;
        panelPosSaved = false;
        isKeyboardUp  = false;

        if (dialogueInput != null) dialogueInput.gameObject.SetActive(false);
        if (sendButton != null) sendButton.gameObject.SetActive(false);

        inlineCloseBtn?.gameObject.SetActive(false);
        nextBtn?.gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
        isOpen = false;

        if (buyBreadButton != null)
            buyBreadButton.gameObject.SetActive(false);
        if (acceptJobButton != null)
            acceptJobButton.gameObject.SetActive(false);

        if (declineJobButton != null)
            declineJobButton.gameObject.SetActive(false);
        henryMode   = false;
        jacquesMode = false;
        genericMode = false;
        detectiveMode = false;
        shopMode    = false;
    }

    public void OpenBakerChat(BakerShop bakerShop)
    {
        currentBakerShop = bakerShop;

        gameObject.SetActive(true);
        if (dialoguePanel != null && dialoguePanel.transform.root != null)
            dialoguePanel.transform.root.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen = true;
        chatMode = true;

        if (resources != null) resources.DrainOnConversation();

        if (dialogueText != null)
            dialogueText.text = "Bonjour. Je suis le boulanger.\nVeux-tu du pain pour 5$ ?";

        if (dialogueInput != null)
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueInput.text = "";
            // No auto-focus here — activating the field immediately pops the mobile
            // keyboard the instant a chat opens, before the player can read anything.
            // It only opens now when the player deliberately taps the field.
        }

        if (sendButton != null)
            sendButton.gameObject.SetActive(true);

        if (buyBreadButton != null)
        {
            buyBreadButton.gameObject.SetActive(true);
            buyBreadButton.onClick.RemoveAllListeners();
            buyBreadButton.onClick.AddListener(() =>
            {
                if (currentBakerShop != null)
                {
                    currentBakerShop.BuyBread();
                }
            });
        }

        currentCapturable.Clear();

        if (bakerClient != null)
        {
            StartCoroutine(bakerClient.ResetMemory(
                onDone: () =>
                {
                    Debug.Log("Mémoire du boulanger réinitialisée.");
                },
                onError: (err) =>
                {
                    Debug.LogWarning("Erreur reset mémoire boulanger: " + err);
                }
            ));
        }
    }
    public void OpenJobOffer(JobNPC jobNPC, string description, float energyCost, int moneyReward)
    {
        currentJobNPC = jobNPC;

        gameObject.SetActive(true);
        if (dialoguePanel != null && dialoguePanel.transform.root != null)
            dialoguePanel.transform.root.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen = true;
        chatMode = false;

        if (resources != null) resources.DrainOnConversation();

        if (dialogueText != null)
        {
            int energyPercent = Mathf.RoundToInt(energyCost * 100f);
            dialogueText.text =
                $"Travail: {description}\nCoût: {energyPercent}% énergie\nRécompense: {moneyReward}$\n\nTu acceptes ?";
        }

        if (dialogueInput != null)
            dialogueInput.gameObject.SetActive(false);

        if (sendButton != null)
            sendButton.gameObject.SetActive(false);

        if (buyBreadButton != null)
            buyBreadButton.gameObject.SetActive(false);

        if (acceptJobButton != null)
        {
            acceptJobButton.gameObject.SetActive(true);

            var tmp = acceptJobButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = "Accepter";

            acceptJobButton.onClick.RemoveAllListeners();
            acceptJobButton.onClick.AddListener(() =>
            {
                if (currentJobNPC != null)
                {
                    bool success = currentJobNPC.DoJob();
                    if (dialogueText != null)
                    {
                        dialogueText.text = success
                            ? "Merci. Le travail est fait."
                            : "Tu es trop fatigué.";
                    }
                }
            });
        }

        if (declineJobButton != null)
        {
            declineJobButton.gameObject.SetActive(true);

            var tmp = declineJobButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = "Refuser";

            declineJobButton.onClick.RemoveAllListeners();
            declineJobButton.onClick.AddListener(() =>
            {
                Close();
            });
        }

        currentCapturable.Clear();
    }

    public void OpenSleep(Auberge auberge, int cost)
    {
        currentAuberge = auberge;

        gameObject.SetActive(true);
        if (dialoguePanel != null && dialoguePanel.transform.root != null)
            dialoguePanel.transform.root.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen = true;
        chatMode = false;

        if (resources != null) resources.DrainOnConversation();

        if (dialogueText != null)
        {
            dialogueText.text =
                $"Auberge\nDormir pour {cost}$\n\nTu veux dormir ?";
        }

        if (dialogueInput != null)
            dialogueInput.gameObject.SetActive(false);

        if (sendButton != null)
            sendButton.gameObject.SetActive(false);

        if (buyBreadButton != null)
            buyBreadButton.gameObject.SetActive(false);

        // BOTÓN DORMIR
        if (acceptJobButton != null)
        {
            acceptJobButton.gameObject.SetActive(true);

            var tmp = acceptJobButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = "Dormir";

            acceptJobButton.onClick.RemoveAllListeners();
            acceptJobButton.onClick.AddListener(() =>
            {
                if (currentAuberge != null)
                {
                    bool success = currentAuberge.Sleep();

                    if (dialogueText != null)
                    {
                        dialogueText.text = success
                            ? "Tu dors bien. Tu es reposé."
                            : "Tu n'as pas assez d'argent.";
                    }
                }
            });
        }

        // BOTÓN SALIR
        if (declineJobButton != null)
        {
            declineJobButton.gameObject.SetActive(true);

            var tmp = declineJobButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null) tmp.text = "Sortir";

            declineJobButton.onClick.RemoveAllListeners();
            declineJobButton.onClick.AddListener(() =>
            {
                Close();
            });
        }

        currentCapturable.Clear();
    }
    public void OpenJacquesChat(JacquesChatClient client, Transform npcAnchor)
    {
        jacquesClient = client;

        gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen = true;
        chatMode = true;
        jacquesMode = true;

        EnterCaptionMode();
        SpeechCaptionUI.Instance?.SetAnchors(npcAnchor, GameObject.FindWithTag("Player")?.transform);

        if (resources != null) resources.DrainOnConversation();
        if (dialogueInput != null) { dialogueInput.gameObject.SetActive(true); dialogueInput.text = ""; }
        if (sendButton != null) sendButton.gameObject.SetActive(true);
        if (buyBreadButton != null) buyBreadButton.gameObject.SetActive(false);
        if (acceptJobButton != null) acceptJobButton.gameObject.SetActive(false);
        if (declineJobButton != null) declineJobButton.gameObject.SetActive(false);

        currentCapturable.Clear();

        StartCoroutine(jacquesClient.ResetSession(() =>
        {
            var processed = DynamicWordHighlighter.ProcessText(
                "Bienvenue au [port|puerto] ! Je m'appelle Jacques. D'où viens-tu ?", out var capturable);
            currentCapturable.Clear();
            foreach (var kvp in capturable) currentCapturable[kvp.Key] = kvp.Value;
            SpeechCaptionUI.Instance?.ShowNpcLine("Jacques", processed, HandleLinkClicked);

            // Este saludo es fijo (no viene del servidor, así que no trae su
            // propia replyTranslation) — para alguien que no sabe nada de
            // francés, la primera frase del juego sin traducción le bloquea
            // antes de empezar. Al ser una única frase fija, la traducción
            // también se escribe a mano en vez de pedirla al servidor.
            if (GameSettings.IsBasic)
                SpeechCaptionUI.Instance?.ShowTranslation("¡Bienvenido al puerto! Me llamo Jacques. ¿De dónde vienes?");
        }));
    }

    public void OpenGenericChat(string npcId, string personalityId, string displayName, string gender, GenericNPCChatClient client, Transform npcAnchor)
    {
        genericClient = client;
        genericNpcId = npcId;
        genericPersonalityId = personalityId;
        genericDisplayName = displayName;
        genericGender = gender;

        gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen = true;
        chatMode = true;
        genericMode = true;

        EnterCaptionMode();
        SpeechCaptionUI.Instance?.SetAnchors(npcAnchor, GameObject.FindWithTag("Player")?.transform);
        SpeechCaptionUI.Instance?.ShowNpcLine(genericDisplayName, "Bonjour...", null);

        if (resources != null) resources.DrainOnConversation();

        if (dialogueInput != null)
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueInput.text = "";
            // No auto-focus here — activating the field immediately pops the mobile
            // keyboard the instant a chat opens, before the player can read anything.
            // It only opens now when the player deliberately taps the field.
        }

        if (sendButton != null) sendButton.gameObject.SetActive(true);
        if (buyBreadButton != null) buyBreadButton.gameObject.SetActive(false);
        if (acceptJobButton != null) acceptJobButton.gameObject.SetActive(false);
        if (declineJobButton != null) declineJobButton.gameObject.SetActive(false);

        currentCapturable.Clear();

        StartCoroutine(client.ResetMemory(npcId));
    }

    // Abre la conversación con un NPC de caso (modo detective). La frase de
    // entrada se muestra en el acto, sin llamar al servidor: en un caso de
    // misterio dos de ellas son piezas del caso (Marcel presentándose, Fabre
    // dando por hecho que fue un robo) y no pueden depender de que el modelo
    // se acuerde de decirlas en su primera respuesta.
    public void OpenDetectiveChat(DetectiveChatClient client, string displayName, string greeting, string greetingTranslation, Transform npcAnchor)
    {
        detectiveClient = client;
        detectiveDisplayName = displayName;

        gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen = true;
        chatMode = true;
        detectiveMode = true;

        EnterCaptionMode();
        SpeechCaptionUI.Instance?.SetAnchors(npcAnchor, GameObject.FindWithTag("Player")?.transform);

        if (resources != null) resources.DrainOnConversation();
        if (dialogueInput != null) { dialogueInput.gameObject.SetActive(true); dialogueInput.text = ""; }
        if (sendButton != null) sendButton.gameObject.SetActive(true);
        if (buyBreadButton != null) buyBreadButton.gameObject.SetActive(false);
        if (acceptJobButton != null) acceptJobButton.gameObject.SetActive(false);
        if (declineJobButton != null) declineJobButton.gameObject.SetActive(false);

        currentCapturable.Clear();

        // NO se borra la memoria del personaje al reabrir la conversación.
        //
        // Antes sí: cada vez que el jugador volvía a hablar con alguien, ese
        // alguien había olvidado todo lo dicho. Se podía acorralar a un testigo,
        // salir, entrar otra vez y ser recibido como un desconocido. El progreso
        // nunca se perdía —los hallazgos los guarda LevelManager y se reenvían—
        // pero la sensación era la de rellenar un formulario, no la de
        // interrogar a una persona, que es sobre lo que se sostiene el juego
        // entero.
        //
        // El hilo no crece sin fin: el servidor recorta a los últimos 16
        // mensajes por personaje (ver detectiveSessions en server.js). El coste
        // es un prompt algo más largo por turno, y se paga a gusto.
        var processed = DynamicWordHighlighter.ProcessText(greeting, out var capturable);
        currentCapturable.Clear();
        foreach (var kvp in capturable) currentCapturable[kvp.Key] = kvp.Value;
        SpeechCaptionUI.Instance?.ShowNpcLine(displayName, processed, HandleLinkClicked);

        if (GameSettings.IsBasic && !string.IsNullOrEmpty(greetingTranslation))
            SpeechCaptionUI.Instance?.ShowTranslation(greetingTranslation);

        // La frase de entrada se muestra desde el cliente, así que no venía
        // con audio: era la única línea del juego que se leía y no se oía,
        // en cualquier idioma. Se pide aparte con la voz del personaje.
        if (greetingTTS == null) greetingTTS = gameObject.AddComponent<PhraseTTSClient>();
        string greetingVoice = client.NpcId + GameSettings.DossierSuffix;   // "", "_en", "_it" o "_ca"
        StartCoroutine(greetingTTS.FetchAudio(
            greeting,   // el servidor ya quita las etiquetas [fr|es] antes de sintetizar
            audio => NPCVoicePlayer.Instance?.PlayVoice(audio, greeting),
            _ => { },   // sin voz se sigue jugando: el texto ya está en pantalla
            greetingVoice));
    }

    public void OpenPoliceChallenge(string challengeLine, System.Action<string> onAnswer, System.Action onTimeout)
    {
        gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen   = true;
        chatMode = true;

        if (resources != null) resources.DrainOnConversation();

        string processed = DynamicWordHighlighter.ProcessText(challengeLine, out var capturable);
        currentCapturable.Clear();
        foreach (var kvp in capturable) currentCapturable[kvp.Key] = kvp.Value;
        if (dialogueText != null) dialogueText.text = processed;

        if (dialogueInput != null)
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueInput.text = "";
            dialogueInput.onSubmit.RemoveAllListeners();
            dialogueInput.onSubmit.AddListener(answer =>
            {
                dialogueInput.onSubmit.RemoveAllListeners();
                onAnswer?.Invoke(answer);
            });
        }

        if (sendButton != null)
        {
            sendButton.gameObject.SetActive(true);
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(() =>
            {
                string a = dialogueInput != null ? dialogueInput.text : "";
                dialogueInput?.gameObject.SetActive(false);
                sendButton.gameObject.SetActive(false);
                onAnswer?.Invoke(a);
            });
        }

        if (buyBreadButton  != null) buyBreadButton.gameObject.SetActive(false);
        if (acceptJobButton != null) acceptJobButton.gameObject.SetActive(false);
        if (declineJobButton!= null) declineJobButton.gameObject.SetActive(false);
    }

    public void OpenShopMenu(string text, string buyFoodLabel, string buyWaterLabel,
        System.Action onBuyFood, System.Action onBuyWater, System.Action onLeave)
    {
        gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();
        isOpen    = true;
        chatMode  = false;
        shopMode  = true;

        if (resources != null) resources.DrainOnConversation();
        if (dialogueText  != null) dialogueText.text = text;
        if (dialogueInput != null) dialogueInput.gameObject.SetActive(false);
        if (sendButton    != null) sendButton.gameObject.SetActive(false);
        if (buyBreadButton!= null) buyBreadButton.gameObject.SetActive(false);

        if (acceptJobButton != null)
        {
            acceptJobButton.gameObject.SetActive(true);
            var lbl = acceptJobButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (lbl != null) lbl.text = buyFoodLabel;
            acceptJobButton.onClick.RemoveAllListeners();
            acceptJobButton.onClick.AddListener(() => onBuyFood?.Invoke());
        }

        if (declineJobButton != null)
        {
            declineJobButton.gameObject.SetActive(true);
            var lbl = declineJobButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (lbl != null) lbl.text = buyWaterLabel;
            declineJobButton.onClick.RemoveAllListeners();
            declineJobButton.onClick.AddListener(() => onBuyWater?.Invoke());
        }

        currentCapturable.Clear();
    }

    public void ShowShopResult(string msg)
    {
        if (dialogueText    != null) dialogueText.text = msg;
        if (acceptJobButton != null) acceptJobButton.gameObject.SetActive(false);
        if (declineJobButton!= null) declineJobButton.gameObject.SetActive(false);
    }

    public void OpenHenryChat()
    {
        gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCloseButton();
        HideNextButton();

        isOpen = true;
        chatMode = true;
        henryMode = true;

        if (dialogueText != null)
            dialogueText.text = "Henry Beaumont.\nJe vous écoute.";

        if (dialogueInput != null)
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueInput.text = "";
            // No auto-focus here — activating the field immediately pops the mobile
            // keyboard the instant a chat opens, before the player can read anything.
            // It only opens now when the player deliberately taps the field.
        }

        if (sendButton != null)
            sendButton.gameObject.SetActive(true);

        // Ocultar botones de panadero / trabajos
        if (buyBreadButton != null) buyBreadButton.gameObject.SetActive(false);
        if (acceptJobButton != null) acceptJobButton.gameObject.SetActive(false);
        if (declineJobButton != null) declineJobButton.gameObject.SetActive(false);

        
    }

}
