using UnityEngine;
using TMPro;
using System.IO;
using System.Threading.Tasks;

public class ChatUIInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    private ChatController controller;

    public void Initialize(ChatController controller)
    {
        this.controller = controller;
    }

    public async void OnSendButtonClicked()
    {
        if (controller == null)
            return;

        string message = inputField.text;

        await controller.SendTextMessage(message);

        inputField.text = "";
    }
    public async void OnSendImageClicked()
    {
        if (controller == null)
            return;

        string path = Path.Combine(Application.dataPath, "Chat_TCP_UDP/StreamingAssets/test.png");

        if (!File.Exists(path))
        {
            Debug.LogWarning("Image not found in StreamingAssets");
            return;
        }

        await controller.SendImage(path);
    }
}