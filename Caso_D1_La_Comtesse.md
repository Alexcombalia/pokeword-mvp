# CASO DÉBUTANT 1 — « Le collier de la comtesse »

**Versión:** 1.0 — 18/08/2026
**Origen:** relato del guionista, reescrito con los tres arreglos acordados (ver §14).
**Estado:** programado y probado contra Gemini el 18/08/2026. Falta el arte de los tres personajes.

Es **el primer nivel del juego**: enseña a moverse, a hablar, a capturar palabras y a investigar, a alguien que además no habla francés. Es el caso más simple de los diez y el más difícil de diseñar.

---

## 1. Ficha

| | |
|---|---|
| **Nivel** | id 1 — sustituye a *Le bateau de Jacques* |
| **Escena** | Una sola: `SalonCondesa` |
| **NPCs** | 3: Nadia, el doctor Lemaire y Piotr |
| **Duración** | ~12-15 turnos. En débutant cada turno pasa por la lectura en voz alta, así que en tiempo real son 8-12 min |
| **Crimen** | Homicidio involuntario + robo. **Nunca se muestra nada**: no hay cadáver, ni sangre, ni violencia |
| **★ Résolu** | Nadia acepta la explicación |
| **★ Mots** | Capturar las 6 palabras objetivo (en débutant la 2ª estrella es de vocabulario, no de careo) |
| **★ Chronomètre** | Por debajo de 20 minutos |

---

## 2. El brief

> **Marsella, 1941.**
> La condesa **Irina Volkova**, rusa exiliada, apareció muerta anoche en su salón. El médico firmó: **ataque al corazón**.
> Su doncella no se lo cree, y te ha llamado. Es lo que tiene: no le queda dinero, pero no quiere que esto acabe así.
> Esa misma noche desapareció el **collar de diamantes** de la condesa. La única joya de valor que le quedaba.

---

## 3. La verdad del caso

**Piotr Sokolov**, joyero del mercado negro, visitó a la condesa esa noche. Le llevó **una botella de coñac de regalo** — con algo dentro para dormirla profundamente y poder llevarse el collar sin ruido.

El coñac importa: en la Francia ocupada de 1941 no se encuentra. Hay achicoria en vez de café y sucedáneos en vez de licor. Una botella de coñac auténtico solo puede venir del mercado negro — y Piotr es del mercado negro.

No quería matarla. Pero la condesa era vieja y tenía el corazón débil, y la dosis fue demasiado. Murió dormida. Piotr cogió el collar y se fue.

El **doctor Lemaire** llegó por la mañana con prisa, antes del toque de queda, vio a una mujer mayor muerta en su butaca y escribió lo más probable: el corazón. No miró más.

**Piotr ha vuelto al salón esta mañana**, muy cordial, a ofrecerse para "ayudar con las cosas de la señora". Un joyero rondando las pertenencias de una muerta. Es lo que lo pone al alcance del jugador, y es lo que lo pierde.

### Cronología

| Hora | Qué pasó | Quién lo sabe |
|---|---|---|
| ~21:00 | Piotr visita a la condesa y le deja una botella de regalo | Nadia vio a un hombre de espaldas, sin verle la cara |
| 22:00 | Nadia le lleva su té. La condesa está bien | Nadia |
| ~22:30 | La condesa se sirve de la botella y se duerme | Nadie |
| Noche | Piotr vuelve a por el collar. La condesa no despierta | Piotr |
| Mañana | El doctor firma "corazón" sin examinarla | Lemaire |

---

## 4. La cadena de deducción

Cuatro pasos, uno por conversación. Ningún NPC señala a nadie: **Nadia nunca dice que Piotr mienta**, solo cuenta lo que vio.

