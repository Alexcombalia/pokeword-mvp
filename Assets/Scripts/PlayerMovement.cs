using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private DialogueSystem dialogue;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        dialogue = FindObjectOfType<DialogueSystem>();
    }

    private void Update()
    {
        // Si hay diálogo abierto, detener movimiento
        if (dialogue != null && dialogue.IsOpen())
        {
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal"); // A/D o Flechas
        float y = Input.GetAxisRaw("Vertical");   // W/S o Flechas

        moveInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        if (dialogue != null && dialogue.IsOpen())
            return;

        rb.velocity = moveInput * moveSpeed;
    }
}