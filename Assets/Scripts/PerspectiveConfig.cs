using UnityEngine;

// Place one of these in every scene that needs a perspective depth effect.
// PerspectiveScaler on the player reads it automatically on scene load.
public class PerspectiveConfig : MonoBehaviour
{
    [Header("Vertical bounds (world space Y)")]
    [Tooltip("Y position of the farthest walkable point (top of scene)")]
    public float topY = 3f;
    [Tooltip("Y position of the nearest walkable point (bottom of scene)")]
    public float bottomY = -3f;

    [Header("Scale curve")]
    [Tooltip("X = 0 is topY (far/small), X = 1 is bottomY (near/large). Y = scale multiplier.")]
    public AnimationCurve scaleCurve = new AnimationCurve(
        new Keyframe(0f, 0.65f, 0f, 0f),
        new Keyframe(1f, 1.35f, 0f, 0f)
    );

    [Header("Sorting order")]
    [Tooltip("Sorting order when player is at topY (behind far objects)")]
    public int sortingOrderTop    = -5;
    [Tooltip("Sorting order when player is at bottomY (in front of near objects)")]
    public int sortingOrderBottom =  5;

    [Header("Toggle")]
    public bool enablePerspective = true;
}
