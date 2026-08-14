using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Menú de niveles — reemplaza al mundo abierto explorable. Casillas 2-6
// aparecen bloqueadas ("Bientôt") hasta que se construyan sus niveles.
public class LevelSelectUI : MonoBehaviour
{
    public static LevelSelectUI Instance { get; private set; }

    static readonly Color BgDeep    = new Color(0.012f, 0.031f, 0.059f, 0.97f);
    static readonly Color NavyMid   = new Color(0.027f, 0.067f, 0.118f, 0.98f);
    static readonly Color NavyLight = new Color(0.051f, 0.122f, 0.208f, 1.00f);
    static readonly Color Gold      = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color Cream     = new Color(0.941f, 0.902f, 0.827f, 1.00f);
    static readonly Color CreamDim  = new Color(0.941f, 0.902f, 0.827f, 0.55f);
    static readonly Color LockedBg  = new Color(0.05f,  0.05f,  0.06f,  0.6f);
    static readonly Color TextDark  = new Color(0.012f, 0.031f, 0.059f, 1f);

    GameObject root;
    GameObject gridPanel;
    GameObject briefPanel;
    TMP_Text   briefTitleText;
    TMP_Text   briefSummaryText;
    TMP_Text   briefStarsText;
    TMP_Text   startBtnLabel;
    TMP_Text   briefBackBtnLabel;
    LevelDefinition selectedLevel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        root.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        root.SetActive(true);
        RefreshGrid();
        ShowGrid();
    }

    public void Hide()
    {
        root.SetActive(false);
        gameObject.SetActive(false);
    }

    void ShowGrid()
    {
        gridPanel.SetActive(true);
        briefPanel.SetActive(false);
    }

    void OpenBrief(LevelDefinition def)
    {
        selectedLevel = def;
        gridPanel.SetActive(false);
        briefPanel.SetActive(true);
        briefTitleText.text   = def.DisplayTitle;
        briefSummaryText.text = def.DisplaySummary;

        bool es = GameSettings.IsBasic;
        int wordCount = def.targetWords.Count;
        int starSecs  = Mathf.CeilToInt(def.starTimeSeconds);
        // Nada de "★" aquí a propósito — ese glyph no existe en la fuente
        // TMP por defecto y se ve como un cuadrado vacío (mismo bug que ya
        // se corrigió en las estrellas de verdad, ver StarIcon.cs).
        briefStarsText.text = es
            ? $"- Consigue el objetivo\n- Captura las {wordCount} palabras clave\n- Hazlo en menos de {starSecs}s"
            : $"- Atteins l'objectif\n- Capture les {wordCount} mots clés\n- Fais-le en moins de {starSecs}s";

        startBtnLabel.text     = es ? "EMPEZAR" : "COMMENCER";
        briefBackBtnLabel.text = es ? "Volver" : "Retour";
    }

    void StartSelected()
    {
        if (selectedLevel == null) return;
        Hide();
        LevelManager.Instance?.StartLevel(selectedLevel.id);
    }

    void RefreshGrid()
    {
        foreach (Transform child in gridPanel.transform) Destroy(child.gameObject);

        MakeLabel(gridPanel.transform, "Title", GameSettings.IsBasic ? "NIVELES" : "NIVEAUX",
                  new Vector2(0, 780), new Vector2(600, 70),
                  44, Gold, TextAlignmentOptions.Center, FontStyles.Bold);

        // Back button — top-left, standard/discoverable position (the old
        // bottom "Fermer" was easy to miss, especially near screen edges).
        var backGO = new GameObject("BtnBack", typeof(RectTransform), typeof(Image));
        backGO.transform.SetParent(gridPanel.transform, false);
        var backRT = backGO.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0f, 1f);
        backRT.anchorMax = new Vector2(0f, 1f);
        backRT.pivot     = new Vector2(0f, 1f);
        backRT.anchoredPosition = new Vector2(24, -40);
        backRT.sizeDelta = new Vector2(150, 64);
        backGO.GetComponent<Image>().color = Color.clear;
        var backLbl = new GameObject("Label", typeof(RectTransform));
        backLbl.transform.SetParent(backGO.transform, false);
        var blRT = backLbl.GetComponent<RectTransform>();
        blRT.anchorMin = Vector2.zero; blRT.anchorMax = Vector2.one;
        blRT.anchoredPosition = Vector2.zero; blRT.sizeDelta = Vector2.zero;
        var backTxt = backLbl.AddComponent<TextMeshProUGUI>();
        backTxt.text = GameSettings.IsBasic ? "< Volver" : "< Retour";
        backTxt.fontSize = 26;
        backTxt.color = Cream;
        backTxt.alignment = TextAlignmentOptions.Left;
        backTxt.fontStyle = FontStyles.Bold;
        backTxt.raycastTarget = false;
        var backBtn = backGO.AddComponent<Button>();
        backBtn.targetGraphic = backGO.GetComponent<Image>();
        backBtn.onClick.AddListener(Hide);

        int col = 0, row = 0;
        const int perRow = 3;
        float startX = -280f, startY = 600f, stepX = 280f, stepY = 280f;

        // Campaña de 10 niveles en dos tramos: 1-5 débutant (con la mecánica
        // de lectura en francés), 6-10 avancé (sin cambios) — quien elige
        // avancé en el menú principal empieza directo en el 6, se salta el
        // apoyo de débutant. Ver GameSettings.IsBasic (fijado en
        // MainMenuController.PlayAdvanced/PlayBeginner y resincronizado por
        // LevelManager.StartLevel según el id jugado).
        int startId = GameSettings.IsBasic ? 1 : 6;
        int endId   = GameSettings.IsBasic ? 5 : 10;

        for (int id = startId; id <= endId; id++)
        {
            var def = LevelDatabase.Get(id);
            var pos = new Vector2(startX + col * stepX, startY - row * stepY);
            BuildLevelTile(gridPanel.transform, id, def, pos);
            col++;
            if (col >= perRow) { col = 0; row++; }
        }
    }

    void BuildLevelTile(Transform parent, int id, LevelDefinition def, Vector2 pos)
    {
        bool built = def != null;

        var go = new GameObject("Level" + id, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(220, 220);
        go.GetComponent<Image>().color = built ? NavyLight : LockedBg;

        var numGO = new GameObject("Num", typeof(RectTransform));
        numGO.transform.SetParent(go.transform, false);
        var nRT = numGO.GetComponent<RectTransform>();
        nRT.anchorMin = new Vector2(0, 0.4f); nRT.anchorMax = new Vector2(1, 1f);
        nRT.offsetMin = Vector2.zero; nRT.offsetMax = Vector2.zero;
        var numTxt = numGO.AddComponent<TextMeshProUGUI>();
        numTxt.text      = id.ToString();
        numTxt.fontSize  = 56;
        numTxt.color     = built ? Gold : CreamDim;
        numTxt.fontStyle = FontStyles.Bold;
        numTxt.alignment = TextAlignmentOptions.Center;

        var result = LevelManager.Instance?.GetBestResult(id);
        int stars  = result != null ? result.stars : 0;

        if (built)
        {
            var starSprite = StarIcon.GetSprite();
            for (int i = 0; i < 3; i++)
            {
                var starGO = new GameObject("Star" + i, typeof(RectTransform), typeof(Image));
                starGO.transform.SetParent(go.transform, false);
                var sRT = starGO.GetComponent<RectTransform>();
                sRT.anchorMin = sRT.anchorMax = new Vector2(0.5f, 0.16f);
                sRT.pivot     = new Vector2(0.5f, 0.5f);
                sRT.anchoredPosition = new Vector2(-46 + i * 46, 0);
                sRT.sizeDelta = new Vector2(38, 38);
                var starImg = starGO.GetComponent<Image>();
                starImg.sprite = starSprite;
                starImg.color  = i < stars ? Gold : new Color(1f, 0.82f, 0.4f, 0.25f);
                starImg.raycastTarget = false;
            }
        }
        else
        {
            var lockedGO = new GameObject("Locked", typeof(RectTransform));
            lockedGO.transform.SetParent(go.transform, false);
            var lRT = lockedGO.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0f); lRT.anchorMax = new Vector2(1, 0.4f);
            lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;
            var lockedTxt = lockedGO.AddComponent<TextMeshProUGUI>();
            lockedTxt.text      = GameSettings.IsBasic ? "Próximamente" : "Bientôt";
            lockedTxt.fontSize  = 19;
            lockedTxt.color     = CreamDim;
            lockedTxt.alignment = TextAlignmentOptions.Center;
        }

        if (built)
        {
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            btn.onClick.AddListener(() => OpenBrief(def));
        }
    }

    void BuildUI()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 180;

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

        // ── Grid panel ──
        gridPanel = new GameObject("GridPanel", typeof(RectTransform));
        gridPanel.transform.SetParent(root.transform, false);
        var gpRT = gridPanel.GetComponent<RectTransform>();
        gpRT.anchorMin = Vector2.zero; gpRT.anchorMax = Vector2.one;
        gpRT.offsetMin = Vector2.zero; gpRT.offsetMax = Vector2.zero;

        // ── Brief panel ──
        briefPanel = new GameObject("BriefPanel", typeof(RectTransform));
        briefPanel.transform.SetParent(root.transform, false);
        var bpRT = briefPanel.GetComponent<RectTransform>();
        bpRT.anchorMin = Vector2.zero; bpRT.anchorMax = Vector2.one;
        bpRT.offsetMin = Vector2.zero; bpRT.offsetMax = Vector2.zero;

        var briefCard = new GameObject("Card", typeof(RectTransform), typeof(Image));
        briefCard.transform.SetParent(briefPanel.transform, false);
        var bcRT = briefCard.GetComponent<RectTransform>();
        bcRT.anchorMin = bcRT.anchorMax = new Vector2(0.5f, 0.5f);
        bcRT.pivot     = new Vector2(0.5f, 0.5f);
        bcRT.anchoredPosition = Vector2.zero;
        bcRT.sizeDelta = new Vector2(620, 580);
        briefCard.GetComponent<Image>().color = NavyMid;

        briefTitleText = MakeLabel(briefCard.transform, "BriefTitle", "", new Vector2(0, 170), new Vector2(540, 100),
                                    29, Gold, TextAlignmentOptions.Center, FontStyles.Bold);
        briefTitleText.enableWordWrapping = true;

        briefSummaryText = MakeLabel(briefCard.transform, "BriefSummary", "", new Vector2(0, 65), new Vector2(500, 100),
                                      24, Cream, TextAlignmentOptions.Center, FontStyles.Normal);
        briefSummaryText.enableWordWrapping = true;

        // Las 3 condiciones de estrella, explícitas, antes de empezar —
        // resuelve la duda de "qué necesito hacer para sacar más o menos
        // estrellas" sin tener que adivinarlo jugando.
        briefStarsText = MakeLabel(briefCard.transform, "BriefStars", "", new Vector2(0, -45), new Vector2(500, 120),
                                    20, CreamDim, TextAlignmentOptions.Center, FontStyles.Normal);
        briefStarsText.enableWordWrapping = true;

        var startBtnGO = MakeBtn(briefCard.transform, "BtnStart", "COMMENCER", new Vector2(0, -150), new Vector2(380, 74),
                Gold, TextDark, 28, FontStyles.Bold, StartSelected);
        startBtnLabel = startBtnGO.GetComponentInChildren<TMP_Text>();

        var briefBackGO = MakeBtn(briefCard.transform, "BtnBack", "Retour", new Vector2(0, -225), new Vector2(240, 50),
                Color.clear, CreamDim, 21, FontStyles.Normal, ShowGrid);
        briefBackBtnLabel = briefBackGO.GetComponentInChildren<TMP_Text>();
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
