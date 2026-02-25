using UnityEngine;
using TMPro;
using System.IO;
using System.Threading.Tasks;
using SFB;

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

        if (string.IsNullOrWhiteSpace(message))
            return;

        await controller.SendTextMessage(message);

        inputField.text = "";
    }
    public async void OnSendImageClicked()
    {
        if (controller == null)
            return;

        var extensions = new[]
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Image",
            "",
            extensions,
            false
        );

        if (paths == null || paths.Length == 0)
            return;

        string path = paths[0];

        if (!File.Exists(path))
            return;

        await controller.SendImage(path);
    }
    public async void OnAttachFileClicked()
    {
        if (controller == null)
            return;

        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select File",
            "",
            "",
            false
        );

        if (paths == null || paths.Length == 0)
            return;

        string path = paths[0];

        if (!File.Exists(path))
            return;

        await controller.SendFile(path);
    }
}