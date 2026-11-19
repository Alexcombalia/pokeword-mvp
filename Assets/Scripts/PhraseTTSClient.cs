using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Cliente mínimo para /voice/synthesize — a diferencia de
// Jacques/GenericNPCChatClient, aquí no hay conversación ni sesión: solo
// "dame el audio de este texto en francés" para el botón de escuchar
// pronunciación de BeginnerTurnController.
public class PhraseTTSClient : MonoBehaviour
{
    private const string serverUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/voice/synthesize";

    [System.Serializable] private class SynthRequest  { public string text; public string voiceId; }
    [System.Serializable] private class SynthResponse { public bool ok; public string audioBase64; }

    // voiceId opcional: null usa la voz de lectura del idioma activo (botón de
    // pronunciación). Pasarlo sirve para que una frase suene con la voz de un
    // personaje concreto — el saludo de entrada de cada NPC, que se muestra
    // desde el cliente y por tanto no viene con audio del servidor.
    public IEnumerator FetchAudio(string text, System.Action<string> onAudioBase64, System.Action<string> onError, string voiceId = null)
    {
        // Sin voiceId el servidor usaba la voz genérica, que es francesa: en una
        // partida en inglés se oía el texto inglés leído con acento francés.
        string voice = voiceId ?? (GameSettings.IsEnglish ? "read_en"
                                 : GameSettings.IsItalian ? "read_it"
                                 : GameSettings.IsCatalan ? "read_ca" : "generic");
        var req = new SynthRequest { text = text, voiceId = voice };
        string json = JsonUtility.ToJson(req);

        using (var request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) { onError?.Invoke(request.error); yield break; }

            var resp = JsonUtility.FromJson<SynthResponse>(request.downloadHandler.text);
            if (resp == null || !resp.ok || string.IsNullOrEmpty(resp.audioBase64)) { onError?.Invoke("Synthesis failed"); yield break; }
            onAudioBase64?.Invoke(resp.audioBase64);
        }
    }
}
