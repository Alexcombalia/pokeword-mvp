using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class JacquesChatClient : MonoBehaviour
{
    private const string serverUrl = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/jacques";
    private const string resetUrl  = "https://pokeword-npc-server-kvml-production.up.railway.app/npc/jacques/reset";

    private string sessionId;

    private void Awake() { sessionId = System.Guid.NewGuid().ToString(); }

    [System.Serializable] private class JacquesRequest { public string message; public string sessionId; public string difficulty; public string levelObjective; }
    [System.Serializable] private class ResetRequest   { public string sessionId; }
    [System.Serializable] private class JacquesResponse { public bool ok; public string reply; public string replyTranslation; public string playerMessageTranslation; public string playerMessagePhonetic; public bool objectiveComplete; public string audioBase64; }

    public void NewSession() { sessionId = System.Guid.NewGuid().ToString(); }

    // onReply args: reply, replyTranslation, playerMessageTranslation,
    // playerMessagePhonetic (guía de pronunciación en español para débutant,
    // ver BeginnerTurnController — null/"" fuera de básico), objectiveComplete
    // (solo relevante con un nivel activo; false si no) y audioBase64 — el TTS
    // NO se dispara aquí (ver abajo); el llamador decide cuándo reproducirlo,
    // para que débutant pueda retenerlo detrás del gate de lectura.
    public IEnumerator SendMessage(string message, System.Action<string, string, string, string, bool, string> onReply, System.Action<string> onError)
    {
        string levelObjective = LevelManager.Instance != null && LevelManager.Instance.IsActive
            ? LevelManager.Instance.CurrentLevel?.npcObjectiveKey
            : null;

        var req = new JacquesRequest { message = message, sessionId = sessionId, difficulty = GameSettings.Difficulty, levelObjective = levelObjective };
        string json = JsonUtility.ToJson(req);

        using (var request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) { onError?.Invoke(request.error); yield break; }

            var resp = JsonUtility.FromJson<JacquesResponse>(request.downloadHandler.text);
            if (resp == null || !resp.ok) { onError?.Invoke("Erreur Jacques"); yield break; }
            onReply?.Invoke(resp.reply, resp.replyTranslation, resp.playerMessageTranslation, resp.playerMessagePhonetic, resp.objectiveComplete, resp.audioBase64);
        }
    }

    public IEnumerator ResetSession(System.Action onDone = null)
    {
        NewSession();
        var req = new ResetRequest { sessionId = sessionId };
        string json = JsonUtility.ToJson(req);

        using (var request = new UnityWebRequest(resetUrl, "POST"))
        {
            request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            onDone?.Invoke();
        }
    }
}
