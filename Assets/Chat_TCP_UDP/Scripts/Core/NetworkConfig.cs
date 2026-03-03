using UnityEngine;

public enum ProtocolType
{
    TCP,
    UDP
}

public enum Role
{
    Client,
    Server
}

public enum AppMode
{
    Single,
    LocalTest
}

public class NetworkConfig : MonoBehaviour
{
    public static NetworkConfig Instance;

    public ProtocolType Protocol = ProtocolType.TCP;
    public Role Role;
    public AppMode Mode = AppMode.Single;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}