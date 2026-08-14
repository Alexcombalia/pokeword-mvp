using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUI : MonoBehaviour
{
    public static QuizUI Instance { get; private set; }

    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color NavyDark   = new Color(0.027f, 0.059f, 0.102f, 0.97f);
    static readonly Color NavyCard   = new Color(0.051f, 0.122f, 0.208f, 1.00f);
    static readonly Color Gold       = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color Cream      = new Color(0.941f, 0.902f, 0.827f, 1.00f);
    static readonly Color GreenFb    = new Color(0.180f, 0.690f, 0.341f, 1.00f);
    static readonly Color RedFb      = new Color(0.859f, 0.212f, 0.212f, 1.00f);
    static readonly Color BtnNeutral = new Color(0.075f, 0.180f, 0.298f, 1.00f);

    // ── Runtime refs ──────────────────────────────────────────────────────────
    Canvas       rootCanvas;
    GameObject   overlay;

    // Top bar
    Image[]      dots       = new Image[3];
    TMP_Text     streakText;

    // Question area
    TMP_Text     contextText;
    TMP_Text     promptText;
    TMP_Text     wordText;

    // MC area
    GameObject   mcArea;
    Button[]     mcButtons  = new Button[4];
    TMP_Text[]   mcLabels   = new TMP_Text[4];

    // Hint (basic mode only)
    GameObject hintArea;
    Button     hintBtn;
    TMP_Text   hintLabel;

    // Free area
    GameObject     freeArea;
    TMP_InputField freeInput;
    Button         freeOkBtn;

    // Feedback strip
    GameObject   feedbackStrip;
    TMP_Text     feedbackIcon;
    TMP_Text     feedbackText;
    TMP_Text     answerReveal;
    Button       continueBtn;

    // Result screen
    GameObject   resultScreen;
    TMP_Text     resultTitle;
    TMP_Text     resultScore;
    TMP_Text     resultReward;
    TMP_Text     resultWords;
    Button       resultContinueBtn;

    // ── State ─────────────────────────────────────────────────────────────────
    List<QuizQuestion> questions;
    int   qIndex;
    int   correctCount;
    bool  awaitingContinue;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
        overlay.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show(List<QuizQuestion> qs)
    {
        questions    = qs;
        qIndex       = 0;
        correctCount = 0;
        overlay.SetActive(true);
        ShowQuestion();
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    void ShowQuestion()
    {
        awaitingContinue = false;
        var q = questions[qIndex];

        // Progress dots
        for (int i = 0; i < dots.Length; i++)
            dots[i].color = (i <= qIndex) ? Gold : new Color(1,1,1,0.25f);

        // Streak
        int streak = QuizSystem.Instance != null ? QuizSystem.Instance.CurrentStreak : 0;
        // Sin emoji "🔥": los emoji no se renderizan con la fuente TMP por
        // defecto (necesitan un atlas de color aparte, no solo un glyph SDF).
        streakText.text = streak > 0 ? $"{streak}" : "";

        // Question area
        contextText.text = !string.IsNullOrEmpty(q.word.context)
            ? $"<i>\"{q.word.context}\"</i>" : "";
        promptText.text  = q.GetPrompt();
        wordText.text    = q.GetWordDisplay();

        bool isMC = q.type == QuestionType.MCFrToEs || q.type == QuestionType.MCEsToFr;
        mcArea.SetActive(isMC);
        freeArea.SetActive(!isMC);
        feedbackStrip.SetActive(false);

        // Hint button: only in basic mode, reset label and re-enable
        bool showHint = GameSettings.IsBasic;
        hintArea.SetActive(showHint);
        if (showHint)
        {
            hintLabel.text       = "Ver pista";
            hintBtn.interactable = true;
        }

        if (isMC)
        {
            for (int i = 0; i < 4; i++)
            {
                mcLabels[i].text = q.options[i];
                SetButtonColor(mcButtons[i], BtnNeutral);
                int captured = i;
                mcButtons[i].onClick.RemoveAllListeners();
                mcButtons[i].onClick.AddListener(() => OnMCAnswer(captured));
                mcButtons[i].interactable = true;
            }
        }
        else
        {
            freeInput.text = "";
            freeInput.ActivateInputField();
        }
    }

    void OnMCAnswer(int index)
    {
        if (awaitingContinue) return;
        awaitingContinue = true;

        var q       = questions[qIndex];
        bool correct = index == q.correctIndex;

        for (int i = 0; i < 4; i++)
        {
            mcButtons[i].interactable = false;
            if (i == q.correctIndex)          SetButtonColor(mcButtons[i], GreenFb);
            else if (i == index && !correct)  SetButtonColor(mcButtons[i], RedFb);
        }

        RegisterAnswer(q, correct);
    }

    void OnFreeAnswer()
    {
        if (awaitingContinue) return;
        awaitingContinue = true;

        var q       = questions[qIndex];
        bool correct = q.CheckAnswer(freeInput.text);
        RegisterAnswer(q, correct);
    }

    void RegisterAnswer(QuizQuestion q, bool correct)
    {
        QuizSystem.Instance?.OnAnswerGiven(q, correct);
        if (correct) { correctCount++; Haptics.Medium(); }
        else Haptics.Light();
        ShowFeedback(q, correct);
    }

    void ShowFeedback(QuizQuestion q, bool correct)
    {
        hintArea.SetActive(false);
        feedbackStrip.SetActive(true);
        feedbackStrip.GetComponent<Image>().color = correct ? GreenFb : RedFb;
        feedbackIcon.text  = correct ? "OK" : "X"; // sin glyphs "✓"/"✗" — mismo bug de fuente
        feedbackText.text  = correct ? "Correct !" : "Raté !";
        answerReveal.text  = correct ? "" : $"Réponse : {q.GetCorrectAnswer()}";
        continueBtn.GetComponentInChildren<TMP_Text>().text = qIndex < questions.Count - 1
            ? "Continuer →" : "Résultats →";
    }

    void OnContinue()
    {
        qIndex++;
        if (qIndex < questions.Count)
        {
            feedbackStrip.SetActive(false);
            ShowQuestion();
        }
        else
        {
            ShowResult();
        }
    }

    void ShowResult()
    {
        mcArea.SetActive(false);
        freeArea.SetActive(false);
        feedbackStrip.SetActive(false);
        resultScreen.SetActive(true);

        QuizSystem.Instance?.OnQuizComplete(correctCount);

        if      (correctCount == 3) resultTitle.text = "Parfait !";
        else if (correctCount == 2) resultTitle.text = "Bien joué !";
        else                        resultTitle.text  = "Continue à apprendre !";

        resultScore.text  = $"{correctCount} / 3";
        resultReward.text = correctCount >= 3 ? "+15 francs" :
                            correctCount == 2 ? "+7 francs"  : "";

        var sb = new System.Text.StringBuilder();
        foreach (var q in questions)
            sb.AppendLine($"<color=#FFD166>{q.word.french}</color>  —  {q.word.spanish}");
        resultWords.text = sb.ToString();
    }

    void OnResultContinue()
    {
        resultScreen.SetActive(false);
        overlay.SetActive(false);
    }

    // ── Canvas builder ────────────────────────────────────────────────────────

    void BuildCanvas()
    {
        rootCanvas                  = gameObject.AddComponent<Canvas>();
        rootCanvas.renderMode       = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder     = 200;

        var scaler                  = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight   = 0f;

        gameObject.AddComponent<GraphicRaycaster>();

        // Full-screen dark overlay
        overlay = MakeGO("Overlay", transform);
        Stretch(overlay);
        AddImage(overlay, null, NavyDark);

        // Centered card
        var card = MakeGO("Card", overlay.transform);
        var cRT  = card.GetComponent<RectTransform>();
        cRT.anchorMin        = new Vector2(0.5f, 0.5f);
        cRT.anchorMax        = new Vector2(0.5f, 0.5f);
        cRT.pivot            = new Vector2(0.5f, 0.5f);
        cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta        = new Vector2(820, 560);
        AddImage(card, null, NavyCard);
        AddRounded(card);

        BuildTopBar(card.transform);
        BuildQuestionArea(card.transform);
        BuildMCArea(card.transform);
        BuildFreeArea(card.transform);
        BuildHintArea(card.transform);
        BuildFeedbackStrip(card.transform);
        BuildResultScreen(card.transform);
    }

    void BuildTopBar(Transform parent)
    {
        var bar = MakeGO("TopBar", parent);
        var rt  = bar.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(1, 1);
        rt.pivot            = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -10);
        rt.sizeDelta        = new Vector2(-40, 48);

        // 3 progress dots (left side)
        for (int i = 0; i < 3; i++)
        {
            var dot = MakeGO($"Dot{i}", bar.transform);
            var dRT = dot.GetComponent<RectTransform>();
            dRT.anchorMin = dRT.anchorMax = new Vector2(0, 0.5f);
            dRT.pivot     = new Vector2(0, 0.5f);
            dRT.anchoredPosition = new Vector2(i * 30, 0);
            dRT.sizeDelta = new Vector2(20, 20);
            dots[i] = AddImage(dot, MakeCircle(32), new Color(1,1,1,0.25f));
        }

        // Streak (right side)
        var streakGO = MakeGO("Streak", bar.transform);
        var sRT      = streakGO.GetComponent<RectTransform>();
        sRT.anchorMin = new Vector2(1, 0.5f);
        sRT.anchorMax = new Vector2(1, 0.5f);
        sRT.pivot     = new Vector2(1, 0.5f);
        sRT.anchoredPosition = Vector2.zero;
        sRT.sizeDelta        = new Vector2(120, 40);
        streakText = AddText(streakGO, "", 26, Gold, TextAlignmentOptions.Right);
    }

    void BuildQuestionArea(Transform parent)
    {
        var area = MakeGO("QuestionArea", parent);
        var rt   = area.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(1, 1);
        rt.pivot            = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -70);
        rt.sizeDelta        = new Vector2(-60, 200);

        var ctxGO = MakeGO("Context", area.transform);
        StretchWithPad(ctxGO, area.GetComponent<RectTransform>(), 0, -110, 0, 0);
        contextText = AddText(ctxGO, "", 16, new Color(0.7f,0.8f,0.9f,1f), TextAlignmentOptions.Center);
        contextText.fontStyle = FontStyles.Italic;

        var prmGO = MakeGO("Prompt", area.transform);
        StretchWithPad(prmGO, area.GetComponent<RectTransform>(), 0, -150, 0, 0);
        promptText = AddText(prmGO, "", 20, Cream, TextAlignmentOptions.Center);

        var wrdGO = MakeGO("Word", area.transform);
        StretchWithPad(wrdGO, area.GetComponent<RectTransform>(), 0, -200, 0, 0);
        wordText = AddText(wrdGO, "", 38, Gold, TextAlignmentOptions.Center);
        wordText.fontStyle = FontStyles.Bold;
    }

    void BuildMCArea(Transform parent)
    {
        mcArea = MakeGO("MCArea", parent);
        var rt = mcArea.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 0);
        rt.anchorMax        = new Vector2(1, 0);
        rt.pivot            = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 80);
        rt.sizeDelta        = new Vector2(-60, 80);

        for (int i = 0; i < 4; i++)
        {
            var btnGO = MakeGO($"MC{i}", mcArea.transform);
            var bRT   = btnGO.GetComponent<RectTransform>();
            bRT.anchorMin        = new Vector2(i * 0.25f, 0);
            bRT.anchorMax        = new Vector2(i * 0.25f + 0.25f, 1);
            bRT.pivot            = new Vector2(0.5f, 0.5f);
            bRT.anchoredPosition = Vector2.zero;
            bRT.sizeDelta        = new Vector2(-8, 0);
            AddImage(btnGO, null, BtnNeutral);

            var lbl = MakeGO("Label", btnGO.transform);
            Stretch(lbl);
            mcLabels[i] = AddText(lbl, "", 20, Cream, TextAlignmentOptions.Center);

            mcButtons[i] = btnGO.AddComponent<Button>();
            mcButtons[i].targetGraphic = btnGO.GetComponent<Image>();
        }
    }

    void BuildFreeArea(Transform parent)
    {
        freeArea = MakeGO("FreeArea", parent);
        var rt   = freeArea.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 0);
        rt.anchorMax        = new Vector2(1, 0);
        rt.pivot            = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 80);
        rt.sizeDelta        = new Vector2(-60, 70);

        // Input field
        var fieldGO = MakeGO("InputField", freeArea.transform);
        var fRT     = fieldGO.GetComponent<RectTransform>();
        fRT.anchorMin        = new Vector2(0, 0);
        fRT.anchorMax        = new Vector2(0.78f, 1);
        fRT.pivot            = new Vector2(0, 0.5f);
        fRT.anchoredPosition = Vector2.zero;
        fRT.sizeDelta        = Vector2.zero;
        AddImage(fieldGO, null, BtnNeutral);

        freeInput             = fieldGO.AddComponent<TMP_InputField>();
        freeInput.onSubmit.AddListener(_ => OnFreeAnswer());

        var textArea = MakeGO("TextArea", fieldGO.transform);
        Stretch(textArea);
        var inputText  = AddText(textArea, "", 22, Cream, TextAlignmentOptions.Left);
        inputText.margin = new Vector4(10, 0, 10, 0);

        var placeholder = MakeGO("Placeholder", fieldGO.transform);
        Stretch(placeholder);
        var phText = AddText(placeholder, "Ta réponse…", 22,
            new Color(0.6f, 0.7f, 0.8f, 0.7f), TextAlignmentOptions.Left);
        phText.margin = new Vector4(10, 0, 10, 0);
        phText.fontStyle = FontStyles.Italic;

        freeInput.textComponent   = inputText;
        freeInput.placeholder     = phText;
        freeInput.textViewport    = textArea.GetComponent<RectTransform>();

        // OK button
        var okGO = MakeGO("OkBtn", freeArea.transform);
        var oRT  = okGO.GetComponent<RectTransform>();
        oRT.anchorMin        = new Vector2(0.80f, 0);
        oRT.anchorMax        = new Vector2(1.00f, 1);
        oRT.pivot            = new Vector2(1, 0.5f);
        oRT.anchoredPosition = Vector2.zero;
        oRT.sizeDelta        = Vector2.zero;
        AddImage(okGO, null, Gold);

        var okLbl = MakeGO("Label", okGO.transform);
        Stretch(okLbl);
        var okTxt = AddText(okLbl, "OK", 24, NavyDark, TextAlignmentOptions.Center);
        okTxt.fontStyle = FontStyles.Bold;

        freeOkBtn = okGO.AddComponent<Button>();
        freeOkBtn.targetGraphic = okGO.GetComponent<Image>();
        freeOkBtn.onClick.AddListener(OnFreeAnswer);
    }

    void BuildHintArea(Transform parent)
    {
        hintArea = MakeGO("HintArea", parent);
        var rt = hintArea.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 172);
        rt.sizeDelta        = new Vector2(280, 36);

        var btnGO = MakeGO("HintBtn", hintArea.transform);
        Stretch(btnGO);
        AddImage(btnGO, null, new Color(0.12f, 0.25f, 0.40f, 1f));

        var lbl = MakeGO("Label", btnGO.transform);
        Stretch(lbl);
        hintLabel = AddText(lbl, "Ver pista", 18, new Color(0.8f, 0.9f, 1f, 1f), TextAlignmentOptions.Center);

        hintBtn = btnGO.AddComponent<Button>();
        hintBtn.targetGraphic = btnGO.GetComponent<Image>();
        hintBtn.onClick.AddListener(OnHintClicked);

        hintArea.SetActive(false);
    }

    void OnHintClicked()
    {
        var q = questions[qIndex];
        hintLabel.text = $"= {q.GetCorrectAnswer()}";
        hintBtn.interactable = false;
    }

    void BuildFeedbackStrip(Transform parent)
    {
        feedbackStrip = MakeGO("FeedbackStrip", parent);
        var rt = feedbackStrip.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 0);
        rt.anchorMax        = new Vector2(1, 0);
        rt.pivot            = new Vector2(0.5f, 0);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(0, 75);
        var stripImg = feedbackStrip.AddComponent<Image>();
        stripImg.color = GreenFb;

        var iconGO = MakeGO("Icon", feedbackStrip.transform);
        var iRT    = iconGO.GetComponent<RectTransform>();
        iRT.anchorMin = iRT.anchorMax = new Vector2(0, 0.5f);
        iRT.pivot     = new Vector2(0, 0.5f);
        iRT.anchoredPosition = new Vector2(20, 0);
        iRT.sizeDelta        = new Vector2(40, 40);
        feedbackIcon = AddText(iconGO, "OK", 30, Color.white, TextAlignmentOptions.Center);

        var fbTextGO = MakeGO("FbText", feedbackStrip.transform);
        var tRT      = fbTextGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0, 0); tRT.anchorMax = new Vector2(0.65f, 1);
        tRT.pivot     = new Vector2(0, 0.5f);
        tRT.anchoredPosition = new Vector2(66, 0);
        tRT.sizeDelta        = Vector2.zero;
        feedbackText = AddText(fbTextGO, "", 22, Color.white, TextAlignmentOptions.Left);
        feedbackText.fontStyle = FontStyles.Bold;

        var revGO = MakeGO("Answer", feedbackStrip.transform);
        var rRT   = revGO.GetComponent<RectTransform>();
        rRT.anchorMin = new Vector2(0, 0); rRT.anchorMax = new Vector2(0.65f, 1);
        rRT.pivot     = new Vector2(0, 0.5f);
        rRT.anchoredPosition = new Vector2(66, -22);
        rRT.sizeDelta        = Vector2.zero;
        answerReveal = AddText(revGO, "", 17, new Color(1,1,1,0.85f), TextAlignmentOptions.Left);

        var ctnGO = MakeGO("ContinueBtn", feedbackStrip.transform);
        var cRT   = ctnGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(1, 0.5f); cRT.anchorMax = new Vector2(1, 0.5f);
        cRT.pivot     = new Vector2(1, 0.5f);
        cRT.anchoredPosition = new Vector2(-20, 0);
        cRT.sizeDelta        = new Vector2(200, 50);
        AddImage(ctnGO, null, Color.white);

        var cLbl = MakeGO("Label", ctnGO.transform);
        Stretch(cLbl);
        AddText(cLbl, "Continuer →", 20, new Color(0.1f,0.1f,0.1f,1f), TextAlignmentOptions.Center)
            .fontStyle = FontStyles.Bold;

        continueBtn = ctnGO.AddComponent<Button>();
        continueBtn.targetGraphic = ctnGO.GetComponent<Image>();
        continueBtn.onClick.AddListener(OnContinue);

        feedbackStrip.SetActive(false);
    }

    void BuildResultScreen(Transform parent)
    {
        resultScreen = MakeGO("ResultScreen", parent);
        Stretch(resultScreen);
        AddImage(resultScreen, null, NavyCard);

        var titleGO = MakeGO("Title", resultScreen.transform);
        var tRT     = titleGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0, 1); tRT.anchorMax = new Vector2(1, 1);
        tRT.pivot     = new Vector2(0.5f, 1);
        tRT.anchoredPosition = new Vector2(0, -60);
        tRT.sizeDelta        = new Vector2(-40, 60);
        resultTitle = AddText(titleGO, "", 40, Gold, TextAlignmentOptions.Center);
        resultTitle.fontStyle = FontStyles.Bold;

        var scoreGO = MakeGO("Score", resultScreen.transform);
        var sRT2    = scoreGO.GetComponent<RectTransform>();
        sRT2.anchorMin = new Vector2(0, 1); sRT2.anchorMax = new Vector2(1, 1);
        sRT2.pivot     = new Vector2(0.5f, 1);
        sRT2.anchoredPosition = new Vector2(0, -130);
        sRT2.sizeDelta        = new Vector2(-40, 50);
        resultScore = AddText(scoreGO, "", 34, Cream, TextAlignmentOptions.Center);

        var rewGO = MakeGO("Reward", resultScreen.transform);
        var rRT2  = rewGO.GetComponent<RectTransform>();
        rRT2.anchorMin = new Vector2(0, 1); rRT2.anchorMax = new Vector2(1, 1);
        rRT2.pivot     = new Vector2(0.5f, 1);
        rRT2.anchoredPosition = new Vector2(0, -185);
        rRT2.sizeDelta        = new Vector2(-40, 40);
        resultReward = AddText(rewGO, "", 24, GreenFb, TextAlignmentOptions.Center);

        var wordsGO = MakeGO("Words", resultScreen.transform);
        var wRT     = wordsGO.GetComponent<RectTransform>();
        wRT.anchorMin = new Vector2(0, 0); wRT.anchorMax = new Vector2(1, 1);
        wRT.pivot     = new Vector2(0.5f, 0.5f);
        wRT.anchoredPosition = new Vector2(0, -40);
        wRT.sizeDelta        = new Vector2(-80, -280);
        resultWords = AddText(wordsGO, "", 20, Cream, TextAlignmentOptions.Center);

        var contGO = MakeGO("ContinueBtn", resultScreen.transform);
        var crRT   = contGO.GetComponent<RectTransform>();
        crRT.anchorMin = new Vector2(0.5f, 0); crRT.anchorMax = new Vector2(0.5f, 0);
        crRT.pivot     = new Vector2(0.5f, 0);
        crRT.anchoredPosition = new Vector2(0, 30);
        crRT.sizeDelta        = new Vector2(260, 56);
        AddImage(contGO, null, Gold);

        var rcLbl = MakeGO("Label", contGO.transform);
        Stretch(rcLbl);
        var rcTxt = AddText(rcLbl, "Continuer", 24, NavyDark, TextAlignmentOptions.Center);
        rcTxt.fontStyle = FontStyles.Bold;

        resultContinueBtn = contGO.AddComponent<Button>();
        resultContinueBtn.targetGraphic = contGO.GetComponent<Image>();
        resultContinueBtn.onClick.AddListener(OnResultContinue);

        resultScreen.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject MakeGO(string n, Transform parent)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void StretchWithPad(GameObject go, RectTransform _, float l, float b, float r, float t)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }

    static Image AddImage(GameObject go, Sprite sprite, Color color)
    {
        var img    = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color  = color;
        return img;
    }

    static TMP_Text AddText(GameObject go, string text, float size, Color color, TextAlignmentOptions align)
    {
        var t           = go.AddComponent<TextMeshProUGUI>();
        t.text          = text;
        t.fontSize      = size;
        t.color         = color;
        t.alignment     = align;
        t.enableWordWrapping = true;
        return t;
    }

    static void SetButtonColor(Button btn, Color color)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    static void AddRounded(GameObject go)
    {
        // No built-in rounded corners in Unity UI without sprite; plain rect is fine
    }

    static Sprite MakeCircle(int size)
    {
        var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r    = size * 0.5f;
        var center = new Vector2(r, r);
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist  = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                byte  alpha = (byte)(Mathf.Clamp01((r - dist) + 0.5f) * 255);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
