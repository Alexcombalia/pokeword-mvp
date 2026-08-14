// COMISARIO FABRE — mesa junto a la puerta del Dragon Vert.
// Caso 1, « Le violoniste du Dragon Vert ». Expediente: server.js → case1_fabre.
//
// El mismo personaje en los cinco casos, por eso su clase no lleva nada
// específico del caso 1: qué valida en cada uno lo decide el nivel, vía
// npcObjectiveKeys → case1_fabre, case2_fabre, etc.
//
// En el caso 1 está dentro del bar, no en la Oficina Portuaria, porque el
// nivel es de escena única. Ya ha mirado el callejón, ha decidido que fue un
// robo y se está terminando el café antes de cerrar el expediente — su frase
// de entrada es esa posición de partida, y es lo que el jugador tiene que
// desmontar. No escucha las conversaciones del café: todo hay que contárselo.
[UnityEngine.RequireComponent(typeof(FabreChatClient))]
public class FabreNPC : DetectiveNPC
{
    public override string DisplayName => "Commissaire Fabre";

    public override string Greeting =>
        "Un [vol|robo] qui a mal tourné. Ça arrive, dans ce [quartier|barrio].";

    protected override string GreetingTranslation =>
        "Un robo que salió mal. Pasa, en este barrio.";
}
