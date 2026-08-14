using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NPCVoicePlayer : MonoBehaviour
{
    public static NPCVoicePlayer Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 3f;
    }

    public void PlayVoice(string base64Audio)
    {
        if (string.IsNullOrEmpty(base64Audio)) return;
        StartCoroutine(LoadAndPlay(base64Audio));
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
        string path = System.IO.Path.Combine(Application.temporaryCachePath, "npc_voice.mp3");
        System.IO.File.WriteAllBytes(path, audioBytes);
        string url = "file://" + path;
#endif

        using (var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;

            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip == null) yield break;

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
