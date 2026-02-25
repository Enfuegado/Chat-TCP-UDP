using UnityEngine;

public class ServerBootstrapper : MonoBehaviour
{
    [SerializeField] private TCPServer tcpServer;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ServerInputHandler inputHandler;

    private ChatController controller;

    void Start()
    {
        controller = new ChatController(tcpServer, chatUIView);
        inputHandler.Initialize(controller);
    }
}