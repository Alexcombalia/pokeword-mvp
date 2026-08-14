using UnityEngine;

// Attach to any sprite that should sort by Y position.
// Lower Y = in front, higher Y = behind.
[RequireComponent(typeof(SpriteRenderer))]
public class YSorter : MonoBehaviour
{
    [SerializeField] private int baseOrder = 0;

    // Vertical distance from transform.position down to the sprite's visual "feet" line.
    // Needed because different characters (player vs. NPCs) can have their pivot at
    // different heights — sorting by raw transform Y alone misaligns exactly where two
    // sprites visually overlap. Tune per-character if characters still clip incorrectly.
    [SerializeField] private float feetOffset = 0f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetFeetOffset(float offset) => feetOffset = offset;

    private void LateUpdate()
    {
        sr.sortingOrder = baseOrder + Mathf.RoundToInt(-(transform.position.y + feetOffset) * 100f);
    }
}
