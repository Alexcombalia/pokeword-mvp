using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Overlay shown only while a nivel/reto is active: cronómetro + panel de
// palabras objetivo. Palabras en negro por defecto, negrita + dorado al
// capturarlas (sin tachado, según indicación explícita del usuario).
//
// Estética pergamino (ver UXUI_pergamino.jpeg, carpeta Pokeword): todos los
// paneles de esta pantalla usan la misma textura de papel + borde tinta que
// SpeechCaptionUI, en vez del azul marino original.
public class LevelHUD : MonoBehaviour
{
    public static LevelHUD Instance { get; private set; }

    static readonly Color InkBrown   = new Color(0.235f, 0.161f, 0.098f, 1f); // #3C291A — texto general sobre pergamino
    static readonly Color GoldInk    = new Color(0.612f, 0.420f, 0.039f, 1f); // #9C6B0A — mismo tono que FrenchHexOnLight

    // Tintes que se aplican SOBRE la textura de pergamino (multiplican), no
    // colores planos — así el panel de "objetivo conseguido" sigue viéndose
    // a papel, solo que con un matiz verdoso.
    static readonly Color ParchmentDefault = Color.white;
    static readonly Color ParchmentSuccess = new Color(0.72f, 0.86f, 0.70f, 1f);

    // Mismo color por idioma que en las burbujas de diálogo y el cuaderno
    // (SpeechCaptionUI.FrenchColor/SpanishColor), pero en tono oscuro — el
    // panel de palabras tiene fondo claro y el dorado/azul claro original
    // no se lee bien ahí (mismo motivo por el que ya se cambió el dorado
    // por verde en esta pantalla). "Capturada" se indica con negrita, ya
    // que el color ahora identifica el idioma, no el progreso.
    const string FrenchHexOnLight  = "#9C6B0A";
    const string SpanishHexOnLight = "#1F5C8A";
    const string CapturedGreenHex  = "#1E8A3C"; // palabra en francés ya capturada
    static readonly Color TimerLow = new Color(0.85f, 0.20f, 0.20f, 1f);

    GameObject root;
    TMP_Text   timerText;
    GameObject objBg;
    TMP_Text   objectiveText;
    TMP_Text   starsLineText;
    GameObject wordPanel;
    GameObject entriesContainer;
    TMP_Text   headerTxt;
    List<TMP_Text> wordEntries = new List<TMP_Text>();

