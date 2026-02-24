using TMPro;
using UnityEngine;

public class ChatUIView : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject messagePrefab;

    public void DisplayMessage(string message)
    {
        GameObject newMessage = Instantiate(messagePrefab, content);
        TMP_Text text = newMessage.GetComponentInChildren<TMP_Text>();
        text.text = message;
    }

}