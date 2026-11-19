// AGENTE BONNET — policía de uniforme. El que cierra el caso.
// Caso débutant 1. Expediente: server.js → cased1_police.
//
// Habla poquísimo, a propósito: no investiga, no opina, no da consejos. Está
// esperando a que alguien le traiga un nombre.
//
// Funciona como PUERTA, y es la pieza que convierte el nivel en un caso de
// verdad. Mientras al jugador le falte alguna de las dos pruebas —que Piotr
// vende el coñac (l_piotr) y que Piotr estuvo allí ayer (p_yesterday)— se
// niega a dejarle acusar, sin decirle qué le falta. Con las dos, le pregunta
// cuál de los tres fue.
//
// Y aquí se puede PERDER el caso: acusar a Nadia o al doctor dispara
// pol_wrong, que es el failCheckpoint del nivel. Hasta este nivel, un caso
// solo podía perderse por tiempo — y en modo detective el tiempo no hace
// perder, así que no había forma de fallar.
[UnityEngine.RequireComponent(typeof(PoliceChatClient))]
public class PoliceNPC : DetectiveNPC
{
    public override string DisplayName => "Agent Bonnet";

    public override string Greeting =>
        "Agent Bonnet. J'attends un nom.";

    protected override string GreetingTranslation =>
        "Agente Bonnet. Espero un nombre.";

    protected override string GreetingEn =>
        "Officer Bonnet. I am waiting for a name.";

    protected override string GreetingTranslationEn =>
        "Agente Bonnet. Espero un nombre.";

    protected override string GreetingIt =>
        "Agente Bonnet. Aspetto un nome.";

    protected override string GreetingTranslationIt =>
        "Agente Bonnet. Espero un nombre.";

    protected override string GreetingCa =>
        "Agent Bonnet. Espero un nom.";

    protected override string GreetingTranslationCa =>
        "Agente Bonnet. Espero un nombre.";

    protected override string GreetingAgain   => "Alors ?";
    protected override string GreetingAgainEn => "Well?";
    protected override string GreetingAgainIt => "Allora?";
    protected override string GreetingAgainCa => "I doncs?";

    protected override string GreetingAgainTranslation   => "¿Y bien?";
    protected override string GreetingAgainTranslationEn => "¿Y bien?";
    protected override string GreetingAgainTranslationIt => "¿Y bien?";
    protected override string GreetingAgainTranslationCa => "¿Y bien?";
}
