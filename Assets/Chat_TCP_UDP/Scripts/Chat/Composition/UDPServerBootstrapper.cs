using UnityEngine;

public class UDPServerBootstrapper : MonoBehaviour
{
    [SerializeField] private UDPServer udpServer;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;

    private ChatController controller;

    void Start()
    {
        controller = new ChatController(udpServer, chatUIView);
        inputHandler.Initialize(controller);
    }
    private void OnDestroy()
    {
        controller?.Dispose();
    }
}