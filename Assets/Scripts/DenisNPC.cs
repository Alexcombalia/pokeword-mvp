// DENIS CHARRIER, 44 — antiguo gendarme, cliente habitual. EL CULPABLE.
// Caso 1, « Le violoniste du Dragon Vert ». Expediente: server.js → case1_denis.
//
// Cordial y servicial: esa colaboración es su escudo. Tiene DOS mentiras (el
// arma y la hora) y con UNA sola contradicción encima aguanta — solo se rompe
// cuando el jugador le pone las dos delante. Esa puerta es la mecánica más
// delicada de todo el modo detective; está implementada en el servidor con dos
// checkpoints internos (d_confront_gun, d_confront_time).
//
// Su frase de entrada es su presentación, y es deliberada: es el NPC más
// accesible del bar y el único que se dirige al jugador. Así el nombre "Denis"
// entra en la partida sin depender de que el jugador sepa preguntar por él, y
// además habla con el culpable antes de sospechar de él.
[UnityEngine.RequireComponent(typeof(DenisChatClient))]
public class DenisNPC : DetectiveNPC
{
    public override string DisplayName => "Denis";

    public override string Greeting =>
        "Denis Charrier. Un ancien de la maison. Si je peux vous aider, de policier à policier.";

    protected override string GreetingTranslation =>
        "Denis Charrier. Un antiguo del cuerpo. Si puedo ayudarle, de policía a policía.";
}
