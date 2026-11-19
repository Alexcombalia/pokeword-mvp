# Lingua Noir — GDD del Modo Detective (modo avanzado)

**Versión:** 1.0 — 11/08/2026
**Estado:** diseño cerrado a falta de las decisiones abiertas del final. **Nada de esto está programado todavía.**
**Regla de trabajo:** este documento se cierra ANTES de tocar código. Gemini no improvisa nada que no esté escrito aquí.

---

## 1. Qué es el modo detective

El modo avanzado de Lingua Noir deja de ser "una campaña más difícil" y pasa a ser un **juego de misterio**. El jugador es **inspector de policía** en la Marsella de 1941 y resuelve 5 casos que le van cayendo por turno — nadie le contrata y nadie le paga, así que ningún caso depende de que haya un cliente que lo encargue. Toda la mecánica es **hablar y escuchar a NPCs**: no hay combate, ni inventario, ni minijuegos.

La pieza nueva de diseño: **algunos NPCs mienten**. El jugador tiene que averiguar qué es verdad, quién miente y por qué. Se resuelve un caso cruzando lo que dice uno con lo que dice otro — es decir, **hablando más**.

### Público objetivo
Hombres y mujeres de 15-35 años que **ya hablan francés** pero **les da vergüenza hablar en voz alta** y quieren mejorar su speaking.

Todo el diseño se somete a esto. Consecuencias directas:
- **No se penaliza el silencio ni la lentitud.** Nada te expulsa de un nivel por tardar.
- **El objetivo del jugador nunca es "practicar francés"; es "saber quién fue".** Hablar es la herramienta, no el ejercicio. Quien no se atreve a hablar por hablar, sí se atreve a preguntar cuando necesita algo.
- **Se puede repreguntar siempre.** En modo detective no hay mecánica de "escuchar sin repetición" (existe en débutant y ahí se queda): un NPC repite, reformula y aclara todas las veces que haga falta. Confundirse no cuesta nada.

### Alcance
**5 casos. Ni uno más hasta terminarlos.** Si funcionan, se hacen más. Duración objetivo: **5-15 min por caso**. Cada caso usa **varias escenas y varios NPCs**, todos ya existentes en el juego (ver §4).

---

## 2. Reglas del sistema (comunes a los 5 casos)

### 2.1 Tiempo — cronómetro, no cuenta atrás
- El nivel muestra un **cronómetro que sube** ("llevas 6:42 investigando"), nunca una cuenta atrás.
- **Ningún caso se pierde por tiempo.** No hay límite duro. `timeLimitSeconds` deja de usarse como fallo en modo detective.
- El tiempo **sigue contando para estrellas**, pero con umbrales muy holgados: la estrella de tiempo se gana **al doble del tiempo previsto de resolución**. Un jugador que va con calma la consigue igual; solo la pierde quien se atasca de verdad.
- El cronómetro **corre también al cambiar de escena** (ya funciona así en débutant).
- El tiempo es lo que se compara en el fantasma/ranking con amigos, sin que eso cueste estrellas a nadie.

### 2.2 Estrellas (3 por caso, independientes y visibles en el brief)
| Estrella | Condición |
|---|---|
| ★ **Résolu** | Explicar al comisario lo que pasó y que lo dé por bueno. Es la estrella obligatoria: sin ella el caso no está superado. |
| ★ **Le menteur** | Haber pillado la mentira: confrontar al mentiroso con la contradicción **dentro de la investigación**, antes de ir al comisario. Se puede resolver un caso sin hacerlo (deduciendo), pero esta estrella exige el careo. |
| ★ **Chronomètre** | Terminar por debajo del umbral holgado del caso. |

**Botón de pista:** disponible en los 5 casos, cuesta **1 estrella** de la partida actual — una sola vez, por muchas pistas que se pidan. Da la siguiente pregunta útil, nunca la solución. Cada caso define su lista de pistas en orden de investigación y se muestra la primera cuyo hallazgo aún falte, así siempre habla de lo siguiente que toca.

Dos reglas que salieron al construirlo (14/08/2026):
- **Si la pista resuelve el caso, deja de ser una válvula de escape y pasa a ser el camino corto.** *"A Odette se le escapan cosas de los clientes, pregúntale qué llevaban encima"* sí; *"fue Marcel"* no.
- **Pulsarlo cuando ya no queda nada que pistar no cuesta estrella.** Sería cobrarle al jugador por comprobar que va bien.

> ⚠️ Este botón **no existía** hasta el 14/08/2026: `hintAvailable` llevaba meses en `LevelDefinition` y `LevelManager.UseHint()` no lo llamaba nadie.

