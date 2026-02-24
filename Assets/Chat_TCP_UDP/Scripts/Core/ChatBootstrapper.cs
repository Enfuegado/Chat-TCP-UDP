using UnityEngine;

public class ChatBootstrapper : MonoBehaviour
{
    [SerializeField] private TCPClient tcpClient;
    [SerializeField] private ChatUIView chatUIView;

    private ChatController controller;

    void Start()
    {
        controller = new ChatController(tcpClient, chatUIView);
    }
}