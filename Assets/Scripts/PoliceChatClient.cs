// Cliente de chat del agente Bonnet — caso débutant 1.
// Todo el fontanero HTTP está en DetectiveChatClient; aquí solo vive la
// identidad, que es lo que decide el endpoint y el expediente del servidor.
public class PoliceChatClient : DetectiveChatClient
{
    public override string NpcId => "police";
}