### 2.3 El expediente del caso — la regla técnica más importante
Cada caso define **por escrito, aquí**, para cada NPC implicado:
- **Lo que sabe de verdad** (hechos que puede contar).
- **Lo que oculta o miente**, y **por qué** (un mentiroso sin motivo se cae a la primera).
- Su **frase clave guionizada**: una línea fija que el NPC dice literalmente cuando se toca ese tema. El resto de la conversación es libre con Gemini, pero **esa frase no cambia nunca** (mismo patrón híbrido que ya usa Jacques).
- **Qué rompe su mentira**: la información concreta que, si el jugador se la pone delante, le obliga a rectificar.
- **Qué checkpoint reporta al servidor** cuando suelta el dato.

Regla dura para los prompts: **el NPC no puede inventar hechos nuevos sobre el caso.** Si el jugador pregunta algo que no está en su expediente, contesta que no lo sabe, que no estaba allí, o desvía hacia su propia vida — nunca improvisa una pista. Esto es lo que hace que el caso sea resoluble el lunes y el jueves igual.

**Segunda regla dura, igual de importante: los checkpoints se validan por CONTENIDO, no por gramática.** Si el jugador dice *« Odette t'a vu. Tu mens. »* —cuatro palabras, sin estilo indirecto ni concordancia— la contradicción **cuenta**. Vale cualquier formulación en la que se entienda que le está poniendo delante el dato contrario: agramatical, con una palabra en español, o telegráfica. Ningún NPC exige que se lo digan bien dicho ni corrige el francés del jugador salvo que se lo pidan.

*Motivo:* confrontar a alguien con una contradicción (estilo indirecto + pasado + contraste) es de las cosas más difíciles de decir en un idioma extranjero, y es la acción obligatoria de todos los casos. El público objetivo son personas a las que **les da vergüenza hablar**. Si el careo exige buen francés, cada caso se atasca justo en su momento clave. El techo de dificultad de un caso tiene que estar en la deducción, nunca en la gramática.

### 2.4 La resolución — el comisario (sin UI nueva)
No hay pantalla de acusación. Al final de cada caso el jugador va a ver al **comisario Fabre** y **le explica en francés lo que cree que pasó**. El comisario es un NPC normal, con su objetivo por checkpoints, que valida tres cosas:

1. **Quién fue** (`accuse_right`)
2. **Qué mentira se descubrió** (`lie_named`)
3. **La prueba o el detalle que lo sostiene** (`proof_named`)

Comportamiento del comisario:
- **No acepta un nombre suelto.** Si el jugador solo dice "C'est Marcel", pide que se explique: *"Et pourquoi lui ? Qu'est-ce qui vous fait dire ça ?"*
- **Si la acusación es falsa, no se pierde el caso**: el comisario dice que no le cuadra y **por qué** no le cuadra, y el jugador vuelve a investigar. Equivocarse cuesta tiempo, nada más. Es deliberado: castigar el error con derrota es exactamente lo que calla a un jugador tímido.
- **No corrige el francés.** Si entiende, acepta. Un francés torpe pero comprensible vale igual que uno perfecto — esta regla es innegociable para el público objetivo.
- Es **el mismo personaje en los 5 casos**, así el jugador aprende a hablar con él y el momento final gana peso.

**Ubicación:** escena **Oficina Portuaria**, hoy vacía, revestida como *Bureau du commissaire*. Coste cero de arte nuevo. **Excepción: en el caso 1 Fabre está dentro del propio bar**, sentado a una mesa junto a la puerta, terminándose el café — porque ese caso es de escena única. Es el mismo personaje y el mismo script.

### 2.5 El cuaderno y el panel de hallazgos
Se reutiliza el cuaderno de palabras existente (`PokeWorldController`). **Sin cuaderno de pistas nuevo.**

En su lugar, el panel de objetivos de arriba (`LevelHUD`) gana una **zona de hallazgos**: cada vez que un NPC suelta un dato clave, se escribe sola una línea nueva **en verde**, con una animación de entrada. El jugador no toma notas; es un registro automático de lo que ya ha oído.

- Las líneas de hallazgo son **hechos literales**, nunca conclusiones: *"Marcel llevaba algo metálico bajo el abrigo"*, no *"Marcel podría ser el asesino"*.
- El panel **nunca** marca una mentira, ni señala sospechosos, ni sugiere el siguiente paso (eso es el botón de pista).
- **Por qué:** en un juego de idiomas el esfuerzo de comprensión se come el de deducción. Un jugador peleando por entender lo que oye no tiene atención libre para notar que dos frases encajan. El panel verde le confirma que ha entendido bien y deja el trabajo mental donde debe estar: en relacionar. Poner dos hechos contradictorios uno debajo del otro hace visible la contradicción sin nombrarla.

