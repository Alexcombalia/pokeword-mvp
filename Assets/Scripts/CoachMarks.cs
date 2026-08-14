using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Pistas contextuales de una sola vez — "aprender haciendo" en vez del
// manual de texto de varias páginas que tenía TutorialSystem (retirado).
// Cada pista aparece junto al elemento real la primera vez que importa,
// se toca o se auto-oculta sola, y no vuelve a aparecer nunca (PlayerPrefs).
public class CoachMarks : MonoBehaviour
{
    public static CoachMarks Instance { get; private set; }

    static readonly Color BubbleBg   = new Color(1.000f, 0.820f, 0.400f, 0.97f);
    static readonly Color BubbleText = new Color(0.012f, 0.031f, 0.059f, 1f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 450; // above DialogueSystem (400) — a coachmark always wins

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        DontDestroyOnLoad(gameObject);
    }

    public void ShowOnce(string key, string text, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, float duration = 9f)
    {
        // GuidedTutorial ya cubre moverse/hablar la primera vez con sus
        // propios pasos bloqueantes — evita que una burbuja de CoachMarks
        // aparezca encima y se pisen los dos sistemas.
        if (GuidedTutorial.Instance != null && GuidedTutorial.Instance.IsRunning) return;

        string prefKey = "coach_" + key;
        if (PlayerPrefs.GetInt(prefKey, 0) == 1) return;
        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();

        StartCoroutine(ShowBubble(text, anchor, pivot, anchoredPos, size, duration));
    }

    IEnumerator ShowBubble(string text, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, float duration)
    {
        var go = new GameObject("Coach", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = BubbleBg;

        bool dismissed = false;
        var btn = go.AddComponent<Button>(); // tap to dismiss early
        btn.targetGraphic = go.GetComponent<Image>();
        btn.onClick.AddListener(() => dismissed = true);

        var lblGO = new GameObject("Label", typeof(RectTransform));
        lblGO.transform.SetParent(go.transform, false);
        var lRT = lblGO.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.offsetMin = new Vector2(14, 8); lRT.offsetMax = new Vector2(-14, -8);
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 22;
        tmp.color     = BubbleText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = true;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16;
        tmp.fontSizeMax = 22;

        float t = 0f;
        while (t < duration && !dismissed)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(go);
    }
}
