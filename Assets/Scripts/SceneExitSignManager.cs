using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Señal de madera permanente sobre cada SceneExit cuyo cruce esté permitido
// ahora mismo (mismo criterio que SceneExit.IsCrossingAllowedNow) — a
// diferencia del cartel-hint de SceneExit (solo aparece al entrar en la
// zona de trigger) o de la flecha de GuidedTutorial (una sola vez, se
// pierde para siempre), esto es visible desde lejos y todas las veces que
// haga falta, como un cartel real del pueblo. Sigue la posición de cada
// SceneExit en pantalla con el mismo cálculo world-to-screen que
// SpeechCaptionUI/GuidedTutorial.
public class SceneExitSignManager : MonoBehaviour
{
    public static SceneExitSignManager Instance { get; private set; }

    static readonly Color WoodPost  = new Color(0.271f, 0.169f, 0.086f, 1f);
    static readonly Color WoodPlank = new Color(0.557f, 0.365f, 0.192f, 1f);
    static readonly Color WoodEdge  = new Color(0.290f, 0.180f, 0.094f, 1f);
    static readonly Color TextCream = new Color(0.961f, 0.914f, 0.808f, 1f);

    class Sign
    {
        public SceneExit exit;
        public RectTransform rt;
        public GameObject go;
        public TMP_Text nameLabel;
        public TMP_Text keyLabel;
    }

    RectTransform canvasRT;
    Camera cam;
    readonly List<Sign> signs = new List<Sign>();
    string builtForScene;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
        SceneManager.sceneLoaded += (_, __) => { builtForScene = null; cam = null; };
    }

    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (builtForScene != sceneName) RebuildSigns(sceneName);

        if (cam == null) cam = FindObjectOfType<Camera>();
        if (cam == null || signs.Count == 0) return;

        foreach (var sign in signs)
        {
            bool allowed = sign.exit != null && sign.exit.IsCrossingAllowedNow;
            if (sign.go.activeSelf != allowed) sign.go.SetActive(allowed);
            if (!allowed) continue;

            Vector3 screenPos = cam.WorldToScreenPoint(sign.exit.transform.position + Vector3.up * 1.4f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, null, out Vector2 localPoint);
            sign.rt.anchoredPosition = localPoint;
        }
    }

    void RebuildSigns(string sceneName)
    {
        builtForScene = sceneName;
        foreach (var sign in signs)
            if (sign.go != null) Destroy(sign.go);
        signs.Clear();

        foreach (var exit in FindObjectsOfType<SceneExit>())
        {
            var sign = BuildSign(exit.DestinationLabel);
            sign.exit = exit;
            sign.go.SetActive(false);
            signs.Add(sign);
        }
    }

    Sign BuildSign(string destinationLabel)
    {
        var root = new GameObject("Sign", typeof(RectTransform));
        root.transform.SetParent(canvasRT, false);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = rootRT.anchorMax = new Vector2(0.5f, 0.5f);
        rootRT.pivot     = new Vector2(0.5f, 0.5f);
        rootRT.sizeDelta = new Vector2(210, 120);

        // Un poco de inclinación aleatoria — cartel de pueblo clavado a mano,
        // no un elemento de UI perfectamente recto.
        float tilt = Random.Range(-4f, 4f);

        // Poste — tabla vertical corta, detrás del cartel, asomando por abajo.
        var postGO = new GameObject("Post", typeof(RectTransform), typeof(Image));
        postGO.transform.SetParent(root.transform, false);
        var postRT = postGO.GetComponent<RectTransform>();
        postRT.anchorMin = postRT.anchorMax = new Vector2(0.5f, 0f);
        postRT.pivot     = new Vector2(0.5f, 1f);
        postRT.anchoredPosition = new Vector2(0, 10);
        postRT.sizeDelta = new Vector2(18, 50);
        postGO.GetComponent<Image>().color = WoodPost;

        // Tabla — el cartel en sí, con borde oscuro (mismo patrón "ink border
        // detrás, panel encima" usado en LevelHUD/BeginnerTurnController).
        var edgeGO = new GameObject("Edge", typeof(RectTransform), typeof(Image));
        edgeGO.transform.SetParent(root.transform, false);
        var edgeRT = edgeGO.GetComponent<RectTransform>();
        edgeRT.anchorMin = edgeRT.anchorMax = new Vector2(0.5f, 1f);
        edgeRT.pivot     = new Vector2(0.5f, 1f);
        edgeRT.anchoredPosition = new Vector2(0, 0);
        edgeRT.sizeDelta = new Vector2(214, 82);
        edgeRT.localEulerAngles = new Vector3(0, 0, tilt);
        edgeGO.GetComponent<Image>().color = WoodEdge;

        var plankGO = new GameObject("Plank", typeof(RectTransform), typeof(Image));
        plankGO.transform.SetParent(root.transform, false);
        var plankRT = plankGO.GetComponent<RectTransform>();
        plankRT.anchorMin = plankRT.anchorMax = new Vector2(0.5f, 1f);
        plankRT.pivot     = new Vector2(0.5f, 1f);
        plankRT.anchoredPosition = new Vector2(0, -4);
        plankRT.sizeDelta = new Vector2(200, 70);
        plankRT.localEulerAngles = new Vector3(0, 0, tilt);
        plankGO.GetComponent<Image>().color = WoodPlank;

        var nameGO = new GameObject("Name", typeof(RectTransform));
        nameGO.transform.SetParent(plankGO.transform, false);
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = Vector2.zero; nameRT.anchorMax = new Vector2(1f, 1f);
        nameRT.offsetMin = new Vector2(8, 22); nameRT.offsetMax = new Vector2(-8, -8);
        var nameLabel = nameGO.AddComponent<TextMeshProUGUI>();
        nameLabel.text = destinationLabel;
        nameLabel.color = TextCream;
        nameLabel.alignment = TextAlignmentOptions.Center;
        nameLabel.fontStyle = FontStyles.Bold;
        nameLabel.enableAutoSizing = true;
        nameLabel.fontSizeMin = 16;
        nameLabel.fontSizeMax = 26;
        nameLabel.raycastTarget = false;

        var keyGO = new GameObject("Key", typeof(RectTransform));
        keyGO.transform.SetParent(plankGO.transform, false);
        var keyRT = keyGO.GetComponent<RectTransform>();
        keyRT.anchorMin = new Vector2(0f, 0f); keyRT.anchorMax = new Vector2(1f, 0f);
        keyRT.pivot = new Vector2(0.5f, 0f);
        keyRT.anchoredPosition = new Vector2(0, 6);
        keyRT.sizeDelta = new Vector2(0, 20);
        var keyLabel = keyGO.AddComponent<TextMeshProUGUI>();
        keyLabel.text = "(E)";
        keyLabel.color = TextCream;
        keyLabel.alignment = TextAlignmentOptions.Center;
        keyLabel.fontSize = 16;
        keyLabel.raycastTarget = false;

        return new Sign { rt = rootRT, go = root, nameLabel = nameLabel, keyLabel = keyLabel };
    }

    void BuildCanvas()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 130; // encima de LevelHUD (120), debajo de QuizUI/DialogueSystem (200/400)

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        canvasRT = GetComponent<RectTransform>();
    }
}
