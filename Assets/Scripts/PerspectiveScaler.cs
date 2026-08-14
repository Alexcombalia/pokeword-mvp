using UnityEngine;

// Add this to the persistent Player prefab.
// GameBootstrap calls SetBaseScale() on scene load.
// This script then applies a Y-position curve multiplier on top of that base scale
// and adjusts the SpriteRenderer sortingOrder for correct depth layering.
[DefaultExecutionOrder(10)]
public class PerspectiveScaler : MonoBehaviour
{
    private Vector3          baseScale = Vector3.one;
    private PerspectiveConfig config;
    private SpriteRenderer   sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Called by GameBootstrap after it determines the scene's base scale.
    public void SetBaseScale(Vector3 scale)
    {
        baseScale = scale;
    }

    // Called by GameBootstrap after scene load so we pick up the new PerspectiveConfig.
    public void RefreshConfig()
    {
        config = FindObjectOfType<PerspectiveConfig>();
    }

    private void Update()
    {
        if (config == null || !config.enablePerspective)
        {
            transform.localScale = baseScale;
            return;
        }

        float t          = Mathf.InverseLerp(config.topY, config.bottomY, transform.position.y);
        float multiplier = config.scaleCurve.Evaluate(t);

        transform.localScale = new Vector3(
            baseScale.x * multiplier,
            baseScale.y * multiplier,
            baseScale.z
        );

        if (sr != null)
            sr.sortingOrder = Mathf.RoundToInt(
                Mathf.Lerp(config.sortingOrderTop, config.sortingOrderBottom, t)
            );
    }
}
