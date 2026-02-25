using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIView : MonoBehaviour, IChatView
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject imagePrefab;

    public void DisplayText(string message)
    {
        GameObject newMessage = Instantiate(messagePrefab, content);
        TMP_Text text = newMessage.GetComponent<TMP_Text>();
        text.text = message;
    }

    public void DisplayImage(byte[] data)
    {
        Texture2D texture = new Texture2D(2, 2);

        if (!texture.LoadImage(data))
        {
            Debug.LogWarning("Failed to load image data");
            return;
        }

        GameObject imageGO = Instantiate(imagePrefab, content);
        Image img = imageGO.GetComponent<Image>();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        img.sprite = sprite;
        img.SetNativeSize();
    }

    public void DisplayAudio(byte[] data, string fileName)
    {
        Debug.Log($"[UI] Audio received ({fileName}) - {data.Length} bytes");
    }

    public void DisplayFile(byte[] data, string fileName)
    {
        Debug.Log($"[UI] File received ({fileName}) - {data.Length} bytes");
    }

}