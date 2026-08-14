// Cliente de chat de Odette — caso 1, « Le violoniste du Dragon Vert ».
// Todo el fontanero HTTP está en DetectiveChatClient; aquí solo vive la
// identidad, que es lo que decide el endpoint y el expediente del servidor.
public class OdetteChatClient : DetectiveChatClient
{
    public override string NpcId => "odette";
}
