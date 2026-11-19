// ODETTE LACROIX, 27 — camarera del Dragon Vert.
// Caso 1, « Le violoniste du Dragon Vert ». Expediente: server.js → case1_odette.
//
// Lo vio todo y no entendió nada. Es honesta y no oculta nada: sus cuatro
// hechos están abiertos desde el primer minuto. Lo que está cerrado no es su
// información, es el significado — y ese lo abre el jugador.
//
// Su frase de entrada es el detalle humano del caso: el violín sigue en la
// barra porque ella no ha querido moverlo.
[UnityEngine.RequireComponent(typeof(OdetteChatClient))]
public class OdetteNPC : DetectiveNPC
{
    public override string DisplayName => "Odette";

    public override string Greeting =>
        "Bonjour... Le [violon|violín] est encore sur le comptoir. Je n'ai pas voulu le bouger.";

    protected override string GreetingTranslation =>
        "Buenos días... El violín sigue en la barra. No he querido moverlo.";

    protected override string GreetingEn =>
        "Hello... The [violin|violín] is still on the counter. I didn't want to move it.";

    protected override string GreetingTranslationEn =>
        "Hola... El violín sigue en la barra. No he querido moverlo.";

    protected override string GreetingIt =>
        "Buongiorno... Il [violino|violín] è ancora sul bancone. Non ho voluto spostarlo.";

    protected override string GreetingTranslationIt =>
        "Buenos días... El violín sigue en la barra. No he querido moverlo.";

    protected override string GreetingCa =>
        "Bon dia... El [violí|violín] encara és a la barra. No l'he volgut moure.";

    protected override string GreetingTranslationCa =>
        "Buenos días... El violín sigue en la barra. No he querido moverlo.";

    protected override string GreetingAgain   => "Vous revoilà.";
    protected override string GreetingAgainEn => "You're back.";
    protected override string GreetingAgainIt => "Eccola di nuovo.";
    protected override string GreetingAgainCa => "Ja torna a ser aquí.";
}
