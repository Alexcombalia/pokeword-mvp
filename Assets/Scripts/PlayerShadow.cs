using UnityEngine;

// Add to the Player prefab.
// Creates the shadow as a top-level GameObject (not a child) so it is NOT
// affected by the player's scale. Follows the player position every LateUpdate.
public class PlayerShadow : MonoBehaviour
{
    [SerializeField] private Vector2 worldOffset = new Vector2(0f, -0.28f); // offset in world units
    [SerializeField] private Vector2 worldSize   = new Vector2(0.9f, 0.32f); // world-space size
    [SerializeField] private float   opacity     = 0.45f;

    private SpriteRenderer playerSR;
    private GameObject     shadowGO;
    private SpriteRenderer shadowSR;

    private void Awake()
    {
        playerSR = GetComponent<SpriteRenderer>();

        shadowGO = new GameObject("PlayerShadow");
        DontDestroyOnLoad(shadowGO);

        shadowSR        = shadowGO.AddComponent<SpriteRenderer>();
        shadowSR.sprite = BuildEllipseSprite(128, 48);
        shadowSR.color  = new Color(0f, 0f, 0f, opacity);
        shadowSR.sortingLayerName = playerSR != null ? playerSR.sortingLayerName : "Default";
        shadowSR.sortingOrder     = -10;

        shadowGO.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
    }

    private void LateUpdate()
    {
        if (shadowGO == null) return;

        // Follow player in world space, unaffected by player scale
        shadowGO.transform.position = transform.position + new Vector3(worldOffset.x, worldOffset.y, 0f);

        // Stay just behind the player in sorting order
        if (playerSR != null && shadowSR != null)
            shadowSR.sortingOrder = playerSR.sortingOrder - 1;
    }

    private void OnDestroy()
    {
        if (shadowGO != null) Destroy(shadowGO);
    }

    private static Sprite BuildEllipseSprite(int w, int h)
    {
        var tex    = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color32[w * h];
        float cx   = w * 0.5f, cy = h * 0.5f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x + 0.5f - cx) / cx;
                float dy = (y + 0.5f - cy) / cy;
                float d  = dx * dx + dy * dy;
                float a  = Mathf.Clamp01(1f - d);
                a        = Mathf.Pow(a, 0.5f);
                pixels[y * w + x] = new Color32(0, 0, 0, (byte)(a * 255));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }
}
