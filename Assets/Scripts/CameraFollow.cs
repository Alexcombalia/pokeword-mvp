using UnityEngine;

// Add to the Main Camera in every gameplay scene. Classic 2D "camera follows the
// player, zoomed in, world bigger than the screen" — like Pokémon GBA. Follows the
// player but stays clamped inside the background art's bounds (no blue void), while
// guaranteeing the player never fully leaves the visible frame near map edges.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Camera half-height as a multiple of the background art's half-height — 1 = matches it exactly, higher = less zoom (more visible left/right), lower = more zoom.")]
    [SerializeField] private float zoomFactor = 1.2f;
    [SerializeField] private float smoothTime = 0.08f;

    // Fraction of the half-viewport the player is allowed to stray from the
    // bounds-clamped camera center. Keeping the player visible always wins over
    // staying inside the background art near a map edge.
    private const float VisibleMargin = 0.85f;

    private Transform target;
    private Vector3 velocity;
    private Bounds bgBounds;
    private bool hasBounds;
    private Camera cam;

    // Same navy used across the HUD/dialogue UI — when zoomFactor > 1 the viewport is
    // taller than the background art, so a thin strip of camera clear color is
    // unavoidable top/bottom. Matching the UI navy instead of the default sky-blue
    // clear color makes it read as an intentional letterbox, not a broken background.
    private static readonly Color LetterboxColor = new Color(0.051f, 0.122f, 0.208f, 1f);

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic) return;

        if (cam.clearFlags == CameraClearFlags.SolidColor || cam.clearFlags == CameraClearFlags.Skybox)
            cam.backgroundColor = LetterboxColor;

        hasBounds = TryGetBackgroundBounds(out bgBounds);
        if (hasBounds)
            cam.orthographicSize = Mathf.Max(0.5f, bgBounds.extents.y * zoomFactor);

        var player = GameObject.FindWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
            else return;
        }

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        if (hasBounds)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            // If the viewport is bigger than the background on an axis (e.g. zoomFactor
            // > 1 makes the view taller than the art), there's no room to pan on that
            // axis at all — always center on the background itself there.
            float minX = bgBounds.min.x + halfW, maxX = bgBounds.max.x - halfW;
            float minY = bgBounds.min.y + halfH, maxY = bgBounds.max.y - halfH;

            desired.x = maxX >= minX ? Mathf.Clamp(desired.x, minX, maxX) : bgBounds.center.x;
            desired.y = maxY >= minY ? Mathf.Clamp(desired.y, minY, maxY) : bgBounds.center.y;

            // Safety net: never let the bounds clamp push the player out of frame
            // (walkable areas aren't guaranteed to exactly match the painted background).
            float marginX = halfW * VisibleMargin;
            float marginY = halfH * VisibleMargin;
            desired.x = Mathf.Clamp(desired.x, target.position.x - marginX, target.position.x + marginX);
            desired.y = Mathf.Clamp(desired.y, target.position.y - marginY, target.position.y + marginY);
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    // Background art/video objects are named "Background*" in every scene. Returns
    // their combined world-space bounds, or false if none were found.
    private static bool TryGetBackgroundBounds(out Bounds bounds)
    {
        bool found = false;
        bounds = new Bounds();

        foreach (var r in FindObjectsOfType<Renderer>())
        {
            if (!r.gameObject.activeInHierarchy) continue;
            if (!r.gameObject.name.StartsWith("Background")) continue;

            if (!found) { bounds = r.bounds; found = true; }
            else bounds.Encapsulate(r.bounds);
        }

        return found;
    }
}