### 2.6 Escala de dificultad de los 5 casos
| Caso | Escenas | NPCs | Mentirosos | Mentira |
|---|---|---|---|---|
| 1 | **1** | 4 | 1 | Dos mentiras del mismo NPC; con una sola sostiene |
| 2 | 3 | 4 | 1 + 1 testigo equivocado de buena fe | Hay que distinguir mentir de confundirse |
| 3 | 3 | 4 | 2 | La mentira protege a alguien; el caso no tiene culpable |
| 4 | 3 | 4 | 1 con coartada construida | Hay que romper una coartada, no una frase |
| 5 | 4-5 | 6 | 2 en cadena | Repaso: reaparecen personajes de los casos 1-4 |

---

## 3. Arco de fondo (ligero, nunca obligatorio)

Marsella, 1941. El detective llega buscando trabajo y acaba encadenando casos pequeños del puerto. Entre el caso 2 y el 5 se va dibujando que varios hilos sueltos (una caja desaparecida, un barco que zarpa de noche, gente que calla) apuntan a **la villa de la colina** — el caso 5.

**Regla:** el arco se cuenta solo con frases sueltas de NPCs. **No hay cinemáticas, ni misiones de historia, ni personajes que seguir.** Cada caso se entiende y se disfruta suelto. Esto es una cicatriz del rediseño anterior: la historia ya hundió el proyecto una vez.

---

## 4. Elenco y escenas disponibles (todo ya existe en el proyecto)

| Escena | NPCs colocados | Uso en modo detective |
|---|---|---|
| **Puerto** | Jacques (cliente dedicado) | Casos 4, 5 |
| **Muelle** | Raphael (estibador), Antoine (pescador), Albert (capitán) | Casos 4, 5 |
| **Playa** | Rémy (pescador), Sandra (vieja), Francis (cliente bar) | Caso 5 |
| **Plaza** | Andre (niño), Paul (viejo), Adrien (vendedor) | Casos 2, 5 |
| **Auberge** | Baptiste (posadero) | Caso 2 |
| **Bar Dragon Verde** | ~~Armel (barman), Patrick (cliente)~~ → **vaciada**: Odette, Corentin, Marcel y Fabre (4 NPCs nuevos) | **Caso 1** (escena única), 3, 5 |
| **La Canebière** | Camille (mujer elegante), Pierre (policía) | Caso 3 |
| **Calle St Jean** | Ivan (policía), Andre (joven) | Caso 3 |
| **Le Parier** | Louise, Marie (vecinas) | Caso 5 |
| **Oficina Portuaria** | *vacía* → **comisario Fabre** (NPC nuevo) | Casos 2-5 |
| **Villa François** | *vacía* → **M. François** (NPC nuevo) | Caso 5 |
| Mercado, Pescadería, Épicerie | vacías | Sin uso por ahora |

**NPCs nuevos a crear: 6** — Odette, Corentin, Marcel y Fabre (caso 1, en el Bar Dragon Verde) y M. François (caso 5). Fabre se reutiliza en los cinco casos; a partir del caso 2 aparece en la Oficina Portuaria.

**Decisión del 14/08/2026 sobre el Bar Dragon Verde:** se vacía por completo. Armel y Patrick se borran de la escena — el caso 1 empieza de cero ahí para no arrastrar NPCs del modo débutant. Cada personaje del caso 1 lleva **su propio script dedicado**, nada de NPC genérico parametrizado: es más código repetido, pero elimina las confusiones al depurar prompts.

⚠️ **Colisión de nombres:** hay dos NPCs llamados *Andre* (el niño de Plaza y el joven de Calle St Jean). En un juego de coartadas eso es veneno. Propuesta: el de Calle St Jean pasa a llamarse **Émile**.

---

## 5. Los 5 casos

---

### CASO 1 — « Le violoniste du Dragon Vert » (El violinista del Dragón Verde)

> ⚠️ **Este caso vive en su propio documento: `Caso_1_Le_Violoniste.md`, que manda sobre este resumen.**
> Sustituye al antiguo caso 1 «Les filets coupés» (Rémy y las redes cortadas), **anulado el 14/08/2026**. El caso de las redes puede recuperarse más adelante para otro nivel; los NPCs del Muelle y la Playa quedan libres.

**Enseña a jugar.** Una sola escena, cuatro NPCs, un culpable con dos mentiras.

- **Escena:** **una sola**, `BarDragonVerde` revestida como el café *Le Dragon Vert*. Sin transiciones.
- **Brief:** Marius Toussaint, violinista del local, apareció muerto de un disparo en el callejón. Dos cosas no cuadran: tocó una pieza que no era la suya y cortó el turno diez minutos antes.
- **Duración:** ~30 turnos ≈ 13-15 min · **★ Chronomètre:** 24 min

**La verdad:** fue **Marcel Charrier**, ex gendarme, que vendía nombres a cambio de un visado para su mujer y sus hijos. Marius lo descubrió y le dio un plazo. La pieza cambiada era una señal de aviso de Corentin — y es lo que sacó a Marius al callejón antes de tiempo.

