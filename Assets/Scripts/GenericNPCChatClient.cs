using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GenericNPCChatClient : MonoBehaviour
{
    private const string serverUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/generic";
    private const string resetUrl  = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/generic/reset";

    [System.Serializable]
    private class GenericRequest
    {
        public string message;
        public string personalityId;
        public string npcId;
        public string gender;
        public string difficulty;
        public string levelObjective;
    }

    [System.Serializable]
    private class GenericResponse
    {
        public bool ok;
        public string npc;
        public string personalityId;
        public string userMessage;
        public string reply;
        public string replyTranslation;
        public string playerMessageTranslation;
        public string playerMessagePhonetic;
        public bool objectiveComplete;
        public string[] checkpointsReached;
        public string audioBase64;
    }

    [System.Serializable]
    private class ResetRequest
    {
        public string npcId;
    }

    // onReply args: reply, replyTranslation, playerMessageTranslation,
    // playerMessagePhonetic (guía de pronunciación en español, débutant),
    // objectiveComplete (sin usar en niveles con checkpoints, solo por
    // compatibilidad de forma), checkpointsReached (retos alcanzados por
    // ESTE npcId en esta conversación — ver LevelManager.ReportCheckpoints),
    // audioBase64 — el TTS NO se dispara aquí; el llamador decide cuándo
    // reproducirlo, para que débutant pueda retenerlo tras el gate de lectura.
    public IEnumerator SendMessage(string message, string personalityId, string npcId, string gender,
        System.Action<string, string, string, string, bool, string[], string> onReply, System.Action<string> onError)
    {
        // Solo se manda si ESTE npcId es uno de los NPCs con objetivo del
        // nivel activo — así los NPCs de ambiente de la misma escena (p.ej.
        // Antoine/Albert en Muelle mientras Raphael es el objetivo) siguen
        // charla normal sin poder disparar el objetivo por error.
        string levelObjective = null;
        if (LevelManager.Instance != null && LevelManager.Instance.IsActive
            && LevelManager.Instance.CurrentLevel?.npcObjectiveKeys != null
            && LevelManager.Instance.CurrentLevel.npcObjectiveKeys.TryGetValue(npcId, out var key))
        {
            levelObjective = key;
        }

        var reqObj = new GenericRequest { message = message, personalityId = personalityId, npcId = npcId, gender = gender, difficulty = GameSettings.Difficulty, levelObjective = levelObjective };
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

            var resp = JsonUtility.FromJson<GenericResponse>(request.downloadHandler.text);
            if (resp == null || !resp.ok)
            {
                onError?.Invoke("Réponse NPC invalide");
                yield break;
            }

            onReply?.Invoke(resp.reply, resp.replyTranslation, resp.playerMessageTranslation, resp.playerMessagePhonetic, resp.objectiveComplete, resp.checkpointsReached, resp.audioBase64);
        }
    }

    public IEnumerator ResetMemory(string npcId, System.Action onDone = null, System.Action<string> onError = null)
    {
        var reqObj = new ResetRequest { npcId = npcId };
        string json = JsonUtility.ToJson(reqObj);

        using (var request = new UnityWebRequest(resetUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                onError?.Invoke(request.error);
            else
                onDone?.Invoke();
        }
    }
}
