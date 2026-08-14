using System.Collections.Generic;
using UnityEngine;

// Miguel — spanish refugee in the port. Scripted dialogue, no AI.
public class TutorialNPC : MonoBehaviour
{
    private static readonly List<List<string>> conversations = new List<List<string>>
    {
        new List<string>
        {
            "¡Español! Por fin alguien que me entiende.\n" +
            "Llevo tres semanas en este [port|puerto].",
            "Los franceses son amables pero... nadie habla español.\n" +
            "Tienes que aprender su [langue|lengua], ¿entiendes?",
            "Busca a Jacques, el [marin|marinero]. Está por el [quai|muelle].\n" +
            "Él tiene [travail|trabajo] para gente como nosotros."
        },
        new List<string>
        {
            "Jacques ya te dio [travail|trabajo] ? Qué bien.\n" +
            "Este [port|puerto] es peligroso por las [nuits|noches].",
            "Cuida tus [papiers|papeles]. La [police|policía] los pide a cada rato.",
            "Y aprende [français|francés]... es tu mejor [arme|arma] aquí."
        },
        new List<string>
        {
            "¿Cómo va tu [français|francés] ? Cada [jour|día] un poco mejor, ¿no?",
            "Yo ya entiendo casi todo. Pero hablar... eso cuesta más.\n" +
            "La [peur|miedo] te bloquea la [langue|lengua].",
            "Sigue hablando con la [gente|gens]. Es la única manera."
        }
    };

    private int convIndex = 0;
    private bool playerInRange = false;
    private DialogueSystem dialogueSystem;
    private InteractIndicator indicator;

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        indicator      = GetComponent<InteractIndicator>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (dialogueSystem == null || dialogueSystem.IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed)
        {
            indicator?.Hide();
            var conv = conversations[convIndex % conversations.Count];
            dialogueSystem.Open(new List<string>(conv));
            convIndex++;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        indicator?.Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        indicator?.Hide();
    }
}
