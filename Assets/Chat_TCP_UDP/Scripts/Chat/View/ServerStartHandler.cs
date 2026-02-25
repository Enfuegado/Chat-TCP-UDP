using UnityEngine;
using System.Threading.Tasks;

public class ServerStartHandler : MonoBehaviour
{
    [SerializeField] private int port = 5555;
    [SerializeField] private TCPServer server;

    public async void OnStartServerClicked()
    {
        if (server == null)
            return;

        await server.StartServer(port);
    }
}