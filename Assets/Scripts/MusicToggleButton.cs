using UnityEngine;
using UnityEngine.UI;

// Botón de silenciar la música. Overlay propio y persistente, igual que el
// profesor: así está en el menú Y dentro del nivel sin duplicarlo en cada
// pantalla, y no depende de que ninguna escena lo monte.
//
// Silencia SOLO la música. Las voces de los NPCs y la lectura en voz alta
// siguen sonando — son el juego, no ambientación, y quien apaga la música
// normalmente no quiere apagar eso.
public class MusicToggleButton : MonoBehaviour
{
    public static MusicToggleButton Instance { get; private set; }

    static readonly Color NavyLight = new Color(0.051f, 0.122f, 0.208f, 0.85f);
    static readonly Color Gold      = new Color(1.000f, 0.820f, 0.400f, 1.00f);
    static readonly Color GoldDim   = new Color(1.000f, 0.820f, 0.400f, 0.45f);

    Image icon;
    RectTransform btnRT;

    // Posición de reposo, cuando el pergamino de objetivos no estorba.
    // El botón del profesor se cuelga 110 px por debajo de esta (ver
    // TeacherPanel), así que los dos se mueven juntos.
    public const float BaseY = -320f;

    public static float StackY =>
        Mathf.Min(BaseY, LevelHUD.BannerBottomY - 24f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        Refresh();
    }

    // El pergamino de objetivos crece con cada hallazgo, así que este botón no
    // puede vivir en una posición fija: se recoloca por debajo de él.
    void Update()
    {
        if (btnRT == null) return;
        float y = StackY;
        if (!Mathf.Approximately(btnRT.anchoredPosition.y, y))
            btnRT.anchoredPosition = new Vector2(btnRT.anchoredPosition.x, y);
    }

    void Toggle()
    {
        MusicManager.Muted = !MusicManager.Muted;
        MusicManager.Instance?.ApplyMuteSetting();
        Refresh();
    }

    void Refresh()
    {
        bool muted = MusicManager.Muted;
        icon.sprite = SoundIcon.GetSprite(muted);
        // Apagado se ve apagado: además del aspa, el icono pierde intensidad.
        // Con solo el aspa, de reojo y a 48 px, los dos estados se parecen.
        icon.color  = muted ? GoldDim : Gold;
    }

    void BuildUI()
    {
        var canvas          = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        // Por encima del menú (100) y del HUD, por debajo del profesor (460):
        // con el panel del profesor abierto este botón queda tapado, que es lo
        // correcto — ese panel es modal.
        canvas.sortingOrder = 455;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        var btnGO = new GameObject("MusicBtn", typeof(RectTransform), typeof(Image));
        btnGO.transform.SetParent(transform, false);
        var rt = btnGO.GetComponent<RectTransform>();
        btnRT = rt;
        // Donde estaba el panel de "palabras a encontrar", que se retiró al
        // dejar de ser objetivo (02/09/2026). Es el mejor sitio libre: por
        // debajo del pergamino de objetivos y fuera del pulgar.
        //
        // El botón del profesor va justo debajo, en (-24,-355).
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-24, BaseY);
        rt.sizeDelta = new Vector2(84, 84);
        var bg = btnGO.GetComponent<Image>();
        bg.sprite = RoundedRectSprite.GetSprite();
        bg.type   = Image.Type.Sliced;
        bg.color  = NavyLight;

        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(btnGO.transform, false);
        var iRT = iconGO.GetComponent<RectTransform>();
        iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one;
        iRT.offsetMin = new Vector2(16, 16); iRT.offsetMax = new Vector2(-16, -16);
        icon = iconGO.GetComponent<Image>();
        icon.raycastTarget = false;

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.onClick.AddListener(Toggle);
    }
}
