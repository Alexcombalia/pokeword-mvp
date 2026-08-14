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
}
