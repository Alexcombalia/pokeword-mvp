// DOCTOR LEMAIRE, 55 — firmó el certificado de muerte.
// Caso débutant 1. Expediente: server.js → cased1_lemaire.
//
// No es mala persona: es un hombre cansado que tenía prisa por llegar a casa
// antes del toque de queda y escribió lo más probable sin mirar. Le da
// vergüenza, pero lo admite si se le pregunta de frente.
//
// SIN ÉL NO HAY CASO. Su frase —no había ninguna herida, así que si no fue el
// corazón fue algo que bebió— es la que ata la botella con la muerte. Sin esa
// mitad, el jugador solo tiene un ladrón que miente. En el relato original del
// guionista este personaje no llevaba a ninguna parte.
[UnityEngine.RequireComponent(typeof(LemaireChatClient))]
public class LemaireNPC : DetectiveNPC
{
    public override string DisplayName => "Docteur Lemaire";

    public override string Greeting =>
        "Je crois que c'est une crise cardiaque. Elle était vieille. C'est normal.";

    protected override string GreetingTranslation =>
        "Creo que fue un infarto. Era mayor. Es lo normal.";

    protected override string GreetingEn =>
        "I think it was a heart attack. She was old. That is normal.";

    protected override string GreetingTranslationEn =>
        "Creo que fue un infarto. Era mayor. Es lo normal.";

    protected override string GreetingIt =>
        "Credo sia stato un infarto. Era anziana. È normale.";

    protected override string GreetingTranslationIt =>
        "Creo que fue un infarto. Era mayor. Es lo normal.";

    protected override string GreetingCa =>
        "Crec que va ser un infart. Era gran. És normal.";

    protected override string GreetingTranslationCa =>
        "Creo que fue un infarto. Era mayor. Es lo normal.";

    protected override string GreetingAgain   => "Encore vous. Je vous écoute.";
    protected override string GreetingAgainEn => "You again. I'm listening.";
    protected override string GreetingAgainIt => "Ancora lei. La ascolto.";
    protected override string GreetingAgainCa => "Vostè un altre cop. L'escolto.";

    protected override string GreetingAgainTranslation   => "Usted otra vez. Le escucho.";
    protected override string GreetingAgainTranslationEn => "Usted otra vez. Le escucho.";
    protected override string GreetingAgainTranslationIt => "Usted otra vez. Le escucho.";
    protected override string GreetingAgainTranslationCa => "Usted otra vez. Le escucho.";
}
