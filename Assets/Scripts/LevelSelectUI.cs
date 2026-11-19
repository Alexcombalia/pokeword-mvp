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
    TMP_Text   briefStarsHeader;
    TMP_Text[] briefStarLines = new TMP_Text[3];
    TMP_Text   briefHintNote;
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

        // Las tres estrellas, cada una en su línea y con su icono de estrella
        // de verdad (StarIcon: el glyph "★" no existe en la fuente TMP por
        // defecto y sale como un cuadrado vacío).
        //
        // Antes era una lista de tres guiones sin encabezado, y en las pruebas
        // nadie entendía que se pudiera salir del nivel con 1, 2 o 3 — se leía
        // como una lista de deberes obligatorios. Ahora se dice el rango con
        // todas las letras, se marca cuál es la única obligatoria, y las otras
        // dos quedan como lo que son: retos que puedes dejar pasar.
        //
        // El texto de la 2ª cambia según el nivel: en un caso avanzado la
        // estrella es romper al mentiroso, no capturar palabras. Prometer una
        // cosa y puntuar otra es la peor forma de perder una estrella.
        bool   liarStar = !string.IsNullOrEmpty(def.secondStarCheckpoint);
        string starTime = TimeText.Threshold(def.starTimeSeconds);

        briefStarsHeader.text = LangText.Pick(
            "Puedes acabar con 1, 2 o 3 estrellas",
            "Tu peux finir avec 1, 2 ou 3 étoiles",
            "You can finish with 1, 2 or 3 stars",
            "Puoi finire con 1, 2 o 3 stelle",
            "Pots acabar amb 1, 2 o 3 estrelles");

        briefStarLines[0].text = (def.detectiveMode
            ? LangText.Pick("Resuelve el caso", "Résous l'affaire", "Solve the case", "Risolvi il caso", "Resol el cas")
            : LangText.Pick("Consigue el objetivo", "Atteins l'objectif", "Reach the objective", "Raggiungi l'obiettivo", "Aconsegueix l'objectiu"))
            + "   " + LangText.Pick("(obligatoria)", "(obligatoire)", "(required)", "(obbligatoria)", "(obligatòria)");

        briefStarLines[1].text = liarStar
            ? LangText.Pick("Haz confesar al mentiroso", "Fais craquer le menteur",
                            "Break the liar", "Fai crollare il bugiardo", "Fes caure el mentider")
            : LangText.Pick($"Habla {def.spokenWordsTarget} palabras en voz alta",
                            $"Dis {def.spokenWordsTarget} mots à voix haute",
                            $"Speak {def.spokenWordsTarget} words out loud",
                            $"Di' {def.spokenWordsTarget} parole ad alta voce",
                            $"Digues {def.spokenWordsTarget} paraules en veu alta");

        briefStarLines[2].text = LangText.Pick($"Hazlo en menos de {starTime}", $"Fais-le en moins de {starTime}",
                                               $"Do it in under {starTime}", $"Fallo in meno di {starTime}", $"Fes-ho en menys de {starTime}");

        // Solo donde hay botón de pista: si no, es una regla sobre algo que el
        // jugador no va a ver en este nivel.
        briefHintNote.text = def.hintAvailable
            ? LangText.Pick("Pedir una pista te cuesta una estrella.",
                            "Demander un indice coûte une étoile.",
                            "Asking for a hint costs you a star.",
                            "Chiedere un indizio costa una stella.",
                            "Demanar una pista et costa una estrella.")
            : "";

        startBtnLabel.text     = LangText.Pick("EMPEZAR", "COMMENCER", "START", "INIZIA", "COMENÇAR");
        briefBackBtnLabel.text = LangText.Pick("Volver", "Retour", "Back", "Indietro", "Tornar");
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

        MakeLabel(gridPanel.transform, "Title", LangText.Pick("NIVELES", "NIVEAUX", "LEVELS", "LIVELLI", "NIVELLS"),
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
        backTxt.text = LangText.Pick("< Volver", "< Retour", "< Back", "< Indietro", "< Tornar");
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
        // Los casos del modo detective son los niveles 6-10 por dentro, pero
        // el jugador los ve como "Caso 1..5": para él son los casos, no la
        // segunda mitad de una campaña de diez.
        numTxt.text      = (def != null ? def.TileNumber : id).ToString();
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
            lockedTxt.text      = LangText.Pick("Próximamente", "Bientôt", "Coming soon", "Prossimamente", "Properament");
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
        bcRT.sizeDelta = new Vector2(620, 760);
        briefCard.GetComponent<Image>().color = NavyMid;

        briefTitleText = MakeLabel(briefCard.transform, "BriefTitle", "", new Vector2(0, 300), new Vector2(540, 90),
                                    29, Gold, TextAlignmentOptions.Center, FontStyles.Bold);
        briefTitleText.enableWordWrapping = true;

        // El resumen creció mucho: en las pruebas la gente no entendía que era
        // un detective, ni que había un posible asesinato, hasta que se lo
        // explicaban de viva voz. Ahora el brief lo dice con todas las letras,
        // y necesita sitio — antes cabían tres líneas.
        briefSummaryText = MakeLabel(briefCard.transform, "BriefSummary", "", new Vector2(0, 120), new Vector2(540, 250),
                                      23, Cream, TextAlignmentOptions.Center, FontStyles.Normal);
        briefSummaryText.enableWordWrapping = true;

        // Las 3 condiciones de estrella, explícitas y antes de empezar, cada
        // una con su estrella dibujada al lado. El encabezado es lo que de
        // verdad faltaba: sin él la lista se leía como tres obligaciones y
        // nadie sabía que se puede salir con una sola.
        briefStarsHeader = MakeLabel(briefCard.transform, "BriefStarsHeader", "", new Vector2(0, -40), new Vector2(540, 28),
                                      19, Gold, TextAlignmentOptions.Center, FontStyles.Bold);

        for (int i = 0; i < 3; i++)
        {
            var row = new GameObject("StarRow" + i, typeof(RectTransform));
            row.transform.SetParent(briefCard.transform, false);
            var rowRT = row.GetComponent<RectTransform>();
            rowRT.anchorMin = rowRT.anchorMax = new Vector2(0.5f, 0.5f);
            rowRT.pivot     = new Vector2(0.5f, 0.5f);
            rowRT.anchoredPosition = new Vector2(0, -78f - i * 32f);
            rowRT.sizeDelta = new Vector2(500, 30);

            var icon = new GameObject("Star", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(row.transform, false);
            var iRT = icon.GetComponent<RectTransform>();
            iRT.anchorMin = iRT.anchorMax = new Vector2(0f, 0.5f);
            iRT.pivot     = new Vector2(0f, 0.5f);
            iRT.anchoredPosition = Vector2.zero;
            iRT.sizeDelta = new Vector2(22, 22);
            var iImg = icon.GetComponent<Image>();
            iImg.sprite = StarIcon.GetSprite();
            iImg.color  = Gold;
            iImg.raycastTarget = false;

            var lbl = MakeLabel(row.transform, "Label", "", Vector2.zero, new Vector2(460, 30),
                                 19, Cream, TextAlignmentOptions.Left, FontStyles.Normal);
            var lRT = lbl.GetComponent<RectTransform>();
            lRT.anchorMin = lRT.anchorMax = new Vector2(0f, 0.5f);
            lRT.pivot     = new Vector2(0f, 0.5f);
            lRT.anchoredPosition = new Vector2(32, 0);
            briefStarLines[i] = lbl;
        }

        briefHintNote = MakeLabel(briefCard.transform, "BriefHintNote", "", new Vector2(0, -178), new Vector2(500, 26),
                                   16, CreamDim, TextAlignmentOptions.Center, FontStyles.Italic);

        var startBtnGO = MakeBtn(briefCard.transform, "BtnStart", "COMMENCER", new Vector2(0, -230), new Vector2(380, 74),
                Gold, TextDark, 28, FontStyles.Bold, StartSelected);
        startBtnLabel = startBtnGO.GetComponentInChildren<TMP_Text>();

        var briefBackGO = MakeBtn(briefCard.transform, "BtnBack", "Retour", new Vector2(0, -305), new Vector2(240, 50),
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
