using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.8f;

    public float CurrentSpeed => rb != null ? rb.velocity.magnitude : 0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private DialogueSystem dialogue;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        dialogue = FindObjectOfType<DialogueSystem>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (GetComponent<YSorter>() == null)
            gameObject.AddComponent<YSorter>();
    }

    private void Update()
    {
        if (dialogue != null && dialogue.IsOpen())
        {
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            return;
        }

        float x = Input.GetAxisRaw("Horizontal") + VirtualJoystick.Direction.x;
        float y = Input.GetAxisRaw("Vertical")   + VirtualJoystick.Direction.y;
        moveInput = new Vector2(x, y).normalized;

        if (animator != null)
        {
            float speed = moveInput.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetFloat("DirY", moveInput.y);
            animator.speed = speed > 0.05f ? 1f : 0f;
        }

        if (spriteRenderer != null)
        {
            if (x < 0)
                spriteRenderer.flipX = true;
            else if (x > 0)
                spriteRenderer.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        if (dialogue != null && dialogue.IsOpen())
            return;

        Vector2 targetVelocity = moveInput * moveSpeed;

        // Fast acceleration, slower deceleration — feels weighted but responsive
        bool accelerating = targetVelocity.magnitude > rb.velocity.magnitude;
        float lerpFactor  = accelerating ? 0.55f : 0.22f;

        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, lerpFactor);
    }
}