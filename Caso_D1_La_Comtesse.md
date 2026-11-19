# CASO DÉBUTANT 1 — « Le collier de la comtesse »

**Versión:** 2.0 — 18/08/2026. Añadidos el agente de policía, la acusación cruzada de Piotr y la posibilidad de perder el caso.
**Estado:** programado, compilando y desplegado en Railway. Falta el arte de los cuatro personajes y testearlo.

Es **el primer nivel del juego**: enseña a moverse, a hablar, a capturar palabras y a investigar, a alguien que además no habla francés.

---

## 1. Ficha

| | |
|---|---|
| **Nivel** | id 1 — sustituye a *Le bateau de Jacques* |
| **Escena** | Una sola: `SalonCondesa` |
| **NPCs** | 4: Nadia, el doctor Lemaire, Piotr y el agente Bonnet |
| **Crimen** | Homicidio involuntario + robo. **Nunca se muestra nada**: no hay cadáver, ni sangre, ni violencia |
| **★ Résolu** | Darle el nombre correcto al agente |
| **★ Mots** | Capturar las 6 palabras objetivo (en débutant la 2ª estrella es de vocabulario) |
| **★ Chronomètre** | Por debajo de 20 minutos |
| **Se puede PERDER** | Sí. Acusar a un inocente termina el caso en el acto |

---

## 2. El brief

> **Marsella, 1941.**
> La condesa **Irina Volkova**, rusa exiliada, apareció muerta anoche en su salón. El médico firmó: **ataque al corazón**.
> Su doncella no se lo cree, y te ha llamado. No le queda dinero, pero no quiere que esto acabe así.
> Esa misma noche desapareció el **collar de diamantes** de la condesa.

---

## 3. La verdad del caso

**Piotr Sokolov**, joyero del mercado negro, fue a ver a la condesa **ayer por la noche** y le **vendió** una botella de coñac — con algo dentro para dormirla profundamente y poder llevarse el collar sin ruido.

No quería matarla. La condesa era vieja y tenía el corazón débil, y la dosis fue demasiado. Murió dormida. Piotr cogió el collar y se fue, convencido de que nadie lo había visto.

El **doctor Lemaire** llegó por la mañana con prisa, antes del toque de queda, vio a una mujer mayor muerta en su butaca y escribió lo más probable. No miró más.

**Piotr ha vuelto esta mañana**, muy cordial, a ofrecerse para ayudar con las cosas de la señora. Es lo que lo pone al alcance del jugador, y es lo que lo pierde.

El coñac importa: en la Francia ocupada de 1941 no se encuentra. Hay achicoria en vez de café y sucedáneos en vez de licor. El coñac auténtico solo viene del mercado negro.

---

## 4. La cadena de deducción

1. **Nadia**: ayer por la noche vino un hombre y **le estaba vendiendo algo** a la señora. **No le vio la cara.** Llevaba un maletín — un comerciante de joyas, cree ella.
2. **El doctor**: no hay nada en el cuerpo, ninguna herida. Y **solo si se le pregunta qué más pudo matarla**: una bebida fuerte, un coñac, puede matar un corazón débil.
3. **El doctor**, preguntado por quién vende coñac: **Piotr**, ilegalmente. Y si se le pregunta cómo lo sabe, admite que se lo compra él.
4. **Piotr** niega haber estado allí. Puesto ante el coñac, **admite la venta pero la sitúa "hace días"** — y en la misma frase **acusa a Nadia**: estaba sola con ella, el corazón se para, el collar desaparece.
5. **← AQUÍ ESTÁ EL CASO.** Nadia vio a alguien vendiéndole algo **ayer**. Lo único que se le vendió es el coñac de Piotr. Su "hace días" se cae.
6. Con la fecha **y** el coñac encima, **Piotr confiesa**.
7. El jugador le da el nombre al **agente Bonnet**.

**Regla de oro:** ningún NPC señala a nadie. Nadia no puede: no le vio la cara. Y una testigo que hubiera reconocido al culpable lo habría dicho en su primera frase — es ella quien paga la investigación.

---

## 5. Expediente por NPC

### 5.1 NADIA PETROVA, 24 — la doncella. **La testigo principal.**
Sirvió a la condesa nueve años. No cree que fuera el corazón y lo dijo en voz alta en cuanto llegó la policía. Nadie la contrata y nadie paga: el caso le ha caído al inspector por turno. Honesta, cálida, directa.

