using UnityEngine;

public class TCPClientBootstrapper : MonoBehaviour
{
    [SerializeField] private TCPClient tcpClient;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;
    [SerializeField] private ClientConnectionUIHandler connectionHandler;

    private ChatController controller;

    void Start()
    {
        controller = new ChatController(tcpClient, chatUIView);

        inputHandler.Initialize(controller);
        connectionHandler.Initialize(controller);
    }
        private void OnDestroy()
    {
        controller?.Dispose();
    }
}