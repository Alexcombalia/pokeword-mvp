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
        statusText.text  = success
            ? (es ? "¡Objetivo conseguido!" : "Objectif réussi !")
            : (es ? "Se acabó el tiempo — inténtalo de nuevo." : "Temps écoulé — réessaie.");
        statusText.color = success ? Gold : Fail;

        // Por qué se ganó o no cada estrella, en concreto — no solo el
        // icono relleno/vacío, sino la razón (tiempo exacto, palabras que
        // faltaron), para que quede claro qué hacer mejor la próxima vez.
        int elapsedSec  = Mathf.FloorToInt(result.timeSeconds);
        int starSec     = Mathf.CeilToInt(level.starTimeSeconds);
        int totalWords  = level.targetWords.Count;
        int captured    = LevelManager.Instance != null ? LevelManager.Instance.CapturedTargetCount() : 0;
        int missing     = Mathf.Max(0, totalWords - captured);

        objectiveLineText.text = starObjective
            ? (es ? "Objetivo conseguido" : "Objectif atteint")
            : (es ? "No conseguiste el objetivo a tiempo" : "Objectif non atteint à temps");
        objectiveLineText.color = starObjective ? LineOk : LineFail;

        wordsLineText.text = starWords
            ? (es ? $"Capturaste las {totalWords} palabras" : $"Tu as capturé les {totalWords} mots")
            : (es ? $"Te faltaron {missing} palabra(s) ({captured}/{totalWords})"
                  : $"Il te manque {missing} mot(s) ({captured}/{totalWords})");
        wordsLineText.color = starWords ? LineOk : LineFail;

        speedLineText.text = success
            ? (starSpeed
                ? (es ? $"Lo hiciste en {elapsedSec}s (menos de {starSec}s)" : $"Fait en {elapsedSec}s (moins de {starSec}s)")
                : (es ? $"Tardaste {elapsedSec}s (necesitabas menos de {starSec}s)" : $"{elapsedSec}s (il fallait moins de {starSec}s)"))
            : (es ? "No hubo estrella de velocidad — no llegaste a tiempo" : "Pas d'étoile de vitesse — temps écoulé");
        speedLineText.color = starSpeed ? LineOk : LineFail;

        // Mismo aviso que la pista, una línea por muleta usada — para que
        // quede claro por qué se perdió cada estrella deducida.
        string hintLine = hintUsed ? (es ? "Pista usada (-1 estrella)" : "Indice utilisé (-1 étoile)") : "";
        string skipLine = skipPenalty ? (es ? "Saltos usados (-1 estrella)" : "Indices sautés (-1 étoile)") : "";
        hintText.text = string.Join("\n", new[] { hintLine, skipLine }.Where(s => !string.IsNullOrEmpty(s)));

        // Botón de siguiente nivel: solo si existe (1 estrella mínima
        // desbloquea el siguiente, regla ya cerrada en el diseño). Con solo
        // el Nivel 1 construido todavía no aparece — es normal, no un bug.
        pendingNext = (result.stars >= 1) ? LevelDatabase.Get(level.id + 1) : null;
        nextBtnGO.SetActive(pendingNext != null);
        if (pendingNext != null)
            nextBtnLabel.text = es ? "SIGUIENTE NIVEL >" : "NIVEAU SUIVANT >";

        retryBtnLabel.text = es ? "REINTENTAR" : "RÉESSAYER";
        menuBtnLabel.text  = es ? "Menú de niveles" : "Menu des niveaux";
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
        cardRT.sizeDelta = new Vector2(600, 780);
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

        nextBtnGO = MakeBtn(card.transform, "BtnNext", "NIVEAU SUIVANT >", new Vector2(0, -90), new Vector2(420, 68),
                Gold, TextDark, 25, FontStyles.Bold, NextLevel);
        nextBtnLabel = nextBtnGO.GetComponentInChildren<TMP_Text>();
        nextBtnGO.SetActive(false);

        var retryBtnGO = MakeBtn(card.transform, "BtnRetry", "RÉESSAYER", new Vector2(0, -180), new Vector2(400, 62),
                new Color(0.051f, 0.122f, 0.208f, 1f), Gold, 25, FontStyles.Bold, Retry);
        retryBtnLabel = retryBtnGO.GetComponentInChildren<TMP_Text>();

        var menuBtnGO = MakeBtn(card.transform, "BtnMenu", "Menu des niveaux", new Vector2(0, -260), new Vector2(360, 54),
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
