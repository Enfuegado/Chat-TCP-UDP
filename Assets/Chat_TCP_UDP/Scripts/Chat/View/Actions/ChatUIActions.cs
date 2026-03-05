using UnityEngine;

public class ChatUIActions : MonoBehaviour
{
    private ChatBootstrapper bootstrapper;

    private void Awake()
    {
        bootstrapper = FindObjectOfType<ChatBootstrapper>();

        if (bootstrapper == null)
        {
            Debug.LogError("ChatBootstrapper not found in scene.");
        }
    }

    public void SwitchProtocol()
    {
        if (bootstrapper != null)
        {
            bootstrapper.SwitchProtocol();
        }
    }
}