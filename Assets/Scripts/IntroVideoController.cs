using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class IntroVideoController : MonoBehaviour
{
    [SerializeField] private float waitSeconds = 3f;

    private void Start()
    {
        Invoke(nameof(GoToPuerto), waitSeconds);
        BuildSkipButton();
    }

    public void SkipVideo()
    {
        CancelInvoke(nameof(GoToPuerto));
        GoToPuerto();
    }

    private void GoToPuerto()
    {
        SceneManager.LoadScene("Puerto");
    }

    private void BuildSkipButton()
    {
        var cvGO = new GameObject("SkipCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var cv = cvGO.GetComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 10;

        var scaler = cvGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight   = 0f;

        var btnGO = new GameObject("BtnSkip", typeof(RectTransform), typeof(Image));
        btnGO.transform.SetParent(cvGO.transform, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
        rt.pivot     = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-30, 30);
        rt.sizeDelta        = new Vector2(220, 54);

        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.55f);

        var lbl = new GameObject("Label", typeof(RectTransform));
        lbl.transform.SetParent(btnGO.transform, false);
        var lRT = lbl.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.anchoredPosition = Vector2.zero; lRT.sizeDelta = Vector2.zero;

        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text      = "Omitir vídeo  >";
        tmp.fontSize  = 22f;
        tmp.color     = new Color(0.94f, 0.90f, 0.83f, 0.85f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        var cols = btn.colors;
        cols.normalColor      = new Color(0, 0, 0, 0.55f);
        cols.highlightedColor = new Color(0.1f, 0.1f, 0.1f, 0.75f);
        cols.pressedColor     = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        btn.colors = cols;
        btn.onClick.AddListener(SkipVideo);
    }
}
