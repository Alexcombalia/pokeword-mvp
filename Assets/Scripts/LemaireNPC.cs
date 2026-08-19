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
        "J'ai écrit : le cœur. Elle était vieille. C'est normal.";

    protected override string GreetingTranslation =>
        "Escribí: el corazón. Era mayor. Es lo normal.";

    protected override string GreetingEn =>
        "I wrote: the heart. She was old. That is normal.";

    protected override string GreetingTranslationEn =>
        "Escribí: el corazón. Era mayor. Es lo normal.";

    protected override string GreetingIt =>
        "Ho scritto: il cuore. Era anziana. È normale.";

    protected override string GreetingTranslationIt =>
        "Escribí: el corazón. Era mayor. Es lo normal.";
}