**Los cuatro NPCs** (todos nuevos, ver expedientes completos en el documento del caso):

| NPC | Papel | Miente |
|---|---|---|
| **Odette Lacroix**, 27, camarera | Lo vio todo y no entendió nada. Da los cuatro hechos en bruto | No |
| **Corentin Vidal**, 50, dueño | Sabe que la pieza era una señal y que alguien preguntaba por Marius. **No sabe quién** | No — calla, no miente |
| **Marcel Charrier**, 44, ex gendarme | **El culpable.** Cordial y colaborador | **Dos mentiras**: el arma y la hora |
| **Comisario Fabre** | Cierra el caso. **En este caso está dentro del bar**, no en su despacho | No |

**La deducción:** Corentin tiene la *importancia* del dato (alguien preguntó por la salida de Marius); Odette tiene el *nombre* y cree que no significa nada. El jugador es el único que tiene las dos mitades. Ningún NPC nombra a un sospechoso.

**★ Le menteur:** romper a Marcel con **las dos** contradicciones. Con una sola, sostiene.

**Vocabulario objetivo:** `violon`, `ruelle`, `coup de feu`, `arme`, `mentir`, `horaire`, `sortir`, `témoin`, `prévenir`, `papiers`.

---

### CASO 2 — «La caisse d'Adrien» (La caja de Adrien)

**Enseña la diferencia entre mentir y equivocarse.** El testigo fiable es un niño al que nadie cree.

- **Escenas:** Plaza → Auberge → Oficina Portuaria
- **Brief:** *"Adrien, le marchand de la place, dit qu'on lui a volé une caisse de café. Il accuse un gamin du quartier."*
- **Tiempo previsto:** 8 min · **★ Chronomètre:** 16 min

**La verdad:** **no hubo robo.** Adrien vendió la caja al mercado negro y denunció un robo falso para cubrirse. Acusa a Andre, el niño, porque nadie le cree a un crío.

**Expediente:**

| NPC | Escena | Sabe de verdad | Miente | Frase clave guionizada | Se rompe con | Checkpoint |
|---|---|---|---|---|---|---|
| **Adrien** (culpable) | Plaza | Que él mismo entregó la caja detrás de la Auberge a las 6 de la mañana. | **Dice que la caja estaba en el puesto al amanecer y que desapareció mientras atendía.** | *« La caisse était là au lever du jour. Je servais un client, et quand je me suis retourné, elle n'était plus là. »* | Que el jugador le cuente que **Baptiste le vio entregar una caja detrás de la Auberge de madrugada**. | `adrien_cracks` |
| **Andre** (niño, acusado) | Plaza | Vio a Adrien cargar la caja **él mismo** en una carreta, temprano. Lo dice a la primera, claro y sin miedo — pero nadie le hace caso porque es un niño. | No | *« Je l'ai vu, moi. C'est lui qui a mis la caisse dans la charrette. Personne ne me croit. »* | — | `kid_testimony` |
| **Paul** (viejo) | Plaza | Cree de verdad que vio a Andre rondando el puesto "esta mañana" — pero era **ayer**. No miente: **se confunde de día**. Si el jugador le pregunta con paciencia por otros detalles (qué tiempo hacía, quién más estaba), se da cuenta solo y rectifica. | No (error de buena fe) | *« Je l'ai vu tourner autour de l'étal ce matin… enfin, je crois que c'était ce matin. »* | Paciencia: preguntarle por detalles hasta que él mismo se corrija. **No se rompe acusándole.** | `paul_corrects` |
| **Baptiste** (posadero) | Auberge | Vio a Adrien entregar una caja a un desconocido detrás de la Auberge de madrugada. No quiere líos: solo lo cuenta si el jugador **le pregunta por Adrien directamente** y le explica que hay un niño acusado injustamente. | No (calla por prudencia) | *« Je ne veux pas d'histoires… mais oui, je l'ai vu donner une caisse à quelqu'un derrière l'auberge, avant le jour. »* | — | `baptiste_talks` |

**★ Le menteur:** obtener `adrien_cracks`.

**Resolución con Fabre:** quién (Adrien, y el niño es inocente) · mentira (no hubo robo, la vendió él) · prueba (Baptiste le vio entregarla; el testimonio de Andre encaja con eso).

**Nota de diseño:** Paul es el corazón del caso — enseña que un testimonio falso no siempre es una mentira. Y obliga al jugador a **hacer preguntas de seguimiento** en vez de una sola pregunta directa, que es exactamente la habilidad de conversación que se quiere entrenar.

**Vocabulario objetivo:** `caisse` (caja), `voler` (robar), `témoin` (testigo), `se tromper` (equivocarse), `croire` (creer), `charrette` (carreta).

