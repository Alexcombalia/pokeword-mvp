using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-50)]
public class MobileControlsSetup : MonoBehaviour
{
    public static MobileControlsSetup Instance { get; private set; }

    [Tooltip("Show mobile controls while testing in the Unity Editor")]
    [SerializeField] private bool showInEditor = true;

    // Marrón cuero cálido en vez de azul marino — a tono con el resto de la
    // UI, que pasó a estética pergamino (ver UXUI_pergamino.jpeg, carpeta
    // Pokeword). Solo color: la forma/posición de los controles no se toca,
    // esta zona ya se depuró bastante (joystick/E/P).
    static readonly Color BgColor   = new Color(0.290f, 0.204f, 0.129f, 0.60f);
    static readonly Color GoldColor = new Color(1.000f, 0.820f, 0.400f, 0.90f);
    static readonly Color KnobColor = new Color(1.000f, 0.820f, 0.400f, 0.75f);

    private GameObject joystickGO;
    private GameObject btnInteract;
    private GameObject btnPokeWorld;
    private GameObject btnClose;
    private DialogueSystem dialogue;
    private PokeWorldController pokeWorld;

    private void Awake()
    {
        // Singleton: keep the first instance alive across all scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        // Always render and receive input above scene-level canvases
        canvas.sortingOrder = 50;

        // Scale UI proportionally on all screen sizes
        var scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 0.5f;
        }

#if UNITY_EDITOR
        if (!showInEditor) return;
#elif !UNITY_ANDROID && !UNITY_IOS
        return;
#endif
        Build();
    }

    void Build()
    {
        Sprite circle = MakeCircleSprite(128);

        // ── Joystick — bottom-right (portrait 1080×1920 canvas) ──────────
        var jsBG = MakeGO("JoystickBG", transform);
        ApplyRT(jsBG, new Vector2(1, 0), new Vector2(0.5f, 0.5f), new Vector2(-180, 220), new Vector2(200, 200));
        AddImage(jsBG, circle, BgColor);

        var jsKnob = MakeGO("JoystickKnob", jsBG.transform);
        ApplyRT(jsKnob, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 80));
        AddImage(jsKnob, circle, KnobColor);

        var joystick = jsBG.AddComponent<VirtualJoystick>();
        joystick.knob = jsKnob.GetComponent<RectTransform>();
        joystickGO = jsBG;

        // ── Interact button E — bottom-left ───────────────────────────────
        btnInteract  = MakeButton("BtnInteract",  new Vector2(0, 0), new Vector2(140, 260), 130, "E",
            () => MobileInput.PressInteract(), circle);

        // ── PokeWorld button P — above E, left ───────────────────────────
        btnPokeWorld = MakeButton("BtnPokeWorld", new Vector2(0, 0), new Vector2(140, 420), 130, "P",
            () => MobileInput.PressPokeWorld(), circle);

        // ── Close button X — right side next to dialogue panel, hidden by default ───
        // "X" en vez del glyph "✕": ese carácter no existe en la fuente TMP por
        // defecto y se ve como un cuadrado vacío (mismo bug que las estrellas).
        btnClose = MakeButton("BtnClose", new Vector2(1, 0), new Vector2(-80, 420), 130, "X",
            () => MobileInput.PressClose(), circle, fontScale: 0.58f);
        btnClose.SetActive(false);

        // MobileInput lives on this same persistent GameObject
        if (GetComponent<MobileInput>() == null)
            gameObject.AddComponent<MobileInput>();
    }

    private void Update()
    {
        if (joystickGO == null) return;
        if (dialogue  == null) dialogue  = FindObjectOfType<DialogueSystem>();
        if (pokeWorld == null) pokeWorld = FindObjectOfType<PokeWorldController>();

        bool anyOpen = (dialogue  != null && dialogue.IsOpen()) ||
                       (pokeWorld != null && pokeWorld.IsOpen());

        joystickGO.SetActive(!anyOpen);
        if (btnInteract  != null) btnInteract.SetActive(!anyOpen);
        if (btnPokeWorld != null) btnPokeWorld.SetActive(!anyOpen);
        if (btnClose     != null) btnClose.SetActive(anyOpen);
    }

    GameObject MakeButton(string name, Vector2 anchor, Vector2 pos, float size,
                    string label, UnityEngine.Events.UnityAction callback, Sprite circle,
                    float fontScale = 0.42f)
    {
        var go = MakeGO(name, transform);
        ApplyRT(go, anchor, new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));

        var img = AddImage(go, circle, BgColor);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors        = btn.colors;
        colors.pressedColor = new Color(1f, 0.82f, 0.4f, 0.55f);
        btn.colors        = colors;
        btn.onClick.AddListener(callback);

        // Label
        var lbl    = MakeGO("Label", go.transform);
        var lblRT  = lbl.GetComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.anchoredPosition = Vector2.zero; lblRT.sizeDelta = Vector2.zero;

        var tmp          = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text         = label;
        tmp.fontSize     = size * fontScale;
        tmp.color        = GoldColor;
        tmp.alignment    = TextAlignmentOptions.Center;
        tmp.fontStyle    = FontStyles.Bold;
        tmp.raycastTarget = false;

        return go;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static GameObject MakeGO(string n, Transform parent)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void ApplyRT(GameObject go, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt          = go.GetComponent<RectTransform>();
        rt.anchorMin    = anchor; rt.anchorMax = anchor;
        rt.pivot        = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta    = size;
    }

    static Image AddImage(GameObject go, Sprite sprite, Color color)
    {
        var img    = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color  = color;
        return img;
    }

    static Sprite MakeCircleSprite(int size)
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
