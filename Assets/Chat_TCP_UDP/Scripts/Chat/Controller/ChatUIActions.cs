using UnityEngine;

public class ChatUIActions : MonoBehaviour
{
    public void SwitchProtocol()
    {
        ChatBootstrapper bootstrapper = FindObjectOfType<ChatBootstrapper>();

        if (bootstrapper != null)
        {
            bootstrapper.SwitchProtocol();
        }
        else
        {
            Debug.LogError("ChatBootstrapper not found.");
        }
    }
    public void ReturnToMenu()
{
    ChatBootstrapper bootstrapper = FindObjectOfType<ChatBootstrapper>();

    if (bootstrapper != null)
    {
        bootstrapper.ReturnToMenu();
    }
    else
    {
        Debug.LogError("ChatBootstrapper not found.");
    }
}
}