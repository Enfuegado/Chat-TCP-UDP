using UnityEngine;
using System.Threading.Tasks;

public class UDPServerStartHandler : MonoBehaviour
{
    [SerializeField] private int port = 5555;
    [SerializeField] private UDPServer server;

    public async void OnStartServerClicked()
    {
        if (server == null)
            return;

        await server.StartServer(port);
    }
}