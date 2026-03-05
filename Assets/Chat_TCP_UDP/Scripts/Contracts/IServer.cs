using System.Threading.Tasks;

public interface IServer : IChatConnection
{
    Task StartServer(int port);
}