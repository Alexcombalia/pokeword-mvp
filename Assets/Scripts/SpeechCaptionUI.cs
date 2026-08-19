using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sustituye al panel de chat estilo WhatsApp para las conversaciones del
// flujo de niveles (Jacques, NPCs genéricos): globos estilo pergamino
// (ver UXUI_pergamino.jpeg, carpeta Pokeword) en vez del panel azul marino
// original. Un turno cada vez, sin historial ni scroll.
//
// Color fijo por idioma en TODA la app (burbujas y cuaderno de palabras,
// ver PokeWorldController): francés siempre dorado, español siempre azul —
// aquí en su variante "tinta sobre papel" (más oscura, para leerse bien
// sobre el fondo claro de pergamino; ver FrenchInk/SpanishInk).
//
// Layout: la burbuja de Jacques flota sobre su cabeza (sigue su posición en
// el mundo) para que quede claro quién habla; su traducción (débutant) va
// DENTRO de esa misma burbuja, como segunda línea, no en una burbuja aparte.
// Tu línea (con su propia traducción) va FIJA abajo — antes seguía al
// jugador en el mundo y, como el jugador siempre está pegado al NPC al
// hablar, las burbujas terminaban solapándose entre sí.
public class SpeechCaptionUI : MonoBehaviour
{
    public static SpeechCaptionUI Instance { get; private set; }

    public static readonly Color FrenchColor  = new Color(1.000f, 0.820f, 0.400f, 1f); // dorado — #FFD166
    public static readonly Color SpanishColor = new Color(0.702f, 0.851f, 1.000f, 1f); // azul suave — #B3D9FF

    // Mismas variantes "sobre claro" que LevelHUD (FrenchHexOnLight/SpanishHexOnLight)
    // — el fondo ahora es pergamino claro, no azul marino, así que el dorado/
    // azul brillante de arriba se leería mal.
    static readonly Color FrenchInk  = new Color(0.612f, 0.420f, 0.039f, 1f); // #9C6B0A
    static readonly Color SpanishInk = new Color(0.122f, 0.361f, 0.541f, 1f); // #1F5C8A
    static readonly Color InkDark    = new Color(0.235f, 0.161f, 0.098f, 1f); // texto neutro (nombres)

    RectTransform canvasRT;
    Camera sceneCamera;

    GameObject npcBubble;
    RectTransform npcBubbleRT;
    TMP_Text npcNameLabel;
    TMP_Text npcText;
    TMP_Text npcTranslationText;
    TMPLinkClickHandler npcLinkHandler;

    GameObject playerBubble;
    TMP_Text playerText;
    TMP_Text playerTranslationText;
    TMPLinkClickHandler playerTranslationLinkHandler;