1. **Nadia** dice que esa noche vino un hombre con una botella de regalo, pero **no le vio la cara**. No sabe quién era, y lo dice sin reservas: es ella quien ha llamado al detective, así que no se guarda nada.
2. **El doctor** admite que no la examinó, y añade lo único que sí sabe: **no había ninguna herida**. Si no fue el corazón, fue algo que bebió o comió.
3. **← AQUÍ ESTÁ EL CASO.** Botella + "algo que bebió" = la bebida la mató. Son dos frases de dos personas que no han hablado entre sí.
4. **Piotr** se presenta como joyero y dice que hace semanas que no ve a la condesa. El jugador, ya con un sospechoso, **vuelve a preguntarle a Nadia cómo era el hombre**: no le vio la cara, pero **llevaba un maletín de joyero**.
5. Nadia dice además que **la botella sigue ahí, y que es coñac de verdad** — algo que no se ve desde que empezó la guerra.
6. Preguntado por lo que vende, **Piotr presume**: *lo encuentro todo, café, coñac, lo que quiera*. Es su argumento de venta, y es lo que lo condena sin que él se dé cuenta.
7. **← AQUÍ ESTÁ EL CASO.** El jugador tiene dos cosas distintas, y necesita las dos: que **estuvo allí** (vieron a un joyero) y que **la bebida es suya** (coñac de mercado negro, y él consigue esas cosas).
8. Con las dos encima, **Piotr confiesa**.
9. El jugador se lo cuenta a **Nadia**, que es quien le contrató.

**Regla de oro:** igual que en el modo avanzado, el jugador nunca recibe una conclusión. La diferencia con un caso avanzado es la cantidad de piezas —cinco en vez de nueve— y que con **una sola** contradicción basta para romper al culpable.

---

## 5. Expediente por NPC

### 5.1 NADIA PETROVA, 24 — la doncella. **La clienta.**
Sirvió a la condesa nueve años. Es quien ha llamado al detective y quien paga, con lo poco que tiene. Honesta, cálida, directa. No sospecha de nadie: solo sabe que su señora estaba bien a las diez y que el médico no miró nada.

| Qué sabe | Condición |
|---|---|
| **[N1]** La condesa estaba bien a las diez. Le llevó su té de siempre | Abierto |
| **[N2]** Esa noche vino un hombre con una botella de regalo. **No le vio la cara** — lo vio de espaldas, en el pasillo | Abierto |
| **[N3]** El collar de diamantes ha desaparecido. Era lo único de valor que le quedaba | Abierto |
| **[N3b]** **La botella sigue en el salón, y es coñac de verdad** — no se ve desde la guerra. Para ella es solo una rareza, no una pista | Se abre si preguntan por la botella o por lo que bebió |
| **[N4]** Lo único que sí vio: **llevaba un maletín de joyero**. Para ella no significa nada, en el mercado hay joyeros | **Solo si le preguntan cómo era el hombre o qué llevaba** |
| **[N5]** No se cree lo del corazón. Por eso ha llamado | Abierto |

**Nunca hace:** decir que alguien miente · acusar · adivinar quién era el hombre · sacar conclusiones.

**Por qué no sabe quién fue, y por qué importa:** una testigo que hubiera reconocido al culpable lo diría en su primera frase — ella es quien paga para que se sepa. Que se lo callara solo tendría sentido si escondiera algo, y no esconde nada. Su desconocimiento es lo que hace honesto el caso.

**Es también quien cierra el caso** (§6).

---

### 5.2 DOCTOR LEMAIRE, 55 — el médico
Firmó el certificado. No es mala persona: es un hombre cansado que tenía prisa por llegar a casa antes del toque de queda. Le da vergüenza admitirlo, pero lo admite si se le pregunta de frente.

| Qué sabe | Condición |
|---|---|
| **[L1]** Escribió "el corazón" porque era una mujer mayor. Es lo normal | Abierto |
| **[L2]** **No la examinó bien.** Tenía prisa, el toque de queda | Se abre si el jugador le pregunta si la examinó, o cómo lo sabe |
| **[L3]** **No había ninguna herida, ninguna.** Si no fue el corazón, fue algo que bebió o comió | Se abre con [L2] |

