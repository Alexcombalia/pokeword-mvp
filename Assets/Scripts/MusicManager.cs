using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip gameMusic;
    private AudioSource audioSource;

    private const float BaseVolume = 0.07f;
    private const float DuckedFactor = 0.25f; // volumen durante grabación de voz, ver SetDucked/VoiceRecorder

    private static readonly string[] silentScenes = { "MainMenu", "IntroVideo" };

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = gameMusic;
        audioSource.loop = true;
        audioSource.volume = BaseVolume;
        audioSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Baja la música mientras se graba la voz del jugador (VoiceRecorder) —
    // sospecha razonable de que interfiere con el reconocimiento de voz.
    // Solo dura la ventana de grabación en sí, no toda la partida, para no
    // sacrificar ambientación el resto del tiempo.
    public void SetDucked(bool ducked)
    {
        if (audioSource == null) return;
        audioSource.volume = ducked ? BaseVolume * DuckedFactor : BaseVolume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isSilent = System.Array.IndexOf(silentScenes, scene.name) >= 0;

        if (isSilent)
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }
        else
        {
            if (!audioSource.isPlaying) audioSource.Play();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
