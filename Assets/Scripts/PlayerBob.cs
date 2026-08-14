using UnityEngine;

// Runs after PerspectiveScaler (order 10) so it reads the already-computed scale
// and applies squash/stretch + tilt on top without fighting the scaler.
[DefaultExecutionOrder(20)]
public class PlayerBob : MonoBehaviour
{
    [Header("Bob (squash & stretch)")]
    [SerializeField] private float bobFrequency = 2.6f;   // cycles per second
    [SerializeField] private float bobAmplitude = 0.09f;  // 0.09 = 9% stretch

    [Header("Tilt")]
    [SerializeField] private float maxTiltAngle = 5f;     // degrees
    [SerializeField] private float tiltSmoothing = 9f;

    private Rigidbody2D rb;
    private float phase;
    private float currentTilt;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        float speed       = rb != null ? rb.velocity.magnitude : 0f;
        float speedFactor = Mathf.Clamp01(speed / 2f);

        // ── Bob ──────────────────────────────────────────────────────────────
        phase += Time.deltaTime * bobFrequency * Mathf.PI * 2f;

        float bob = Mathf.Sin(phase) * bobAmplitude * speedFactor;

        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(
            s.x * (1f - bob * 0.35f),  // slight X squash as Y stretches
            s.y * (1f + bob),
            s.z
        );

        // ── Tilt ─────────────────────────────────────────────────────────────
        float targetTilt = 0f;
        if (rb != null && speed > 0.08f)
            targetTilt = -Mathf.Sign(rb.velocity.x) * maxTiltAngle * speedFactor;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothing);
        transform.rotation = Quaternion.Euler(0f, 0f, currentTilt);
    }
}
