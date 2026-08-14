using UnityEngine;

// Base de los NPCs del modo detective. Misma mecánica de acercarse y pulsar E
// que JacquesNPC/GenericNPC; lo que cambia es que cada personaje declara su
// identidad y su frase de entrada en su propio archivo.
//
// La frase de entrada NO viene del servidor: se muestra al abrir, antes de
// gastar un turno de conversación. En un caso de misterio eso importa más que
// en modo débutant, porque dos de ellas (la de Denis presentándose y la de
// Fabre dando por hecho que fue un robo) son piezas del caso, y no pueden
// depender de que el modelo se acuerde de decirlas en su primera respuesta.
[RequireComponent(typeof(InteractIndicator))]
public abstract class DetectiveNPC : MonoBehaviour
{
    // Nombre que se ve en el globo de pergamino.
    public abstract string DisplayName { get; }

    // Frase de entrada, en francés, ya etiquetada con [francés|español] para
    // que sus palabras se puedan capturar como cualquier otra.
    public abstract string Greeting { get; }

    // Traducción de la frase de entrada. El modo detective es avanzado y no la
    // usa, pero se deja escrita: es una sola frase fija por personaje y evita
    // que quede sin traducir si algún día se abre el modo a débutant.
    protected virtual string GreetingTranslation => null;

    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private DetectiveChatClient chatClient;
    private InteractIndicator indicator;

    protected virtual void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        chatClient = GetComponent<DetectiveChatClient>();
        indicator = GetComponent<InteractIndicator>();
        if (GetComponent<YSorter>() == null)
            gameObject.AddComponent<YSorter>();

        if (chatClient == null)
            Debug.LogError($"[{DisplayName}] falta su DetectiveChatClient en el mismo GameObject.", this);
    }

    private void Update()
    {
        if (!playerInRange || dialogueSystem == null || chatClient == null) return;

        if (!dialogueSystem.IsOpen() && (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed))
        {
            indicator?.Hide();
            dialogueSystem.OpenDetectiveChat(chatClient, DisplayName, Greeting, GreetingTranslation, transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        indicator?.Show();
        Haptics.Light();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        indicator?.Hide();
    }
}
