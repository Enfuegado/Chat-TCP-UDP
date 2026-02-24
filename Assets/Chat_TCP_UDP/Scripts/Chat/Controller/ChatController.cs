using System;

public class ChatController
{
    private readonly IClient client;
    private readonly ChatUIView view;

    public ChatController(IClient client, ChatUIView view)
    {
        this.client = client;
        this.view = view;

        client.OnMessageReceived += HandleMessageReceived;
    }

    private void HandleMessageReceived(string message)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            view.DisplayMessage(message);
        });
    }
}