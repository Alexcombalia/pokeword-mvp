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

// MODO DETECTIVE — una línea del checklist de objetivos.
// A diferencia de LevelCheckpoint (un id = una línea), aquí una línea puede
// necesitar VARIOS ids: "pillar a Marcel en dos mentiras" no está hecha hasta
// que están las dos. Y los objetivos no son lo que completa el nivel: el caso
// se cierra con el comisario, no con haber hablado con todos.
public class LevelObjective
{
    public string labelFr;
    public string labelEs;
    public string[] requiredIds;

    public LevelObjective(string labelFr, string labelEs, params string[] requiredIds)
    {
        this.labelFr     = labelFr;
        this.labelEs     = labelEs;
        this.requiredIds = requiredIds;
    }

    public string DisplayLabel => GameSettings.IsBasic ? labelEs : labelFr;
}

// MODO DETECTIVE — un hallazgo. Cuando el jugador oye el dato y el servidor
// reporta su id, esta frase aparece escrita en verde en el panel de arriba.
// Son HECHOS LITERALES, nunca conclusiones: "Marcel llevaba algo metálico bajo
// el abrigo", jamás "Marcel podría ser el asesino". El panel no señala
// sospechosos ni marca mentiras — eso lo hace el jugador.
public class LevelFinding
{
    public string id;
    public string textFr;
    public string textEs;

    public LevelFinding(string id, string textFr, string textEs)
    {
        this.id     = id;
        this.textFr = textFr;
        this.textEs = textEs;
    }

    public string DisplayText => GameSettings.IsBasic ? textEs : textFr;
}

// Una pista del botón de ayuda. Se muestra la PRIMERA cuyo `untilId` aún no
// se haya alcanzado, así la pista siempre habla de lo siguiente que falta.
//
// Regla de contenido: da la siguiente PREGUNTA ÚTIL, nunca la solución. "A
// Odette se le escapan cosas de los clientes, pregúntale qué hacía Marcel esa
// tarde" sí; "fue Marcel" no. Si la pista resuelve el caso, deja de ser una
// válvula de escape y pasa a ser el camino corto.
public class LevelHint
{
    public string untilId;
    public string textFr;
    public string textEs;

    public LevelHint(string untilId, string textFr, string textEs)
    {
        this.untilId = untilId;
        this.textFr  = textFr;
        this.textEs  = textEs;
    }

    public string DisplayText => GameSettings.IsBasic ? textEs : textFr;
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

    // ─── MODO DETECTIVE ───────────────────────────────────────────────────
    // Cambia tres reglas del nivel, ver GDD_Modo_Detective.md §2:
    // 1) el reloj SUBE y no se pierde nunca por tiempo (timeLimitSeconds deja
    //    de usarse como fallo; starTimeSeconds sigue dando la estrella);
    // 2) la segunda estrella no es de palabras capturadas sino de haber
    //    roto al mentiroso (secondStarCheckpoint);
    // 3) el checklist del HUD son `objectives` y no `checkpoints`, porque en
    //    un caso lo que completa el nivel (convencer al comisario) no es lo
    //    mismo que lo que hay que hacer para llegar hasta ahí.
    public bool detectiveMode;

    // Número que se pinta en la baldosa del menú. Los casos son los niveles
    // 6-10 (el tramo avancé que LevelSelectUI ya reservaba), pero el jugador
    // los ve como "Caso 1..5". 0 = usar el id.
    public int displayNumber;

    // Id del reto que concede la 2ª estrella en modo detective (★ Le menteur).
    public string secondStarCheckpoint;

    // Checklist visible del caso. `checkpoints` sigue siendo lo que COMPLETA
    // el nivel; esto es solo la guía de qué hacer.
    public List<LevelObjective> objectives;

    // Líneas verdes que se van escribiendo solas según se descubren.
    public List<LevelFinding> findings;

    // Pistas del botón de ayuda, en orden de investigación. Requiere
    // hintAvailable. Usar una cuesta 1 estrella (una sola vez, por muchas
    // que se pidan — ver LevelManager.HintUsed).
    public List<LevelHint> hints;

    // Carta de cierre del caso, antes de la pantalla de estrellas.
    //
    // UNO SOLO por desenlace, nunca ramificado por el camino seguido. Un caso
    // tiene una respuesta correcta: quien la acierta ve `epilogue`, y da igual
    // cómo llegó. Las diferencias entre partidas ganadas se pagan en estrellas,
    // no en historia.
    public string epilogueFr;
    public string epilogueEs;

    // Cierre cuando el jugador se equivoca de culpable y acusa a un inocente
    // (ver failCheckpoint). No es "otro final" del mismo desenlace: es haber
    // perdido el caso, y el juego tiene que decirlo con la historia, no solo
    // con una pantalla de fallo.
    public string epilogueFailFr;
    public string epilogueFailEs;

    // Id que, al alcanzarse, TERMINA EL NIVEL COMO FALLIDO en el acto.
    //
    // Hasta ahora un caso solo podía perderse por tiempo, y en modo detective
    // ni eso. Existe desde que hay casos que terminan con una acusación
    // explícita: acusar al inocente es perder, y perder tiene que doler algo
    // más que no sumar estrellas. Ver LevelManager.ReportCheckpoints.
    public string failCheckpoint;

    public LevelDefinition(int id, string title, string sceneName,
        string briefTitle, string briefSummary, string briefTitleEs, string briefSummaryEs,
        List<TargetWord> targetWords, float timeLimitSeconds, float starTimeSeconds, string npcObjectiveKey,
        bool hintAvailable = false,
        Dictionary<string, string> npcObjectiveKeys = null, List<LevelCheckpoint> checkpoints = null,
        List<string> allowedScenes = null,
        bool detectiveMode = false, int displayNumber = 0, string secondStarCheckpoint = null,
        List<LevelObjective> objectives = null, List<LevelFinding> findings = null,
        List<LevelHint> hints = null, string epilogueFr = null, string epilogueEs = null,
        string epilogueFailFr = null, string epilogueFailEs = null, string failCheckpoint = null)
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
        this.detectiveMode     = detectiveMode;
        this.displayNumber     = displayNumber;
        this.secondStarCheckpoint = secondStarCheckpoint;
        this.objectives        = objectives;
        this.findings          = findings;
        this.hints             = hints;
        this.epilogueFr        = epilogueFr;
        this.epilogueEs        = epilogueEs;
        this.epilogueFailFr    = epilogueFailFr;
        this.epilogueFailEs    = epilogueFailEs;
        this.failCheckpoint    = failCheckpoint;
    }

    // Siguiente pista útil: la primera cuyo reto todavía no se ha alcanzado.
    public LevelHint NextHint(HashSet<string> reached)
    {
        if (hints == null) return null;
        foreach (var h in hints)
            if (!reached.Contains(h.untilId)) return h;
        return null;
    }

    public string DisplayTitle   => GameSettings.IsBasic ? briefTitleEs   : briefTitle;
    public string DisplaySummary => GameSettings.IsBasic ? briefSummaryEs : briefSummary;
    public int    TileNumber     => displayNumber > 0 ? displayNumber : id;
    public string DisplayEpilogue     => GameSettings.IsBasic ? epilogueEs     : epilogueFr;
    public string DisplayEpilogueFail => GameSettings.IsBasic ? epilogueFailEs : epilogueFailFr;
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
