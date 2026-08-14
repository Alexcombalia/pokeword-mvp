using System.Collections.Generic;

// Caja piloto "El Puerto" — campaña débutant de 5 niveles (1-5), construida
// sobre NPCs y escenas que ya existen en el mundo. Arco narrativo: Jacques
// (mentor) va soltando a Godofredo en la ciudad — Muelle (trabajo), Playa
// (amistad/nostalgia), un recado entre ambos (retransmitir información,
// cruza escenas de verdad), y de vuelta con Jacques como examen final antes
// de pasar a avancé (nivel 6). Ver memory: project_pokeword_redesign.
public static class LevelDatabase
{
    public static readonly List<LevelDefinition> All = new List<LevelDefinition>
    {
        new LevelDefinition(
            id: 1,
            title: "Le bateau de Jacques",
            sceneName: "Puerto",
            briefTitle: "Niveau 1 — Le bateau de Jacques",
            briefSummary: "Parle avec Jacques et découvre le nom de son bateau.",
            briefTitleEs: "Nivel 1 — El barco de Jacques",
            briefSummaryEs: "Habla con Jacques y descubre el nombre de su barco.",
            targetWords: new List<TargetWord>
            {
                new TargetWord("bateau",    "barco"),
                new TargetWord("capitaine", "capitán"),
                new TargetWord("mer",       "mar"),
                new TargetWord("port",      "puerto"),
            },
            timeLimitSeconds: 180f, // 3 minutos — 90s se quedaba corto para débutant con la mecánica nueva de lectura
            starTimeSeconds: 90f, // 1:30 — estrella de velocidad
            npcObjectiveKey: "level1_boat_name",
            hintAvailable: false
        ),

        new LevelDefinition(
            id: 2,
            title: "Un coup de main au quai",
            sceneName: "Muelle",
            briefTitle: "Niveau 2 — Un coup de main au quai",
            briefSummary: "Parle avec les dockers du quai jusqu'à ce que l'un d'eux te dise où trouver à manger bon marché.",
            briefTitleEs: "Nivel 2 — Manos en el muelle",
            briefSummaryEs: "Habla con la gente del muelle sobre su trabajo hasta que alguien te diga dónde conseguir comida barata.",
            targetWords: new List<TargetWord>
            {
                new TargetWord("travail", "trabajo"),
                new TargetWord("caisse",  "caja"),
                new TargetWord("lourd",   "pesado"),
                new TargetWord("ami",     "amigo"),
            },
            timeLimitSeconds: 240f,
            starTimeSeconds: 120f,
            npcObjectiveKey: null, // NPCs genéricos, no Jacques — usa npcObjectiveKeys
            hintAvailable: false,
            // Los 3 NPCs del muelle valen igual — cualquiera puede dar los
            // 2 retos, así no hay "NPC equivocado" posible.
            npcObjectiveKeys: new Dictionary<string, string>
            {
                { "npc_playa_11", "level2_raphael_friend" }, // Raphael, estibador
                { "npc_playa_13", "level2_antoine_friend" }, // Antoine, pescador
                { "npc_playa_12", "level2_albert_friend" },  // Albert, capitán
            },
            checkpoints: new List<LevelCheckpoint>
            {
                new LevelCheckpoint("work", "Demande-lui pour son travail", "Pregúntale por su trabajo"),
                new LevelCheckpoint("food", "Fais-toi dire où trouver à manger bon marché", "Consigue que te diga dónde hay comida barata"),
            }
        ),

        new LevelDefinition(
            id: 3,
            title: "La femme de la plage",
            sceneName: "Playa",
            briefTitle: "Niveau 3 — La femme de la plage",
            briefSummary: "Parle avec les gens de la plage du Marseille d'autrefois.",
            briefTitleEs: "Nivel 3 — La mujer de la playa",
            briefSummaryEs: "Habla con la gente de la playa sobre cómo era Marsella antes.",
            targetWords: new List<TargetWord>
            {
                new TargetWord("mer",       "mar"),
                new TargetWord("poisson",   "pescado"),
                new TargetWord("autrefois", "antes"),
                new TargetWord("souvenir",  "recuerdo"),
            },
            timeLimitSeconds: 300f, // hay que encontrar y hablar con los 3 NPCs de la escena, no solo mantener una conversación larga
            starTimeSeconds: 150f,
            npcObjectiveKey: null,
            hintAvailable: false,
            // A diferencia del nivel 2, aquí cada NPC tiene SU PROPIO reto —
            // hay que hablar con los 3, no vale cualquiera para todo.
            npcObjectiveKeys: new Dictionary<string, string>
            {
                { "npc_playa_04", "level3_sandra_secret" },  // Sandra, vieux
                { "npc_playa_03", "level3_remy_secret" },    // Rémy, pescador
                { "npc_playa_02", "level3_francis_secret" }, // Francis, cliente_bar
            },
            checkpoints: new List<LevelCheckpoint>
            {
                new LevelCheckpoint("sandra_before",     "Demande à Sandra comment était Marseille avant",        "Pregúntale a Sandra cómo era Marsella antes"),
                new LevelCheckpoint("remy_fish_count",   "Demande à Rémy combien de poissons il a pêchés",        "Pregúntale a Rémy cuántos peces ha pescado hoy"),
                new LevelCheckpoint("francis_job_dream", "Découvre le métier et le rêve de Francis",              "Descubre en qué trabaja Francis y cuál es su sueño"),
            }
        ),

        new LevelDefinition(
            id: 4,
            title: "La commission",
            sceneName: "Puerto",
            briefTitle: "Niveau 4 — La commission",
            briefSummary: "Rémy a un message pour Raphael — va le lui porter à la plage puis au quai.",
            briefTitleEs: "Nivel 4 — El recado",
            briefSummaryEs: "Rémy tiene un mensaje para Raphael — habla con él en la playa y luego llévaselo a Raphael en el muelle.",
            targetWords: new List<TargetWord>
            {
                new TargetWord("message",  "mensaje"),
                new TargetWord("demain",   "mañana"),
                new TargetWord("attendre", "esperar"),
                new TargetWord("ami",      "amigo"),
            },
            timeLimitSeconds: 240f,
            starTimeSeconds: 120f,
            npcObjectiveKey: null,
            hintAvailable: false,
            // Rémy y Raphael, cada uno con su propio reto — antes Rémy era
            // "blando" (oculto), ahora ambos son checkpoints visibles.
            npcObjectiveKeys: new Dictionary<string, string>
            {
                { "npc_playa_03", "level4_remy_reveal" },
                { "npc_playa_11", "level4_raphael_deliver" },
            },
            checkpoints: new List<LevelCheckpoint>
            {
                new LevelCheckpoint("remy_message",     "Obtiens le message de Rémy", "Consigue el mensaje de Rémy"),
                new LevelCheckpoint("raphael_received", "Porte-le à Raphael",         "Entrégaselo a Raphael"),
            },
            allowedScenes: new List<string> { "Puerto", "Playa", "Muelle" }
        ),

        new LevelDefinition(
            id: 5,
            title: "L'épreuve de Jacques",
            sceneName: "Puerto",
            briefTitle: "Niveau 5 — L'épreuve de Jacques",
            briefSummary: "Jacques veut voir si tu sais te débrouiller en français, maintenant.",
            briefTitleEs: "Nivel 5 — La prueba de Jacques",
            briefSummaryEs: "Demuestra a Jacques que ya sabes defenderte en francés.",
            targetWords: new List<TargetWord>
            {
                new TargetWord("bateau",  "barco"),
                new TargetWord("port",    "puerto"),
                new TargetWord("ami",     "amigo"),
                new TargetWord("travail", "trabajo"),
                new TargetWord("retour",  "vuelta"),
                new TargetWord("prêt",    "listo"),
            },
            timeLimitSeconds: 270f,
            starTimeSeconds: 135f,
            npcObjectiveKey: "level5_jacques_trust", // Jacques usa su propio endpoint dedicado, no npcObjectiveKeys
            hintAvailable: false
        ),
    };

    public static LevelDefinition Get(int id)
    {
        foreach (var lvl in All)
            if (lvl.id == id) return lvl;
        return null;
    }
}
