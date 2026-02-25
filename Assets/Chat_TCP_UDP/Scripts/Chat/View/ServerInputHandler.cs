using TMPro;
using UnityEngine;
using System.Threading.Tasks;

public class ServerInputHandler : MonoBehaviour
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

        await controller.SendTextMessage(inputField.text);

        inputField.text = "";
    }
}