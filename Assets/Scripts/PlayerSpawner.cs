using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("Position used when the scene has no SpawnPoint object")]
    [SerializeField] private Vector2 defaultSpawnPosition = new Vector2(-3f, -1f);

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
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var spawn = GameObject.Find("SpawnPoint");
        player.transform.position = spawn != null
            ? spawn.transform.position
            : (Vector3)defaultSpawnPosition;
    }
}
