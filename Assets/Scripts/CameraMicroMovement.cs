using UnityEngine;

// Add to Main Camera in each scene.
// Applies a very subtle breathing sway when the player moves,
// breaking the "frozen image" feeling without moving the camera visibly.
public class CameraMicroMovement : MonoBehaviour
{
    [SerializeField] private float amplitude  = 0.025f;  // world units — keep very small
    [SerializeField] private float frequency  = 1.6f;    // sway cycles per second
    [SerializeField] private float smoothing  = 3.5f;    // how fast sway fades in/out

    private Vector3        basePosition;
    private float          phase;
    private float          currentAmplitude;
    private PlayerMovement player;

    private void Start()
    {
        basePosition = transform.position;
        player       = FindObjectOfType<PlayerMovement>();
    }

    private void Update()
    {
        if (player == null) player = FindObjectOfType<PlayerMovement>();

        float speed          = player != null ? player.CurrentSpeed : 0f;
        float targetAmp      = speed > 0.05f ? amplitude : 0f;
        currentAmplitude     = Mathf.Lerp(currentAmplitude, targetAmp, Time.deltaTime * smoothing);

        if (currentAmplitude > 0.0005f)
            phase += Time.deltaTime * frequency * Mathf.PI * 2f;

        float ox = Mathf.Sin(phase)         * currentAmplitude * 0.4f;
        float oy = Mathf.Sin(phase * 2f)   * currentAmplitude * 0.25f;

        transform.position = Vector3.Lerp(
            transform.position,
            basePosition + new Vector3(ox, oy, 0f),
            Time.deltaTime * smoothing * 2f
        );
    }
}
