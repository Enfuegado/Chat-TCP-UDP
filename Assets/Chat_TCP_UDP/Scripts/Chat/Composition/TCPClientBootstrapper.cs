using UnityEngine;

public class TCPClientBootstrapper : MonoBehaviour
{
    [SerializeField] private TCPClient tcpClient;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;
    [SerializeField] private ClientConnectionUIHandler connectionHandler;

    private ChatController controller;

    private async void Start()
    {
        controller = new ChatController(tcpClient, chatUIView);

        inputHandler.Initialize(controller);
        connectionHandler.Initialize(controller);

        await controller.Connect("127.0.0.1", 7777);
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}