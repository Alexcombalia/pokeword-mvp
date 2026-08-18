using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private static bool created = false;

    [Header("Prefabs a mantener en todo el juego")]
    public GameObject gameManagerPrefab;
    public GameObject hudPrefab;
    public GameObject playerPrefab;
    public GameObject globalUIPrefab;
    public GameObject musicManagerPrefab;

    private static readonly string[] menuScenes = { "MainMenu", "IntroVideo" };

    private static readonly string[] interiorScenes =
    {
        "Auberge", "BarDragonVerde", "Epicerie",
        "Pescaderia", "OficinaPortuaria", "VillaFrancois",
        "SalonCondesa"
    };

    // Escala base del player por tipo de escena.
    //
    // Los interiores estaban en 0.8, valor que nunca se retocó cuando se
    // ajustaron los exteriores a 0.35 — de ahí que el player saliera más del
    // doble de grande al entrar en el Bar Dragon Verde.
    //
    // 0.35 no es un número al azar: las cámaras de todas las escenas son
    // ortográficas de tamaño 5 y los fondos son imágenes de 1536x1024 a PPU
    // 100 puestas a escala ~1, o sea que interiores y exteriores encuadran
    // prácticamente igual. Con el mismo encuadre, el player tiene que medir
    // lo mismo. Si al verlo en pantalla se queda corto, el siguiente escalón
    // razonable es 0.45 (el que usa Muelle).
    private static readonly Vector3 interiorScale = new Vector3(0.35f, 0.35f, 1f);
    private static readonly Vector3 exteriorScale  = new Vector3(0.35f, 0.35f, 1f);

    // Per-scene overrides — takes priority over interior/exterior defaults
    private static readonly System.Collections.Generic.Dictionary<string, Vector3> sceneScaleOverrides =
        new System.Collections.Generic.Dictionary<string, Vector3>
    {
        { "Muelle",      new Vector3(0.45f, 0.45f, 1f) },
        { "CalleStJean", new Vector3(0.5f,  0.5f,  1f) },
    };

    private GameObject hudInstance;
    private GameObject playerInstance;

    private void Awake()
    {
        if (created) { Destroy(gameObject); return; }

        created = true;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // GameManager — always persistent
        if (FindObjectOfType<ResourceSystem>() == null && gameManagerPrefab != null)
        {
            var gm = Instantiate(gameManagerPrefab);
            gm.name = "GameManager";
            DontDestroyOnLoad(gm);
        }

        // GlobalUI — always persistent (needed for dialogue system)
        if (FindObjectOfType<DialogueSystem>() == null && globalUIPrefab != null)
        {
            var ui = Instantiate(globalUIPrefab);
            ui.name = "GlobalUI";
            DontDestroyOnLoad(ui);
        }

        // NPCVoicePlayer — persistent
        if (NPCVoicePlayer.Instance == null)
        {
            var vp = new GameObject("NPCVoicePlayer");
            vp.AddComponent<NPCVoicePlayer>();
            DontDestroyOnLoad(vp);
        }

        // LevelSystem — persistent
        if (LevelSystem.Instance == null)
        {
            var ls = new GameObject("LevelSystem");
            ls.AddComponent<LevelSystem>();
            DontDestroyOnLoad(ls);
        }

        // QuizSystem — persistent
        if (QuizSystem.Instance == null)
        {
            var qs = new GameObject("QuizSystem");
            qs.AddComponent<QuizSystem>();
            DontDestroyOnLoad(qs);
        }

        // QuizUI — persistent canvas overlay (sortingOrder 200)
        if (QuizUI.Instance == null)
        {
            var qui = new GameObject("QuizUI");
            qui.AddComponent<QuizUI>();
            DontDestroyOnLoad(qui);
        }

        // WordCapturePopup — learning card shown on word capture (sortingOrder 150)
        if (WordCapturePopup.Instance == null)
        {
            var wcp = new GameObject("WordCapturePopup");
            wcp.AddComponent<WordCapturePopup>();
            DontDestroyOnLoad(wcp);
        }

        // TutorialSystem — runs once on first Puerto visit
        if (TutorialSystem.Instance == null)
        {
            var ts = new GameObject("TutorialSystem");
            ts.AddComponent<TutorialSystem>();
            DontDestroyOnLoad(ts);
        }

        // LevelManager — persistent, drives the nivel/reto structure
        if (LevelManager.Instance == null)
        {
            var lm = new GameObject("LevelManager");
            lm.AddComponent<LevelManager>();
            DontDestroyOnLoad(lm);
        }

        // LevelHUD — persistent overlay (timer + target word panel), hidden unless a level is active
        if (LevelHUD.Instance == null)
        {
            var lh = new GameObject("LevelHUD");
            lh.AddComponent<LevelHUD>();
            DontDestroyOnLoad(lh);
        }

        // LevelResultUI — persistent overlay shown when a level ends
        if (LevelResultUI.Instance == null)
        {
            var lr = new GameObject("LevelResultUI");
            lr.AddComponent<LevelResultUI>();
            DontDestroyOnLoad(lr);
        }

        // LevelSelectUI — persistent overlay, the new menú de niveles
        if (LevelSelectUI.Instance == null)
        {
            var lsu = new GameObject("LevelSelectUI");
            lsu.AddComponent<LevelSelectUI>();
            DontDestroyOnLoad(lsu);
        }

        // CoachMarks — persistent overlay, contextual "aprender haciendo" hints
        if (CoachMarks.Instance == null)
        {
            var cm = new GameObject("CoachMarks");
            cm.AddComponent<CoachMarks>();
            DontDestroyOnLoad(cm);
        }

        // GuidedTutorial — persistent overlay, tutorial forzado (una vez)
        // que espera la acción real: mover el joystick, pulsar E, pulsar P
        if (GuidedTutorial.Instance == null)
        {
            var gt = new GameObject("GuidedTutorial");
            gt.AddComponent<GuidedTutorial>();
            DontDestroyOnLoad(gt);
        }

        // SpeechCaptionUI — persistent overlay, globos de diálogo sobre cada
        // personaje (sustituye al panel de chat para Jacques/NPCs genéricos)
        if (SpeechCaptionUI.Instance == null)
        {
            var scu = new GameObject("SpeechCaptionUI");
            scu.AddComponent<SpeechCaptionUI>();
            DontDestroyOnLoad(scu);
        }

        // BeginnerTurnController — persistent overlay, gate de "léelo/dilo en
        // francés" del modo débutant (ver DialogueSystem.SendChatMessage)
        if (BeginnerTurnController.Instance == null)
        {
            var btc = new GameObject("BeginnerTurnController");
            btc.AddComponent<BeginnerTurnController>();
            DontDestroyOnLoad(btc);
        }

        // SceneExitSignManager — persistent overlay, carteles de madera
        // siempre visibles sobre las salidas cuyo cruce esté permitido ahora
        if (SceneExitSignManager.Instance == null)
        {
            var sesm = new GameObject("SceneExitSignManager");
            sesm.AddComponent<SceneExitSignManager>();
            DontDestroyOnLoad(sesm);
        }

        // Music — persistent
        if (FindObjectOfType<MusicManager>() == null && musicManagerPrefab != null)
        {
            var music = Instantiate(musicManagerPrefab);
            music.name = "MusicManager";
            DontDestroyOnLoad(music);
        }

        // HUD — persistent but starts hidden
        if (FindObjectOfType<HUDController>() == null && hudPrefab != null)
        {
            hudInstance = Instantiate(hudPrefab);
            hudInstance.name = "HUDCanvas";
            if (hudInstance.GetComponent<HUDSetup>() == null)
                hudInstance.AddComponent<HUDSetup>();
            DontDestroyOnLoad(hudInstance);
            hudInstance.SetActive(false);
        }

        // Player — persistent but starts hidden
        if (GameObject.FindWithTag("Player") == null && playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab);
            playerInstance.name = "Player";
            DontDestroyOnLoad(playerInstance);
            playerInstance.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Destroy duplicate EventSystems
        var eventSystems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        for (int i = 1; i < eventSystems.Length; i++)
            Destroy(eventSystems[i].gameObject);

        // Zoomed-in camera that follows the player and stays clamped inside each
        // scene's background art bounds — see CameraFollow for why.
        // Not using Camera.main here: most scene cameras in this project are named
        // "Main Camera" but aren't actually tagged "MainCamera", so Camera.main
        // returns null for them. Each scene has exactly one Camera, so find it directly.
        var mainCam = FindObjectOfType<Camera>();
        if (mainCam != null && mainCam.GetComponent<CameraFollow>() == null)
            mainCam.gameObject.AddComponent<CameraFollow>();

        bool isMenuScene = System.Array.IndexOf(menuScenes, scene.name) >= 0;

        if (hudInstance == null) hudInstance = GameObject.Find("HUDCanvas");
        if (playerInstance == null) playerInstance = GameObject.FindWithTag("Player");

        // NOTE: HUDCanvas's root GameObject also holds MobileControlsSetup
        // (joystick, botones E/P) — nunca desactivar el root entero. Los
        // elementos legacy (comida/agua/energía, misión, nivel viejo, dinero,
        // días) se ocultan uno a uno en HUDSetup.HideLegacyElements() en vez
        // de aquí, precisamente para no tocar el joystick.
        if (hudInstance != null) hudInstance.SetActive(!isMenuScene);
        if (playerInstance != null)
        {
            playerInstance.SetActive(!isMenuScene);
            Vector3 targetScale;
            if (sceneScaleOverrides.TryGetValue(scene.name, out Vector3 overrideScale))
                targetScale = overrideScale;
            else
            {
                bool isInterior = System.Array.IndexOf(interiorScenes, scene.name) >= 0;
                targetScale = isInterior ? interiorScale : exteriorScale;
            }

            var perspScaler = playerInstance.GetComponent<PerspectiveScaler>();
            if (perspScaler != null)
            {
                perspScaler.SetBaseScale(targetScale);
                perspScaler.RefreshConfig();
            }
            else
            {
                playerInstance.transform.localScale = targetScale;
            }
        }
    }
}
