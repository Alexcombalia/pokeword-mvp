using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Applies the "Encre & Méditerranée" palette and layout to the HUD at runtime.
// Attach to HUDCanvas. Runs before other scripts so the layout is ready immediately.
[DefaultExecutionOrder(-60)]
public class HUDSetup : MonoBehaviour
{
    // ── Palette ───────────────────────────────────────────────────────────
    static readonly Color NavyPanel   = new Color(0.051f, 0.122f, 0.208f, 0.92f);
    static readonly Color NavyDark    = new Color(0.027f, 0.067f, 0.118f, 1.00f);
    static readonly Color NavyBox     = new Color(0.051f, 0.122f, 0.208f, 0.70f);
    static readonly Color Cream       = new Color(0.940f, 0.902f, 0.827f, 1.00f);
    static readonly Color Gold        = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color BarFood     = new Color(0.784f, 0.643f, 0.290f, 1.00f);
    static readonly Color BarWater    = new Color(0.227f, 0.561f, 0.710f, 1.00f);
    static readonly Color BarEnergy   = new Color(0.878f, 0.471f, 0.251f, 1.00f);

    private void Awake()
    {
        var scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 0.5f;
        }
        ApplyColors();
        ApplyLayout();
        HideLegacyElements();
    }

    // El rediseño de niveles (ver memory: project_pokeword_redesign) ya no
    // usa comida/agua/energía/misión/dinero/días/nivel-por-palabras — se
    // ocultan uno a uno (nunca el HUDCanvas root: MobileControlsSetup vive
    // en ese mismo GameObject y debe seguir visible).
    static readonly string[] legacyElements =
    {
        "HUDPanel",
        "FoodLabel", "FoodBar_BG",
        "WaterLabel", "WaterBar_BG",
        "EnergyLabel", "EnergyBar_BG",
        "BoxDay", "DayText",
        "BoxMoney", "MoneyLabel", "MoneyText",
        "BoxMision", "MissionText",
        "BoxLevel", "LevelText",
        "BoxLevel (1)", "LevelProgressText",
    };

    void HideLegacyElements()
    {
        foreach (var name in legacyElements)
        {
            var go = FindDeep(name);
            if (go != null) go.SetActive(false);
        }
    }

    // ── Colors ────────────────────────────────────────────────────────────
    void ApplyColors()
    {
        SetImageColor("HUDPanel",       NavyPanel);
        SetImageColor("FoodBar_BG",     NavyDark);
        SetImageColor("WaterBar_BG",    NavyDark);
        SetImageColor("EnergyBar_BG",   NavyDark);
        SetImageColor("FoodBar_Fill",   BarFood);
        SetImageColor("WaterBar_Fill",  BarWater);
        SetImageColor("EnergyBar_Fill", BarEnergy);
        SetImageColor("BoxDay",         Color.clear);  // no box, text directly on HUD
        SetImageColor("BoxMoney",       Color.clear);  // no box, text directly on HUD
        SetImageColor("BoxMision",      NavyBox);
        SetImageColor("BoxLevel",       NavyBox);
        SetImageColor("BoxLevel (1)",   NavyBox);

        // Labels
        SetTextColor("FoodLabel",   Cream, 14);
        SetTextColor("WaterLabel",  Cream, 14);
        SetTextColor("EnergyLabel", Cream, 14);
        SetTextColor("MoneyLabel",  Cream, 14);

        // Values
        SetTextColor("MoneyText",         Gold,  24);
        SetTextColor("DayText",           Cream, 20);
        SetTextColor("MissionText",       Cream, 38);
        SetTextColor("LevelText",         Gold,  28);
        SetTextColor("LevelProgressText", Cream, 20);

        // Translate labels to French
        SetText("FoodLabel",   "Nourriture");
        SetText("WaterLabel",  "Eau");
        SetText("EnergyLabel", "Énergie");
        SetText("MoneyLabel",  "Francs");
    }

    // ── Layout (portrait 1080×1920) ───────────────────────────────────────
    void ApplyLayout()
    {
        // HUDPanel — full-width strip at top, 110px tall
        var panelGO = FindDeep("HUDPanel");
        if (panelGO != null)
        {
            var rt = panelGO.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 110);
        }

        // ── Resource bars — three compact groups, portrait width ──────────
        // Food: x=10
        SetRT("FoodLabel",  topLeft, new Vector2(0, 1), new Vector2(10, -10), new Vector2(80, 20));
        SetRT("FoodBar_BG", topLeft, new Vector2(0, 1), new Vector2(10, -36), new Vector2(120, 22));
        SetFill("FoodBar_Fill");

        // Water: x=152
        SetRT("WaterLabel",  topLeft, new Vector2(0, 1), new Vector2(152, -10), new Vector2(60, 20));
        SetRT("WaterBar_BG", topLeft, new Vector2(0, 1), new Vector2(152, -36), new Vector2(120, 22));
        SetFill("WaterBar_Fill");

        // Energy: x=284
        SetRT("EnergyLabel",  topLeft, new Vector2(0, 1), new Vector2(284, -10), new Vector2(75, 20));
        SetRT("EnergyBar_BG", topLeft, new Vector2(0, 1), new Vector2(284, -36), new Vector2(120, 22));
        SetFill("EnergyBar_Fill");

        // ── Right info cluster — Jour + Francs ────────────────────────────
        SetRT("BoxDay",     topRight, new Vector2(1, 1), new Vector2(-8, -8),  new Vector2(160, 40));
        SetRT("DayText",    topRight, new Vector2(1, 1), new Vector2(-8, -8),  new Vector2(160, 40));
        SetRT("MoneyLabel", topRight, new Vector2(1, 1), new Vector2(-8, -52), new Vector2(160, 18));
        SetRT("BoxMoney",   topRight, new Vector2(1, 1), new Vector2(-8, -72), new Vector2(160, 32));
        SetRT("MoneyText",  topRight, new Vector2(1, 1), new Vector2(-8, -72), new Vector2(160, 32));

        // ── Mission text — full width below HUD ──────────────────────────
        SetRT("BoxMision",   topLeft, new Vector2(0, 1), new Vector2(10, -118), new Vector2(1060, 130));
        SetRT("MissionText", topLeft, new Vector2(0, 1), new Vector2(10, -118), new Vector2(1060, 130));

        // ── Level boxes — left side, below mission ────────────────────────
        SetRT("BoxLevel",          topLeft, new Vector2(0, 1), new Vector2(10, -256), new Vector2(400, 58));
        SetRT("LevelText",         topLeft, new Vector2(0, 1), new Vector2(10, -256), new Vector2(400, 58));
        SetRT("BoxLevel (1)",      topLeft, new Vector2(0, 1), new Vector2(10, -318), new Vector2(400, 48));
        SetRT("LevelProgressText", topLeft, new Vector2(0, 1), new Vector2(10, -318), new Vector2(400, 48));

        // ── Text config ────────────────────────────────────────────────────
        ConfigText("DayText",           autoSize: false, minSize: 10, maxSize: 20, wrap: false,
                   align: TextAlignmentOptions.Right);
        ConfigText("MoneyLabel",        autoSize: false, minSize: 10, maxSize: 14, wrap: false,
                   align: TextAlignmentOptions.Right);
        ConfigText("MoneyText",         autoSize: false, minSize: 10, maxSize: 24, wrap: false,
                   align: TextAlignmentOptions.Right);
        ConfigText("LevelText",         autoSize: true,  minSize: 16, maxSize: 28, wrap: false,
                   align: TextAlignmentOptions.Left);
        ConfigText("LevelProgressText", autoSize: false, minSize: 16, maxSize: 20, wrap: false,
                   align: TextAlignmentOptions.Left);
        ConfigText("MissionText",       autoSize: true,  minSize: 16, maxSize: 30, wrap: true,
                   align: TextAlignmentOptions.Left);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static readonly Vector2 topLeft   = new Vector2(0,    1);
    static readonly Vector2 topRight  = new Vector2(1,    1);
    static readonly Vector2 topCenter = new Vector2(0.5f, 1);

    void SetRT(string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = FindDeep(name);
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.localScale = Vector3.one;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    // Resets a bar fill to fully stretch inside its parent BG.
    void SetFill(string name)
    {
        var go = FindDeep(name);
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.localScale       = Vector3.one;
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
    }

    void ConfigText(string name, bool autoSize, float minSize, float maxSize, bool wrap,
                    TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var go = FindDeep(name);
        if (go == null) return;
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp == null) return;
        tmp.enableWordWrapping = wrap;
        tmp.overflowMode       = TMPro.TextOverflowModes.Overflow;
        tmp.enableAutoSizing   = autoSize;
        tmp.alignment          = align;
        if (autoSize) { tmp.fontSizeMin = minSize; tmp.fontSizeMax = maxSize; }
    }

    void SetImageColor(string name, Color color)
    {
        var go = FindDeep(name);
        if (go == null) return;
        var img = go.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    void SetTextColor(string name, Color color, int fontSize = 0)
    {
        var go = FindDeep(name);
        if (go == null) return;
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp == null) return;
        tmp.color = color;
        if (fontSize > 0) tmp.fontSize = fontSize;
    }

    void SetText(string name, string text)
    {
        var go = FindDeep(name);
        if (go == null) return;
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp != null) tmp.text = text;
    }

    GameObject FindDeep(string name)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }
}
