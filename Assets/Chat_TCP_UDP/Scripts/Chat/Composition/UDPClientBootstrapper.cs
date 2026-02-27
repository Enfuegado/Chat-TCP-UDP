using UnityEngine;

public class UDPClientBootstrapper : MonoBehaviour
{
    [SerializeField] private UDPClient udpClient;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;
    [SerializeField] private ClientConnectionUIHandler connectionHandler;

    private ChatController controller;

    void Start()
    {
        controller = new ChatController(udpClient, chatUIView);

        inputHandler.Initialize(controller);
        connectionHandler.Initialize(controller);
    }
}