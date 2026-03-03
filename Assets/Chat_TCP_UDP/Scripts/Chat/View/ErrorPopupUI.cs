using UnityEngine;
using TMPro;

public class ErrorPopupUI : MonoBehaviour
{
    public GameObject popupRoot;
    public TMP_Text messageText;

    public void Show(string message)
    {
        messageText.text = message;
        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        popupRoot.SetActive(false);
    }
}