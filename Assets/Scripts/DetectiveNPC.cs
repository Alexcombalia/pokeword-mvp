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
    protected virtual string GreetingCa => null;
    protected virtual string GreetingTranslationCa => null;

    // ── Volver a hablar con alguien ──────────────────────────────────────
    //
    // La frase de entrada se recitaba ENTERA cada vez que se abría la
    // conversación. La primera vez presenta al personaje; a partir de la
    // segunda suena a máquina, y este juego se sostiene entero sobre la
    // sensación de estar hablando con una persona.
    //
    // Desde la segunda vez se usa una línea corta de "ya nos conocemos". Si
    // un personaje no la define, se repite la larga: nunca se queda mudo.
    protected virtual string GreetingAgain   => null;
    protected virtual string GreetingAgainEn => null;
    protected virtual string GreetingAgainIt => null;
    protected virtual string GreetingAgainCa => null;
    protected virtual string GreetingAgainTranslation   => null;
    protected virtual string GreetingAgainTranslationEn => null;
    protected virtual string GreetingAgainTranslationIt => null;
    protected virtual string GreetingAgainTranslationCa => null;

    // Por instancia, así que se reinicia solo al recargar la escena — es
    // decir, una vez por partida del nivel, que es justo lo que se quiere.
    bool hasGreeted;

    string PickLang(string fr, string en, string it, string ca) =>
        (GameSettings.IsEnglish && !string.IsNullOrEmpty(en)) ? en :
        (GameSettings.IsItalian && !string.IsNullOrEmpty(it)) ? it :
        (GameSettings.IsCatalan && !string.IsNullOrEmpty(ca)) ? ca : fr;

    string ActiveGreeting
    {
        get
        {
            if (hasGreeted)
            {
                string again = PickLang(GreetingAgain, GreetingAgainEn, GreetingAgainIt, GreetingAgainCa);
                if (!string.IsNullOrEmpty(again)) return again;
            }
            return PickLang(Greeting, GreetingEn, GreetingIt, GreetingCa);
        }
    }

    string ActiveGreetingTranslation
    {
        get
        {
            if (hasGreeted && !string.IsNullOrEmpty(PickLang(GreetingAgain, GreetingAgainEn, GreetingAgainIt, GreetingAgainCa)))
                return PickLang(GreetingAgainTranslation, GreetingAgainTranslationEn, GreetingAgainTranslationIt, GreetingAgainTranslationCa);
            return PickLang(GreetingTranslation, GreetingTranslationEn, GreetingTranslationIt, GreetingTranslationCa);
        }
    }

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
        else
            // La conversación ya NO se borra al reabrir el diálogo (así el
            // testigo recuerda lo que le acabas de preguntar), pero sí tiene
            // que empezar limpia en cada PARTIDA: si no, al reintentar el caso
            // el personaje seguiría acordándose del intento anterior y podría
            // dar por dicho algo que en esta partida nadie ha dicho.
            //
            // Aquí es el sitio exacto: Start() corre una vez por carga de
            // escena, que es una vez por partida del nivel.
            StartCoroutine(chatClient.ResetMemory());
    }

    private void Update()
    {
        if (!playerInRange || dialogueSystem == null || chatClient == null) return;

        if (!dialogueSystem.IsOpen() && (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed))
        {
            indicator?.Hide();
            // Se leen ANTES de marcar hasGreeted: la primera vez tienen que
            // devolver todavía la presentación larga.
            string line = ActiveGreeting;
            string tr   = ActiveGreetingTranslation;
            hasGreeted = true;

            dialogueSystem.OpenDetectiveChat(chatClient, DisplayName, line, tr, transform);
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
