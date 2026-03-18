using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var spawn = GameObject.Find("SpawnPoint");
        var player = GameObject.FindWithTag("Player");

        if (spawn != null && player != null)
        {
            player.transform.position = spawn.transform.position;
        }
    }
}