| Qué sabe | Condición |
|---|---|
| **[N1]** La condesa estaba bien a las diez. Le llevó su té | Abierto |
| **[N2]** Ayer por la noche vino un hombre y **le vendía algo** a la señora. Insiste en la venta: madame no tenía dinero e iba vendiendo sus cosas | Abierto |
| **[N3]** **No le vio la cara.** Lo vio de espaldas, en el pasillo | Abierto |
| **[N4]** Llevaba un maletín. Un comerciante de joyas, **cree** — es una impresión, no un hecho | Si preguntan cómo era o qué llevaba |
| **[N5]** El collar ha desaparecido | Abierto |
| **[N6]** Si le dicen que Piotr la acusa: *«No tengo nada, señor. Mire mi cuarto.»* — se defiende ofreciendo que la registren, no acusando de vuelta | Si se lo cuentan |

**Nunca hace:** adivinar quién era · acusar · decir que alguien miente.

### 5.2 DOCTOR LEMAIRE, 55 — el médico
Firmó el certificado con prisa. Tiene un secreto propio, y no es la condesa: **le compra coñac de estraperlo a Piotr.**

| Qué sabe | Condición |
|---|---|
| **[L1]** Escribió "el corazón" porque era mayor | Abierto |
| **[L2]** **No hay nada en el cuerpo.** Ninguna herida | Abierto |
| **[L3]** No la examinó bien. Tenía prisa, el toque de queda | Si preguntan |
| **[L4]** **Una bebida fuerte, un coñac, puede matar un corazón débil** | **Solo si preguntan qué más pudo matarla.** Nadie le pregunta eso a un médico si no lo está pensando ya |
| **[L5]** **Piotr vende el coñac**, ilegalmente | A la primera. Es el nivel fácil: hacerle repetir la pregunta a un principiante es perderlo |
| **[L6]** Lo sabe porque **se lo compra** | Solo si preguntan cómo lo sabe |

### 5.3 PIOTR SOKOLOV, 48 — joyero del mercado negro. **EL CULPABLE**
Cordial y servicial. Cree que nadie lo vio, y esa certeza es lo que le da valor para mentir.

**Su defensa en tres tiempos:**

1. **Negación** — no estuvo allí, no vio a la condesa ayer.
2. **Admisión parcial** — puesto ante el coñac, no lo niega: sí, lo vende; sí, se lo vendió a ella. **Pero hace días.** Si la botella ya estaba en la casa, cualquiera pudo usarla.
3. **Contraacusación** — en la misma frase, se la devuelve a la doncella. *«La chica estaba sola con ella. El corazón se para, el collar desaparece. Piénselo, señor.»* Lo dice como razonamiento, no como insulto.

**Cómo se rompe — hacen falta DOS cosas:**

| Pieza | Qué se le pone delante | Qué contesta si va sola |
|---|---|---|
| **A — la fecha** | Nadia vio a alguien vendiéndole algo **ayer por la noche** | *«Mucha gente le vende cosas a las señoras mayores. No fui yo.»* |
| **B — el coñac** | La bebida que la mató es la que él vende | *«Le vendo coñac a media Marsella. La botella ya estaba allí.»* |

**Las dos respuestas son ciertas, y por eso resisten.** Es el cruce lo que lo deja sin suelo: lo único que se le vendió esa noche era su botella.

**Su última frase es una defensa:** *«No soy un asesino. Soy un ladrón.»*

### 5.4 AGENTE BONNET — policía de uniforme. **La puerta.**
Habla poquísimo. No investiga, no opina, no aconseja. Espera a que alguien le traiga un nombre.

- **CERRADO** mientras le falte al jugador `l_piotr` (Piotr vende el coñac) **o** `p_yesterday` (Piotr estuvo allí ayer). Responde solo: *«Todavía no tiene bastantes pruebas para decidir.»* **No dice qué falta**, no nombra a nadie, no explica.
- **ABIERTO** con las dos: *«Bien. ¿Y quién fue? ¿La doncella, el médico o el joyero?»*
- El nombre que le den es definitivo. No avisa, no deja rectificar, no reacciona.

---

## 6. Cómo se gana y cómo se pierde

**Se gana** dándole el nombre de Piotr → `pol_right`. Sale el epílogo y luego las estrellas.

**Se pierde** acusando a Nadia o al doctor → `pol_wrong`, que es el `failCheckpoint` del nivel. El caso **termina en el acto**, no al cerrar la conversación: acabas de dar un nombre y el juego te responde inmediatamente.

No hace falta la confesión de Piotr para ganar. Con las dos pruebas, el agente acepta igual — la confesión es el momento dramático, no el requisito.

---

## 7. Objetivos y hallazgos

**Objetivos (blanco, 5). Ninguno nombra al culpable:**
- ☐ Escucha a Nadia: qué pasó ayer por la noche → `n_selling`
- ☐ Pregunta al doctor qué pudo matarla → `l_drink`
- ☐ Averigua quién vende esa bebida → `l_piotr`
- ☐ Rompe su coartada: dice que vino hace días → `p_yesterday`
- ☐ Dale tu respuesta final al agente → `pol_right`

