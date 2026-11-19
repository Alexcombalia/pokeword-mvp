// PIOTR SOKOLOV, 48 — joyero del mercado negro. EL CULPABLE.
// Caso débutant 1. Expediente: server.js → cased1_piotr.
//
// Le llevó a la condesa una botella "de regalo" para dormirla y llevarse el
// collar sin ruido. No quería matarla, pero el corazón de una mujer mayor no
// aguantó la dosis. Ha vuelto al salón esta mañana ofreciéndose a ayudar con
// las cosas de la señora — un joyero rondando las pertenencias de una muerta.
// Es lo que lo pone al alcance del jugador en una escena única, y es lo que
// lo pierde.
//
// Se rompe con UNA sola contradicción, no dos como Marcel: en cuanto le dicen
// que estuvo allí esa noche, confiesa. Un principiante no puede sostener dos
// careos en francés.
[UnityEngine.RequireComponent(typeof(PiotrChatClient))]
public class PiotrNPC : DetectiveNPC
{
    public override string DisplayName => "Piotr";

    public override string Greeting =>
        "Piotr. Bijoutier. Je viens aider avec les affaires de madame.";

    protected override string GreetingTranslation =>
        "Piotr. Joyero. Vengo a ayudar con las cosas de la señora.";

    protected override string GreetingEn =>
        "Piotr. Jeweller. I came to help with madam's things.";

    protected override string GreetingTranslationEn =>
        "Piotr. Joyero. Vengo a ayudar con las cosas de la señora.";

    protected override string GreetingIt =>
        "Piotr. Gioielliere. Vengo ad aiutare con le cose della signora.";

    protected override string GreetingTranslationIt =>
        "Piotr. Joyero. Vengo a ayudar con las cosas de la señora.";

    protected override string GreetingCa =>
        "Piotr. Joier. Vinc a ajudar amb les coses de la senyora.";

    protected override string GreetingTranslationCa =>
        "Piotr. Joyero. Vengo a ayudar con las cosas de la señora.";

    protected override string GreetingAgain   => "Encore une question ? Je vous en prie.";
    protected override string GreetingAgainEn => "Another question? Please, go ahead.";
    protected override string GreetingAgainIt => "Un'altra domanda? Prego.";
    protected override string GreetingAgainCa => "Una altra pregunta? Endavant.";

    protected override string GreetingAgainTranslation   => "¿Otra pregunta? Adelante.";
    protected override string GreetingAgainTranslationEn => "¿Otra pregunta? Adelante.";
    protected override string GreetingAgainTranslationIt => "¿Otra pregunta? Adelante.";
    protected override string GreetingAgainTranslationCa => "¿Otra pregunta? Adelante.";
}