**Nunca hace:** decir que fue un asesinato · nombrar a nadie · hablar del cuerpo con detalle.

**Su función es dar la mitad que Nadia completa.** Sin [L3] este caso no tiene deducción, solo un ladrón mentiroso.

---

### 5.3 PIOTR SOKOLOV, 48 — joyero del mercado negro. **EL CULPABLE**
Cordial, servicial, encantado de ayudar. Ha vuelto esta mañana con la excusa de echar una mano con las cosas de la señora. Está tranquilo: nadie sospecha de una muerte natural.

| Qué dice | Verdad |
|---|---|
| **[P1] LA MENTIRA** — no ve a la condesa desde hace semanas, y no ha visto el collar | Falso. Estuvo allí esa noche y tiene el collar escondido |
| **[P2]** Compra y vende joyas, y no pregunta de dónde vienen | Verdad, lo admite sin problema |
| **[P3] CONFESIÓN** — estuvo allí; la botella era para dormirla y llevarse el collar; no quería matarla; no despertó | **Solo cuando el jugador le dice que VIERON a un joyero allí esa noche** |

**Está convencido de que nadie lo vio.** Entró y salió de noche y la doncella estaba en la cocina. Esa certeza es lo que le da valor para sostener la mentira.

**Y es vanidoso.** Si le preguntan qué vende, presume de conseguirlo todo: café, coñac, lo que le pidan. Cree que eso lo hace útil y lo pone por encima de toda sospecha. Es exactamente lo que lo pierde, y nunca se da cuenta.

**Cómo se rompe — hacen falta DOS cosas, y ninguna basta sola:**

| Pieza | Qué se le pone delante | Qué contesta si va sola |
|---|---|---|
| **A — estuvo allí** | Vieron a un joyero aquí esa noche, con su maletín | *« Un bijoutier ? Il y en a beaucoup au marché. »* |
| **B — la bebida es suya** | La botella es coñac de verdad, de mercado negro, y él consigue esas cosas | *« Du cognac ? J'en vends à tout le monde. »* |

**Las dos respuestas son ciertas, y por eso resisten.** Hay muchos joyeros en el mercado y él vende coñac a media Marsella. Ninguna de las dos piezas lo señala por separado. Es el cruce de ambas lo que lo deja sin suelo: alguien vio a un joyero aquí, y la botella que la mató solo la puede conseguir él.

Con una sola pieza aguanta indefinidamente, y ser acusado a gritos no le hace nada.

**Su última frase es una defensa, no una disculpa:** *"Yo no soy un asesino. Soy un ladrón."*

**Nunca hace:** confesar sin que le pongan delante que estuvo allí · negar ser joyero · amenazar · inventar cómplices.

---

## 6. El cierre — se lo cuentas a Nadia

No hay comisario en este caso. El detective **informa a su clienta**, que es quien le contrató. Es más simple, no necesita un cuarto NPC, y enseña el bucle de "explicar el caso" con la persona más amable de la sala.

Nadia valida tres cosas, y cada una cabe en una frase corta:

1. **`f_name`** — fue **Piotr**
2. **`f_drink`** — la **bebida** que le trajo es lo que la mató
3. **`f_necklace`** — se llevó el **collar**

**Comportamiento:**
- **No acepta un nombre suelto.** *« Piotr ? Pourquoi lui ? »*
- **Equivocarse no pierde el caso.** Dice que eso no encaja y el jugador sigue investigando.
- **No corrige el francés.** Si entiende, acepta. Innegociable.
- Ella ya sabía que vino un hombre; lo que **no** sabía es que fuera un ladrón ni que la bebida la matara. Por eso hay algo que contarle.

---

## 7. Las frases fijas (francés A1-A2 + español)

Francés de principiante: frases de cinco o seis palabras, presente y passé composé, sin subordinadas. Van ya etiquetadas con `[francés|español]`.

### Nadia — cálida, directa, agradecida

