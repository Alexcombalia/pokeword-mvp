using System.Collections.Generic;

[System.Serializable]
public class TargetWord
{
    public string french;
    public string spanish;

    public TargetWord(string fr, string es)
    {
        french = fr;
        spanish = es;
    }
}

// Un "reto" visible dentro de un nivel — checklist mostrado en LevelHUD,
// se va marcando a medida que el servidor reporta su id en
// checkpointsReached (ver GenericNPCChatClient/DialogueSystem/LevelManager).
public class LevelCheckpoint
{
    public string id;
    public string labelFr;
    public string labelEs;

    public LevelCheckpoint(string id, string labelFr, string labelEs)
    {
        this.id      = id;
        this.labelFr = labelFr;
        this.labelEs = labelEs;
    }

    public string DisplayLabel => GameSettings.IsBasic ? labelEs : labelFr;
}

// A level's fixed configuration — box "El Puerto", pilot caja (see memory:
// project_pokeword_redesign). Plain C# data, not a ScriptableObject: with a
// handful of levels a static list is simpler to author/read than asset files,
// and needs no Unity Editor step to wire up.
public class LevelDefinition
{
    public int    id;
    public string title;             // shown on the level tile / result screen
    public string sceneName;         // scene the player is dropped into on start
    public string briefTitle;        // pre-level brief screen (français — modo avancé)
    public string briefSummary;
    public string briefTitleEs;      // mismo brief en español — modo débutant
    public string briefSummaryEs;
    public List<TargetWord> targetWords;
    public float  timeLimitSeconds;  // hard cap — reaching 0 fails the level
    public float  starTimeSeconds;   // finishing at or under this earns the speed star
    public string npcObjectiveKey;   // sent to the server so it knows which objective to track (see LEVEL_OBJECTIVES in server.js) — solo para el flujo dedicado de Jacques
    public bool   hintAvailable;     // hint button only shown from levels with real deduction (4+)

    // Objetivo por NPC genérico (npcId -> clave de LEVEL_OBJECTIVES) — un
    // nivel puede tener varios NPCs genéricos "vestidos" con instrucción de
    // objetivo (p.ej. nivel 2: cualquiera de los 3 estibadores; nivel 3: uno
    // distinto por NPC). Null si el nivel no usa NPCs genéricos con objetivo
    // (p.ej. Jacques, que sigue usando npcObjectiveKey de arriba, en su
    // propio endpoint dedicado).
    public Dictionary<string, string> npcObjectiveKeys;

    // Retos visibles del nivel (checklist en LevelHUD) — null para niveles
    // de objetivo simple (1, 5), que siguen con ObjectiveComplete directo.
    // Se completa cuando LevelManager.CheckpointsReached contiene el id de
    // TODOS los checkpoints de esta lista, sin importar qué NPC concreto
    // reportó cada uno.
    public List<LevelCheckpoint> checkpoints;

    // Escenas a las que SceneExit deja cruzar mientras este nivel está
    // activo — null significa bloqueo total (comportamiento de siempre,
    // niveles de una sola escena). Ver SceneExit.cs.
    public List<string> allowedScenes;

    public LevelDefinition(int id, string title, string sceneName,
        string briefTitle, string briefSummary, string briefTitleEs, string briefSummaryEs,
        List<TargetWord> targetWords, float timeLimitSeconds, float starTimeSeconds, string npcObjectiveKey,
        bool hintAvailable = false,
        Dictionary<string, string> npcObjectiveKeys = null, List<LevelCheckpoint> checkpoints = null,
        List<string> allowedScenes = null)
    {
        this.id               = id;
        this.title             = title;
        this.sceneName         = sceneName;
        this.briefTitle        = briefTitle;
        this.briefSummary      = briefSummary;
        this.briefTitleEs      = briefTitleEs;
        this.briefSummaryEs    = briefSummaryEs;
        this.targetWords       = targetWords;
        this.timeLimitSeconds  = timeLimitSeconds;
        this.starTimeSeconds   = starTimeSeconds;
        this.npcObjectiveKey   = npcObjectiveKey;
        this.hintAvailable     = hintAvailable;
        this.npcObjectiveKeys  = npcObjectiveKeys;
        this.checkpoints       = checkpoints;
        this.allowedScenes     = allowedScenes;
    }

    public string DisplayTitle   => GameSettings.IsBasic ? briefTitleEs   : briefTitle;
    public string DisplaySummary => GameSettings.IsBasic ? briefSummaryEs : briefSummary;
}

// Best result reached so far for a level — persisted via SaveSystem.
// "difficulty" separa el progreso: débutant es más fácil que avancé, así
// que sus estrellas no deben mezclarse ni contar la una para la otra.
[System.Serializable]
public class LevelResult
{
    public int    levelId;
    public int    stars;
    public float  timeSeconds;
    public bool   completed;
    public string difficulty;
}
