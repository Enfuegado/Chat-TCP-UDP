using UnityEngine;

public class UDPClientBootstrapper : MonoBehaviour
{
    [SerializeField] private UDPClient udpClient;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;
    [SerializeField] private ClientConnectionUIHandler connectionHandler;

    private ChatController controller;

    private async void Start()
    {
        controller = new ChatController(udpClient, chatUIView);

        inputHandler.Initialize(controller);
        connectionHandler.Initialize(controller);

        await controller.Connect("127.0.0.1", 7777);
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}