using UnityEngine;

public class ClientBootstrapper : MonoBehaviour
{
    [SerializeField] private MonoBehaviour connectionComponent;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;
    [SerializeField] private ClientConnectionUIHandler connectionHandler;

    private ChatController controller;
    private IClient client;

    private async void Start()
    {
        client = connectionComponent as IClient;

        if (client == null)
        {
            Debug.LogError("Connection component must implement IClient.");
            return;
        }

        controller = new ChatController(client, chatUIView);

        inputHandler.Initialize(controller);
        connectionHandler.Initialize(controller);

        await controller.Connect("127.0.0.1", 7777);
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}