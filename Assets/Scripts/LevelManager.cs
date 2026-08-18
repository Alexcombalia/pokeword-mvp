using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Core state machine for the new level/retos structure (menú de niveles,
// reemplaza al RPG narrativo — ver memory: project_pokeword_redesign).
// Persistent singleton, same lifecycle pattern as LevelSystem/QuizSystem.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public LevelDefinition CurrentLevel     { get; private set; }
    public bool            IsActive         { get; private set; }
    public float           TimeRemaining    { get; private set; }

    // Modo detective: el reloj SUBE ("llevas 6:42 investigando") y el caso no
    // se pierde nunca por tiempo. Es una regla de público objetivo, no de
    // dificultad: una cuenta atrás es exactamente lo que calla a alguien al
    // que ya le da vergüenza hablar. El tiempo se sigue midiendo para la
    // estrella y para comparar con amigos, pero no puede hacer perder.
    public float           TimeElapsed      { get; private set; }
    public bool            ObjectiveComplete{ get; private set; }
    public bool            HintUsed         { get; private set; }

    // Veces que se pulsó "Saltar" en la lectura en francés (débutant, ver
    // BeginnerTurnController) durante ESTE nivel — 3 o más penaliza -1
    // estrella al terminar (ver EndLevel). Reintentar sin límite dentro de
    // un mismo turno NO cuenta aquí, solo pulsar el botón cuenta.
    public int             SkipCount        { get; private set; }

    // Ids de retos (LevelDefinition.checkpoints) ya alcanzados en ESTE
    // nivel — se van sumando desde CUALQUIER NPC con objetivo de la escena
    // (ver ReportCheckpoints). El nivel se da por completo en cuanto
    // contiene el id de todos los checkpoints definidos.
    public HashSet<string> CheckpointsReached { get; private set; } = new HashSet<string>();

    // Clave compuesta "id:dificultad" — débutant y avancé llevan progreso
    // de estrellas independiente, nunca se mezclan (débutant es más fácil,
    // sería injusto que valiera lo mismo para avancé).
    private Dictionary<string, LevelResult> bestResults = new Dictionary<string, LevelResult>();

    private static string ResultKey(int levelId, string difficulty) => $"{levelId}:{difficulty}";

    // Palabras objetivo capturadas EN ESTE INTENTO — separado a propósito
    // del cuaderno de vocabulario permanente de WordSystem (que nunca se
    // borra, por diseño: "la lista de palabras persiste entre niveles").
    // Si usáramos el cuaderno permanente para la estrella de palabras, un
    // jugador podría repartir la captura entre varios reintentos y hacer
    // trampa; reiniciar esto en cada StartLevel garantiza que cada intento
    // cuenta desde cero.
    private HashSet<string> capturedThisAttempt = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        bestResults = SaveSystem.LoadLevelResults();
    }

    private void Update()
    {
        if (!IsActive) return;

        // El reloj sigue corriendo aunque ya se haya conseguido el objetivo
        // — el jugador puede seguir hablando para intentar la estrella de
        // palabras/velocidad hasta que cierre la conversación o se acabe el
        // tiempo (ver ReportObjectiveComplete / FinishLevelNow).
        if (CurrentLevel != null && CurrentLevel.detectiveMode)
        {
            TimeElapsed += Time.deltaTime;
            return; // sin límite duro: un caso no se pierde por tiempo
        }

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndLevel(ObjectiveComplete);
        }
    }

    public void StartLevel(int id)
    {
        var def = LevelDatabase.Get(id);
        if (def == null) return;

        CurrentLevel      = def;
        TimeRemaining     = def.timeLimitSeconds;
        TimeElapsed       = 0f;
        ObjectiveComplete = false;
        HintUsed          = false;
        SkipCount         = 0;
        IsActive           = true;
        capturedThisAttempt.Clear();
        CheckpointsReached.Clear();

        // El id manda sobre el modo, siempre — así toda la campaña de 10
        // niveles (1-5 débutant, 6-10 avancé) queda sincronizada sin tener
        // que fijar GameSettings.Difficulty a mano en MainMenuController,
        // LevelResultUI y LevelSelectUI por separado. Esto es lo que hace
        // que, al terminar el nivel 5 y entrar al 6, todo lo que ya lee
        // GameSettings.IsBasic en el resto del juego pase a comportarse como
        // avancé sin tocarlo (ver plan: sección 5).
        GameSettings.Difficulty = id >= 6 ? "medium" : "basic";

        // En modo detective el aviso es el contrario: el reloj está ahí pero
        // no persigue a nadie. Decirle "date prisa" a un jugador al que le da
        // vergüenza hablar es justo lo que no queremos.
        CoachMarks.Instance?.ShowOnce(def.detectiveMode ? "timer_case" : "timer",
            def.detectiveMode
                ? "Le temps monte, mais on ne perd jamais une affaire par le temps. Prends ton temps."
                : (GameSettings.IsBasic
                    ? "El tiempo empieza en cuanto entras — ¡date prisa!"
                    : "Le temps commence dès que tu entres — dépêche-toi !"),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -260), new Vector2(340, 70));

        SceneManager.LoadScene(def.sceneName);
        GuidedTutorial.Instance?.RunIfFirstTime();

        // Solo los niveles que de verdad cruzan escenas (p.ej. nivel 4)
        // necesitan explicar que aquí SÍ se puede — en el resto SceneExit
        // bloquea todo cruce, así que no aplicaría.
        if (def.allowedScenes != null)
            GuidedTutorial.Instance?.ExplainSceneCrossingOnce(def.allowedScenes, def.sceneName);
    }

    // Called from DialogueSystem once the server confirms the level's
    // conversation objective was met (objectiveComplete field, see server.js).
    // NO termina el nivel al instante — el jugador puede seguir hablando
    // (para intentar la estrella de palabras) y cierra cuando quiera; el
    // nivel solo acaba al cerrar esa conversación (ver DialogueSystem.Close)
    // o si se acaba el tiempo antes.
    public void ReportObjectiveComplete()
    {
        if (!IsActive || ObjectiveComplete) return;
        ObjectiveComplete = true;
    }

    // Llamado por DialogueSystem cuando un NPC genérico con objetivo
    // devuelve checkpointsReached (ver server.js). Une lo que reporte
    // CUALQUIER NPC de la escena en un único set por nivel — así da igual
    // si un reto lo cumple un NPC u otro (nivel 2) o si cada uno solo
    // conoce el suyo (nivel 3): el nivel se completa en cuanto el set
    // contiene el id de TODOS los checkpoints definidos.
    public void ReportCheckpoints(IEnumerable<string> ids)
    {
        if (!IsActive || CurrentLevel?.checkpoints == null || ids == null) return;

        foreach (var id in ids) CheckpointsReached.Add(id);

        // Acusar al inocente pierde el caso, y lo pierde EN EL ACTO — no al
        // cerrar la conversación, como pasa al ganar. Es deliberado: el
        // jugador acaba de dar un nombre y el juego tiene que responderle
        // inmediatamente, sin dejarle seguir hablando como si nada.
        //
        // Antes de esto un caso solo podía perderse por tiempo, y en modo
        // detective el tiempo no hace perder, así que literalmente no había
        // forma de fallar un caso.
        if (!string.IsNullOrEmpty(CurrentLevel.failCheckpoint)
            && CheckpointsReached.Contains(CurrentLevel.failCheckpoint))
        {
            EndLevel(false);
            return;
        }

        if (CurrentLevel.checkpoints.TrueForAll(c => CheckpointsReached.Contains(c.id)))
            ReportObjectiveComplete();
    }

    // Llamado por DialogueSystem.Close() cuando el jugador cierra una
    // conversación de Jacques que ya había completado el objetivo.
    public void FinishLevelNow()
    {
        if (!IsActive) return;
        EndLevel(true);
    }

    public void UseHint()
    {
        if (!IsActive) return;
        HintUsed = true;
    }

    // Llamado por BeginnerTurnController cuando el jugador pulsa "Saltar"
    // (disponible tras 3 intentos fallidos de lectura en un turno). Sin
    // penalización por fallar la lectura en sí — solo pulsar Saltar 3 o más
    // veces en el nivel cuenta, ver EndLevel.
    public void UseSkip()
    {
        if (!IsActive) return;
        SkipCount++;
    }

    public bool IsTargetWord(string frenchWord)
    {
        if (CurrentLevel == null || string.IsNullOrEmpty(frenchWord)) return false;
        string w = NormalizeFrenchWord(frenchWord);
        foreach (var tw in CurrentLevel.targetWords)
            if (NormalizeFrenchWord(tw.french) == w) return true;
        return false;
    }

    // Comprueba lo capturado EN ESTE INTENTO (no el cuaderno permanente) —
    // ver comentario en capturedThisAttempt.
    public bool IsWordCaptured(string frenchWord)
    {
        if (string.IsNullOrEmpty(frenchWord)) return false;
        return capturedThisAttempt.Contains(NormalizeFrenchWord(frenchWord));
    }

    // El NPC puede decir "poissons" cuando la palabra objetivo del nivel es
    // "poisson" (singular/plural) — deben contar igual. Solo normaliza para
    // COMPARAR (nunca se muestra al jugador), así que no hace falta ser
    // lingüísticamente perfecto: basta con que el mismo criterio se aplique
    // siempre a los dos lados de la comparación (palabra capturada Y palabra
    // objetivo), incluso si para alguna palabra concreta "sobre-recorta" —
    // seguirá coincidiendo consigo misma. Cubre el plural regular (-s), el
    // de -eau/-eu (-x, p.ej. bateau/bateaux) y una pequeña lista de
    // irregulares que sí son palabra objetivo en algún nivel.
    static readonly Dictionary<string, string> IrregularFrenchPlurals = new Dictionary<string, string>
    {
        { "travaux", "travail" },
    };

    static string NormalizeFrenchWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        string w = word.ToLowerInvariant().Trim();
        if (IrregularFrenchPlurals.TryGetValue(w, out var singular)) return singular;
        if ((w.EndsWith("s") || w.EndsWith("x")) && w.Length > 2) return w.Substring(0, w.Length - 1);
        return w;
    }

    // Llamado por WordSystem.Capture() cada vez que el jugador toca una
    // palabra en una conversación, esté o no ya en su cuaderno permanente —
    // lo que cuenta aquí es "la dijeron/tocaste en ESTE intento", no si ya
    // la sabías de antes.
    public void NotifyWordCaptured(string frenchWord)
    {
        if (!IsActive || string.IsNullOrEmpty(frenchWord)) return;
        capturedThisAttempt.Add(NormalizeFrenchWord(frenchWord));
    }

    public int CapturedTargetCount()
    {
        if (CurrentLevel == null) return 0;

        int count = 0;
        foreach (var tw in CurrentLevel.targetWords)
            if (IsWordCaptured(tw.french)) count++;
        return count;
    }

    // Devuelve el mejor resultado para la dificultad ACTUAL — cada
    // dificultad tiene su propio progreso, ver ResultKey.
    public LevelResult GetBestResult(int levelId)
    {
        return bestResults.TryGetValue(ResultKey(levelId, GameSettings.Difficulty), out var r) ? r : null;
    }

    // Called when the player backs out to the level menu without finishing.
    public void ExitLevel()
    {
        IsActive     = false;
        CurrentLevel = null;
    }

    private void EndLevel(bool success)
    {
        IsActive = false;
        NPCVoicePlayer.Instance?.Stop(); // corta la voz de Jacques si seguía sonando

        float elapsed = CurrentLevel.detectiveMode
            ? TimeElapsed
            : CurrentLevel.timeLimitSeconds - TimeRemaining;

        bool starObjective = success;

        // La 2ª estrella cambia de significado en modo detective: no es haber
        // capturado todas las palabras sino haber roto al mentiroso en un
        // careo (★ Le menteur). El caso se puede resolver deduciendo, sin
        // pasar por ahí — por eso es estrella y no requisito.
        // La 2ª estrella la decide el NIVEL, no el modo. Si define
        // secondStarCheckpoint es un reto concreto (en avancé, romper al
        // mentiroso); si no, se cae a la estrella de palabras.
        //
        // Esto importa desde que los casos débutant sustituyen a la campaña
        // vieja: un principiante está ahí para aprender vocabulario, así que
        // sus casos siguen premiando capturar palabras, mientras que en avancé
        // —donde ya se habla francés— el segundo reto es el careo. Mismo modo
        // detective, distinta segunda estrella, sin ramas por dificultad.
        bool starWords = !string.IsNullOrEmpty(CurrentLevel.secondStarCheckpoint)
            ? CheckpointsReached.Contains(CurrentLevel.secondStarCheckpoint)
            : (CurrentLevel.targetWords.Count > 0 &&
               CapturedTargetCount() >= CurrentLevel.targetWords.Count);

        bool starSpeed = success && elapsed <= CurrentLevel.starTimeSeconds;

        int rawStars = (starObjective ? 1 : 0) + (starWords ? 1 : 0) + (starSpeed ? 1 : 0);

        // Pista y saltos son válvulas de escape distintas ("no lo sé" vs "no
        // consigo pasar la lectura") — deducciones independientes, hasta -2
        // si se usaron ambas, más simple de razonar que "gana la peor".
        bool skipPenalty = SkipCount >= 3;
        int stars = rawStars;
        if (HintUsed)    stars = Mathf.Max(0, stars - 1);
        if (skipPenalty) stars = Mathf.Max(0, stars - 1);

        var result = new LevelResult
        {
            levelId     = CurrentLevel.id,
            stars       = stars,
            timeSeconds = elapsed,
            completed   = success,
            difficulty  = GameSettings.Difficulty,
        };

        SaveResultIfBetter(result);

        LevelResultUI.Instance?.Show(CurrentLevel, result, success, starObjective, starWords, starSpeed, HintUsed, skipPenalty);
    }

    private void SaveResultIfBetter(LevelResult result)
    {
        string key = ResultKey(result.levelId, result.difficulty);
        bool better = !bestResults.TryGetValue(key, out var existing)
            || result.stars > existing.stars
            || (result.stars == existing.stars && result.completed && result.timeSeconds < existing.timeSeconds);

        if (better)
        {
            bestResults[key] = result;
            SaveSystem.SaveLevelResults(bestResults);
        }
    }
}
