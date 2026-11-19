// MARCEL CHARRIER, 44 — antiguo gendarme, cliente habitual. EL CULPABLE.
// Caso 1, « Le violoniste du Dragon Vert ». Expediente: server.js → case1_marcel.
//
// Cordial y servicial: esa colaboración es su escudo. Tiene DOS mentiras (el
// arma y la hora) y con UNA sola contradicción encima aguanta — solo se rompe
// cuando el jugador le pone las dos delante. Esa puerta es la mecánica más
// delicada de todo el modo detective; está implementada en el servidor con dos
// checkpoints internos (m_confront_gun, m_confront_time).
//
// Su frase de entrada es su presentación, y es deliberada: es el NPC más
// accesible del bar y el único que se dirige al jugador. Así el nombre "Marcel"
// entra en la partida sin depender de que el jugador sepa preguntar por él, y
// además habla con el culpable antes de sospechar de él.
[UnityEngine.RequireComponent(typeof(MarcelChatClient))]
public class MarcelNPC : DetectiveNPC
{
    public override string DisplayName => "Marcel";

    public override string Greeting =>
        "Marcel Charrier. Un ancien de la maison. Si je peux vous aider, de policier à policier.";

    protected override string GreetingTranslation =>
        "Marcel Charrier. Un antiguo del cuerpo. Si puedo ayudarle, de policía a policía.";

    protected override string GreetingEn =>
        "Marcel Charrier. One of the old force. If I can help you, from one policeman to another.";

    protected override string GreetingTranslationEn =>
        "Marcel Charrier. Un antiguo del cuerpo. Si puedo ayudarle, de policía a policía.";

    protected override string GreetingIt =>
        "Marcel Charrier. Uno della vecchia guardia. Se posso aiutarla, da poliziotto a poliziotto.";

    protected override string GreetingTranslationIt =>
        "Marcel Charrier. Un antiguo del cuerpo. Si puedo ayudarle, de policía a policía.";

    protected override string GreetingCa =>
        "Marcel Charrier. Un de la vella guàrdia. Si el puc ajudar, de policia a policia.";

    protected override string GreetingTranslationCa =>
        "Marcel Charrier. Un antiguo del cuerpo. Si puedo ayudarle, de policía a policía.";

    protected override string GreetingAgain   => "Je vous écoute.";
    protected override string GreetingAgainEn => "I'm listening.";
    protected override string GreetingAgainIt => "La ascolto.";
    protected override string GreetingAgainCa => "L'escolto.";
}
