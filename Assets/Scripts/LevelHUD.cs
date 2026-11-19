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

    GameObject timerBg;      // se recoloca en modo detective: el panel de arriba crece con el texto
    GameObject hintBtn;
    TMP_Text   hintBtnLabel;
    GameObject hintPanel;
    TMP_Text   hintPanelText;

    // Última cadena medida en el panel de objetivos — el alto solo se recalcula
    // cuando el texto cambia de verdad, no en cada frame.
    string lastFittedText;

    // ─── DÓNDE TERMINA EL PERGAMINO DE OBJETIVOS ─────────────────────────
    //
    // Y negativa desde el borde superior de la pantalla. La leen los botones
    // flotantes de la derecha (silenciar música, profesor) para colocarse por
    // debajo.
    //
    // Hace falta porque el panel NO tiene un alto fijo: crece con cada hallazgo
    // descubierto y, desde que la letra es más grande (16/09/2026), bastante
    // más. Con una posición fija, los botones acababan tapados a mitad de caso.
    //
    // Fuera de un nivel vale 0: en el menú no hay pergamino y los botones se
    // quedan donde los pusieron.
    float bannerBottom;
    public static float BannerBottomY =>
        (Instance != null && Instance.root != null && Instance.root.activeSelf)
            ? Instance.bannerBottom : 0f;

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

        bool detective = lm.CurrentLevel != null && lm.CurrentLevel.detectiveMode;

        // En modo detective el reloj sube y NUNCA se pone en rojo: no hay
        // cuenta atrás que meta prisa, solo un cronómetro que informa.
        int totalSeconds = detective
            ? Mathf.FloorToInt(lm.TimeElapsed)
            : Mathf.CeilToInt(Mathf.Max(0f, lm.TimeRemaining));
        timerText.text  = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        timerText.color = (!detective && totalSeconds <= 10) ? TimerLow : GoldInk;

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
                objectiveText.text = detective
                    ? BuildCaseText(lm) + "\n" + (es
                        ? LangText.Pick("Caso cerrado. Cierra la conversación (X) cuando quieras.",
                                        "Affaire classée. Ferme la conversation (X) quand tu veux.",
                                        "Case closed. Close the conversation (X) whenever you like.",
                                        "Caso chiuso. Chiudi la conversazione (X) quando vuoi.") : null)
                    : hasCheckpoints
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
                objectiveText.text = detective
                    ? BuildCaseText(lm)
                    : hasCheckpoints ? BuildCheckpointText(lm) : lm.CurrentLevel.DisplaySummary;
            }

            // Mismo criterio que la pantalla de resultado: la 2ª estrella solo
            // es la del careo si el nivel declara secondStarCheckpoint.
            if (detective && !string.IsNullOrEmpty(lm.CurrentLevel.secondStarCheckpoint))
            {
                string starTime = TimeText.Threshold(lm.CurrentLevel.starTimeSeconds);
                starsLineText.text = es
                    ? LangText.Pick($"Resuelto  ·  El mentiroso  ·  menos de {starTime}",
                                    $"Résolu  ·  Le menteur  ·  moins de {starTime}",
                                    $"Solved  ·  The liar  ·  under {starTime}",
                                    $"Risolto  ·  Il bugiardo  ·  meno di {starTime}",
                                    $"Resolt  ·  El mentider  ·  menys de {starTime}") : null;
            }
            else
            {
                // La 2ª estrella de los casos débutant dejó de ser vocabulario
                // y pasó a ser habla (30/08/2026): el HUD tiene que prometer lo
                // mismo que la pantalla de resultado va a puntuar.
                int spoken      = lm.WordsSpoken;
                int target      = lm.CurrentLevel.spokenWordsTarget;
                string starTime = TimeText.Threshold(lm.CurrentLevel.starTimeSeconds);
                starsLineText.text = es
                    ? LangText.Pick($"Objetivo  ·  hablar {spoken}/{target}  ·  menos de {starTime}",
                                    $"Objectif  ·  parler {spoken}/{target}  ·  moins de {starTime}",
                                    $"Objective  ·  speak {spoken}/{target}  ·  under {starTime}",
                                    $"Obiettivo  ·  parlare {spoken}/{target}  ·  meno di {starTime}",
                                    $"Objectiu  ·  parlar {spoken}/{target}  ·  menys de {starTime}") : null;
            }

            // El caso empieza con cuatro objetivos y va sumando hallazgos, así
            // que el panel no puede tener un alto fijo: o sobra papel al
            // principio o falta al final. Se ajusta al texto que haya ahora.
            if (detective) FitBannerToText();
        }

        RefreshWordColors(lm);
    }

    // Ajusta el alto del panel de objetivos a su contenido y baja con él todo
    // lo que cuelga debajo. Solo recalcula cuando el texto ha cambiado.
    void FitBannerToText()
    {
        if (objectiveText == null || objBg == null) return;
        if (objectiveText.text == lastFittedText) return;
        lastFittedText = objectiveText.text;

        var otRT = objectiveText.GetComponent<RectTransform>();

        // preferredHeight solo es fiable con el auto-size apagado: con él
        // encendido TMP encoge la letra para caber en el rect que ya tiene, y
        // devolvería el alto actual en vez del que hace falta.
        // En el primer frame el layout puede no haberse resuelto y rect.width
        // valer 0; medir con ancho 0 devolvería un alto disparatado.
        float width = otRT.rect.width > 1f ? otRT.rect.width : 920f;
        float textH = objectiveText.GetPreferredValues(objectiveText.text, width, 0f).y;
        otRT.sizeDelta = new Vector2(-40, textH);

        // 8 arriba + 12 de aire + 46 de la línea de estrellas (10 de margen
        // inferior + 36 de alto), los mismos valores con los que se construyó.
        float bannerH = Mathf.Clamp(textH + 66f, 130f, 900f);
        ResizePanel(objBg, new Vector2(960, bannerH), 6f);
        bannerBottom = -(20f + bannerH);

        // Mismas separaciones que en el diseño original (el panel medía 150 y
        // el reloj estaba 15 por debajo, el de palabras 95), pero calculadas
        // en vez de escritas a mano.
        float bottom = 20f + bannerH;
        MovePanel(timerBg,   new Vector2(0,   -(bottom + 15f)));
        MovePanel(wordPanel, new Vector2(-16, -(bottom + 95f)));
    }

    // Mueve un panel con su marco — mismo motivo que ResizePanel: el borde es
    // un hermano, no un hijo, y no se entera de nada por su cuenta.
    static void MovePanel(GameObject panel, Vector2 pos)
    {
        if (panel == null) return;
        panel.GetComponent<RectTransform>().anchoredPosition = pos;
        var border = panel.transform.parent.Find(panel.name + "Border");
        if (border != null) border.GetComponent<RectTransform>().anchoredPosition = pos;
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

    // Panel del modo detective: arriba los objetivos (qué hacer), abajo los
    // hallazgos que se van escribiendo solos en verde según se descubren.
    //
    // La zona verde no es adorno. En un juego de idiomas el esfuerzo de
    // comprensión se come el de deducción: quien está peleando por entender lo
    // que oye no tiene atención libre para notar que dos frases encajan. Ver
    // cada dato que ha entendido bien, escrito y en su sitio, le devuelve esa
    // atención — y poner "Marcel llevaba algo metálico" justo encima de "Marcel
    // dice que no lleva arma" hace visible la contradicción sin nombrarla.
    string BuildCaseText(LevelManager lm)
    {
        var sb = new System.Text.StringBuilder();
        var def = lm.CurrentLevel;

        if (def.objectives != null)
        {
            for (int i = 0; i < def.objectives.Count; i++)
            {
                var obj = def.objectives[i];
                bool done = true;
                foreach (var id in obj.requiredIds)
                    if (!lm.CheckpointsReached.Contains(id)) { done = false; break; }

                string line = $"<color={(done ? CapturedGreenHex : FrenchHexOnLight)}>{obj.DisplayLabel}</color>";
                sb.Append(done ? $"<b>{line}</b>" : line);
                if (i < def.objectives.Count - 1) sb.Append('\n');
            }
        }

        // Solo se listan los hallazgos YA descubiertos: la lista crece con la
        // investigación en vez de enseñar de entrada cuántos huecos quedan por
        // rellenar, que sería contarle al jugador cuánto le falta por deducir.
        if (def.findings != null)
        {
            bool first = true;
            foreach (var f in def.findings)
            {
                if (!lm.CheckpointsReached.Contains(f.id)) continue;
                if (first)
                {
                    sb.Append("\n\n<color=" + CapturedGreenHex + ">");
                    sb.Append(LangText.Pick("Lo que sabes:", "Ce que tu sais :", "What you know:", "Quello che sai:", "El que saps:"));
                    sb.Append("</color>");
                    first = false;
                }
                sb.Append($"\n<color={CapturedGreenHex}>· {f.DisplayText}</color>");
            }
        }

        return sb.ToString();
    }

    void RefreshWordColors(LevelManager lm)
    {
        if (lm.CurrentLevel == null) return;
        bool showSpanish = GameSettings.IsBasic;

        for (int i = 0; i < lm.CurrentLevel.ActiveTargetWords.Count && i < wordEntries.Count; i++)
        {
            var tw = lm.CurrentLevel.ActiveTargetWords[i];
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
        ApplyLayoutFor(def);

        // El panel de "palabras a encontrar" queda OCULTO desde el 02/09/2026.
        // Capturar vocabulario dejó de ser objetivo y dejó de dar estrella (la
        // 2ª es ahora de habla, ver LevelDefinition.spokenWordsTarget), así que
        // enseñar una lista de palabras pendientes prometía un reto que ya no
        // existe — y ocupaba la mejor esquina de la pantalla.
        //
        // Se oculta en vez de borrarse: el sistema de captura sigue vivo (el
        // cuaderno, la tarjeta de traducción, el tutorial) y el panel podría
        // volver como opcional. SetPanelVisible se lleva también el marco de
        // tinta, que es un hermano y no un hijo — apagar solo el panel dejaría
        // el recuadro flotando vacío.
        SetPanelVisible(wordPanel, false);
        headerTxt.text = LangText.Pick("Palabras a encontrar", "Mots à trouver", "Words to find", "Parole da trovare", "Paraules a trobar");
        foreach (Transform child in entriesContainer.transform) Destroy(child.gameObject);
        wordEntries.Clear();

        if (def == null) return;

        float y = 0f;
        foreach (var tw in def.ActiveTargetWords)
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

        // El aviso "estas palabras van a salir en la conversación, ¡tócalas!"
        // se retiró el 02/09/2026, junto con el panel al que señalaba: apuntaba
        // a un recuadro que ya no se dibuja, y salía flotando bajo el botón del
        // profesor sin nada debajo.
        //
        // Lo que explicaba lo enseña ahora el tutorial guiado, y mejor: con una
        // palabra de prueba que el jugador toca de verdad (GuidedTutorial,
        // paso 2), en vez de un cartel que se lee y se olvida.
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
        objectiveText.fontSize  = 32;
        objectiveText.color     = InkBrown;
        objectiveText.alignment = TextAlignmentOptions.Center;
        objectiveText.fontStyle = FontStyles.Bold;
        objectiveText.enableWordWrapping = true;
        objectiveText.enableAutoSizing = true;
        // El MÍNIMO es lo que de verdad manda: con el texto largo, TMP encoge
        // hasta ahí y ese era el cuerpo que se leía en la práctica.
        objectiveText.fontSizeMin = 25;
        objectiveText.fontSizeMax = 34;

        // Segunda línea, siempre visible: qué necesitas para cada una de
        // las 3 estrellas — la duda explícita de "cómo sé si lo hago mejor
        // o peor" queda resuelta con esto en pantalla todo el rato.
        var starsLineGO = new GameObject("StarsLine", typeof(RectTransform));
        starsLineGO.transform.SetParent(objBg.transform, false);
        var slRT = starsLineGO.GetComponent<RectTransform>();
        slRT.anchorMin = new Vector2(0f, 0f); slRT.anchorMax = new Vector2(1f, 0f);
        slRT.pivot     = new Vector2(0.5f, 0f);
        slRT.anchoredPosition = new Vector2(0, 10);
        slRT.sizeDelta = new Vector2(-40, 36);
        starsLineText = starsLineGO.AddComponent<TextMeshProUGUI>();
        starsLineText.fontSize  = 24;
        starsLineText.color     = InkBrown;
        starsLineText.alignment = TextAlignmentOptions.Center;
        starsLineText.fontStyle = FontStyles.Italic;

        // ── Timer — below the objective banner ──
        timerBg = new GameObject("TimerBg", typeof(RectTransform), typeof(Image));
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
        timerText.fontSize  = 46;
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

        // ── Botón de pista + panel donde se lee ──
        // Abajo a la izquierda, lejos del micro (abajo al centro) para que no
        // se pulse sin querer mientras se habla. Solo aparece en niveles con
        // hintAvailable; usarlo cuesta 1 estrella, una sola vez por muchas
        // pistas que se pidan.
        hintPanel = new GameObject("HintPanel", typeof(RectTransform), typeof(Image));
        hintPanel.transform.SetParent(root.transform, false);
        var hpRT = hintPanel.GetComponent<RectTransform>();
        hpRT.anchorMin = hpRT.anchorMax = new Vector2(0f, 0f);
        hpRT.pivot     = new Vector2(0f, 0f);
        hpRT.anchoredPosition = new Vector2(24, 108);
        hpRT.sizeDelta = new Vector2(560, 130);
        var hpImg = hintPanel.GetComponent<Image>();
        hpImg.sprite = ParchmentSprite.GetPaper(); hpImg.type = Image.Type.Sliced; hpImg.color = ParchmentDefault;
        AddInkBorder(hintPanel, 5f);

        var hpTxtGO = new GameObject("HintText", typeof(RectTransform));
        hpTxtGO.transform.SetParent(hintPanel.transform, false);
        var hptRT = hpTxtGO.GetComponent<RectTransform>();
        hptRT.anchorMin = Vector2.zero; hptRT.anchorMax = Vector2.one;
        hptRT.offsetMin = new Vector2(18, 12); hptRT.offsetMax = new Vector2(-18, -12);
        hintPanelText = hpTxtGO.AddComponent<TextMeshProUGUI>();
        hintPanelText.fontSize  = 29;
        hintPanelText.color     = InkBrown;
        hintPanelText.alignment = TextAlignmentOptions.Center;
        hintPanelText.enableWordWrapping = true;
        hintPanelText.enableAutoSizing = true;
        hintPanelText.fontSizeMin = 22;
        hintPanelText.fontSizeMax = 29;

        // Tocar el panel lo cierra — sin botón de cerrar, que en un móvil
        // sería un objetivo más pequeño que el propio panel.
        var hpBtn = hintPanel.AddComponent<Button>();
        hpBtn.targetGraphic = hpImg;
        hpBtn.onClick.AddListener(() => SetHintPanelVisible(false));
        SetHintPanelVisible(false);

        hintBtn = new GameObject("HintButton", typeof(RectTransform), typeof(Image));
        hintBtn.transform.SetParent(root.transform, false);
        var hbRT = hintBtn.GetComponent<RectTransform>();
        hbRT.anchorMin = hbRT.anchorMax = new Vector2(0f, 0f);
        hbRT.pivot     = new Vector2(0f, 0f);
        hbRT.anchoredPosition = new Vector2(24, 28);
        hbRT.sizeDelta = new Vector2(240, 64);
        var hbImg = hintBtn.GetComponent<Image>();
        hbImg.sprite = ParchmentSprite.GetPaper(); hbImg.type = Image.Type.Sliced; hbImg.color = ParchmentDefault;
        AddInkBorder(hintBtn, 5f);

        var hbTxtGO = new GameObject("HintButtonLabel", typeof(RectTransform));
        hbTxtGO.transform.SetParent(hintBtn.transform, false);
        var hbtRT = hbTxtGO.GetComponent<RectTransform>();
        hbtRT.anchorMin = Vector2.zero; hbtRT.anchorMax = Vector2.one;
        hbtRT.offsetMin = Vector2.zero; hbtRT.offsetMax = Vector2.zero;
        hintBtnLabel = hbTxtGO.AddComponent<TextMeshProUGUI>();
        hintBtnLabel.fontSize  = 26;
        hintBtnLabel.color     = InkBrown;
        hintBtnLabel.alignment = TextAlignmentOptions.Center;
        hintBtnLabel.fontStyle = FontStyles.Bold;
        hintBtnLabel.raycastTarget = false;

        var hintButton = hintBtn.AddComponent<Button>();
        hintButton.targetGraphic = hbImg;
        hintButton.onClick.AddListener(OnHintPressed);
        SetPanelVisible(hintBtn, false);
    }

    // Muestra u oculta un panel CON su marco. El borde que crea AddInkBorder
    // es un GameObject hermano, no un hijo, así que apagar solo el panel
    // dejaría un marco flotando vacío en pantalla.
    static void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel == null) return;
        panel.SetActive(visible);
        var border = panel.transform.parent.Find(panel.name + "Border");
        if (border != null) border.gameObject.SetActive(visible);
    }

    void SetHintPanelVisible(bool visible) => SetPanelVisible(hintPanel, visible);

    void OnHintPressed()
    {
        var lm = LevelManager.Instance;
        if (lm == null || !lm.IsActive || lm.CurrentLevel == null) return;

        var hint = lm.CurrentLevel.NextHint(lm.CheckpointsReached);
        if (hint == null)
        {
            hintPanelText.text = GameSettings.IsBasic
                ? "Ya lo tienes todo. Ve a cerrar el caso."
                : "Tu as tout ce qu'il faut. Va classer l'affaire.";
        }
        else
        {
            // La estrella solo se pierde si la pista dice algo de verdad —
            // pulsar el botón cuando ya no queda nada que pistar no penaliza.
            lm.UseHint();
            hintPanelText.text = hint.DisplayText;
        }

        SetHintPanelVisible(true);
        RefreshHintButtonLabel(lm);
    }

    void RefreshHintButtonLabel(LevelManager lm)
    {
        if (hintBtnLabel == null) return;
        bool es = GameSettings.IsBasic;
        hintBtnLabel.text = lm != null && lm.HintUsed
            ? (LangText.Pick("Pista", "Indice", "Hint", "Indizio", "Pista"))
            : (LangText.Pick("Pista  (-1 estrella)", "Indice  (-1 étoile)", "Hint  (-1 star)", "Indizio  (-1 stella)", "Pista  (-1 estrella)"));
    }

    // El HUD se construye una vez con la forma de un nivel débutant (una línea
    // de objetivo y ya). Un caso trae cuatro objetivos y hasta ocho hallazgos,
    // así que el panel de arriba pasa de 150 a 430 px y todo lo que colgaba
    // debajo baja con él. Se aplica al cambiar de nivel, no en cada frame.
    void ApplyLayoutFor(LevelDefinition def)
    {
        bool detective = def != null && def.detectiveMode;

        // En modo detective el texto es una lista, y una lista centrada se lee
        // mucho peor que alineada a la izquierda. Y el auto-size se apaga: el
        // panel crece para caber el texto, no al revés — encoger la letra para
        // meter cuatro objetivos y ocho hallazgos en un rect fijo acabaría en
        // un cuerpo ilegible.
        objectiveText.alignment        = detective ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Center;
        objectiveText.enableAutoSizing = !detective;
        if (detective) objectiveText.fontSize = 28;
        objectiveText.fontSizeMin = 25;
        objectiveText.fontSizeMax = 34;

        lastFittedText = null; // fuerza recalcular el alto al entrar al nivel

        if (detective)
        {
            FitBannerToText();
        }
        else
        {
            ResizePanel(objBg, new Vector2(960, 150), 6f);
            objectiveText.GetComponent<RectTransform>().sizeDelta = new Vector2(-40, 100);
            MovePanel(timerBg,   new Vector2(0,   -185));
            MovePanel(wordPanel, new Vector2(-16, -265));
            bannerBottom = -170f;
        }

        bool showHint = def != null && def.hintAvailable && def.hints != null && def.hints.Count > 0;
        SetPanelVisible(hintBtn, showHint);
        SetHintPanelVisible(false);
        RefreshHintButtonLabel(LevelManager.Instance);
    }

    // Redimensiona un panel y su marco a la vez. AddInkBorder copia el tamaño
    // en el momento de construir, así que cambiar solo el panel dejaría el
    // marco del tamaño viejo.
    static void ResizePanel(GameObject panel, Vector2 size, float borderPad)
    {
        if (panel == null) return;
        panel.GetComponent<RectTransform>().sizeDelta = size;
        var border = panel.transform.parent.Find(panel.name + "Border");
        if (border != null)
            border.GetComponent<RectTransform>().sizeDelta = size + new Vector2(borderPad * 2f, borderPad * 2f);
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
