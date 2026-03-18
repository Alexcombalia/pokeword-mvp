using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSignal : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private void OnMouseDown()
    {
        if (string.IsNullOrEmpty(targetScene)) return;

        // Si existe ResourceSystem, aplicamos drenaje por cambio de escena
        var resources = FindObjectOfType<ResourceSystem>();
        if (resources != null)
        {
            resources.DrainOnSceneChange();
        }

        SceneManager.LoadScene(targetScene);
    }
}