---

### CASO 3 — «La femme du Dragon Vert» (La mujer del Dragón Verde)

**Dos personas mienten y ninguna es culpable.** El caso enseña que a veces se calla para proteger.

- **Escenas:** Bar Dragon Verde → La Canebière → Calle St Jean → Oficina Portuaria
- **Brief:** *"Une femme a disparu du Dragon Vert il y a deux nuits. Sa logeuse a prévenu la police. Trouve ce qui lui est arrivé."*
- **Tiempo previsto:** 10 min · **★ Chronomètre:** 20 min

**La verdad:** **no hay delito.** La mujer se marchó por su propia voluntad, huyendo de un marido violento. Camille le dio dinero y le buscó sitio en un tren. Armel y Patrick lo saben a medias y callan para no delatarla.

**Expediente:**

| NPC | Escena | Sabe de verdad | Miente | Frase clave guionizada | Se rompe con | Checkpoint |
|---|---|---|---|---|---|---|
| **Armel** (barman) | Bar Dragon Verde | Ella estuvo allí esa noche, con **una maleta**, y se fue sola por su propio pie. Dice la verdad a medias: admite que estuvo, calla lo de la maleta hasta que le pregunten por qué llevaba equipaje o si parecía asustada. | Por omisión | *« Elle était là, oui. Elle est partie toute seule, personne ne l'a forcée. »* → si insisten: *« Elle avait une valise. Une valise, pour une femme qui rentrait chez elle ? »* | Preguntar por la maleta o por su estado de ánimo. | `suitcase_found` |
| **Patrick** (cliente) | Bar Dragon Verde | Vio a un **hombre** buscándola al día siguiente, gritando. Tiene miedo de ese hombre. | **Dice primero que no vio nada esa noche.** Miente por miedo. | Que el jugador le diga que **Armel ya ha contado que ella estaba allí** — entonces admite lo del hombre. | `husband_revealed` |
| **Camille** (mujer elegante) | La Canebière | Le dio dinero y la ayudó a coger un tren. | **Dice que apenas la conocía.** Miente **para protegerla**, no para salvarse. | **No se rompe con pruebas.** Solo habla si el jugador le dice, en francés, que **no viene a denunciarla** y que sabe lo del marido. La confianza es la llave, no la evidencia. | `camille_trusts` |
| **Ivan** (policía) | Calle St Jean | El expediente oficial: la denuncia la puso la casera, no un familiar. Confirma que **nadie ha denunciado un crimen**, solo una ausencia. | No | *« Personne n'a signalé de crime. Juste une absence. Ce n'est pas la même chose. »* | — | `no_crime` |

**★ Le menteur:** obtener `husband_revealed` (romper el miedo de Patrick con lo que dijo Armel).

**Resolución con Fabre:** el jugador debe convencerle de **cerrar el caso**: no hubo delito · ella se fue por su voluntad · huía de su marido · y **no hace falta decir a dónde fue**. Fabre pregunta expresamente *« Et où est-elle allée ? »*. Si el jugador da el destino, el caso se resuelve igual pero **Camille no vuelve a hablarle en el caso 5** (aparece, pero cortante). Es la única consecuencia persistente del juego y no se anuncia.

**Vocabulario objetivo:** `disparaître` (desaparecer), `valise` (maleta), `avoir peur` (tener miedo), `protéger` (proteger), `mari` (marido), `faire confiance` (confiar).

---

### CASO 4 — «Le bateau de nuit» (El barco de noche)

**Una coartada construida, no una frase suelta.** Hay que desmontarla por partes.

- **Escenas:** Puerto → Muelle → Oficina Portuaria
- **Brief:** *"Un bateau est sorti du port trois nuits de suite, sans être inscrit au registre. Le capitaine Albert jure qu'il dormait."*
- **Tiempo previsto:** 12 min · **★ Chronomètre:** 24 min

**La verdad:** **Albert** sacaba el barco de noche para llevar mercancía a la villa de la colina. Su coartada tiene tres patas y las tres son falsas: dice que dormía en la pensión (Baptiste no le vio), que su barco tenía el motor roto (Raphael lo oyó arrancar), y que Jacques puede confirmarlo (Jacques no confirma nada — no miente por él).

**Expediente:**

