// Cliente de chat de Nadia — caso débutant 1, « Le collier de la comtesse ».
// Todo el fontanero HTTP está en DetectiveChatClient; aquí solo vive la
// identidad, que es lo que decide el endpoint y el expediente del servidor.
public class NadiaChatClient : DetectiveChatClient
{
    public override string NpcId => "nadia";
}
