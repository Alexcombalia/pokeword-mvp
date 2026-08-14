// CORENTIN VIDAL, 50 — dueño del Dragon Vert.
// Caso 1, « Le violoniste du Dragon Vert ». Expediente: server.js → case1_corentin.
//
// Honesto, con un tema tabú: no protege a un asesino, protege una red. Su
// candado es de VOLUNTAD, no de memoria — no quiere hablar de la señal, pero
// no finge haberla olvidado. Se abre a la segunda insistencia.
//
// Nunca dice el nombre de Denis: no lo sabe. Sí admite que lo conoce, porque
// es cliente suyo desde hace años y negarlo sería mentir, y Corentin no miente.
[UnityEngine.RequireComponent(typeof(CorentinChatClient))]
public class CorentinNPC : DetectiveNPC
{
    public override string DisplayName => "Corentin";

    public override string Greeting =>
        "Le [café|café] est ouvert. C'est tout ce que je peux faire aujourd'hui.";

    protected override string GreetingTranslation =>
        "El café está abierto. Es todo lo que puedo hacer hoy.";
}