| NPC | Escena | Sabe de verdad | Miente | Frase clave guionizada | Se rompe con | Checkpoint |
|---|---|---|---|---|---|---|
| **Albert** (culpable) | Muelle | Que salió tres noches con carga hacia la costa, pagado por alguien de la villa. | **Coartada de 3 patas** (pensión / motor roto / Jacques como testigo). Cede una pata cada vez que le derriban una. | *« Je dormais. Mon moteur est cassé de toute façon, demandez à Jacques. »* | Tres golpes: (1) Baptiste no le vio en la pensión, (2) Raphael oyó el motor, (3) Jacques dice que él no ha confirmado nada. Con **las tres**, confiesa. | `alibi_1`, `alibi_2`, `alibi_3`, `albert_confesses` |
| **Jacques** (mentor) | Puerto | Que Albert le pidió decir que estuvo con él. **Se negó.** Jacques no miente por nadie — es la brújula moral del jugador desde el modo débutant. | No | *« Albert m'a demandé de dire qu'il était avec moi. J'ai refusé. Je ne mens pas pour les gens, même mes amis. »* | — | `alibi_3` |
| **Raphael** (estibador) | Muelle | Oyó **un motor** en el muelle dos noches seguidas, hacia las 2. Sabe distinguir el sonido de ese barco. | No | *« J'ai entendu un moteur, deux nuits de suite, vers deux heures. Et je connais le bruit de ce bateau-là. »* | — | `alibi_2` |
| **Antoine** (pescador) | Muelle | Vio el barco volver **cargado hasta arriba** y no era pescado: cajas atadas con lona. No sabe qué había dentro y no quiere saberlo. | No | *« Il rentrait chargé. Des caisses, sous une bâche. Ce n'était pas du poisson, ça, je vous le dis. »* | — | `cargo_seen` |

*(La pata 1 de la coartada, `alibi_1`, la desmonta Baptiste desde el caso 2: si el jugador vuelve a la Auberge, Baptiste dice que Albert no durmió allí esa semana. Es reutilización pura de un NPC ya escrito.)*

**★ Le menteur:** desmontar las tres patas (`alibi_1` + `alibi_2` + `alibi_3`).

**Resolución con Fabre:** quién (Albert) · mentira (la coartada, y Jacques se negó a sostenerla) · prueba (el motor que oyó Raphael + la carga que vio Antoine). Fabre cierra con el hilo del arco: *« Et vers où allait ce bateau, à votre avis ? »* → la villa.

**Vocabulario objetivo:** `registre` (registro), `alibi`, `moteur` (motor), `charger` (cargar), `bâche` (lona), `preuve` (prueba), `avouer` (confesar).

---

### CASO 5 — «La villa sur la colline» (La villa de la colina) — FINAL

**Repaso.** Reaparecen los personajes de los 4 casos y hay que usar lo aprendido en cada uno.

- **Escenas:** Puerto → Muelle → Plaza → Bar Dragon Verde → Villa François → Oficina Portuaria
- **Brief:** *"Tout mène à la villa sur la colline. M. François reçoit, sourit, et personne n'ose parler. Fabre ne peut rien faire sans trois témoignages."*
- **Tiempo previsto:** 15 min · **★ Chronomètre:** 30 min

**La verdad:** **M. François** compraba mercancía robada y desviada del puerto. Pagó a Albert por los viajes nocturnos (caso 4) y a Adrien por la caja de café (caso 2). Nadie declara porque le deben dinero o le tienen miedo. François no miente nunca: **elude**. Es el único NPC del juego al que no se le rompe con una contradicción — se le rompe **con los testimonios ya reunidos**.

**Expediente:**

| NPC | Escena | Papel | Frase clave guionizada | Qué necesita para hablar | Checkpoint |
|---|---|---|---|---|---|
| **Albert** | Muelle | Ya confesó el caso 4, pero **no quiere dar el nombre de quien le pagaba**: le da miedo. | *« J'ai avoué pour le bateau. Mais le nom de celui qui payait, ça, je ne le dirai pas. »* | Que el jugador le recuerde que **Jacques se negó a mentir por él** y aun así lo respeta. | `temoin_albert` |
| **Adrien** | Plaza | Ya cazado en el caso 2. Habla si se le ofrece que declarar cuenta a su favor ante Fabre. | *« Si je parle, ça jouera pour moi ? Alors d'accord : la caisse, c'était pour la villa. »* | Que el jugador le explique el trato, en francés. | `temoin_adrien` |
| **Armel** | Bar Dragon Verde | Sabe quién bebe en el bar con dinero de François. Habla si el jugador **le demuestra que guardó el secreto del caso 3**. | *« Vous n'avez rien dit, l'autre fois. Bon. Alors je vous dis ce que je sais. »* | Haber cerrado el caso 3 sin revelar el destino de la mujer. | `temoin_armel` |
| **Camille** | Bar Dragon Verde | **Cuarto testigo alternativo**: si el jugador quemó a Armel o a otro, ella puede sustituirle — **salvo que en el caso 3 revelara a dónde fue la mujer**, en cuyo caso se niega. | *« Je vous ai fait confiance une fois. »* / (si la traicionó) *« Non. Pas avec vous. »* | Confianza intacta del caso 3. | `temoin_camille` |
| **M. François** | Villa François | El objetivo. Nunca miente: elude, invita a tomar algo, habla del tiempo, se ofende con elegancia. Solo se le puede **cerrar la salida** con los testimonios ya obtenidos. | *« Je ne mens pas, monsieur. Je choisis simplement ce que je dis. »* | Que el jugador le ponga delante lo que le contaron Albert, Adrien y Armel/Camille. | `francois_cornered` |
| **Jacques** | Puerto | No aporta pruebas. Aporta el empujón para ir a la villa y una advertencia. Cierra el arco del jugador desde el modo débutant. | *« Tu parlais à peine français quand tu es arrivé. Maintenant tu vas monter là-haut leur poser des questions. »* | — | — |