**1. Entrada (al abrir la conversación)**
> « Merci d'être venu. Le [docteur|médico] dit : le cœur. Moi, je ne le crois pas. »
> *Gracias por venir. El médico dice: el corazón. Yo no me lo creo.*

**2. [N1] La condesa estaba bien**
> « À dix heures, madame allait bien. Je lui ai apporté son thé. »
> *A las diez, la señora estaba bien. Le llevé su té.*

**3. [N2] La visita — sin nombre**
> « Ce soir-là, un homme est venu. Il a apporté une [bouteille|botella], un cadeau. Je n'ai pas vu son visage. »
> *Esa noche vino un hombre. Trajo una botella, un regalo. No le vi la cara.*

**4. [N3] El collar**
> « Le [collier|collar] de madame a disparu. C'était tout ce qu'elle avait. »
> *El collar de la señora ha desaparecido. Era todo lo que tenía.*

**5. [N4] Cómo era — solo si se pregunta**
> « Je n'ai pas vu son visage. Mais il portait une mallette de bijoutier. »
> *No le vi la cara. Pero llevaba un maletín de joyero.*

**5c. [N3b] La botella**
> « La [bouteille|botella] est encore là. C'est du vrai cognac. On n'en voit plus depuis la guerre. »
> *La botella sigue ahí. Es coñac de verdad. No se ve desde la guerra.*

**5b. Si le preguntan a bocajarro si fue Piotr**
> « Je ne sais pas, monsieur. Je n'ai pas vu son visage. »
> *No lo sé, señor. No le vi la cara.*

### Doctor Lemaire — cansado, un poco avergonzado

**6. [L1] La excusa**
> « J'ai écrit : le cœur. Elle était vieille. C'est normal. »
> *Escribí: el corazón. Era mayor. Es lo normal.*

**7. [L2] La admisión**
> « Je n'ai pas bien regardé. J'étais pressé. Le couvre-feu. »
> *No miré bien. Tenía prisa. El toque de queda.*

**8. [L3] La pista** ← la frase que sostiene el caso
> « Il n'y avait pas de blessure. Aucune. Si ce n'est pas le cœur, c'est quelque chose qu'elle a bu. »
> *No había ninguna herida. Ninguna. Si no fue el corazón, es algo que bebió.*

### Piotr — cordial, luego seco

**8b. [P2] El alarde** ← lo que lo condena sin que él lo sepa
> « Je [trouve|encuentro] tout, monsieur. Du café, du cognac, ce que vous voulez. »
> *Lo encuentro todo, señor. Café, coñac, lo que quiera.*

**9. [P1] La mentira**
> « Le [collier|collar] ? Je ne l'ai pas [vu|visto]. Je n'ai pas vu la comtesse depuis des semaines. »
> *¿El collar? No lo he visto. No veo a la condesa desde hace semanas.*

**10. [P3] La confesión**
> « C'est vrai. J'étais là cette [nuit|noche]. La bouteille, c'était pour la faire dormir. Je voulais le collier, rien d'autre. Elle ne s'est pas réveillée. »
> *Es verdad. Estuve allí esa noche. La botella era para dormirla. Quería el collar, nada más. No se despertó.*

**11. Su última frase**
> « Je ne suis pas un assassin. Je suis un voleur. »
> *No soy un asesino. Soy un ladrón.*

### Líneas de apoyo

| Cuándo | Línea |
|---|---|
| Piotr se presenta | « Piotr. Bijoutier. Je viens aider avec les affaires de madame. » |
| Piotr si le acusan sin ninguna de las dos piezas | « Vous vous trompez. Je n'étais pas là. » |
| Piotr con SOLO la pieza A (lo vieron) | « Un bijoutier ? Il y en a beaucoup au marché. » |
| Piotr con SOLO la pieza B (el coñac) | « Du cognac ? J'en vends à tout le monde. » |
| Nadia ante un nombre suelto | « Piotr ? Pourquoi lui ? » |
| Nadia cuando acepta | « Alors ce n'était pas son cœur. Merci, monsieur. » |
| Cualquiera, si le preguntan quién fue | « Je ne sais pas. Ce n'est pas à moi de le dire. » |

