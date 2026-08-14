using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Place this on an invisible trigger zone at the edge of a scene.
// When the player walks into it, a hint appears. Press E to transition.
public class SceneExit : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string targetScene;
    [SerializeField] private string destinationLabel; // e.g. "Marché", "Le Panier"

    // Para que GuidedTutorial pueda encontrar "la salida hacia X" en tiempo
    // de ejecución (FindObjectsOfType<SceneExit>) y señalar su posición en
    // el mundo — ver ExplainSceneCrossingOnce.
    public string TargetScene => targetScene;

    // Para SceneExitSignManager (señal de madera permanente) — mismo
    // criterio que ShowHint/TryTransition, expuesto para no duplicar lógica.
    public string DestinationLabel => destinationLabel;
    public bool IsCrossingAllowedNow => IsCrossingAllowed();

    [Header("UI Hint (assign a world-space or screen-space TMP_Text)")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TMP_Text hintText;

    [Header("Locked message (shown when level is too low)")]
    [SerializeField] private string lockedMessage; // e.g. "Accès interdit. Niveau 4 requis."

    private bool playerInZone = false;

    private void Start()
    {
        HideHint();
    }

    private void Update()
    {
        if (!playerInZone) return;
        if (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed) TryTransition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = true;
        ShowHint();
        Haptics.Light();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = false;
        HideHint();
    }

    private bool IsCrossingAllowed()
    {
        var allowed = LevelManager.Instance?.CurrentLevel?.allowedScenes;
        return allowed != null && allowed.Contains(targetScene);
    }

    private void TryTransition()
    {
        if (string.IsNullOrEmpty(targetScene)) return;

        // Check dialogue open
        var dialogue = FindObjectOfType<DialogueSystem>();
        if (dialogue != null && dialogue.IsOpen()) return;

        // Nivel activo con reto en marcha — por defecto no se puede ir a
        // otra escena, para no repetir la confusión original de "demasiadas
        // cosas a la vez" que motivó el rediseño. Los niveles que sí
        // necesitan cruzar escenas (p.ej. un recado entre dos NPCs) listan
        // explícitamente a qué escenas se puede ir en LevelDefinition.allowedScenes.
        if (LevelManager.Instance != null && LevelManager.Instance.IsActive && !IsCrossingAllowed())
        {
            ShowLevelBlocked();
            return;
        }

        // Check level requirement
        int required = LevelSystem.RequiredLevel(targetScene);
        int current = LevelSystem.Instance != null ? LevelSystem.Instance.CurrentLevel() : 0;

        if (current < required)
        {
            ShowBlocked(required);
            return;
        }

        // Drain resource on scene change
        var resources = FindObjectOfType<ResourceSystem>();
        if (resources != null) resources.DrainOnSceneChange();

        HideHint();
        SceneManager.LoadScene(targetScene);
    }

    private void ShowHint()
    {
        if (hintPanel != null) hintPanel.SetActive(true);
        if (hintText == null) return;

        if (LevelManager.Instance != null && LevelManager.Instance.IsActive && !IsCrossingAllowed())
        {
            hintText.text = GameSettings.IsBasic
                ? $"→ {destinationLabel}  (termina el reto primero)"
                : $"→ {destinationLabel}  (termine ton défi d'abord)";
            return;
        }

        int required = LevelSystem.RequiredLevel(targetScene);
        int current = LevelSystem.Instance != null ? LevelSystem.Instance.CurrentLevel() : 0;

        if (current >= required || required == 0)
            hintText.text = $"→ {destinationLabel}     [E]";
        else
            hintText.text = $"→ {destinationLabel}  (Niveau {required})"; // sin glyph "🔒" — mismo bug de fuente que las estrellas, ver StarIcon.cs
    }

    private void ShowLevelBlocked()
    {
        if (hintText != null)
            hintText.text = GameSettings.IsBasic
                ? "Termina el reto actual primero"
                : "Termine d'abord le défi en cours";
    }

    private void ShowBlocked(int required)
    {
        if (hintText != null)
        {
            string msg = !string.IsNullOrEmpty(lockedMessage)
                ? lockedMessage
                : $"Niveau {required} requis.";
            hintText.text = msg;
        }
    }

    private void HideHint()
    {
        if (hintPanel != null) hintPanel.SetActive(false);
    }
}
