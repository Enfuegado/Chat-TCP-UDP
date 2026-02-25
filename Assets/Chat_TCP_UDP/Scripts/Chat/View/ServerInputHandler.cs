using TMPro;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class ServerInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private string imageFileName = "test.png"; //change name for other image

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

        if (string.IsNullOrWhiteSpace(message))
            return;

        await controller.SendTextMessage(message);

        inputField.text = "";
    }

    public async void OnSendImageClicked()
    {
        if (controller == null)
            return;

        string path = Path.Combine(
            Application.dataPath,
            "Chat_TCP_UDP/StreamingAssets",
            imageFileName
        );

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Image not found at: {path}");
            return;
        }

        await controller.SendImage(path);
    }
}