**Hallazgos (verde):** `n_selling` · `n_case` · `n_necklace` · `l_nocrime` · `l_drink` · `l_piotr` · `p_deny` · `p_sold` · `p_accuses`

Nueve hallazgos. Puestos en orden, `p_sold` ("vendió el coñac, pero hace días") queda debajo de `n_selling` ("ayer por la noche alguien le vendía algo"), y la contradicción de fechas se ve sola.

---

## 8. Banderas

| id | Se marca cuando |
|---|---|
| `n_selling` | Nadia cuenta lo del hombre que le vendía algo ayer |
| `n_case` | Nadia describe el maletín, sin haberle visto la cara |
| `n_necklace` | Nadia cuenta que falta el collar |
| `n_defends` | Nadia responde a la acusación de Piotr |
| `l_nocrime` | El doctor dice que no hay nada en el cuerpo |
| `l_hurry` | El doctor admite que no la examinó |
| `l_drink` | El doctor dice que una bebida fuerte puede matar un corazón débil |
| `l_piotr` | El doctor dice que Piotr vende el coñac |
| `l_buys` | El doctor admite que se lo compra |
| `p_deny` | Piotr niega haber estado allí |
| `p_sold` | Piotr admite la venta, "hace días" |
| `p_accuses` | Piotr acusa a Nadia |
| `p_yesterday` | **Interna.** El jugador le pone la fecha delante |
| `p_cognac` | **Interna.** El jugador le pone el coñac encima |
| `p_broken` | Piotr confiesa. **Requiere `p_yesterday` Y `p_cognac`** |
| `pol_asks` | El agente pregunta cuál de los tres. Requiere `l_piotr` Y `p_yesterday` |
| `pol_right` | El jugador dice Piotr → **gana** |
| `pol_wrong` | El jugador dice Nadia o el doctor → **pierde** |

---

## 9. Trabajo en Unity

Escena `SalonCondesa`, en Build Settings y en `interiorScenes` de `GameBootstrap`.

| Personaje | Scripts | Endpoint |
|---|---|---|
| Nadia | `NadiaNPC` + `NadiaChatClient` | `/npc/nadia` |
| Lemaire | `LemaireNPC` + `LemaireChatClient` | `/npc/lemaire` |
| Piotr | `PiotrNPC` + `PiotrChatClient` | `/npc/piotr` |
| Agente Bonnet | `PoliceNPC` + `PoliceChatClient` | `/npc/police` |

Voces: Nadia joven y cálida; Lemaire mayor y cansado; Piotr meloso; Bonnet seco y oficial (misma voz que Fabre a propósito — es la voz de la autoridad en todo el juego).

**Este nivel carga el tutorial.** `GuidedTutorial` se dispara la primera vez.

---

## 10. Vocabulario objetivo

`collier` · `bouteille` · `nuit` · `docteur` · `vendre` · `voir`

---

## 11. Fallos previsibles

| Riesgo | Corte |
|---|---|
| Nadia adivina quién era | No puede: no le vio la cara. A bocajarro responde que no lo sabe |
| Piotr confiesa pronto | Sin `p_yesterday` **y** `p_cognac` no confiesa. Con una sola, su defensa es cierta y aguanta |
| El jugador se cree la acusación contra Nadia | Nadia se defiende sola: un ladrón no paga una investigación |
| El jugador no llega a lo del coñac | La pista de `l_drink` le manda a preguntarle al doctor qué más pudo matarla |
| **El agente se siente un muro** | Riesgo real y sin resolver. Tiene prohibido decir qué falta. Ver §12 |

---

## 12. Lo que hay que vigilar al testear

1. **Que el jugador entienda que hay que ir al agente.** Es el último objetivo del panel, pero es la primera vez que el juego pide ir a alguien concreto a dar una respuesta.
2. **Que la negativa del agente no frustre.** Tiene prohibido dar pistas sobre lo que falta, y eso puede leerse como un muro en vez de como una puerta. Es el punto donde este nivel puede sentirse injusto.
3. **Que la dificultad no se haya ido de las manos.** Esto empezó siendo el nivel más simple de los diez. Ahora el jugador tiene que sacarle dos cosas al doctor, entender una coartada basada en fechas y no tragarse una acusación falsa — todo en francés de principiante y con el turno de lectura en voz alta de por medio.

---

## 13. Pendiente

- Los cuatro sprites: Nadia, Lemaire, Piotr y el agente.
- El retrato de la condesa que cuelga en la pared y enseña el collar.
- Qué hacer con `IntroVideo`, que sigue siendo la llegada al puerto de un juego que ya no existe.
