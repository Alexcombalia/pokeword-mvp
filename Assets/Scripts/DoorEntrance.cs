using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Place this on a trigger near a door/entrance.
// Uses the existing InteractIndicator system. Press E to enter.
public class DoorEntrance : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string targetScene;
    [SerializeField] private string interiorLabel; // e.g. "Bar le Dragon Vert"

    [Header("Visual indicator (same system as NPCs)")]
    [SerializeField] private InteractIndicator indicator;

    [Header("Optional label text to show above door")]
    [SerializeField] private TMP_Text doorLabel;

    [Header("Locked message")]
    [SerializeField] private string lockedMessage; // e.g. "Fermé. Revenez plus tard."

    private bool playerInZone = false;

    private void Start()
    {
        if (indicator != null) indicator.Hide();
        if (doorLabel != null) doorLabel.text = interiorLabel;
    }

    private void Update()
    {
        if (!playerInZone) return;
        if (Input.GetKeyDown(KeyCode.E) || MobileInput.InteractPressed) TryEnter();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = true;
        if (indicator != null) indicator.Show();
        UpdateIndicatorLabel();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = false;
        if (indicator != null) indicator.Hide();
    }

    private void UpdateIndicatorLabel()
    {
        int required = LevelSystem.RequiredLevel(targetScene);
        int current = LevelSystem.Instance != null ? LevelSystem.Instance.CurrentLevel() : 0;

        // If the indicator has a child TMP_Text, update it
        if (indicator != null)
        {
            var txt = indicator.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.text = (current >= required || required == 0)
                    ? $"[E] Entrer · {interiorLabel}"
                    : $"[E] {interiorLabel}  (Niveau {required})"; // sin glyph "🔒" — mismo bug de fuente, ver SceneExit.cs
            }
        }
    }

    private void TryEnter()
    {
        if (string.IsNullOrEmpty(targetScene)) return;

        var dialogue = FindObjectOfType<DialogueSystem>();
        if (dialogue != null && dialogue.IsOpen()) return;

        int required = LevelSystem.RequiredLevel(targetScene);
        int current = LevelSystem.Instance != null ? LevelSystem.Instance.CurrentLevel() : 0;

        if (current < required)
        {
            ShowLockedFeedback(required);
            return;
        }

        var resources = FindObjectOfType<ResourceSystem>();
        if (resources != null) resources.DrainOnSceneChange();

        if (indicator != null) indicator.Hide();
        SceneManager.LoadScene(targetScene);
    }

    private void ShowLockedFeedback(int required)
    {
        // Update the indicator text temporarily to show the locked reason
        if (indicator != null)
        {
            var txt = indicator.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                string msg = !string.IsNullOrEmpty(lockedMessage)
                    ? lockedMessage
                    : $"Niveau {required} requis.";
                txt.text = msg;
            }
        }
    }
}
