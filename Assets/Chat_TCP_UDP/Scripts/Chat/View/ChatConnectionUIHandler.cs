using UnityEngine;
using System.Threading.Tasks;

public class ChatConnectionUIHandler : MonoBehaviour
{
    [SerializeField] private string serverAddress = "127.0.0.1";
    [SerializeField] private int serverPort = 5555;
    [SerializeField] private TCPClient client;

    public async void OnConnectButtonClicked()
    {
        if (client == null)
            return;

        await client.ConnectToServer(serverAddress, serverPort);
    }
}