    Transform npcAnchor;
    bool npcVisible;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, __) => sceneCamera = null;
    }

    void LateUpdate()
    {
        if (!npcVisible || npcAnchor == null) return;
        if (sceneCamera == null) sceneCamera = FindObjectOfType<Camera>();
        if (sceneCamera == null) return;

        PositionBubble(npcBubbleRT, npcAnchor, 1.7f);
    }

    void PositionBubble(RectTransform bubbleRT, Transform anchor, float worldYOffset)
    {
        Vector3 worldPos  = anchor.position + new Vector3(0f, worldYOffset, 0f);
        Vector3 screenPos = sceneCamera.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, null, out Vector2 localPoint);

        // Clamp dentro de la pantalla — con la cámara muy cerca del personaje
        // (normal en móvil) el punto calculado puede quedar por encima del
        // borde superior y la burbuja de Jacques se vuelve invisible sin que
        // se note por qué (solo queda visible la traducción, fija abajo, y
        // parece que "solo sale en español"). El margen es grande a propósito:
        // el panel "Palabras a encontrar" (LevelHUD) vive justo debajo del
        // cronómetro, arriba a la derecha, y como este canvas se dibuja por
        // encima de LevelHUD, cualquier solape lo tapa por completo. Con dos
        // idiomas dentro de la misma burbuja (nombre + francés + traducción)
        // puede llegar a 3 líneas, así que necesita más margen que antes.
        float marginTop    = 480f;
        float minY = canvasRT.rect.yMin + 60f;
        float maxY = canvasRT.rect.yMax - marginTop;
        localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

        // El clamp horizontal sujetaba el CENTRO del globo con 60 de margen,
        // no sus bordes. Con una burbuja de 620 de ancho y pivote centrado eso
        // dejaba salirse ~250 px por el lado: con un NPC pegado al borde de la
        // escena —el doctor y el agente lo están— media burbuja quedaba fuera
        // de la pantalla. Ahora se sujeta contando su anchura real, así que el
        // globo entero cabe siempre.
        float halfWidth  = bubbleRT.rect.width * 0.5f;
        float marginSide = 24f;
        float minX = canvasRT.rect.xMin + halfWidth + marginSide;
        float maxX = canvasRT.rect.xMax - halfWidth - marginSide;

        // Si el globo fuera más ancho que el canvas no hay margen que valga y
        // Mathf.Clamp con min > max devolvería basura: se centra y ya.
        localPoint.x = (minX <= maxX) ? Mathf.Clamp(localPoint.x, minX, maxX) : 0f;

        bubbleRT.anchoredPosition = localPoint;
    }

    // Llamar antes de mostrar líneas, para que el globo de Jacques sepa a quién seguir.
    public void SetAnchors(Transform npc, Transform player)
    {
        npcAnchor = npc;
    }

    public void ShowNpcLine(string npcName, string processedText, System.Action<string> onWordClicked)
    {
        npcNameLabel.text = npcName.ToUpper();
        npcText.text = processedText;
        npcLinkHandler.OnLinkClicked = onWordClicked;
        npcTranslationText.gameObject.SetActive(false); // se añade después, ver ShowTranslation
        npcBubble.SetActive(true);
        npcVisible = true;
    }

    // isBasic: en débutant el jugador puede escribir/hablar en español, así que
    // la línea principal se muestra en azul (es lo que realmente dijo) y, en
    // cuanto llega la respuesta del servidor, SetPlayerTranslation añade el
    // francés debajo en dorado. En avancé el jugador escribe en francés
    // directamente, así que la línea única va en dorado, como antes.
    public void ShowPlayerLine(string text, bool isBasic)
    {
        playerText.text = text;
        playerText.color = isBasic ? SpanishInk : FrenchInk;
        playerBubble.SetActive(true);
        playerTranslationText.gameObject.SetActive(false);

        // Oculta la línea anterior de Jacques y la traducción del turno
        // anterior — si no, se quedan "colgadas" en pantalla mientras se
        // espera la respuesta nueva (incluida la primera, el saludo inicial).
        npcBubble.SetActive(false);
        npcVisible = false;
        ShowTranslation(null);
    }

    // Traducción al francés de lo que el jugador acaba de escribir/decir en
    // español — llegan en la misma respuesta que la línea del NPC, así que se
    // añade a posteriori en vez de en ShowPlayerLine. Null/"" o idéntica al
    // texto ya mostrado la oculta (evita una línea duplicada si ya escribió en francés).
    //
    // onWordClicked (débutant): si se pasa, las palabras de esta frase (la que
    // el propio jugador acaba de "ganar" leyendo en voz alta, ver
    // BeginnerTurnController) también se procesan con DynamicWordHighlighter y
    // quedan tocables para capturar al cuaderno, igual que las del NPC — más
    // exposición es mejor para quien parte de cero. Sin handler, ruta legacy:
    // texto plano, sin captura (comportamiento idéntico al de antes).
    public void SetPlayerTranslation(string frenchText, System.Action<string> onWordClicked = null)
    {
        if (string.IsNullOrEmpty(frenchText) ||
            string.Equals(frenchText.Trim(), playerText.text.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            playerTranslationText.gameObject.SetActive(false);
            return;
        }

        if (onWordClicked != null)
        {
            playerTranslationText.text = DynamicWordHighlighter.ProcessText(frenchText, out _);
            playerTranslationLinkHandler.OnLinkClicked = onWordClicked;
        }
        else
        {
            playerTranslationText.text = frenchText;
        }

        playerTranslationText.gameObject.SetActive(true);
    }

    // Traducción de apoyo (modo débutant) — segunda línea DENTRO de la misma
    // burbuja de Jacques, en azul, igual que ya funciona para "MOI". Antes
    // era una burbuja aparte y quedaba flotando separada de la de Jacques,
    // dando la sensación de "dos guiones distintos" para una sola réplica.
    // Null/"" la oculta.
    public void ShowTranslation(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            npcTranslationText.gameObject.SetActive(false);
            return;
        }
        npcTranslationText.text = text;
        npcTranslationText.gameObject.SetActive(true);
    }

    public void HideAll()
    {
        npcBubble.SetActive(false);
        playerBubble.SetActive(false);
        npcVisible = false;
        npcAnchor = null;
    }

    void BuildUI()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        // Por encima del canvas de DialogueSystem (400) a propósito: si no,
        // cualquier elemento activo del panel de diálogo (aunque esté vacío)
        // podría interceptar el toque antes de que llegue a la palabra
        // capturable del globo, y la captura dejaría de funcionar en silencio.
        canvas.sortingOrder = 405;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        canvasRT = GetComponent<RectTransform>();

        // Burbuja de Jacques — sigue su posición en el mundo, pivote abajo-centro
        // para que crezca hacia arriba cuando el texto necesita más de una línea.
        npcBubble = BuildBubble(transform, "NpcBubble", InkDark, 620,
            withName: true, withRaycast: true,
            out npcBubbleRT, out npcNameLabel, out npcText);
        npcBubbleRT.pivot = new Vector2(0.5f, 0f);
        npcText.color = FrenchInk;
        npcLinkHandler = npcText.gameObject.AddComponent<TMPLinkClickHandler>();
        npcLinkHandler.text = npcText;

        // Segunda línea, debajo del francés — traducción al español (modo
        // débutant), en la MISMA burbuja en vez de una aparte (ver ShowTranslation).
        var ntGO = new GameObject("Translation", typeof(RectTransform));
        ntGO.transform.SetParent(npcText.transform.parent, false);
        npcTranslationText = ntGO.AddComponent<TextMeshProUGUI>();
        npcTranslationText.fontSize = 26;
        npcTranslationText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        npcTranslationText.alignment = TextAlignmentOptions.Center;
        npcTranslationText.enableWordWrapping = true;
        npcTranslationText.color = SpanishInk;
        npcTranslationText.raycastTarget = false;
        npcTranslationText.gameObject.SetActive(false);

        npcBubble.SetActive(false);

        // Franja fija abajo — tu línea, apilada sin solaparse nunca con la
        // burbuja de Jacques (que vive en el mundo, arriba).
        var stackGO = new GameObject("BottomStack", typeof(RectTransform));
        stackGO.transform.SetParent(transform, false);
        var stackRT = stackGO.GetComponent<RectTransform>();
        stackRT.anchorMin = stackRT.anchorMax = new Vector2(0.5f, 0f);
        stackRT.pivot = new Vector2(0.5f, 0f);
        stackRT.anchoredPosition = new Vector2(0, 460);
        stackRT.sizeDelta = new Vector2(620, 0);

        var stackLayout = stackGO.AddComponent<VerticalLayoutGroup>();
        stackLayout.childAlignment = TextAnchor.LowerCenter;
        stackLayout.spacing = 16;
        stackLayout.childControlWidth = true;
        stackLayout.childControlHeight = true;
        stackLayout.childForceExpandWidth = true;
        stackLayout.childForceExpandHeight = false;

        var stackFitter = stackGO.AddComponent<ContentSizeFitter>();
        stackFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        stackFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        playerBubble = BuildBubble(stackRT, "PlayerBubble", InkDark, 620,
            withName: true, withRaycast: false,
            out _, out var playerNameLabel, out playerText);
        // La etiqueta del jugador estaba fija en francés, incluso en débutant, donde
        // todo lo demás está en español. Sigue el mismo criterio que el resto.
        playerNameLabel.text = LangText.Pick("YO", "MOI", "ME", "IO");
        playerText.color = FrenchInk;

        // Segunda línea, debajo del cuerpo — traducción al francés cuando el
        // jugador escribió en español (ver SetPlayerTranslation).
        var ptGO = new GameObject("Translation", typeof(RectTransform));
        ptGO.transform.SetParent(playerText.transform.parent, false);
        playerTranslationText = ptGO.AddComponent<TextMeshProUGUI>();
        playerTranslationText.fontSize = 26;
        playerTranslationText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        playerTranslationText.alignment = TextAlignmentOptions.Center;
        playerTranslationText.enableWordWrapping = true;
        playerTranslationText.color = FrenchInk;
        // Tocable cuando se pasa un onWordClicked (débutant, ver
        // SetPlayerTranslation) — inofensivo en avancé/rutas legacy, donde
        // nunca se le asigna un handler a este texto.
        playerTranslationText.raycastTarget = true;
        playerTranslationLinkHandler = playerTranslationText.gameObject.AddComponent<TMPLinkClickHandler>();
        playerTranslationLinkHandler.text = playerTranslationText;
        playerTranslationText.gameObject.SetActive(false);

        playerBubble.SetActive(false);
    }

    // Burbuja con altura automática: VerticalLayoutGroup + ContentSizeFitter
    // calculan la altura real a partir del contenido (nombre + texto), en vez
    // de anchors fijos — así el cuadro visible SIEMPRE coincide con el área
    // donde se dibuja y se puede tocar el texto, sin desbordes.
    // "accentColor" solo tiñe la etiqueta de nombre — el marco/relleno son
    // siempre el mismo pergamino, no cambian de color por idioma (así lo
    // marca la referencia: el papel es neutro, el color va en el texto).
    GameObject BuildBubble(Transform parent, string goName, Color accentColor, float width, bool withName, bool withRaycast,
        out RectTransform bubbleRT, out TMP_Text nameLabel, out TMP_Text bodyText)
    {
        var inkSprite   = ParchmentSprite.GetInk();
        var paperSprite = ParchmentSprite.GetPaper();

        var root = new GameObject(goName, typeof(RectTransform));
        root.transform.SetParent(parent, false);
        bubbleRT = root.GetComponent<RectTransform>();
        bubbleRT.sizeDelta = new Vector2(width, 0);

        var rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(22, 22, 14, 14);
        rootLayout.spacing = 6;
        rootLayout.childAlignment = TextAnchor.UpperCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        var rootFitter = root.AddComponent<ContentSizeFitter>();
        rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        rootFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // Borde y relleno se dibujan por debajo del contenido y NO participan
        // en el layout (ignoreLayout) — deben estirarse a todo el tamaño de
        // la burbuja, no apilarse como si fueran un elemento de texto más.
        var borderGO = new GameObject("Border", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        borderGO.transform.SetParent(root.transform, false);
        var bRT = borderGO.GetComponent<RectTransform>();
        bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
        bRT.offsetMin = Vector2.zero; bRT.offsetMax = Vector2.zero;
        var bImg = borderGO.GetComponent<Image>();
        bImg.sprite = inkSprite; bImg.type = Image.Type.Sliced; bImg.color = Color.white;
        bImg.raycastTarget = false;
        borderGO.GetComponent<LayoutElement>().ignoreLayout = true;
        borderGO.transform.SetAsFirstSibling();

        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        fillGO.transform.SetParent(root.transform, false);
        var fRT = fillGO.GetComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = new Vector2(6, 6); fRT.offsetMax = new Vector2(-6, -6);
        var fImg = fillGO.GetComponent<Image>();
        fImg.sprite = paperSprite; fImg.type = Image.Type.Sliced; fImg.color = Color.white;
        fImg.raycastTarget = false;
        fillGO.GetComponent<LayoutElement>().ignoreLayout = true;
        fillGO.transform.SetSiblingIndex(1);

        nameLabel = null;

        if (withName)
        {
            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(root.transform, false);
            nameLabel = nameGO.AddComponent<TextMeshProUGUI>();
            nameLabel.fontSize  = 22;
            nameLabel.fontStyle = FontStyles.Bold;
            nameLabel.alignment = TextAlignmentOptions.Center;
            nameLabel.color     = accentColor;
            nameLabel.raycastTarget = false;
            nameLabel.enableWordWrapping = false;
            var nameLE = nameGO.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 32;
        }

        var bodyGO = new GameObject("Body", typeof(RectTransform));
        bodyGO.transform.SetParent(root.transform, false);
        bodyText = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyText.fontSize  = 34;
        bodyText.fontStyle = FontStyles.Bold;
        bodyText.alignment = TextAlignmentOptions.Center;
        bodyText.enableWordWrapping = true;
        bodyText.enableAutoSizing = false; // tamaño fijo: la burbuja crece con el contenido, no al revés
        bodyText.raycastTarget = withRaycast;
        // Sin contorno oscuro: eso era para leerse sobre fotos/fondos oscuros;
        // sobre pergamino claro un contorno negro se ve sucio, no hace falta.

        return root;
    }
}