**★ Le menteur:** obtener `francois_cornered`.

**Resolución con Fabre:** hacen falta **3 testimonios** (`temoin_*`) + `francois_cornered`. Fabre pide que el jugador se los resuma **uno por uno, en francés** — es deliberadamente el momento de producción oral más largo del juego, y llega cuando el jugador ya lleva 4 casos hablando.

**Vocabulario objetivo:** repaso de los 4 casos + `témoignage` (testimonio), `dénoncer` (denunciar), `dette` (deuda), `menace` (amenaza), `justice`.

---

## 2.7 Multilingüe — el juego ya no es solo de francés

Desde el 18/08/2026 el jugador elige **qué idioma quiere aprender** al abrir el juego. A 24/08/2026 hay cuatro: **francés, inglés, italiano y catalán**. Lo que cambia es lo que hablan los NPCs; **la interfaz sigue en español** en modo débutant. Por eso la pregunta de la primera pantalla es «¿qué idioma quieres aprender?» y no «¿en qué idioma quieres jugar?».

**La regla que sostiene todo:** el idioma **no viaja en su propio campo**. Va como **sufijo de la clave de expediente** que el cliente manda al servidor: `cased1_nadia` es francés, `cased1_nadia_en` inglés, `_it` italiano, `_ca` catalán. El servidor lo deduce con `langOf()`. Así una clave no puede quedar descolgada de su idioma — un expediente italiano interpretado con reglas francesas sería un fallo silencioso y caro.

**Los cinco puntos del servidor que dependen del idioma** (todos tablas indexadas por código, añadir uno es añadir una fila): etiquetado de vocabulario (`TAGGING_EXAMPLES`), reglas del modo básico (`basicInstructionFor`), guía fonética del JSON de respuesta (`PHONETIC`), voces TTS (`TTS_VOICES`, sufijo `_xx`) y reconocimiento de voz (`STT_LANGUAGE_CODES`).

**En el cliente** manda `LangText.Pick(es, fr, en, it, ca)`: **en débutant gana siempre el español**, en avancé gana el idioma que se aprende, y si falta una traducción cae al francés. `GameSettings.TargetLanguage` guarda la elección y `DossierSuffix` la convierte en sufijo por convención, no por tabla.

**El error que se repitió en los tres idiomas nuevos** y que hay que buscar el primero al añadir otro: cadenas de idioma escritas a fuego fuera de este sistema. El saludo de entrada de cada NPC (vive en el cliente, se muestra antes de la primera llamada al servidor), el `languageCode` del TTS, el idioma de `/translate`, el de STT, y textos españoles que *nombran* el francés («AHORA LÉELO EN FRANCÉS») — este último se escapa de cualquier búsqueda de texto francés.

**Aviso sobre el catalán:** Google TTS tiene **una sola voz catalana**, `ca-ES-Standard-B`, y es femenina (comprobado con `GET /voices`). Los cuatro personajes de una escena comparten timbre y se separan con **tono y velocidad**. Se descartó usar voces `es-ES`: suenan a castellanohablante leyendo catalán, y en un juego para aprender el idioma enseñar mal la pronunciación es peor defecto que la falta de variedad.

---

## 6. Notas de implementación (para cuando toque programar)

**Se reutiliza tal cual, sin tocar:** `SpeechCaptionUI` (globos de pergamino), `LevelHUD` (checklist de checkpoints), `SceneExit`/`allowedScenes`, `GenericNPCChatClient`, `LevelManager`, `SaveSystem` (con `difficulty` ya separando débutant de avancé), `VoiceRecorder`, `NPCVoicePlayer`, `PhraseTTSClient`, sistema de estrellas y `LevelResultUI`.

