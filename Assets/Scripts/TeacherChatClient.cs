using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Cliente del profesor de idioma (endpoint /teacher). Hermano pequeño de
// DetectiveChatClient: no manda expediente, no manda checkpoints y no recibe
// audio ni traducciones — solo texto.
//
// No va colgado de ningún NPC de la escena: el profesor no está en el mundo,
// vive en el HUD y se puede abrir desde cualquier sitio, incluso encima de una
// conversación. Por eso es un singleton propio y no un componente de un
// personaje.
public class TeacherChatClient : MonoBehaviour
{
    public static TeacherChatClient Instance { get; private set; }

    private const string serverBase = "https://pokeword-npc-server-kvml-production.up.railway.app";

    [System.Serializable]
    private class TeacherRequest
    {
        public string message;
        public string sessionId;
        public string lang;
        public string npcLine;
        public string playerLine;
    }

    [System.Serializable]
    private class TeacherResponse
    {
        public bool   ok;
        public string reply;
        public string error;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Una sesión por partida e idioma. Si no se separara por idioma, el
    // profesor de catalán heredaría la conversación del de francés al cambiar
    // de idioma en el menú, y seguiría explicando el idioma anterior.
    string SessionId => "teacher_" + GameSettings.TargetLanguage;

    public IEnumerator Ask(string message, System.Action<string> onReply, System.Action<string> onError)
    {
        var caption = SpeechCaptionUI.Instance;
        var body = new TeacherRequest
        {
            message    = message,
            sessionId  = SessionId,
            lang       = GameSettings.TargetLanguage,
            npcLine    = caption != null ? caption.LastNpcLine    : "",
            playerLine = caption != null ? caption.LastPlayerLine : "",
        };

        using (var request = new UnityWebRequest(serverBase + "/teacher", "POST"))
        {
            byte[] raw = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));
            request.uploadHandler   = new UploadHandlerRaw(raw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            TeacherResponse resp;
            try { resp = JsonUtility.FromJson<TeacherResponse>(request.downloadHandler.text); }
            catch { onError?.Invoke("respuesta ilegible"); yield break; }

            if (resp == null || !resp.ok || string.IsNullOrEmpty(resp.reply))
            {
                onError?.Invoke(resp?.error ?? "sin respuesta");
                yield break;
            }

            onReply?.Invoke(resp.reply);
        }
    }

    // Empezar limpio en cada partida, igual que los NPCs (ver
    // DetectiveNPC.Start): si no, el profesor arrastraría las dudas del intento
    // anterior y podría dar por explicado algo que en esta partida nadie ha
    // preguntado.
    public IEnumerator ResetMemory()
    {
        var body = new TeacherRequest { sessionId = SessionId, message = "", lang = GameSettings.TargetLanguage };
        using (var request = new UnityWebRequest(serverBase + "/teacher/reset", "POST"))
        {
            byte[] raw = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));
            request.uploadHandler   = new UploadHandlerRaw(raw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
        }
    }
}
