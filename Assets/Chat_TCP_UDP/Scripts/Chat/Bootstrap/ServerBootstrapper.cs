using UnityEngine;

public class ServerBootstrapper : MonoBehaviour
{
    [SerializeField] private MonoBehaviour connectionComponent;
    [SerializeField] private ChatUIView chatUIView;
    [SerializeField] private ChatInputHandler inputHandler;

    private ChatController controller;
    private IServer server;

    private async void Start()
    {
        server = connectionComponent as IServer;

        if (server == null)
        {
            Debug.LogError("Connection component must implement IServer.");
            return;
        }

        controller = new ChatController(server, chatUIView);

        inputHandler.Initialize(controller);

        await server.StartServer(7777);
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}