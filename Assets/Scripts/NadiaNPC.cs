// NADIA PETROVA, 24 — doncella de la condesa. LA CLIENTA.
// Caso débutant 1. Expedientes: server.js → cased1_nadia (informa) y
// cased1_nadia_final (valida el cierre del caso).
//
// Es quien ha llamado al detective y quien paga, con lo poco que tiene. Eso es
// lo que explica que tres desconocidos le contesten a un extraño, y es la razón
// de que el primer NPC del juego esté de tu lado: en un tutorial conviene que
// la primera persona con la que hablas no sea un obstáculo.
//
// Su regla dura: describe al visitante como "un hombre del mercado" y NUNCA
// da el nombre por su cuenta. El jugador tiene que volver a preguntárselo
// después de pillar a Piotr mintiendo, y ese viaje de vuelta es la única
// deducción real del nivel.
[UnityEngine.RequireComponent(typeof(NadiaChatClient))]
public class NadiaNPC : DetectiveNPC
{
    public override string DisplayName => "Nadia";

    public override string Greeting =>
        "Merci d'être venu. Le [docteur|médico] dit : le cœur. Moi, je ne le crois pas.";

    protected override string GreetingTranslation =>
        "Gracias por venir. El médico dice: el corazón. Yo no me lo creo.";

    protected override string GreetingEn =>
        "Thank you for coming. The [doctor|médico] says: her heart. I don't believe it.";

    protected override string GreetingTranslationEn =>
        "Gracias por venir. El médico dice: el corazón. Yo no me lo creo.";

    protected override string GreetingIt =>
        "Grazie di essere venuto. Il [dottore|médico] dice: il cuore. Io non ci credo.";

    protected override string GreetingTranslationIt =>
        "Gracias por venir. El médico dice: el corazón. Yo no me lo creo.";
}
