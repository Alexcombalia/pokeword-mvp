using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BakerChatClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/baker";

    [System.Serializable]
    private class BakerRequest
    {
        public string message;
        public string difficulty;
    }

    [System.Serializable]
    private class BakerResponse
    {
        public bool ok;
        public string npc;
        public string userMessage;
        public string reply;
        public string replyTranslation;
        public string playerMessageTranslation;
        public string audioBase64;
    }
    public IEnumerator ResetMemory(System.Action onDone = null, System.Action<string> onError = null)
    {
        string resetUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/baker/reset";

        using (var request = new UnityWebRequest(resetUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            onDone?.Invoke();
        }
    }
    public IEnumerator SendMessage(string message, System.Action<string, string, string> onReply, System.Action<string> onError)
    {
        var reqObj = new BakerRequest { message = message, difficulty = GameSettings.Difficulty };
        string json = JsonUtility.ToJson(reqObj);

        using (var request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            string text = request.downloadHandler.text;
            var resp = JsonUtility.FromJson<BakerResponse>(text);

            if (resp == null || !resp.ok)
            {
                onError?.Invoke("Respuesta inválida del servidor");
                yield break;
            }

            NPCVoicePlayer.Instance?.PlayVoice(resp.audioBase64);
            onReply?.Invoke(resp.reply, resp.replyTranslation, resp.playerMessageTranslation);
        }
    }
}