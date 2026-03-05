using UnityEngine;

public class TCPServerBootstrapper : MonoBehaviour
{
    [SerializeField] private TCPServer tcpServer;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;

    private ChatController controller;

    private async void Start()
    {
        controller = new ChatController(tcpServer, chatUIView);

        inputHandler.Initialize(controller);

        await tcpServer.StartServer(7777);
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}