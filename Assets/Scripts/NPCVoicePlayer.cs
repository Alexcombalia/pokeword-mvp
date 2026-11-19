using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NPCVoicePlayer : MonoBehaviour
{
    public static NPCVoicePlayer Instance { get; private set; }

    private AudioSource audioSource;

    // Última voz reproducida, guardada ya decodificada para poder repetirla
    // desde el botón del globo del NPC (ver SpeechCaptionUI): al jugador se le
    // permite volver a escuchar la frase las veces que quiera, y eso NO puede
    // costar otra llamada al servidor ni depender de que la red vaya bien.
    //
    // Vive aquí y no en el globo a propósito: así todos los caminos que ya
    // llaman a PlayVoice —detective, genérico, Jacques, saludo de entrada,
    // panadero, Henry— quedan cubiertos sin tocar ninguno.
    private AudioClip lastClip;
    private string    lastText;

    public bool HasVoice => lastClip != null;

    // El mp3 se escribe en disco para poder cargarlo, y el clip cacheado no
    // puede quedarse apuntando al fichero que va a pisar la frase siguiente.
    // Se alternan dos nombres: el que está sonando nunca es el que se
    // sobrescribe a continuación.
    private int fileSlot;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 3f;
    }

    // El texto que corresponde al audio, para contar palabras escuchadas. Se
    // guarda aparte porque la descarga del clip es asíncrona y el recuento se
    // hace cuando el clip ya existe (así los segundos salen de su duración
    // real, no de una estimación).
    private string pendingText;

    // Cuando el jugador pulsa grabar hay que callar al NPC: si no, el micro
    // recoge la voz del propio personaje, el reconocedor transcribe eso, y el
    // jugador acaba "diciendo" lo que acababa de oír. Además de confuso, saca
    // de la partida.
    private bool suppressPlayback;

    public void PlayVoice(string base64Audio, string spokenText = null)
    {
        if (string.IsNullOrEmpty(base64Audio)) return;
        suppressPlayback = false;   // línea nueva: esta sí debe sonar
        pendingText = spokenText;
        StartCoroutine(LoadAndPlay(base64Audio));
    }

    // Callar al NPC al empezar a grabar.
    //
    // No usa Stop(), que además corta las corrutinas: si la voz todavía se
    // estaba descargando, matarla dejaría la frase sin clip cacheado y el botón
    // de repetir muerto para esa línea. Así el audio termina de cargarse y
    // queda listo para repetir — simplemente no suena ahora.
    public void Silence()
    {
        suppressPlayback = true;
        if (audioSource != null) audioSource.Stop();
    }

    // Vuelve a sonar la última frase, sin red y sin volver a decodificar.
    // Repetir NO afecta a nada más: no gasta turno, no toca los checkpoints y
    // no cuenta como pista — es solo volver a oír.
    public void Replay()
    {
        if (lastClip == null || audioSource == null) return;

        // Repetir CUENTA como escucha. Volver a oír una frase para entenderla
        // es escuchar, y es justo la conducta que este botón existe para
        // fomentar — no contarla sería castigarla.
        LevelManager.Instance?.ReportHeard(lastText, lastClip.length);

        audioSource.Stop();
        audioSource.clip = lastClip;
        audioSource.Play();
    }

    // Se llama al mostrar una línea NUEVA del NPC: hasta que llegue su audio,
    // no hay nada que repetir. Sin esto, una línea de error (que no trae voz)
    // dejaría el botón activo reproduciendo la frase anterior, que ya no es la
    // que está en pantalla.
    public void ClearVoice()
    {
        lastClip = null;
        lastText = null;
    }

    // Corta cualquier voz que siga sonando — necesario al reintentar/salir
    // de un nivel, si no la última frase de Jacques se solapa con el
    // resultado o con el siguiente intento.
    public void Stop()
    {
        StopAllCoroutines();
        if (audioSource != null) audioSource.Stop();
    }

    private IEnumerator LoadAndPlay(string base64Audio)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = "data:audio/mpeg;base64," + base64Audio;
#else
        byte[] audioBytes = System.Convert.FromBase64String(base64Audio);
        fileSlot = 1 - fileSlot;
        string path = System.IO.Path.Combine(Application.temporaryCachePath, "npc_voice_" + fileSlot + ".mp3");
        System.IO.File.WriteAllBytes(path, audioBytes);
        string url = "file://" + path;
#endif

        using (var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;

            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip == null) yield break;

            lastClip = clip;
            lastText = pendingText;
            string heardText = pendingText;
            pendingText = null;

            // El jugador pulsó grabar mientras esto se descargaba. Queda
            // cacheado para el botón de repetir, pero no suena — y NO cuenta
            // como escuchado, porque no lo ha oído.
            if (suppressPlayback) { suppressPlayback = false; yield break; }

            LevelManager.Instance?.ReportHeard(heardText, clip.length);

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
