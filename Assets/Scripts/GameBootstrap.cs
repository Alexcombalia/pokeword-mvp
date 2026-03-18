using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private static bool created = false;

    [Header("Prefabs a mantener en todo el juego")]
    public GameObject gameManagerPrefab;
    public GameObject hudPrefab;
    public GameObject playerPrefab;
    public GameObject globalUIPrefab;



    private void Awake()
    {
        if (created)
        {
            Destroy(gameObject);
            return;
        }

        created = true;
        DontDestroyOnLoad(gameObject);

        // GameManager persistente
        if (FindObjectOfType<ResourceSystem>() == null && gameManagerPrefab != null)
        {
            var gm = Instantiate(gameManagerPrefab);
            gm.name = "GameManager";
            DontDestroyOnLoad(gm);
        }

        // HUD persistente
        if (FindObjectOfType<HUDController>() == null && hudPrefab != null)
        {
            var hud = Instantiate(hudPrefab);
            hud.name = "HUDCanvas";
            DontDestroyOnLoad(hud);
        }

        // UI global persistente (Dialogue + Pokéworld)
        if (FindObjectOfType<DialogueSystem>() == null && globalUIPrefab != null)
        {
            var ui = Instantiate(globalUIPrefab);
            ui.name = "GlobalUI";
            DontDestroyOnLoad(ui);
        }


        // Player persistente
        if (GameObject.FindWithTag("Player") == null && playerPrefab != null)
        {
            var player = Instantiate(playerPrefab);
            player.name = "Player";
            DontDestroyOnLoad(player);
        }

    }
}