---

## 8. Reglas de prompt

Las mismas nueve del modo detective (ver `GDD_Modo_Detective.md` §2.3), con **dos añadidos propios de débutant**:

1. **Francés de principiante SIEMPRE.** Frases de cinco o seis palabras. Presente y passé composé. Nada de subordinadas, nada de argot. Si el jugador escribe en español, se le responde igual en francés simple, sin reproche.
2. **Paciencia infinita con la repetición.** Este es el primer nivel del juego y el jugador puede no entender nada las primeras veces. Repetir, reformular más corto, y nunca dar señales de impaciencia.

Y la de siempre, que aquí importa más que nunca: **los checkpoints se validan por contenido, no por gramática.**

---

## 9. Objetivos y hallazgos

**Objetivos (blanco, 4). Ninguno nombra al culpable:**
- ☐ Escucha a Nadia: qué pasó esa noche → `n_visitor`
- ☐ Pregunta al doctor de qué murió → `l_drink`
- ☐ Descubre quién ha mentido → `p_lie`
- ☐ Cuéntale a Nadia lo que pasó → `f_name` + `f_drink` + `f_necklace`

**Hallazgos (verde, se escriben solos):**

| Bandera | Línea |
|---|---|
| `n_visitor` | Esa noche vino un hombre con una botella. Nadia no le vio la cara |
| `n_necklace` | El collar de diamantes ha desaparecido |
| `l_drink` | No había herida. Si no fue el corazón, fue algo que bebió |
| `n_case` | Nadia no le vio la cara, pero llevaba un maletín de joyero |
| `p_lie` | Piotr dice que no ve a la condesa desde hace semanas |

Cinco hallazgos, la mitad que en el caso avanzado. Puestos en ese orden, `n_case` (un joyero estuvo aquí) queda justo encima de `p_lie` (Piotr dice que no vino), y la contradicción se ve sola — que es justo lo que un principiante necesita, porque no puede sostener dos frases en francés en la cabeza a la vez.

---

## 10. Banderas y checkpoints

| id | Se marca cuando |
|---|---|
| `n_visitor` | Nadia cuenta lo del hombre y la botella, y que no le vio la cara |
| `n_necklace` | Nadia cuenta que falta el collar |
| `n_case` | Nadia describe el maletín de joyero (solo si le preguntan cómo era) |
| `n_bottle` | Nadia cuenta que la botella sigue ahí y es coñac de verdad |
| `l_hurry` | El doctor admite que no la examinó |
| `l_drink` | El doctor dice lo de la herida y la bebida |
| `p_lie` | Piotr dice que hace semanas que no la ve |
| `p_market` | Piotr presume de conseguirlo todo: café, coñac, lo que le pidan |
| `p_seen` | **Interna.** El jugador le dice que vieron a un joyero allí esa noche |
| `p_bottle` | **Interna.** El jugador le pone la bebida encima: el coñac es de mercado negro y él lo consigue |
| `p_broken` | Piotr confiesa. **Puerta dura: requiere `p_seen` Y `p_bottle`** |
| `f_name`, `f_drink`, `f_necklace` | Validaciones de Nadia |

`checkpoints` del nivel (lo que lo completa) = `f_name`, `f_drink`, `f_necklace`.
`secondStarCheckpoint` = **null** → la 2ª estrella es de palabras.

---

## 11. Trabajo en Unity

**Escena: `SalonCondesa`**, creada el 18/08/2026 y ya en Build Settings. Añadida a `interiorScenes` de `GameBootstrap`. Los tres NPCs van repartidos: Nadia junto al aparador del té, el doctor cerca de la puerta, Piotr al lado de la mesita de la botella.

**Scripts nuevos**, mismo patrón que el caso avanzado:

