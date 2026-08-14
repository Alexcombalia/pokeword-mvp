using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Base de los clientes de chat del MODO DETECTIVE.
//
// Cada personaje de un caso tiene su propio endpoint dedicado en el servidor
// (/npc/odette, /npc/corentin, /npc/denis, /npc/fabre) y su propio script con
// su nombre — así, cuando un NPC se comporta mal en una partida de prueba, se
// sabe exactamente qué archivo y qué prompt tocar. Lo único que vive aquí es
// el fontanero HTTP, idéntico para todos: si estuviera copiado en cada
// personaje habría que arreglar el mismo fallo cuatro veces, y nueve cuando
// estén los cinco casos.
//
// Diferencia con GenericNPCChatClient: allí el personaje lo elige un
// personalityId y la memoria se indexa por npcId; aquí el personaje ES el
// endpoint. Ver Pokeword/Caso_1_Le_Violoniste.md §11.
public abstract class DetectiveChatClient : MonoBehaviour
{
    private const string serverBase = "https://pokeword-npc-server-kvml-production.up.railway.app";

    // "odette", "corentin", "denis", "fabre". Da a la vez la ruta del endpoint
    // y la clave con la que el nivel busca el expediente que toca (Fabre sale
    // en los cinco casos con checkpoints distintos, ver npcObjectiveKeys).
    public abstract string NpcId { get; }

    private string ChatUrl  => serverBase + "/npc/" + NpcId;
    private string ResetUrl => serverBase + "/npc/" + NpcId + "/reset";

    [System.Serializable]
    private class DetectiveRequest
    {
        public string message;
        public string sessionId;
        public string difficulty;
        public string levelObjective;
        public string[] knownCheckpoints;
    }

    [System.Serializable]
    private class DetectiveResponse
    {
        public bool ok;
        public string npc;
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
        public string sessionId;
    }

    // onReply args: reply, replyTranslation, playerMessageTranslation,
    // playerMessagePhonetic, objectiveComplete (sin usar en modo detective,
    // solo por compatibilidad de forma con los otros clientes),
    // checkpointsReached, audioBase64. El TTS NO se dispara aquí: lo decide
    // quien llama, igual que en GenericNPCChatClient.
    public IEnumerator SendMessage(string message,
        System.Action<string, string, string, string, bool, string[], string> onReply,
        System.Action<string> onError)
    {
        // El expediente que se aplica lo decide el NIVEL, no el personaje:
        // Fabre reaparece en los cinco casos y en cada uno valida cosas
        // distintas. Sin nivel activo el NPC contesta como personaje pero sin
        // expediente — no puede dar pistas ni marcar checkpoints por error.
        string levelObjective = null;
        if (LevelManager.Instance != null && LevelManager.Instance.IsActive
            && LevelManager.Instance.CurrentLevel?.npcObjectiveKeys != null
            && LevelManager.Instance.CurrentLevel.npcObjectiveKeys.TryGetValue(NpcId, out var key))
        {
            levelObjective = key;
        }

        // Lo que el jugador YA lleva descubierto en el caso. Va en cada
        // petición porque la conversación se reinicia cada vez que se abre el
        // diálogo: sin esto, un jugador que le pone a Denis la primera
        // contradicción, se va a repreguntar a Odette y vuelve con la segunda
        // se encontraría con que la primera ya no cuenta, y la confesión sería
        // inalcanzable salvo hablando con él de una sentada. El servidor solo
        // se queda los ids que son de este personaje.
        string[] known = null;
        if (LevelManager.Instance != null && LevelManager.Instance.CheckpointsReached != null
            && LevelManager.Instance.CheckpointsReached.Count > 0)
        {
            known = new string[LevelManager.Instance.CheckpointsReached.Count];
            LevelManager.Instance.CheckpointsReached.CopyTo(known);
        }

        var reqObj = new DetectiveRequest
        {
            message = message,
            sessionId = NpcId,
            difficulty = GameSettings.Difficulty,
            levelObjective = levelObjective,
            knownCheckpoints = known,
        };
        string json = JsonUtility.ToJson(reqObj);

        using (var request = new UnityWebRequest(ChatUrl, "POST"))
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

            var resp = JsonUtility.FromJson<DetectiveResponse>(request.downloadHandler.text);
            if (resp == null || !resp.ok)
            {
                onError?.Invoke("Réponse NPC invalide");
                yield break;
            }

            onReply?.Invoke(resp.reply, resp.replyTranslation, resp.playerMessageTranslation,
                resp.playerMessagePhonetic, resp.objectiveComplete, resp.checkpointsReached, resp.audioBase64);
        }
    }

    // Se llama al abrir la conversación. Importante en modo detective: si el
    // jugador vuelve a hablar con el mismo NPC a mitad de caso, el personaje
    // arranca limpio pero los checkpoints YA ganados no se pierden — esos los
    // guarda LevelManager, no el servidor.
    public IEnumerator ResetMemory(System.Action onDone = null, System.Action<string> onError = null)
    {
        var reqObj = new ResetRequest { sessionId = NpcId };
        string json = JsonUtility.ToJson(reqObj);

        using (var request = new UnityWebRequest(ResetUrl, "POST"))
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
