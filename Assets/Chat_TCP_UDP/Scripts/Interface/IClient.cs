using System.Threading.Tasks;

public interface IClient : IChatConnection
{
    Task ConnectToServer(string ip, int port);
}