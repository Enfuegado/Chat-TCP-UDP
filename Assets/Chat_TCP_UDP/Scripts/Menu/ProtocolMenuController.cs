using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtocolMenuController : MonoBehaviour
{
    public void StartTCP()
    {
        ProtocolSelection.SelectedProtocol = ProtocolType.TCP;
        SceneManager.LoadScene("ChatScene");
    }

    public void StartUDP()
    {
        ProtocolSelection.SelectedProtocol = ProtocolType.UDP;
        SceneManager.LoadScene("ChatScene");
    }
}