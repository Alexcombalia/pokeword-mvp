using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSignal : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private void OnMouseDown()
    {
        var dialogue = FindObjectOfType<DialogueSystem>();
        if (dialogue != null && dialogue.IsOpen())
            return;

        if (string.IsNullOrEmpty(targetScene)) return;

        var resources = FindObjectOfType<ResourceSystem>();
        if (resources != null)
        {
            resources.DrainOnSceneChange();
        }

        SceneManager.LoadScene(targetScene);
    }
}