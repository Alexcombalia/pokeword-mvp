using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class VoiceRecorder : MonoBehaviour
{
    private const string TranscribeUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/voice/transcribe";
    private const int SampleRate  = 16000;

    // 45s (antes 10). Lo pide la escena final del modo detective: el jugador
    // le explica el caso al comisario de corrido, como el detective de una
    // novela, y en 10 segundos no cabe más que una frase suelta.
    //
    // Dos techos que NO se pueden cruzar y por los que 45 es el máximo seguro:
    // Google speech:recognize es la variante SÍNCRONA y solo admite audio de
    // hasta 60 segundos; y el cuerpo JSON del servidor está limitado a 10 MB
    // (express.json en server.js). A 48 kHz y 16 bits, 45s son ~4,3 MB de
    // audio que en base64 pasan a ~5,8 MB — entra, pero sin mucho margen.
    private const int MaxSeconds  = 45;

    private AudioClip clip;
    private bool isRecording;
    private string recordingDevice;
    private int actualSampleRate = SampleRate;

    public bool IsRecording => isRecording;

    // Fired on the main thread once recording actually starts (after permission granted)
    public event Action OnRecordingStarted;

    // Fired when recording could not actually start (no mic / permission denied)
    public event Action<string> OnRecordingFailed;

    public void StartRecording()
    {
        if (isRecording) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            var callbacks = new UnityEngine.Android.PermissionCallbacks();
            callbacks.PermissionGranted += _ => DoStartRecording();
            UnityEngine.Android.Permission.RequestUserPermission(
                UnityEngine.Android.Permission.Microphone, callbacks);
            return;
        }
#endif
        DoStartRecording();
    }

    private void DoStartRecording()
    {
        string[] devices = Microphone.devices;
        Debug.Log($"[VoiceRecorder] Dispositivos disponibles: {devices.Length}");
        foreach (var d in devices) Debug.Log($"[VoiceRecorder] Dispositivo: {d}");

        // Query caps using explicit name, but record with null (default device)
        // — some Windows drivers (Intel SST) fail with explicit name in Start()
        string capsDevice = devices.Length > 0 ? devices[0] : null;
        Microphone.GetDeviceCaps(capsDevice, out int minFreq, out int maxFreq);
        Debug.Log($"[VoiceRecorder] Caps min={minFreq} max={maxFreq}");

        // Pick the best rate: prefer 16000 for Google STT, fall back to device native rate
        if (minFreq == 0 && maxFreq == 0)
            actualSampleRate = SampleRate;                    // device supports any rate
        else if (maxFreq > 0 && maxFreq < SampleRate)
            actualSampleRate = maxFreq;                       // device can't reach 16000
        else if (minFreq > 0 && minFreq > SampleRate)
            actualSampleRate = minFreq;                       // device minimum is above 16000
        else
            actualSampleRate = SampleRate;                    // 16000 is within range

        Debug.Log($"[VoiceRecorder] Usando rate: {actualSampleRate}");
        clip = Microphone.Start(null, false, MaxSeconds, actualSampleRate);
        Debug.Log($"[VoiceRecorder] Clip null: {clip == null}");

        if (clip == null)
        {
            Debug.LogWarning("[VoiceRecorder] Microphone.Start devolvió null — sin micro disponible o permiso denegado");
            isRecording = false;
            OnRecordingFailed?.Invoke("No se pudo acceder al micrófono");
            return;
        }

        isRecording = true;
        MusicManager.Instance?.SetDucked(true); // la música puede interferir con el reconocimiento de voz
        OnRecordingStarted?.Invoke();
    }

    // Aborta una grabación en curso sin transcribirla — para cuando el
    // jugador cierra la conversación (p.ej. Escape) a media grabación, en
    // vez de tocar el botón de micro para pararla normalmente. Sin esto, la
    // música se quedaba bajada (ver MusicManager.SetDucked) hasta que la
    // grabación expirase sola al llegar a MaxSeconds.
    public void CancelRecording()
    {
        if (!isRecording) return;
        Microphone.End(null);
        isRecording = false;
        MusicManager.Instance?.SetDucked(false);
    }

    // languageHint ("fr"/"es"): cuando el llamador SABE con certeza en qué
    // idioma va a hablar el jugador (p.ej. BeginnerTurnController leyendo
    // una frase francesa concreta), fija ese idioma sin alternativa en el
    // servidor — evita que un francés con acento marcado se detecte entero
    // como español y salga transcrito irreconocible. Null (por defecto)
    // mantiene el reconocimiento bilingüe de siempre.
    public void StopAndTranscribe(Action<string> onResult, Action<string> onError, string languageHint = null)
    {
        if (!isRecording) { Debug.LogWarning("[VoiceRecorder] StopAndTranscribe: no hay grabación activa"); onError?.Invoke("No hay grabación activa"); return; }

        int position = Microphone.GetPosition(null);
        Microphone.End(null);
        isRecording = false;
        MusicManager.Instance?.SetDucked(false); // recuperar volumen normal en cuanto termina de captar audio
        Debug.Log($"[VoiceRecorder] Posición al detener: {position} muestras ({position / (float)actualSampleRate:F2}s)");

        if (position < 100) { Debug.LogWarning("[VoiceRecorder] Audio demasiado corto"); onError?.Invoke("Audio demasiado corto"); return; }

        float[] all = new float[clip.samples * clip.channels];
        clip.GetData(all, 0);

        // Trim to recorded length (mono)
        float[] trimmed = new float[position];
        for (int i = 0; i < position; i++)
        {
            float avg = 0f;
            for (int c = 0; c < clip.channels; c++)
                avg += all[i * clip.channels + c];
            trimmed[i] = avg / clip.channels;
        }

        // Use actualSampleRate so the WAV header matches what we tell Google STT
        AudioClip trimmedClip = AudioClip.Create("voice", position, 1, actualSampleRate, false);
        trimmedClip.SetData(trimmed, 0);

        byte[] wav    = WavEncoder.Encode(trimmedClip);
        string base64 = Convert.ToBase64String(wav);

        // Check RMS to verify actual audio was captured (not silence)
        float rms = 0f;
        foreach (var s in trimmed) rms += s * s;
        rms = Mathf.Sqrt(rms / trimmed.Length);
        Debug.Log($"[VoiceRecorder] Audio RMS: {rms:F4}  (>0.01 = audio detectado, ~0 = silencio)");

        Debug.Log($"[VoiceRecorder] Enviando {wav.Length} bytes al servidor (rate={actualSampleRate}, languageHint={languageHint ?? "auto"})...");
        StartCoroutine(Send(base64, actualSampleRate, onResult, onError, languageHint));
    }

    private IEnumerator Send(string base64, int sampleRate, Action<string> onResult, Action<string> onError, string languageHint)
    {
        // targetLang va SIEMPRE, incluso sin languageHint: sin él, el servidor
        // reconocería en francés + español por defecto, y en una partida en
        // inglés eso transcribe cualquier cosa menos lo que el jugador dijo.
        // languageHint sigue siendo distinto — ese fija el idioma sin
        // alternativa, para cuando se sabe con certeza (leer una frase).
        string json = "{\"audioBase64\":\"" + base64 + "\",\"sampleRate\":" + sampleRate
            + ",\"targetLang\":\"" + GameSettings.TargetLanguage + "\""
            + (string.IsNullOrEmpty(languageHint) ? "" : ",\"languageHint\":\"" + languageHint + "\"")
            + "}";
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(TranscribeUrl, "POST");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        // Sin esto, una conexión colgada nunca dispara ni onResult ni
        // onError — el intento se queda "procesando" para siempre y ni
        // siquiera cuenta como fallo (ver BeginnerTurnController), dejando
        // al jugador sin ninguna salida posible.
        //
        // 120s (antes 15). Con MaxSeconds a 45 una grabación larga son ~5,8 MB
        // en base64: por una conexión móvil normal la subida sola ya se come
        // los 15 segundos viejos, y encima falla DESPUÉS de que el jugador
        // haya hablado 45 — el peor momento posible para perder el turno.
        // Sigue habiendo tope, que es lo que evita el cuelgue infinito.
        req.timeout = 120;

        yield return req.SendWebRequest();

        Debug.Log($"[VoiceRecorder] Respuesta servidor: {req.result} | {req.responseCode} | {req.downloadHandler?.text}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[VoiceRecorder] Error HTTP: {req.error}");
            onError?.Invoke(req.error);
            yield break;
        }

        var resp = JsonUtility.FromJson<Response>(req.downloadHandler.text);
        if (resp != null && resp.ok && !string.IsNullOrEmpty(resp.text))
        {
            Debug.Log($"[VoiceRecorder] Transcripción: {resp.text}");
            onResult?.Invoke(resp.text);
        }
        else
        {
            Debug.LogWarning($"[VoiceRecorder] Sin transcripción. ok={resp?.ok} text='{resp?.text}'");
            onError?.Invoke("Sin transcripción");
        }
    }

    [Serializable] private class Response { public bool ok; public string text; }
}