| Personaje | Scripts | Endpoint |
|---|---|---|
| Nadia | `NadiaNPC` + `NadiaChatClient` | `/npc/nadia` |
| Lemaire | `LemaireNPC` + `LemaireChatClient` | `/npc/lemaire` |
| Piotr | `PiotrNPC` + `PiotrChatClient` | `/npc/piotr` |

**Voces TTS:** Nadia joven y cálida; Lemaire mayor y apurado; Piotr meloso, nada amenazante.

**Este nivel carga el tutorial.** `GuidedTutorial` se dispara la primera vez y enseña moverse, el cuaderno de palabras y el micro. Sus textos ya valen tal cual — son mecánicos y no mencionan la premisa vieja.

---

## 12. Vocabulario objetivo

`collier` (collar) · `bouteille` (botella) · `nuit` (noche) · `docteur` (médico) · `mentir` (mentir) · `voir` (ver)

Seis palabras, todas presentes en las frases fijas, así que salen sí o sí si el jugador habla con los tres.

---

## 13. Fallos previsibles

| Riesgo | Corte |
|---|---|
| Nadia adivina quién era el hombre | No puede: no le vio la cara. Si le preguntan a bocajarro si fue Piotr, responde que no lo sabe |
| Piotr confiesa demasiado pronto | Sin `p_seen` **y** `p_bottle`, no confiesa jamás. Con una sola, su defensa es cierta y aguanta |
| El jugador no llega a la botella | La pista de `n_bottle` le manda a preguntarle a Nadia por ella |
| El doctor no llega a decir lo de la bebida | Es la frase que sostiene el caso: se abre en cuanto admite que no miró bien |
| El jugador no entiende nada y abandona | Traducción al español de cada réplica, guía fonética, pistas, y ningún límite de tiempo |
| El jugador no sabe que hay que volver a hablar con Nadia | Es lo que cubre la pista 4 |

---

## 14. Qué cambió respecto al relato del guionista

1. **Nadia ya no sabe quién fue.** En el original decía literalmente *"eso no es verdad, Piotr estuvo aquí"* — resolvía el caso por el jugador. En una versión intermedia sabía el nombre pero se lo callaba hasta que se lo preguntaban, lo cual era peor: una testigo que ha contratado a un detective para que se sepa la verdad **diría el nombre en su primera frase**. Callárselo solo tendría sentido si escondiera algo, y no esconde nada. Ahora **vio a un hombre de espaldas y no le vio la cara**; lo único que retuvo es el maletín de joyero. La identificación la hace el jugador.
2. **El doctor ha dejado de ser un callejón sin salida.** En el original solo justificaba el certificado. Ahora aporta la frase que ata la muerte a la bebida, sin la cual no hay caso, solo un ladrón.
3. **Piotr confiesa, pero cuesta.** En el original el giro afirmaba que la bebida la mató sin que nada de lo que el jugador reunía lo demostrara. Ahora hay que probarle **dos** cosas —que estuvo allí y que la bebida es suya—, y cada una por separado tiene una respuesta verdadera que la desmonta. El coñac es la pieza nueva: en 1941 no se encuentra, y él presume de ser quien encuentra lo que no se encuentra.
4. **Nadia contrata al detective**, lo que explica por qué tres desconocidos le contestan.
5. **Piotr está en el salón** esta mañana, con la excusa de ayudar con las cosas de la señora — así el caso cabe en una sola localización.

---

## 15. Epílogo (uno solo)

> El collar apareció esa misma tarde, envuelto en un trapo, en el fondo del taller de Piotr.
>
> Nadia no quiso quedárselo. Dijo que era de la señora, y que la señora ya no estaba.
>
> El doctor Lemaire pidió que se revisara el certificado. Fue lo único que pidió en toda su vida.

---

## 16. Pendiente

- **La escena y su nombre**, y el fondo del salón.
- **Los tres sprites**: Nadia, Lemaire, Piotr.
- El retrato de la condesa, que cuelga en la pared y enseña el collar que falta.
- Qué hacer con `IntroVideo`, que sigue siendo la llegada al puerto de un juego que ya no existe.
