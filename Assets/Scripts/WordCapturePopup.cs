using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordCapturePopup : MonoBehaviour
{
    public static WordCapturePopup Instance { get; private set; }

    // Pergamino, igual que SpeechCaptionUI/LevelHUD (ver UXUI_pergamino.jpeg).
    static readonly Color GoldInk    = new Color(0.612f, 0.420f, 0.039f, 1f); // #9C6B0A
    static readonly Color SpanishInk = new Color(0.122f, 0.361f, 0.541f, 1f); // #1F5C8A
    static readonly Color InkBrown   = new Color(0.235f, 0.161f, 0.098f, 1f); // #3C291A
    // Blanco casi puro — la tinta marrón oscura sobre el pergamino (que en
    // pantalla real queda más oscuro que un tono plano por el veteado/sombreado
    // de ParchmentSprite) no se leía bien. La traducción es lo que el jugador
    // más necesita leer de un vistazo, así que aquí sí se prioriza contraste
    // máximo sobre encajar con la paleta "tinta sobre papel" del resto de la carta.
    static readonly Color TranslationHighlight = new Color(0.98f, 0.96f, 0.90f, 1f);

    GameObject card;
    TMP_Text   frText;
    TMP_Text   esText;
    TMP_Text   ctxText;
    Coroutine  hideRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
        card.SetActive(false);
    }

    public void Show(string french, string spanish, string context = "")
    {
        frText.text  = french;
        esText.text  = $"= {spanish}";
        ctxText.text = !string.IsNullOrEmpty(context) ? $"« {context} »" : "";

        card.SetActive(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        float duration = GameSettings.IsBasic ? 3.5f : 2f;
        hideRoutine = StartCoroutine(HideAfter(duration));
    }

    IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        card.SetActive(false);
    }

    void BuildCanvas()
    {
        var cv              = gameObject.AddComponent<Canvas>();
        cv.renderMode       = RenderMode.ScreenSpaceOverlay;
        // Por encima de SpeechCaptionUI (405): la burbuja de Jacques puede
        // ocupar buena parte de la zona central/alta de la pantalla, y con
        // un sortingOrder menor esta tarjeta quedaba tapada detrás — no se
        // veía la palabra recién capturada.
        cv.sortingOrder     = 410;

        var scaler                 = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0f;
        gameObject.AddComponent<GraphicRaycaster>();

        // Card — dead-center, so it's unmistakable a word was just captured
        card = new GameObject("Card", typeof(RectTransform));
        card.transform.SetParent(transform, false);
        var cRT = card.GetComponent<RectTransform>();
        cRT.anchorMin        = new Vector2(0.5f, 0.5f);
        cRT.anchorMax        = new Vector2(0.5f, 0.5f);
        cRT.pivot            = new Vector2(0.5f, 0.5f);
        // Arriba del todo, por encima de donde vive la burbuja de Jacques y
        // el panel de palabras, para que nunca quede tapada ni a medias.
        cRT.anchoredPosition = new Vector2(0, 700);
        cRT.sizeDelta        = new Vector2(460, 130);

        var bg = card.AddComponent<Image>();
        bg.sprite = ParchmentSprite.GetPaper(); bg.type = Image.Type.Sliced; bg.color = Color.white;

        // Borde tinta detrás — HIJO de card (no hermano): así hereda su
        // activo/inactivo con card.SetActive(...) y no se queda visible todo
        // el rato en pantalla como un cuadrado marrón suelto.
        var borderGO = new GameObject("CardBorder", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(card.transform, false);
        borderGO.transform.SetAsFirstSibling();
        var borderRT = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero; borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-5, -5); borderRT.offsetMax = new Vector2(5, 5);
        var borderImg = borderGO.GetComponent<Image>();
        borderImg.sprite = ParchmentSprite.GetInk(); borderImg.type = Image.Type.Sliced; borderImg.color = Color.white;

        // "+1 mot !" label
        var plusGO = Make("Plus", card.transform);
        Anchor(plusGO, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
               new Vector2(18, -14), new Vector2(80, 24));
        var plusT = plusGO.AddComponent<TextMeshProUGUI>();
        plusT.text = "+1 mot !"; plusT.fontSize = 16; plusT.color = GoldInk;
        plusT.fontStyle = FontStyles.Bold; plusT.alignment = TextAlignmentOptions.Left;

        // French word — grande, tinta dorada
        var frGO = Make("French", card.transform);
        Anchor(frGO, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
               new Vector2(0, -16), new Vector2(0, 52));
        frText = frGO.AddComponent<TextMeshProUGUI>();
        frText.fontSize = 38; frText.color = GoldInk;
        frText.fontStyle = FontStyles.Bold; frText.alignment = TextAlignmentOptions.Center;

        // Spanish translation — debajo, blanco cálido en negrita para que se
        // lea con contraste fuerte sobre el pergamino sea cual sea el matiz
        // que le toque por el veteado (ver TranslationHighlight arriba).
        var esGO = Make("Spanish", card.transform);
        Anchor(esGO, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
               new Vector2(0, -72), new Vector2(0, 34));
        esText = esGO.AddComponent<TextMeshProUGUI>();
        esText.fontSize = 27; esText.color = TranslationHighlight;
        esText.fontStyle = FontStyles.Bold;
        esText.alignment = TextAlignmentOptions.Center;

        // Context sentence — italic small at bottom
        var ctxGO = Make("Context", card.transform);
        Anchor(ctxGO, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
               new Vector2(0, 6), new Vector2(-20, 24));
        ctxText = ctxGO.AddComponent<TextMeshProUGUI>();
        ctxText.fontSize = 17; ctxText.color = SpanishInk;
        ctxText.fontStyle = FontStyles.Italic;
        ctxText.alignment = TextAlignmentOptions.Center;
        ctxText.enableWordWrapping = true;
    }

    static GameObject Make(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Anchor(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                       Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
    }
}
