using UnityEngine;

// Base de los NPCs del modo detective. Misma mecánica de acercarse y pulsar E
// que JacquesNPC/GenericNPC; lo que cambia es que cada personaje declara su
// identidad y su frase de entrada en su propio archivo.
//
// La frase de entrada NO viene del servidor: se muestra al abrir, antes de
// gastar un turno de conversación. En un caso de misterio eso importa más que
// en modo débutant, porque dos de ellas (la de Marcel presentándose y la de
// Fabre dando por hecho que fue un robo) son piezas del caso, y no pueden
// depender de que el modelo se acuerde de decirlas en su primera respuesta.
[RequireComponent(typeof(InteractIndicator))]
public abstract class DetectiveNPC : MonoBehaviour
{
    // Nombre que se ve en el globo de pergamino.
    public abstract string DisplayName { get; }

    // Frase de entrada, ya etiquetada con [idioma|español] para que sus
    // palabras se puedan capturar como cualquier otra.
    //
    // Vive en el cliente, no en el servidor, porque se muestra ANTES de la
    // primera llamada. Eso la deja fuera del sistema de idioma del servidor:
    // si un personaje no da su versión inglesa, la conversación abre en
    // francés aunque la partida sea en inglés. Fue justo lo que pasó al
    // probar la primera versión inglesa.
    public abstract string Greeting { get; }

    // Traducción al español de la frase de entrada, para el modo débutant.
    protected virtual string GreetingTranslation => null;

    // Versión inglesa. Null en los personajes del caso avanzado, que solo
    // existe en francés — ahí se usa la francesa y no pasa nada.
    protected virtual string GreetingEn => null;
    protected virtual string GreetingTranslationEn => null;
    protected virtual string GreetingIt => null;
    protected virtual string GreetingTranslationIt => null;

    string ActiveGreeting =>
        (GameSettings.IsEnglish && !string.IsNullOrEmpty(GreetingEn)) ? GreetingEn :
        (GameSettings.IsItalian && !string.IsNullOrEmpty(GreetingIt)) ? GreetingIt : Greeting;

    string ActiveGreetingTranslation =>
        (GameSettings.IsEnglish && !string.IsNullOrEmpty(GreetingEn)) ? GreetingTranslationEn :
        (GameSettings.IsItalian && !string.IsNullOrEmpty(GreetingIt)) ? GreetingTranslationIt : GreetingTranslation;

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
            dialogueSystem.OpenDetectiveChat(chatClient, DisplayName, ActiveGreeting, ActiveGreetingTranslation, transform);
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
