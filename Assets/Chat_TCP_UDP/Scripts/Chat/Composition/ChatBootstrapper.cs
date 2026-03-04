using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChatBootstrapper : MonoBehaviour
{
    public GameObject tcpClientPrefab;
    public GameObject tcpServerPrefab;
    public GameObject udpClientPrefab;
    public GameObject udpServerPrefab;

    private readonly List<GameObject> activeInstances = new List<GameObject>();

    void Start()
    {
        if (NetworkConfig.Instance == null)
        {
            Debug.LogError("NetworkConfig not found.");
            return;
        }

        if (NetworkConfig.Instance.Mode == AppMode.Single)
        {
            CreateInstance(NetworkConfig.Instance.Role);
        }
        else
        {
            StartCoroutine(StartLocalTest());
        }
    }

    void CreateInstance(Role role)
    {
        GameObject instance;

        if (NetworkConfig.Instance.Protocol == ProtocolType.TCP)
        {
            instance = (role == Role.Server)
                ? Instantiate(tcpServerPrefab)
                : Instantiate(tcpClientPrefab);
        }
        else
        {
            instance = (role == Role.Server)
                ? Instantiate(udpServerPrefab)
                : Instantiate(udpClientPrefab);
        }

        activeInstances.Add(instance);
    }

    public void SwitchProtocol()
    {
        NetworkConfig.Instance.Protocol =
            (NetworkConfig.Instance.Protocol == ProtocolType.TCP)
            ? ProtocolType.UDP
            : ProtocolType.TCP;

        StartCoroutine(SwitchAfterCleanup());
    }

    public void ReturnToMenu()
    {
        StartCoroutine(ReturnAfterCleanup());
    }

    private IEnumerator SwitchAfterCleanup()
    {
        foreach (var instance in activeInstances)
        {
            if (instance != null)
            {
                var connection = instance.GetComponent<IChatConnection>();
                connection?.Disconnect();
            }
        }

        yield return null;

        foreach (var instance in activeInstances)
        {
            if (instance != null)
                Destroy(instance);
        }

        activeInstances.Clear();

        if (NetworkConfig.Instance.Mode == AppMode.Single)
        {
            CreateInstance(NetworkConfig.Instance.Role);
        }
        else
        {
            StartCoroutine(StartLocalTest());
        }
    }

    private IEnumerator ReturnAfterCleanup()
    {
        foreach (var instance in activeInstances)
        {
            if (instance != null)
            {
                var connection = instance.GetComponent<IChatConnection>();
                connection?.Disconnect();
            }
        }

        yield return null;

        foreach (var instance in activeInstances)
        {
            if (instance != null)
                Destroy(instance);
        }

        activeInstances.Clear();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    IEnumerator StartLocalTest()
    {
        CreateInstance(Role.Server);
        yield return new WaitForSeconds(0.3f);
        CreateInstance(Role.Client);
    }
}