**Cambios necesarios:**
1. **Cronómetro ascendente sin límite** en modo detective (`timeLimitSeconds` ignorado; `starTimeSeconds` = umbral holgado). Toca `LevelManager` y `LevelHUD`.
2. ~~**Campaña separada:** dos listas en `LevelDatabase`.~~ → **Resuelto de otra forma el 14/08/2026, y sale más barato:** los casos son los **niveles 6-10**, el tramo avancé que `LevelSelectUI` ya reservaba y pintaba bloqueado por no existir. El progreso ya estaba separado (los resultados se guardan por `id:dificultad`), así que no hace falta una segunda lista ni tocar `Get()`. El jugador los ve numerados 1-5 gracias a `displayNumber`.
3. **`LEVEL_OBJECTIVES` en `server.js`:** un bloque por NPC y caso, transcribiendo los expedientes de §5 — frase guionizada literal + prohibición de inventar hechos + checkpoints.
4. **Checkpoints condicionales:** algunos solo se desbloquean si el jugador aporta información de otro NPC (`m_broken`, `c_signal`, `adrien_cracks`, `husband_revealed`, `alibi_*`, `francois_cornered`). Es una condición nueva en el prompt, **no arquitectura nueva**: el criterio del checkpoint pasa a ser "el jugador me ha dicho X".
   **Lección del caso 1:** no basta con describirlo en prosa. Un checkpoint que depende de que el modelo recuerde *"¿me han dicho ya las dos cosas?"* no aguanta. Hay que convertir cada condición en **su propio checkpoint** (`m_confront_gun`, `m_confront_time`) para que la puerta sea una comprobación mecánica sobre una lista que el modelo ya mantiene.
   **Y ojo con el reseteo:** el cliente borra la conversación de un NPC cada vez que se abre el diálogo, así que ese estado no puede vivir solo en el servidor. Cada petición lleva `knownCheckpoints` desde `LevelManager` y el servidor siembra lo que le corresponde a ese personaje. Sin esto, un jugador que sale a repreguntar a otro testigo y vuelve pierde lo que ya había conseguido.

4b. **Dos listas distintas por nivel, no una:** `checkpoints` es lo que **completa** el caso (las validaciones del comisario) y `objectives` es el **checklist guía** del HUD. Confundirlas obligaría a hablar con todos los testigos para poder cerrar, y el diseño dice justo lo contrario. Un objetivo puede además exigir varios ids a la vez.
5. **6 NPCs nuevos, con script dedicado cada uno**: Odette, Corentin, Marcel y Fabre (caso 1, Bar Dragon Verde) y M. François (caso 5). Patrón de referencia: `JacquesChatClient.cs` (híbrido guionizado + IA). Endpoints `/npc/odette`, `/npc/corentin`, `/npc/marcel`, `/npc/fabre`.
6. **Vaciar `BarDragonVerde.unity`**: borrar `NPC_Henry`, `NPC_clientebar` y sus colliders; borrar o desactivar `SceneExitMercado` + `ExitHintMercado` (el caso 1 es de escena única).
7. **Zona de hallazgos en verde** en `LevelHUD` (§2.5), con animación de entrada, alimentada por las banderas de checkpoint.
8. **Renombrar** el Andre de Calle St Jean → Émile.
9. **Estado persistente entre casos** (mínimo): si el jugador reveló el destino de la mujer en el caso 3, Camille se cierra en el caso 5. Un solo booleano en `SaveSystem`.
10. **Voces TTS nuevas** para los NPCs nuevos, distintas entre sí y de las ya usadas. La de Fabre es la que más importa: reaparece en los cinco casos.

**Orden de trabajo acordado:** cerrar este GDD → **programar el caso 1 entero y bien acabado** → probarlo → solo entonces producir los casos 2-5.

**Riesgo principal a vigilar en el caso 1:** que Gemini sostenga las dos mentiras de Marcel sin confesar antes de tiempo (basta una contradicción y se rompe = caso roto) y sin volverse un muro imposible (no acepta la contradicción aunque se la digan = caso atascado). Si eso funciona, los otros cuatro casos son producción; si no, se descubre con un caso hecho y no con cinco.

---

## 7. Decisiones abiertas

1. **¿Se mantiene el modo débutant (los 5 niveles actuales) publicado junto al detective, o el juego que sube a la App Store es solo el modo detective?** El débutant está hecho y funciona, pero apunta a otro público (quien no habla francés).
2. **Coste de API en producción:** publicar en la App Store implica pagar Gemini + TTS + STT de cada jugador. Con 5-15 min de conversación por caso, conviene decidir antes de publicar: límite de partidas gratis, clave propia del usuario, o asumir el coste.
3. ~~**Voz de Fabre y François**~~ → **decidido el 14/08/2026: se actualizan las voces.** Cada NPC nuevo lleva voz TTS propia, distinta entre sí y de las ya usadas, por realismo. Perfiles del caso 1: Odette joven y cálida; Corentin grave y lento; Marcel medio y cordial, **sin gravedad amenazante** (no debe *sonar* culpable); Fabre cansado y neutro. Queda por elegir las voces concretas de Google TTS.
4. **La pieza musical** que toca Marius en el caso 1 como señal. No bloquea la programación.
