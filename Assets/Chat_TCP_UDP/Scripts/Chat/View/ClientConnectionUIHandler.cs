using UnityEngine;

public class ClientConnectionUIHandler : MonoBehaviour
{
    [SerializeField] private string serverAddress = "127.0.0.1";
    [SerializeField] private int serverPort = 5555;

    private ChatController controller;

    public void Initialize(ChatController controller)
    {
        this.controller = controller;
    }

    public async void OnConnectButtonClicked()
    {
        if (controller == null)
            return;

        await controller.Connect(serverAddress, serverPort);
    }
}