    LevelDefinition builtForLevel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
        root.SetActive(false);
    }

    void Update()
    {
        var lm = LevelManager.Instance;
        bool active = lm != null && lm.IsActive;

        if (active != root.activeSelf) root.SetActive(active);
        if (!active) { builtForLevel = null; return; }

        if (builtForLevel != lm.CurrentLevel) BuildWordPanel(lm.CurrentLevel);

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, lm.TimeRemaining));
        timerText.text  = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        timerText.color = totalSeconds <= 10 ? TimerLow : GoldInk;

        // Recordatorio del objetivo SIEMPRE visible mientras dura el nivel —
        // a diferencia de las pistas de CoachMarks (que se ven una sola vez
        // y desaparecen), esto está ahí todo el rato para quien llega sin
        // ninguna idea de qué hacer. Cambia a estado "conseguido" (verde)
        // en cuanto se cumple el objetivo, con instrucción de cómo terminar.
        if (objectiveText != null && lm.CurrentLevel != null)
        {
            bool es = GameSettings.IsBasic;
            bool hasCheckpoints = lm.CurrentLevel.checkpoints != null;

            if (lm.ObjectiveComplete)
            {
                objBg.GetComponent<Image>().color = ParchmentSuccess;
                objectiveText.text = hasCheckpoints
                    ? BuildCheckpointText(lm) + "\n" + (es
                        ? "¡Completado! Cierra la conversación (X) cuando quieras."
                        : "Terminé ! Ferme la conversation (X) quand tu veux.")
                    : (es
                        ? "¡Objetivo conseguido! Sigue hablando para coger más palabras,\no cierra la conversación (X) cuando quieras terminar."
                        : "Objectif atteint ! Continue à parler pour choper plus de mots,\nou ferme la conversation (X) quand tu veux terminer.");
            }
            else
            {
                objBg.GetComponent<Image>().color = ParchmentDefault;
                objectiveText.text = hasCheckpoints ? BuildCheckpointText(lm) : lm.CurrentLevel.DisplaySummary;
            }

            int wordCount = lm.CurrentLevel.targetWords.Count;
            int starSecs  = Mathf.CeilToInt(lm.CurrentLevel.starTimeSeconds);
            starsLineText.text = es
                ? $"Objetivo  ·  {wordCount} palabras  ·  menos de {starSecs}s"
                : $"Objectif  ·  {wordCount} mots  ·  moins de {starSecs}s";
        }

        RefreshWordColors(lm);
    }

    // Lista de retos del nivel (checklist) — mismo patrón negrita+verde que
    // ya usa RefreshWordColors para las palabras capturadas (evita glifos
    // tipo ☐/✅ que podrían no existir en la fuente TMP, mismo motivo por el
    // que "★"/"🔒" tampoco se usan como texto en este proyecto).
    string BuildCheckpointText(LevelManager lm)
    {
        var checkpoints = lm.CurrentLevel.checkpoints;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < checkpoints.Count; i++)
        {
            var cp = checkpoints[i];
            bool done = lm.CheckpointsReached.Contains(cp.id);
            string colorHex = done ? CapturedGreenHex : FrenchHexOnLight;
            string line = $"<color={colorHex}>{cp.DisplayLabel}</color>";
            sb.Append(done ? $"<b>{line}</b>" : line);
            if (i < checkpoints.Count - 1) sb.Append('\n');
        }
        return sb.ToString();
    }

    void RefreshWordColors(LevelManager lm)
    {
        if (lm.CurrentLevel == null) return;
        bool showSpanish = GameSettings.IsBasic;

        for (int i = 0; i < lm.CurrentLevel.targetWords.Count && i < wordEntries.Count; i++)
        {
            var tw = lm.CurrentLevel.targetWords[i];
            bool captured = lm.IsWordCaptured(tw.french);

            string french = $"<color={(captured ? CapturedGreenHex : FrenchHexOnLight)}>{tw.french}</color>";
            string line = showSpanish
                ? $"{french}  —  <color={SpanishHexOnLight}>{tw.spanish}</color>"
                : french;

            wordEntries[i].text      = captured ? $"<b>{line}</b>" : line;
            wordEntries[i].fontStyle = FontStyles.Normal; // el <b> ya aplica negrita solo a lo capturado
        }
    }

    void BuildWordPanel(LevelDefinition def)
    {
        builtForLevel = def;
        headerTxt.text = GameSettings.IsBasic ? "Palabras a encontrar" : "Mots à trouver";
        foreach (Transform child in entriesContainer.transform) Destroy(child.gameObject);
        wordEntries.Clear();

        if (def == null) return;

        float y = 0f;
        foreach (var tw in def.targetWords)
        {
            var go = new GameObject("Word_" + tw.french, typeof(RectTransform));
            go.transform.SetParent(entriesContainer.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, y);
            rt.sizeDelta = new Vector2(0, 36);

            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text      = tw.french;
            txt.fontSize  = 26;
            txt.color     = InkBrown;
            txt.alignment = TextAlignmentOptions.Left;
            txt.enableWordWrapping = false;
            txt.overflowMode = TextOverflowModes.Overflow;
            txt.enableAutoSizing = true;
            txt.fontSizeMin = 15;
            txt.fontSizeMax = 26;
            wordEntries.Add(txt);

            y -= 38f;
        }

        // Más ancho en modo débutant — hay que caber francés + español en
        // la misma línea, no solo la palabra francesa.
        var panelRT = wordPanel.GetComponent<RectTransform>();
        float panelWidth = GameSettings.IsBasic ? 400f : 300f;
        panelRT.sizeDelta = new Vector2(panelWidth, Mathf.Max(110f, -y + 56f));

        // Panel's own anchoredPosition.y is -265 (pivot top-right); place the
        // coachmark just below its dynamic bottom edge.
        float panelBottomY = -265f - panelRT.sizeDelta.y;
        CoachMarks.Instance?.ShowOnce("word_panel",
            GameSettings.IsBasic
                ? "Estas palabras van a salir en la conversación.\n¡Tócalas para capturarlas!"
                : "Ces mots vont apparaître dans la conversation.\nTouche-les pour les capturer !",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16, panelBottomY - 10f), new Vector2(280, 100));
    }

    void BuildCanvas()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120; // below DialogueSystem (400) / QuizUI (200), above base HUD

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

        // ── Objective banner — very top, always visible for the whole level ──
        objBg = new GameObject("ObjectiveBg", typeof(RectTransform), typeof(Image));
        objBg.transform.SetParent(root.transform, false);
        var objRT = objBg.GetComponent<RectTransform>();
        objRT.anchorMin = objRT.anchorMax = new Vector2(0.5f, 1f);
        objRT.pivot     = new Vector2(0.5f, 1f);
        objRT.anchoredPosition = new Vector2(0, -20);
        objRT.sizeDelta = new Vector2(960, 150);
        var objImg = objBg.GetComponent<Image>();
        objImg.sprite = ParchmentSprite.GetPaper(); objImg.type = Image.Type.Sliced; objImg.color = ParchmentDefault;
        AddInkBorder(objBg, 6f);

        var objTxtGO = new GameObject("ObjectiveText", typeof(RectTransform));
        objTxtGO.transform.SetParent(objBg.transform, false);
        var otRT = objTxtGO.GetComponent<RectTransform>();
        otRT.anchorMin = new Vector2(0f, 1f); otRT.anchorMax = new Vector2(1f, 1f);
        otRT.pivot     = new Vector2(0.5f, 1f);
        otRT.anchoredPosition = new Vector2(0, -8);
        otRT.sizeDelta = new Vector2(-40, 100);
        objectiveText = objTxtGO.AddComponent<TextMeshProUGUI>();
        objectiveText.fontSize  = 26;
        objectiveText.color     = InkBrown;
        objectiveText.alignment = TextAlignmentOptions.Center;
        objectiveText.fontStyle = FontStyles.Bold;
        objectiveText.enableWordWrapping = true;
        objectiveText.enableAutoSizing = true;
        objectiveText.fontSizeMin = 19;
        objectiveText.fontSizeMax = 27;

        // Segunda línea, siempre visible: qué necesitas para cada una de
        // las 3 estrellas — la duda explícita de "cómo sé si lo hago mejor
        // o peor" queda resuelta con esto en pantalla todo el rato.
        var starsLineGO = new GameObject("StarsLine", typeof(RectTransform));
        starsLineGO.transform.SetParent(objBg.transform, false);
        var slRT = starsLineGO.GetComponent<RectTransform>();
        slRT.anchorMin = new Vector2(0f, 0f); slRT.anchorMax = new Vector2(1f, 0f);
        slRT.pivot     = new Vector2(0.5f, 0f);
        slRT.anchoredPosition = new Vector2(0, 10);
        slRT.sizeDelta = new Vector2(-40, 28);
        starsLineText = starsLineGO.AddComponent<TextMeshProUGUI>();
        starsLineText.fontSize  = 19;
        starsLineText.color     = InkBrown;
        starsLineText.alignment = TextAlignmentOptions.Center;
        starsLineText.fontStyle = FontStyles.Italic;

        // ── Timer — below the objective banner ──
        var timerBg = new GameObject("TimerBg", typeof(RectTransform), typeof(Image));
        timerBg.transform.SetParent(root.transform, false);
        var tbRT = timerBg.GetComponent<RectTransform>();
        tbRT.anchorMin = tbRT.anchorMax = new Vector2(0.5f, 1f);
        tbRT.pivot     = new Vector2(0.5f, 1f);
        tbRT.anchoredPosition = new Vector2(0, -185);
        tbRT.sizeDelta = new Vector2(160, 60);
        var timerBgImg = timerBg.GetComponent<Image>();
        timerBgImg.sprite = ParchmentSprite.GetPaper(); timerBgImg.type = Image.Type.Sliced; timerBgImg.color = ParchmentDefault;
        AddInkBorder(timerBg, 5f);

        var timerGO = new GameObject("TimerText", typeof(RectTransform));
        timerGO.transform.SetParent(timerBg.transform, false);
        var ttRT = timerGO.GetComponent<RectTransform>();
        ttRT.anchorMin = Vector2.zero; ttRT.anchorMax = Vector2.one;
        ttRT.offsetMin = Vector2.zero; ttRT.offsetMax = Vector2.zero;
        timerText = timerGO.AddComponent<TextMeshProUGUI>();
        timerText.fontSize  = 38;
        timerText.color     = GoldInk;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontStyle = FontStyles.Bold;

        // ── Word panel — pergamino, lado derecho ──
        wordPanel = new GameObject("WordPanel", typeof(RectTransform), typeof(Image));
        wordPanel.transform.SetParent(root.transform, false);
        var wpRT = wordPanel.GetComponent<RectTransform>();
        wpRT.anchorMin = new Vector2(1f, 1f);
        wpRT.anchorMax = new Vector2(1f, 1f);
        wpRT.pivot     = new Vector2(1f, 1f);
        wpRT.anchoredPosition = new Vector2(-16, -265);
        wpRT.sizeDelta = new Vector2(300, 160);
        var wpImg = wordPanel.GetComponent<Image>();
        wpImg.sprite = ParchmentSprite.GetPaper(); wpImg.type = Image.Type.Sliced; wpImg.color = ParchmentDefault;
        AddInkBorder(wordPanel, 5f);

        var headerGO = new GameObject("Header", typeof(RectTransform));
        headerGO.transform.SetParent(wordPanel.transform, false);
        var hRT = headerGO.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 1f);
        hRT.anchorMax = new Vector2(1f, 1f);
        hRT.pivot     = new Vector2(0.5f, 1f);
        hRT.anchoredPosition = new Vector2(0, -10);
        hRT.sizeDelta = new Vector2(-20, 34);
        headerTxt = headerGO.AddComponent<TextMeshProUGUI>();
        headerTxt.text      = "Mots à trouver";
        headerTxt.fontSize  = 25;
        headerTxt.color     = InkBrown;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.enableAutoSizing = true;
        headerTxt.fontSizeMin = 18;
        headerTxt.fontSizeMax = 25;

        entriesContainer = new GameObject("Entries", typeof(RectTransform));
        entriesContainer.transform.SetParent(wordPanel.transform, false);
        var ecRT = entriesContainer.GetComponent<RectTransform>();
        ecRT.anchorMin = new Vector2(0f, 1f);
        ecRT.anchorMax = new Vector2(1f, 1f);
        ecRT.pivot     = new Vector2(0.5f, 1f);
        ecRT.anchoredPosition = new Vector2(0, -48);
        ecRT.offsetMin = new Vector2(20, ecRT.offsetMin.y);
        ecRT.offsetMax = new Vector2(-20, ecRT.offsetMax.y);
    }

    // Marco tipo tinta detrás de un panel de tamaño fijo — mismo efecto que
    // el borde de las burbujas de SpeechCaptionUI, pero sin VerticalLayoutGroup
    // porque estos paneles no cambian de tamaño según su contenido.
    static void AddInkBorder(GameObject panel, float pad)
    {
        var pRT = panel.GetComponent<RectTransform>();
        var border = new GameObject(panel.name + "Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(panel.transform.parent, false);
        border.transform.SetSiblingIndex(panel.transform.GetSiblingIndex());

        var bRT = border.GetComponent<RectTransform>();
        bRT.anchorMin = pRT.anchorMin; bRT.anchorMax = pRT.anchorMax; bRT.pivot = pRT.pivot;
        bRT.anchoredPosition = pRT.anchoredPosition;
        bRT.sizeDelta = pRT.sizeDelta + new Vector2(pad * 2f, pad * 2f);

        var img = border.GetComponent<Image>();
        img.sprite = ParchmentSprite.GetInk(); img.type = Image.Type.Sliced; img.color = Color.white;
    }
}
