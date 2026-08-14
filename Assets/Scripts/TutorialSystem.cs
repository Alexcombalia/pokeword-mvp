using UnityEngine;

// Retirado (2026-07-17): el manual de 6 páginas de texto se sustituyó por
// CoachMarks — pistas contextuales "aprender haciendo" dentro del propio
// Nivel 1, junto al cronómetro, el panel de palabras y el botón de micrófono.
// Se deja este stub (en vez de borrar la clase) porque MainMenuController
// todavía llama a TutorialSystem.Instance.ResetTutorial().
public class TutorialSystem : MonoBehaviour
{
    public static TutorialSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("tutorialDone");
        PlayerPrefs.Save();
    }
}
