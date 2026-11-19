using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// El profesor de idioma: un botón flotante con su cara y, al pulsarlo, un chat
// para preguntar dudas.
//
// Nace de las pruebas: la gente decía que en un juego así se aprende de verdad
// cuando hay alguien al lado a quien preguntar. Hasta ahora, quien no entendía
// una frase solo tenía la traducción al español —y solo en débutant—, o nada;
// y nadie podía preguntar nunca "¿por qué se dice así y no asá?".
//
// Está FUERA de la ficción a propósito. No es un personaje de Marsella, no está
// en la escena y no sabe nada del caso (ver el prompt en el servidor). Por eso
// vive en su propio canvas y no en DialogueSystem.
public class TeacherPanel : MonoBehaviour
{
    public static TeacherPanel Instance { get; private set; }

    static readonly Color BgDim    = new Color(0f, 0f, 0f, 0.55f);
    static readonly Color CardBg   = new Color(0.027f, 0.067f, 0.118f, 0.98f);
    static readonly Color NavyLine = new Color(0.051f, 0.122f, 0.208f, 1.00f);
    static readonly Color Gold     = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color Cream    = new Color(0.941f, 0.902f, 0.827f, 1.00f);
    static readonly Color CreamDim = new Color(0.941f, 0.902f, 0.827f, 0.55f);
    static readonly Color TextDark = new Color(0.012f, 0.031f, 0.059f, 1.00f);

    GameObject      openBtn;         // el botón con la cara, siempre visible en nivel
    RectTransform   openBtnRT;
    TMP_Text        openBtnLabel;
    GameObject      panel;
    TMP_Text        transcriptText;
    TMP_InputField  input;
    TMP_Text        placeholderText;
    TMP_Text        headerText;
    TMP_Text        sendBtnLabel;
    ScrollRect      scroll;
    RectTransform   contentRT;

