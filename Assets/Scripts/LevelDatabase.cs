using System.Collections.Generic;

// Campaña del juego: 10 casos de misterio. 1-5 débutant, 6-10 avancé. El
// jugador es un inspector de policía en todos: el caso le cae por turno,
// nadie le contrata y nadie le paga.
//
// Los niveles 2-5 son todavía la campaña vieja (Jacques, el puerto,
// Godofredo), que quedó retirada del diseño el 18/08/2026 y se irá
// sustituyendo caso a caso. Se dejan funcionando mientras tanto para no
// dejar el juego con un solo nivel jugable.
public static class LevelDatabase
{
    public static readonly List<LevelDefinition> All = new List<LevelDefinition>
    {
        // ─── CASO DÉBUTANT 1 ───────────────────────────────────────────────
        // Sustituye a «Le bateau de Jacques» (18/08/2026): el juego pasa a ser
        // exclusivamente de resolver misterios y el jugador es un inspector
        // de policía. Ver Pokeword/Caso_D1_La_Comtesse.md.
        //
        // Es el PRIMER nivel del juego: enseña a moverse, a hablar, a capturar
        // palabras y a investigar, a alguien que además no habla francés.
        new LevelDefinition(
            id: 1,
            displayNumber: 1,
            detectiveMode: true,
            title: "Le collier de la comtesse",
            sceneName: "SalonCondesa",
            briefTitle: "Cas 1 — Le collier de la comtesse",
            briefSummary: "Tu es INSPECTEUR DE POLICE. Cette affaire est tombée sur ton bureau ce matin.\n\nLa comtesse Volkova est morte cette nuit et le docteur croit à une crise cardiaque. Sa femme de chambre n'y croit pas, et la même nuit le collier de diamants a disparu.\n\nParle aux trois personnes du salon et découvre si on l'a tuée, et qui a pu le faire.",
            briefTitleEs: "Caso 1 — El collar de la condesa",
            briefSummaryEs: "Eres INSPECTOR DE POLICÍA. Este caso te ha caído esta mañana.\n\nLa condesa Volkova apareció muerta anoche y el médico cree que fue un infarto. Su doncella no se lo cree, y esa misma noche desapareció el collar de diamantes.\n\nHabla con los tres que están en el salón y averigua si la mataron, y quién pudo hacerlo.",
            targetWords: new List<TargetWord>
            {
                new TargetWord("collier",   "collar"),
                new TargetWord("bouteille", "botella"),
                new TargetWord("nuit",      "noche"),
                new TargetWord("docteur",   "médico"),
                new TargetWord("vendre",    "vender"),
                new TargetWord("voir",      "ver"),
            },
            targetWordsEn: new List<TargetWord>
            {
                new TargetWord("necklace", "collar"),
                new TargetWord("bottle",   "botella"),
                new TargetWord("night",    "noche"),
                new TargetWord("doctor",   "médico"),
                new TargetWord("sell",     "vender"),
                new TargetWord("see",      "ver"),
            },
            targetWordsIt: new List<TargetWord>
            {
                new TargetWord("collana",  "collar"),
                new TargetWord("bottiglia","botella"),
                new TargetWord("notte",    "noche"),
                new TargetWord("dottore",  "médico"),
                new TargetWord("vendere",  "vender"),
                new TargetWord("vedere",   "ver"),
            },
            targetWordsCa: new List<TargetWord>
            {
                new TargetWord("collaret", "collar"),
                new TargetWord("ampolla",  "botella"),
                new TargetWord("nit",      "noche"),
                new TargetWord("metge",    "médico"),
                new TargetWord("vendre",   "vender"),
                new TargetWord("veure",    "ver"),
            },
            timeLimitSeconds: 0f,   // sin fallo por tiempo, el reloj sube
            // 10 min. El caso se resuelve holgadamente en 20, pero el brief
            // enseña este número antes de empezar y "20:00 min" en el primer
            // nivel del juego suena a deberes: hay quien no entra. No se pierde
            // nada por pasarse (timeLimitSeconds es 0, el reloj solo sube), así
            // que lo único que decide es la 3ª estrella — mejor que sea una
            // meta ambiciosa que un aviso que espanta.
            starTimeSeconds: 600f,
            npcObjectiveKey: null,
            hintAvailable: true,
            npcObjectiveKeys: new Dictionary<string, string>
            {
                { "nadia",   "cased1_nadia" },
                { "lemaire", "cased1_lemaire" },
                { "piotr",   "cased1_piotr" },
                { "police",  "cased1_police" },
            },
            // El caso lo cierra el AGENTE, dándole un nombre. Solo deja darlo
            // con la confesión de Piotr (`p_broken`) o, en su defecto, con las
            // dos pruebas (`l_piotr` + `p_yesterday`) — hasta entonces contesta
            // que no hay bastante. Acertar es `pol_right`; acusar a un inocente
            // es `failCheckpoint` y pierde el caso en el acto.
            checkpoints: new List<LevelCheckpoint>
            {
                new LevelCheckpoint("pol_right", "Donne le bon nom à l'agent", "Dale el nombre correcto al agente"),
            },
            failCheckpoint: "pol_wrong",
            // null a propósito: en débutant la 2ª estrella NO es el careo. Era
            // de vocabulario y desde el 30/08/2026 es de habla — ver
            // spokenWordsTarget justo debajo.
            secondStarCheckpoint: null,
            // 120 palabras dichas por micrófono. Estimación de partida, a
            // recalibrar con el contador de la pantalla final: resolver el caso
            // por el camino corto son ~10-13 turnos (~90 palabras) y jugándolo
            // bien ~20-25 (~200). El umbral quiere separar al que corre al
            // agente del que investiga, no castigar a nadie.
            spokenWordsTarget: 120,
            objectives: new List<LevelObjective>
            {
                // El checklist se ve desde el primer segundo, así que no puede
                // contener nada que el jugador aún no haya averiguado. Dos
                // objetivos se cayeron por eso tras el primer testeo:
                //  · "Averigua quién vende esa bebida" hablaba de una bebida
                //    antes de que nadie hubiera mencionado ninguna. Ahora va
                //    junto con el anterior, sin nombrarla.
                //  · "Rompe su coartada: dice que vino hace días" era el
                //    spoiler entero del caso — la coartada, su contenido y que
                //    es falsa — servido en la pantalla de carga. Eliminado.
                //    `p_yesterday` se queda sin línea propia: los objetivos son
                //    guía, no el índice de todo lo que hay que descubrir.
                new LevelObjective("Écoute Nadia : hier soir",     "Escucha a Nadia: qué pasó ayer por la noche", "n_selling"),
                new LevelObjective("Cherche ce qui a pu la tuer",  "Averigua si algo pudo matarla",               "l_drink", "l_piotr"),
                // NO `p_deny`: ese se marca cuando Piotr DICE la mentira, y el
                // jugador no sabe todavía que lo es — el objetivo poniéndose
                // verde se lo chivaba gratis, y encima solo a quien mira el
                // panel. Va `p_yesterday`: ponerle la fecha delante es el
                // momento en que el jugador la ha descubierto de verdad.
                new LevelObjective("Trouve qui a menti",           "Descubre quién ha mentido",                   "p_yesterday"),
                new LevelObjective("Donne ton nom à l'agent",      "Dale tu respuesta final al agente",           "pol_right"),
            },
            findings: new List<LevelFinding>
            {
                new LevelFinding("n_selling",  "Hier soir un homme vendait quelque chose à la comtesse", "Ayer por la noche un hombre le estaba vendiendo algo a la condesa"),
                new LevelFinding("n_case",     "Nadia n'a pas vu son visage. Il portait une mallette — un marchand de bijoux, croit-elle", "Nadia no le vio la cara. Llevaba un maletín — un comerciante de joyas, cree ella"),
                new LevelFinding("n_necklace", "Le collier de diamants a disparu",                       "El collar de diamantes ha desaparecido"),
                new LevelFinding("l_nocrime",  "Rien sur le corps. Aucune blessure",                     "Nada en el cuerpo. Ninguna herida"),
                new LevelFinding("l_drink",    "Une boisson forte, du cognac, peut tuer un cœur faible",  "Una bebida fuerte, como el coñac, puede matar un corazón débil"),
                new LevelFinding("l_piotr",    "C'est Piotr qui vend le cognac, illégalement",            "Es Piotr quien vende el coñac, ilegalmente"),
                new LevelFinding("p_deny",     "Piotr dit qu'il n'était pas là hier",                     "Piotr dice que ayer no estuvo allí"),
                new LevelFinding("p_sold",     "Piotr admet avoir vendu le cognac à la comtesse — mais il y a des jours", "Piotr admite haberle vendido el coñac a la condesa — pero hace días"),
                new LevelFinding("p_accuses",  "Piotr accuse Nadia : elle était seule avec elle",         "Piotr acusa a Nadia: estaba sola con ella"),
            },
            hints: new List<LevelHint>
            {
                new LevelHint("n_selling",  "Nadia était là hier soir. Demande-lui ce qui s'est passé.",
                                            "Nadia estaba allí ayer por la noche. Pregúntale qué pasó."),
                new LevelHint("n_case",     "Demande à Nadia comment était l'homme, ce qu'il portait.",
                                            "Pregúntale a Nadia cómo era el hombre y qué llevaba."),
                new LevelHint("n_necklace", "Demande à Nadia ce qui manque dans le salon.",
                                            "Pregúntale a Nadia qué falta en el salón."),
                new LevelHint("l_drink",    "Demande au docteur ce qui d'autre peut arrêter un cœur.",
                                            "Pregúntale al doctor qué otra cosa puede parar un corazón."),
                new LevelHint("l_piotr",    "Demande au docteur qui vend du cognac ici.",
                                            "Pregúntale al doctor quién vende coñac aquí."),
                new LevelHint("p_sold",     "Parle du cognac au bijoutier. Vois ce qu'il répond.",
                                            "Háblale del coñac al joyero. A ver qué contesta."),
                new LevelHint("p_yesterday","Il dit être venu il y a des jours. Nadia a vu un homme vendre HIER SOIR.",
                                            "Dice que vino hace días. Nadia vio a un hombre vendiendo AYER POR LA NOCHE."),
                new LevelHint("pol_right",  "L'agent attend. Va lui donner un nom.",
                                            "El agente espera. Ve a darle un nombre."),
            },
            epilogueFr:
                "Piotr Sokolov a été emmené le matin même. Le collier était dans son atelier, enveloppé dans un chiffon.\n\n" +
                "Nadia n'a pas voulu le garder. Elle a dit qu'il était à madame, et que madame n'était plus là.\n\n" +
                "Le docteur Lemaire a demandé qu'on revoie le certificat. C'est la seule chose qu'il ait demandée de sa vie.",
            epilogueEs:
                "Piotr Sokolov fue detenido esa misma mañana. El collar estaba en su taller, envuelto en un trapo.\n\n" +
                "Nadia no quiso quedárselo. Dijo que era de la señora, y que la señora ya no estaba.\n\n" +
                "El doctor Lemaire pidió que se revisara el certificado. Fue lo único que pidió en toda su vida.",
            epilogueFailFr:
                "L'agent a emmené la personne que vous avez nommée. Personne n'a posé d'autre question.\n\n" +
                "Piotr Sokolov a fermé boutique une semaine, par prudence. Puis il a rouvert. Le collier n'a jamais été retrouvé.\n\n" +
                "L'affaire est classée. Elle est fausse, mais elle est classée.",
            epilogueFailEs:
                "El agente se llevó a quien usted nombró. Nadie hizo más preguntas.\n\n" +
                "Piotr Sokolov cerró la tienda una semana, por prudencia. Después volvió a abrir. El collar no apareció nunca.\n\n" +
                "El caso está cerrado. Está mal, pero está cerrado."
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

        // ─── CAMPAÑA DETECTIVE (avancé) — niveles 6-10 ─────────────────────
        // Ocupan el tramo 6-10 que LevelSelectUI ya reservaba para avancé y
        // que hasta ahora salía bloqueado por no existir. El progreso no se
        // mezcla con débutant: son ids distintos, y además LevelManager guarda
        // los resultados por "id:dificultad". El jugador los ve numerados
        // 1-5 (displayNumber), porque para él son "los casos", no "los
        // niveles 6-10". Ver Pokeword/GDD_Modo_Detective.md.
        new LevelDefinition(
            id: 6,
            displayNumber: 1,
            detectiveMode: true,
            title: "Le violoniste du Dragon Vert",
            sceneName: "BarDragonVerde",
            briefTitle: "Cas 1 — Le violoniste du Dragon Vert",
            briefSummary: "Tu es INSPECTEUR DE POLICE. Cette affaire est pour toi.\n\nMarius Toussaint, le violoniste du Dragon Vert, a été tué d'un coup de feu hier soir dans la ruelle derrière le café. Tout le monde l'aimait bien ici.\n\nDeux choses ne collent pas : il a joué un morceau qui n'était pas le sien, et il a arrêté dix minutes plus tôt. Parle aux gens du café et découvre qui l'a tué.",
            briefTitleEs: "Caso 1 — El violinista del Dragón Verde",
            briefSummaryEs: "Mataron de un disparo a Marius Toussaint, el violinista, anoche en el callejón. Dos cosas no cuadran: tocó una pieza que no era la suya y cortó el turno diez minutos antes.",
            targetWords: new List<TargetWord>
            {
                new TargetWord("violon",     "violín"),
                new TargetWord("ruelle",     "callejón"),
                new TargetWord("coup de feu", "disparo"),
                new TargetWord("arme",       "arma"),
                new TargetWord("horaires",   "horarios"),
                new TargetWord("papiers",    "papeles"),
            },
            // En modo detective timeLimitSeconds NO se usa como fallo — el
            // reloj sube y el caso no se pierde nunca por tiempo. Se deja un
            // valor por compatibilidad con el resto del sistema.
            targetWordsEn: new List<TargetWord>
            {
                new TargetWord("violin",  "violín"),
                new TargetWord("alley",   "callejón"),
                new TargetWord("gunshot", "disparo"),
                new TargetWord("gun",     "arma"),
                new TargetWord("hours",   "horarios"),
                new TargetWord("papers",  "papeles"),
            },
            targetWordsIt: new List<TargetWord>
            {
                new TargetWord("violino", "violín"),
                new TargetWord("vicolo",  "callejón"),
                new TargetWord("sparo",   "disparo"),
                new TargetWord("pistola", "arma"),
                new TargetWord("orari",   "horarios"),
                new TargetWord("documenti","papeles"),
            },
            targetWordsCa: new List<TargetWord>
            {
                new TargetWord("violí",   "violín"),
                new TargetWord("carreró", "callejón"),
                new TargetWord("tret",    "disparo"),
                new TargetWord("pistola", "arma"),
                new TargetWord("horaris", "horarios"),
                new TargetWord("papers",  "papeles"),
            },
            timeLimitSeconds: 0f,
            starTimeSeconds: 1440f, // 24 min — ★ Chronomètre, holgado sobre los 13-15 previstos
            npcObjectiveKey: null,
            hintAvailable: true,
            npcObjectiveKeys: new Dictionary<string, string>
            {
                { "odette",   "case1_odette" },
                { "corentin", "case1_corentin" },
                { "marcel",    "case1_marcel" },
                { "fabre",    "case1_fabre" },
            },
            // Lo que CIERRA el caso: convencer al comisario. Ni hablar con
            // todos ni romper a Marcel son obligatorios — con las dos mentiras
            // confirmadas por Odette, Fabre da la orden igual.
            checkpoints: new List<LevelCheckpoint>
            {
                new LevelCheckpoint("f_name",   "Donne un nom au commissaire", "Dale un nombre al comisario", "Give the inspector a name",   "Dai un nome al commissario",   "Dóna un nom al comissari"),
                new LevelCheckpoint("f_lies",   "Expose les deux mensonges",   "Expón las dos mentiras",      "Lay out both lies",            "Esponi le due bugie",          "Exposa les dues mentides"),
                new LevelCheckpoint("f_motive", "Explique qu'il l'attendait",  "Explica que lo estaba esperando", "Explain that he was waiting for him", "Spiega che lo stava aspettando", "Explica que l'estava esperant"),
            },
            // ★ Le menteur: romper a Marcel con las DOS contradicciones. En
            // avancé la 2ª estrella sigue siendo el careo, no la de habla — es
            // el reto propio del caso. spokenWordsTarget se pone igualmente
            // para que el contador de la pantalla final tenga referencia.
            secondStarCheckpoint: "m_broken",
            spokenWordsTarget: 200,
            objectives: new List<LevelObjective>
            {
                new LevelObjective("Écoute Odette : ce qu'elle a vu", "Escucha a Odette: qué vio anoche", "Listen to Odette: what she saw", new[]{ "o_signal" }, "Ascolta Odette: cosa ha visto", "Escolta l'Odette: què va veure"),
                new LevelObjective("Fais parler Corentin du morceau", "Haz hablar a Corentin de la pieza musical", "Get Corentin to talk about the tune", new[]{ "c_signal" }, "Fai parlare Corentin del pezzo", "Fes parlar en Corentin de la peça"),
                // NUNCA nombrar al culpable aquí. El checklist se ve desde el
                // primer segundo, antes de hablar con nadie: decir "pilla a
                // Marcel en dos mentiras" resuelve el caso en la pantalla de
                // carga. El objetivo plantea el reto; el nombre lo pone el
                // jugador. Misma regla que para los NPCs — solo el jugador
                // saca conclusiones.
                // Mismo motivo que en el caso débutant: `m_lie_gun` y
                // `m_lie_time` se marcan cuando Marcel SUELTA cada mentira, y
                // el jugador no sabe aún que lo son. Van los `m_confront_*`,
                // que son el momento en que se las pone delante.
                new LevelObjective("Ici quelqu'un ment. Prends-le en deux mensonges", "Aquí alguien miente. Píllalo en dos mentiras", "Someone here is lying. Catch him in two lies", new[]{ "m_confront_gun", "m_confront_time" }, "Qui qualcuno mente. Coglilo in due bugie", "Aquí algú menteix. Enxampa'l en dues mentides"),
                new LevelObjective("Raconte au commissaire ce qui s'est passé", "Cuéntale al comisario lo que pasó", "Tell the inspector what happened", new[]{ "f_name", "f_lies", "f_motive" }, "Racconta al commissario cos'è successo", "Explica-li al comissari què va passar"),
            },
            // Hechos, nunca conclusiones. El panel no dice quién miente: pone
            // los datos uno debajo de otro y deja que el jugador los cruce.
            findings: new List<LevelFinding>
            {
                new LevelFinding("o_signal",        "Marius a changé de morceau et fini 10 min plus tôt", "Marius cambió de pieza y cortó el turno 10 minutos antes", "Marius changed the tune and finished 10 min early", "Marius ha cambiato pezzo e ha finito 10 min prima", "En Marius va canviar de peça i va plegar 10 min abans"),
                new LevelFinding("o_metal",         "Marcel avait quelque chose de métallique sous son manteau", "Marcel llevaba algo metálico bajo el abrigo", "Marcel had something metal under his coat", "Marcel aveva qualcosa di metallo sotto il cappotto", "En Marcel duia alguna cosa metàl·lica sota l'abric"),
                new LevelFinding("o_exit",          "Marcel est sorti quand la musique s'est arrêtée",     "Marcel salió justo cuando paró la música", "Marcel left the moment the music stopped", "Marcel è uscito appena la musica si è fermata", "En Marcel va sortir just quan la música es va aturar"),
                new LevelFinding("o_asked",         "Marcel demandait les horaires et la sortie de Marius", "Marcel preguntó por los horarios y la salida de Marius", "Marcel had asked about Marius's hours and his exit", "Marcel aveva chiesto degli orari e dell'uscita di Marius", "En Marcel va preguntar pels horaris i la sortida d'en Marius"),
                new LevelFinding("c_signal",        "Le morceau était un signal : « pars, on te cherche »", "La pieza era una señal: «vete, alguien te busca»", "The tune was a signal: \"leave, someone is looking for you\"", "Il pezzo era un segnale: «vattene, qualcuno ti cerca»", "La peça era un senyal: «vés-te'n, algú et busca»"),
                new LevelFinding("c_someone_asked", "Quelqu'un a demandé après Marius. Corentin ignore qui", "Alguien preguntó por Marius. Corentin no sabe quién", "Someone asked after Marius. Corentin doesn't know who", "Qualcuno ha chiesto di Marius. Corentin non sa chi", "Algú va preguntar per en Marius. En Corentin no sap qui"),
                new LevelFinding("m_lie_gun",       "Marcel dit ne plus porter d'arme depuis sa retraite",  "Marcel dice que no lleva arma desde que se retiró", "Marcel says he has not carried a gun since he retired", "Marcel dice di non portare la pistola da quando è in pensione", "En Marcel diu que no porta arma des que es va retirar"),
                new LevelFinding("m_lie_time",      "Marcel dit être parti bien avant le coup de feu",      "Marcel dice que se fue mucho antes del disparo", "Marcel says he left well before the gunshot", "Marcel dice di essere andato via molto prima dello sparo", "En Marcel diu que va marxar molt abans del tret"),
            },
            // Pistas: la siguiente PREGUNTA útil, nunca la solución. En orden
            // de investigación — se muestra la primera que aún no esté hecha.
            // La de m_broken va antes que las del comisario a propósito: quien
            // ya tiene las dos mentiras y pide pista está buscando el careo,
            // y quien solo quiere cerrar puede ignorarla e ir a ver a Fabre.
            hints: new List<LevelHint>
            {
                new LevelHint("o_signal",  "Odette servait hier soir. Demande-lui ce qui s'est passé.",
                                           "Odette servía anoche. Pregúntale qué pasó.",
                                           "Odette was serving last night. Ask her what happened.",
                                            "Odette serviva ieri sera. Chiedile cos'è successo.",
                                            "L'Odette servia ahir al vespre. Pregunta-li què va passar."),
                new LevelHint("o_exit",    "Demande à Odette qui est sorti du bar, et quand.",
                                           "Pregúntale a Odette quién salió del bar, y cuándo.",
                                           "Ask Odette who left the bar, and when.",
                                            "Chiedi a Odette chi è uscito dal bar, e quando.",
                                            "Pregunta-li a l'Odette qui va sortir del bar, i quan."),
                new LevelHint("o_metal",   "Odette parle des habitués sans y penser. Demande-lui ce qu'ils portaient.",
                                           "A Odette se le escapan cosas de los clientes. Pregúntale qué llevaban encima.",
                                           "Odette talks about the regulars without thinking. Ask what they were carrying.",
                                            "Odette parla dei clienti senza pensarci. Chiedile cosa portavano addosso.",
                                            "A l'Odette se li escapen coses dels clients. Pregunta-li què duien a sobre."),
                new LevelHint("o_asked",   "Demande à Odette ce qui s'est passé l'après-midi, avant le concert.",
                                           "Pregúntale a Odette qué pasó esa tarde, antes del concierto.",
                                           "Ask Odette what happened that afternoon, before the performance.",
                                            "Chiedi a Odette cos'è successo quel pomeriggio, prima del concerto.",
                                            "Pregunta-li a l'Odette què va passar aquella tarda, abans del concert."),
                new LevelHint("c_signal",  "Le patron n'aime pas la question du morceau. Reviens dessus.",
                                           "Al dueño no le gusta la pregunta de la pieza. Insiste.",
                                           "The owner doesn't like the question about the tune. Come back to it.",
                                            "Al padrone non piace la domanda sul pezzo. Insisti.",
                                            "A l'amo no li agrada la pregunta de la peça. Insisteix."),
                new LevelHint("c_someone_asked", "Demande à Corentin pourquoi il a donné ce signal ce soir-là.",
                                           "Pregúntale a Corentin por qué dio esa señal justo esa noche.",
                                           "Ask Corentin why he gave that signal that night.",
                                            "Chiedi a Corentin perché ha dato quel segnale quella sera.",
                                            "Pregunta-li a en Corentin per què va donar aquell senyal aquella nit."),
                new LevelHint("m_lie_gun", "Demande à Marcel s'il porte encore une arme.",
                                           "Pregúntale a Marcel si todavía lleva arma.",
                                           "Ask Marcel if he still carries a gun.",
                                            "Chiedi a Marcel se porta ancora la pistola.",
                                            "Pregunta-li a en Marcel si encara porta arma."),
                new LevelHint("m_lie_time","Demande à Marcel à quelle heure il est parti.",
                                           "Pregúntale a Marcel a qué hora se fue.",
                                           "Ask Marcel what time he left.",
                                            "Chiedi a Marcel a che ora è andato via.",
                                            "Pregunta-li a en Marcel a quina hora va marxar."),
                new LevelHint("m_broken",  "Marcel a dit deux choses fausses. Mets-lui les deux devant, pas une.",
                                           "Marcel ha dicho dos cosas falsas. Ponle las dos delante, no una.",
                                           "Marcel said two false things. Put both to him, not one.",
                                            "Marcel ha detto due cose false. Mettigliele davanti tutte e due, non una.",
                                            "En Marcel ha dit dues coses falses. Posa-li les dues al davant, no una."),
                new LevelHint("f_name",    "Le commissaire attend à sa table. Va lui dire qui c'était.",
                                           "El comisario espera en su mesa. Ve a decirle quién fue.",
                                           "The inspector is waiting at his table. Go and tell him who it was.",
                                            "Il commissario aspetta al suo tavolo. Vai a dirgli chi è stato.",
                                            "El comissari espera a la seva taula. Vés a dir-li qui va ser."),
                new LevelHint("f_lies",    "Un nom ne suffit pas. Raconte-lui les deux mensonges.",
                                           "Un nombre no basta. Cuéntale las dos mentiras.",
                                           "A name is not enough. Tell him both lies.",
                                            "Un nome non basta. Raccontagli tutte e due le bugie.",
                                            "Un nom no n'hi ha prou. Explica-li les dues mentides."),
                new LevelHint("f_motive",  "Dis-lui aussi que Marcel demandait par quelle porte Marius sortait.",
                                           "Dile además que Marcel preguntaba por qué puerta salía Marius.",
                                           "Tell him too that Marcel was asking which door Marius used.",
                                            "Digli anche che Marcel chiedeva da quale porta usciva Marius."),
            },
            // Un solo final. Cierra las tres cosas que el caso deja abiertas:
            // qué pasa con el culpable, qué pasa con Corentin —que es quien
            // carga con la ironía— y el violín, que es con lo que empezó todo.
            // No menciona los visados: eso solo se sabe si Marcel lo confiesa,
            // y quien lo consiguió ya lo ha oído de su boca, que es donde más
            // duele. El que investiga mejor no se lleva otro final, se lleva
            // haber estado delante.
            briefTitleEn: "Case 1 — The violinist of the Dragon Vert",
            briefSummaryEn: "You are a POLICE INSPECTOR. This case is yours.\n\nMarius Toussaint, the violinist of the Dragon Vert, was shot dead last night in the alley behind the café. Everyone here liked him.\n\nTwo things don't add up: he played a tune that wasn't his, and he cut his set ten minutes short. Talk to the people in the café and find out who killed him.",
            briefTitleIt: "Caso 1 — Il violinista del Dragon Vert",
            briefSummaryIt: "Sei un ISPETTORE DI POLIZIA. Questo caso è tuo.\n\nMarius Toussaint, il violinista del Dragon Vert, è stato ucciso ieri sera nel vicolo dietro il caffè. Qui gli volevano tutti bene.\n\nDue cose non tornano: ha suonato un pezzo che non era il suo, e ha chiuso il turno dieci minuti prima. Parla con le persone del caffè e scopri chi l'ha ucciso.",
            briefTitleCa: "Cas 1 — El violinista del Dragon Vert",
            briefSummaryCa: "Ets INSPECTOR DE POLICIA. Aquest cas és teu.\n\nEn Marius Toussaint, el violinista del Dragon Vert, va ser mort d'un tret ahir a la nit al carreró de darrere el cafè. Aquí tothom l'estimava.\n\nDues coses no lliguen: va tocar una peça que no era la seva, i va plegar deu minuts abans. Parla amb la gent del cafè i descobreix qui el va matar.",
            epilogueCa:
                "En Marcel Charrier va ser detingut aquell mateix vespre. No es va resistir.\n\n" +
                "En Corentin va saber que el seu senyal — el que havia de salvar en Marius — és el que el va enviar al carreró deu minuts massa aviat. El cafè va estar tancat tres dies.\n\n" +
                "El violí encara és a la barra. Ningú l'ha volgut moure.",
            epilogueIt:
                "Marcel Charrier è stato arrestato quella sera stessa. Non ha opposto resistenza.\n\n" +
                "Corentin ha saputo che il suo segnale — quello che doveva salvare Marius — è ciò che l'ha mandato nel vicolo dieci minuti troppo presto. Il caffè è rimasto chiuso tre giorni.\n\n" +
                "Il violino è ancora sul bancone. Nessuno ha voluto spostarlo.",
            epilogueEn:
                "Marcel Charrier was arrested that same evening. He did not resist.\n\n" +
                "Corentin learned that his signal — the one meant to save Marius — is what sent him into the alley ten minutes too early. The café stayed shut for three days.\n\n" +
                "The violin is still on the counter. Nobody has wanted to move it.",
            epilogueFr:
                "Marcel Charrier a été arrêté le soir même. Il n'a pas résisté.\n\n" +
                "Corentin a appris que son signal — celui qui devait sauver Marius — est ce qui l'a envoyé dans la ruelle dix minutes trop tôt. Le café est resté fermé trois jours.\n\n" +
                "Le violon, lui, est toujours sur le comptoir. Personne n'a voulu le déplacer.",
            epilogueEs:
                "Marcel Charrier fue detenido esa misma noche. No se resistió.\n\n" +
                "Corentin supo que su señal —la que debía salvar a Marius— fue lo que lo mandó al callejón diez minutos antes de tiempo. El café estuvo tres días cerrado.\n\n" +
                "El violín sigue en la barra. Nadie ha querido moverlo."
        ),
    };

    public static LevelDefinition Get(int id)
    {
        foreach (var lvl in All)
            if (lvl.id == id) return lvl;
        return null;
    }
}
