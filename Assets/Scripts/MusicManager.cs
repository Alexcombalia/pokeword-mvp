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
        audioSource.mute = Muted;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Silenciar la música es una preferencia del jugador y se recuerda entre
    // partidas: a quien le molesta la música le molesta SIEMPRE, y tener que
    // apagarla en cada arranque es peor que no poder apagarla.
    //
    // Solo afecta a la música. Las voces de los NPCs y la lectura en voz alta
    // siguen sonando — son el juego, no ambientación.
    public static bool Muted
    {
        get => PlayerPrefs.GetInt("music_muted", 0) == 1;
        set { PlayerPrefs.SetInt("music_muted", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public void ApplyMuteSetting()
    {
        if (audioSource == null) return;
        audioSource.mute = Muted;
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
