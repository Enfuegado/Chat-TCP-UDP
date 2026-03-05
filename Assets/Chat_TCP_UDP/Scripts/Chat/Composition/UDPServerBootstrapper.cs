using UnityEngine;

public class UDPServerBootstrapper : MonoBehaviour
{
    [SerializeField] private UDPServer udpServer;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;

    private ChatController controller;

    private async void Start()
    {
        controller = new ChatController(udpServer, chatUIView);

        inputHandler.Initialize(controller);

        await udpServer.StartServer(7777);
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}