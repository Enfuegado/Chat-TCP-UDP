using UnityEngine;

public class TCPServerBootstrapper : MonoBehaviour
{
    [SerializeField] private TCPServer tcpServer;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;

    private ChatController controller;

    void Start()
    {
        controller = new ChatController(tcpServer, chatUIView);
        inputHandler.Initialize(controller);
    }
}