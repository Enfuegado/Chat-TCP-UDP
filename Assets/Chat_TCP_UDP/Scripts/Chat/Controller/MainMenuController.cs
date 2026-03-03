using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public TMP_Dropdown protocolDropdown;
    public TMP_Dropdown roleDropdown;
    public Toggle localTestToggle;

    void Start()
    {
        if (localTestToggle != null)
        {
            localTestToggle.onValueChanged.AddListener(OnModeChanged);
            OnModeChanged(localTestToggle.isOn);
        }
    }

    void OnModeChanged(bool isIndividualMode)
    {
        if (roleDropdown != null)
        {
            roleDropdown.interactable = isIndividualMode;
        }
    }

    public void StartApp()
    {
        NetworkConfig.Instance.Protocol =
            (ProtocolType)protocolDropdown.value;

        NetworkConfig.Instance.Mode =
            (localTestToggle != null && localTestToggle.isOn)
            ? AppMode.Single
            : AppMode.LocalTest;

        if (NetworkConfig.Instance.Mode == AppMode.Single)
        {
            NetworkConfig.Instance.Role =
                (Role)roleDropdown.value;
        }

        SceneManager.LoadScene("ChatScene");
    }
}