    readonly List<string> lines = new List<string>();
    bool waiting;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        panel.SetActive(false);
        openBtn.SetActive(false);
    }

    // El botón solo tiene sentido dentro de un nivel: en el menú no hay nada
    // sobre lo que preguntar, y taparía las opciones.
    void Update()
    {
        bool inLevel = LevelManager.Instance != null && LevelManager.Instance.IsActive;
        bool show    = inLevel && !panel.activeSelf;
        if (openBtn.activeSelf != show) openBtn.SetActive(show);

        // Cuelga del botón de silenciar, que a su vez sigue al pergamino de
        // objetivos: los dos bajan juntos cuando el panel crece con los
        // hallazgos del caso.
        if (openBtnRT != null)
        {
            float y = MusicToggleButton.StackY - 110f;
            if (!Mathf.Approximately(openBtnRT.anchoredPosition.y, y))
                openBtnRT.anchoredPosition = new Vector2(openBtnRT.anchoredPosition.x, y);
        }

        if (show && openBtnLabel != null)
            openBtnLabel.text = "Profesor de\n" + GameSettings.TargetLanguageName;
    }

    public void Open()
    {
        panel.SetActive(true);
        headerText.text = "PROFESOR DE " + GameSettings.TargetLanguageName.ToUpper();
        placeholderText.text = "Escribe tu duda...";

        // El reloj se para. Si no, preguntar cuesta la estrella de tiempo y el
        // jugador deja de preguntar — justo lo contrario de para qué existe.
        if (LevelManager.Instance != null) LevelManager.Instance.TimerPaused = true;

        if (lines.Count == 0) AddLine(Gold, WelcomeText());
        input.text = "";
        input.ActivateInputField();
    }

    public void Close()
    {
        panel.SetActive(false);
        if (LevelManager.Instance != null) LevelManager.Instance.TimerPaused = false;
    }

    // Se llama al empezar un nivel: la conversación con el profesor empieza
    // limpia en cada partida, igual que la de los NPCs.
    public void ResetConversation()
    {
        lines.Clear();
        if (transcriptText != null) transcriptText.text = "";
        if (TeacherChatClient.Instance != null)
            StartCoroutine(TeacherChatClient.Instance.ResetMemory());
    }

    string WelcomeText()
    {
        string lng = GameSettings.TargetLanguageName;
        return $"Soy tu profesor de {lng}. Pregúntame lo que quieras: qué significa una frase, "
             + $"cómo se dice algo, por qué se escribe así.\n\nDel caso no sé nada — eso es cosa tuya, inspector.";
    }

    void Send()
    {
        string msg = input.text.Trim();
        if (waiting || string.IsNullOrEmpty(msg)) return;
        if (TeacherChatClient.Instance == null) { AddLine(CreamDim, "El profesor no está disponible."); return; }

        AddLine(Cream, "<b>Tú:</b> " + msg);
        input.text = "";
        waiting = true;
        sendBtnLabel.text = "...";

        StartCoroutine(TeacherChatClient.Instance.Ask(msg,
            reply => { waiting = false; sendBtnLabel.text = "ENVIAR"; AddLine(Gold, reply); input.ActivateInputField(); },
            error => { waiting = false; sendBtnLabel.text = "ENVIAR"; AddLine(CreamDim, "No he podido preguntarle (" + error + "). Inténtalo otra vez."); }));
    }

    // El prompt le pide al modelo que no use markdown, pero los modelos lo
    // usan igual de vez en cuando y TMP no lo entiende: se verían los
    // asteriscos en pantalla. Se traducen a las etiquetas ricas de TMP, que sí
    // renderiza. Cinturón y tirantes: instrucción en el servidor y conversión
    // aquí.
    static readonly System.Text.RegularExpressions.Regex BoldMd =
        new System.Text.RegularExpressions.Regex(@"\*\*(.+?)\*\*", System.Text.RegularExpressions.RegexOptions.Singleline);
    static readonly System.Text.RegularExpressions.Regex ItalicMd =
        new System.Text.RegularExpressions.Regex(@"(?<![\*\w])\*(?!\s)(.+?)(?<!\s)\*(?!\*)", System.Text.RegularExpressions.RegexOptions.Singleline);

    static string MarkdownToRichText(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = BoldMd.Replace(s, "<b>$1</b>");
        s = ItalicMd.Replace(s, "<i>$1</i>");
        return s;
    }

    void AddLine(Color color, string text)
    {
        text = MarkdownToRichText(text);
        lines.Add("<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>");
        transcriptText.text = string.Join("\n\n", lines);

        // Al fondo, que es donde está lo último dicho. Hace falta esperar un
        // frame: el ContentSizeFitter todavía no ha recalculado el alto cuando
        // se acaba de cambiar el texto.
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
        yield return null;
        scroll.verticalNormalizedPosition = 0f;
    }

    void BuildUI()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        // Por encima del diálogo (400) y de los globos (405), por debajo del
        // tutorial guiado (470): el profesor tiene que poder abrirse ENCIMA de
        // una conversación, que es justo cuando hace falta.
        canvas.sortingOrder = 460;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        BuildOpenButton();
        BuildPanel();
    }

    void BuildOpenButton()
    {
        openBtn = new GameObject("TeacherBtn", typeof(RectTransform), typeof(Image));
        openBtn.transform.SetParent(transform, false);
        var rt = openBtn.GetComponent<RectTransform>();
        // Debajo del botón de silenciar, en el hueco que dejó el panel de
        // "palabras a encontrar" (retirado el 02/09/2026). La esquina inferior
        // izquierda es del botón de pista y la derecha del joystick.
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 1f);
        openBtnRT = rt;
        rt.anchoredPosition = new Vector2(-24, MusicToggleButton.StackY - 110f);
        rt.sizeDelta = new Vector2(176, 176);
        var img = openBtn.GetComponent<Image>();
        img.sprite = RoundedRectSprite.GetSprite();
        img.type   = Image.Type.Sliced;
        img.color  = NavyLine;

        // La cara. Si algún día hay retrato, basta con dejar el PNG en
        // Assets/Resources/teacher_face.png y aparece solo — sin tocar código
        // ni escenas. Mientras tanto, la silueta con birrete de TeacherIcon.
        var faceGO = new GameObject("Face", typeof(RectTransform), typeof(Image));
        faceGO.transform.SetParent(openBtn.transform, false);
        var fRT = faceGO.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0.5f, 1f); fRT.anchorMax = new Vector2(0.5f, 1f);
        fRT.pivot     = new Vector2(0.5f, 1f);
        fRT.anchoredPosition = new Vector2(0, -8);
        fRT.sizeDelta = new Vector2(116, 116);
        var faceImg = faceGO.GetComponent<Image>();
        var portrait = Resources.Load<Sprite>("teacher_face");
        if (portrait != null) { faceImg.sprite = portrait; faceImg.color = Color.white; }
        else                  { faceImg.sprite = TeacherIcon.GetSprite();      faceImg.color = Gold; }
        faceImg.raycastTarget = false;

        var lblGO = new GameObject("Label", typeof(RectTransform));
        lblGO.transform.SetParent(openBtn.transform, false);
        var lRT = lblGO.GetComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0f, 0f); lRT.anchorMax = new Vector2(1f, 0f);
        lRT.pivot     = new Vector2(0.5f, 0f);
        lRT.anchoredPosition = new Vector2(0, 8);
        lRT.sizeDelta = new Vector2(-10, 48);
        openBtnLabel = lblGO.AddComponent<TextMeshProUGUI>();
        openBtnLabel.fontSize  = 18;
        openBtnLabel.color     = Cream;
        openBtnLabel.alignment = TextAlignmentOptions.Center;
        openBtnLabel.fontStyle = FontStyles.Bold;
        openBtnLabel.raycastTarget = false;
        openBtnLabel.enableWordWrapping = true;

        var btn = openBtn.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(Open);
    }

    void BuildPanel()
    {
        panel = new GameObject("TeacherPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
        pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;
        // El velo bloquea los toques de detrás a propósito: con el panel
        // abierto no se habla con nadie ni se mueve el jugador.
        panel.GetComponent<Image>().color = BgDim;

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(panel.transform, false);
        var cRT = card.GetComponent<RectTransform>();
        cRT.anchorMin = cRT.anchorMax = new Vector2(0.5f, 0.5f);
        cRT.pivot     = new Vector2(0.5f, 0.5f);
        cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta = new Vector2(880, 1150);
        var cImg = card.GetComponent<Image>();
        cImg.sprite = RoundedRectSprite.GetSprite();
        cImg.type   = Image.Type.Sliced;
        cImg.color  = CardBg;

        // ── Cabecera ──
        headerText = MakeLabel(card.transform, "Header", "", new Vector2(0, 520), new Vector2(700, 54),
                                30, Gold, TextAlignmentOptions.Center, FontStyles.Bold);

        // Cerrar (X)
        var closeGO = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image));
        closeGO.transform.SetParent(card.transform, false);
        var clRT = closeGO.GetComponent<RectTransform>();
        clRT.anchorMin = clRT.anchorMax = new Vector2(1f, 1f);
        clRT.pivot     = new Vector2(1f, 1f);
        clRT.anchoredPosition = new Vector2(-16, -16);
        clRT.sizeDelta = new Vector2(60, 60);
        var clImg = closeGO.GetComponent<Image>();
        clImg.sprite = RoundedRectSprite.GetSprite();
        clImg.type   = Image.Type.Sliced;
        clImg.color  = NavyLine;
        var clTxt = MakeLabel(closeGO.transform, "X", "X", Vector2.zero, new Vector2(60, 60),
                               28, Cream, TextAlignmentOptions.Center, FontStyles.Bold);
        clTxt.raycastTarget = false;
        var clBtn = closeGO.AddComponent<Button>();
        clBtn.targetGraphic = clImg;
        clBtn.onClick.AddListener(Close);

        // ── Transcripción, con scroll ──
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(card.transform, false);
        var vRT = viewport.GetComponent<RectTransform>();
        vRT.anchorMin = vRT.anchorMax = new Vector2(0.5f, 0.5f);
        vRT.pivot     = new Vector2(0.5f, 0.5f);
        vRT.anchoredPosition = new Vector2(0, 60);
        vRT.sizeDelta = new Vector2(800, 830);
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.20f);
        viewport.GetComponent<Mask>().showMaskGraphic = true;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 0);
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        transcriptText = content.AddComponent<TextMeshProUGUI>();
        transcriptText.fontSize  = 24;
        transcriptText.color     = Cream;
        transcriptText.alignment = TextAlignmentOptions.TopLeft;
        transcriptText.enableWordWrapping = true;
        transcriptText.raycastTarget = false;
        transcriptText.margin = new Vector4(16, 12, 16, 12);

        scroll = viewport.AddComponent<ScrollRect>();
        scroll.content    = contentRT;
        scroll.viewport   = vRT;
        scroll.horizontal = false;
        scroll.vertical   = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        // ── Campo de escritura ──
        var fieldGO = new GameObject("InputField", typeof(RectTransform), typeof(Image));
        fieldGO.transform.SetParent(card.transform, false);
        var fRT = fieldGO.GetComponent<RectTransform>();
        fRT.anchorMin = fRT.anchorMax = new Vector2(0.5f, 0.5f);
        fRT.pivot     = new Vector2(0.5f, 0.5f);
        fRT.anchoredPosition = new Vector2(-90, -450);
        fRT.sizeDelta = new Vector2(620, 84);
        var fImg = fieldGO.GetComponent<Image>();
        fImg.sprite = RoundedRectSprite.GetSprite();
        fImg.type   = Image.Type.Sliced;
        fImg.color  = NavyLine;

        var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(fieldGO.transform, false);
        var taRT = textArea.GetComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(14, 4); taRT.offsetMax = new Vector2(-14, -4);

        var inputText = MakeLabel(textArea.transform, "Text", "", Vector2.zero, Vector2.zero,
                                   24, Cream, TextAlignmentOptions.Left, FontStyles.Normal);
        Stretch(inputText.rectTransform);

        placeholderText = MakeLabel(textArea.transform, "Placeholder", "", Vector2.zero, Vector2.zero,
                                     24, CreamDim, TextAlignmentOptions.Left, FontStyles.Italic);
        Stretch(placeholderText.rectTransform);

        input = fieldGO.AddComponent<TMP_InputField>();
        input.textViewport  = taRT;
        input.textComponent = inputText;
        input.placeholder   = placeholderText;
        input.lineType      = TMP_InputField.LineType.SingleLine;
        input.onSubmit.AddListener(_ => Send());

        // ── Enviar ──
        var sendGO = new GameObject("SendBtn", typeof(RectTransform), typeof(Image));
        sendGO.transform.SetParent(card.transform, false);
        var sRT = sendGO.GetComponent<RectTransform>();
        sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f, 0.5f);
        sRT.pivot     = new Vector2(0.5f, 0.5f);
        sRT.anchoredPosition = new Vector2(310, -450);
        sRT.sizeDelta = new Vector2(180, 84);
        var sImg = sendGO.GetComponent<Image>();
        sImg.sprite = RoundedRectSprite.GetSprite();
        sImg.type   = Image.Type.Sliced;
        sImg.color  = Gold;
        sendBtnLabel = MakeLabel(sendGO.transform, "Label", "ENVIAR", Vector2.zero, new Vector2(180, 84),
                                  24, TextDark, TextAlignmentOptions.Center, FontStyles.Bold);
        sendBtnLabel.raycastTarget = false;
        var sendBtn = sendGO.AddComponent<Button>();
        sendBtn.targetGraphic = sImg;
        sendBtn.onClick.AddListener(Send);

        MakeLabel(card.transform, "Note",
            "El profesor no sabe nada del caso. Solo de idioma.",
            new Vector2(0, -530), new Vector2(700, 34),
            17, CreamDim, TextAlignmentOptions.Center, FontStyles.Italic);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
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
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.color     = color;
        t.alignment = align;
        t.fontStyle = style;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        return t;
    }
}
