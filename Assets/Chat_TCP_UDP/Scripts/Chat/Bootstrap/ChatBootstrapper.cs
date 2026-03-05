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

    private ProtocolType currentProtocol = ProtocolType.TCP;

    private Coroutine activeRoutine;

    void Start()
    {
        activeRoutine = StartCoroutine(StartDual());
    }

    IEnumerator StartDual()
    {
        CreateServer();
        CreateClient();
        yield break;
    }

    void CreateServer()
    {
        GameObject prefab =
            currentProtocol == ProtocolType.TCP
            ? tcpServerPrefab
            : udpServerPrefab;

        GameObject instance = Instantiate(prefab);
        activeInstances.Add(instance);
    }

    void CreateClient()
    {
        GameObject prefab =
            currentProtocol == ProtocolType.TCP
            ? tcpClientPrefab
            : udpClientPrefab;

        GameObject instance = Instantiate(prefab);
        activeInstances.Add(instance);
    }

    public void SwitchProtocol()
    {
        currentProtocol =
            currentProtocol == ProtocolType.TCP
            ? ProtocolType.UDP
            : ProtocolType.TCP;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(SwitchAfterCleanup());
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

        activeRoutine = StartCoroutine(StartDual());
    }
}