using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIView : MonoBehaviour, IChatView
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private GameObject filePrefab;

    [Header("Error Popup")]
    [SerializeField] private ErrorPopupUI errorPopup;

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
        img.preserveAspect = true;

        RectTransform contentRect = content as RectTransform;
        float maxWidth = contentRect.rect.width - 20f;

        float aspectRatio = (float)texture.height / texture.width;
        float calculatedHeight = maxWidth * aspectRatio;

        LayoutElement layout = imageGO.GetComponent<LayoutElement>();
        if (layout == null)
            layout = imageGO.AddComponent<LayoutElement>();

        layout.preferredWidth = maxWidth;
        layout.preferredHeight = calculatedHeight;
    }

    public void DisplayFile(byte[] data, string fileName)
    {
        if (filePrefab == null)
        {
            Debug.LogError("File prefab not assigned in ChatUIView");
            return;
        }

        GameObject fileGO = Instantiate(filePrefab, content);

        FileMessageUI fileUI = fileGO.GetComponent<FileMessageUI>();

        if (fileUI == null)
        {
            Debug.LogError("FileMessageUI component missing in filePrefab");
            return;
        }

        fileUI.Initialize(data, fileName);
    }

    public void ShowError(string message)
    {
        if (errorPopup == null)
        {
            Debug.LogError("ErrorPopupUI not assigned in ChatUIView");
            return;
        }

        errorPopup.Show(message);
    }
}