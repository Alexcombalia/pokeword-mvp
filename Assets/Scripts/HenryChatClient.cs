using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HenryChatClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/henry";
    [SerializeField] private string resetUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/henry/reset";

    [System.Serializable]
    private class HenryRequest
    {
        public string message;
        public string difficulty;
    }

    [System.Serializable]
    private class HenryResponse
    {
        public bool ok;
        public string npc;
        public string userMessage;
        public string reply;
        public string replyTranslation;
        public string playerMessageTranslation;
        public string audioBase64;
    }

    public IEnumerator SendMessage(string message, System.Action<string, string, string> onReply, System.Action<string> onError)
    {
        var reqObj = new HenryRequest { message = message, difficulty = GameSettings.Difficulty };
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
            var resp = JsonUtility.FromJson<HenryResponse>(text);

            if (resp == null || !resp.ok)
            {
                onError?.Invoke("Réponse Henry invalide");
                yield break;
            }

            NPCVoicePlayer.Instance?.PlayVoice(resp.audioBase64);
            onReply?.Invoke(resp.reply, resp.replyTranslation, resp.playerMessageTranslation);
        }
    }

    public IEnumerator ResetMemory(System.Action onDone = null, System.Action<string> onError = null)
    {
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
}