using UnityEngine;
using TMPro;
using System.Text;

public class UdpServerUI : MonoBehaviour
{
    public int serverPort = 5555;

    [SerializeField] private UDPServer serverReference;
    [SerializeField] private TMP_InputField messageInput;

    private IServer _server;

    void Awake()
    {
        _server = serverReference;
    }

    void Start()
    {
        _server.OnPacketReceived += HandlePacketReceived;
        _server.OnConnected += HandleConnection;
        _server.OnDisconnected += HandleDisconnection;
    }

    public void StartServer()
    {
        _server.StartServer(serverPort);
    }

    public void SendServerMessage()
    {
        if (!_server.isServerRunning)
        {
            Debug.Log("The server is not running");
            return;
        }

        if (string.IsNullOrEmpty(messageInput.text))
        {
            Debug.Log("The chat entry is empty");
            return;
        }

        string message = messageInput.text;

        var packet = new NetworkPacket(
            PacketType.Text,
            Encoding.UTF8.GetBytes(message)
        );

        _server.SendMessageAsync(packet);
    }

    void HandlePacketReceived(NetworkPacket packet)
    {
        if (packet.Type == PacketType.Text)
        {
            string text = Encoding.UTF8.GetString(packet.Data);
            Debug.Log("[UI-Server] Message received from client: " + text);
        }
    }

    void HandleConnection()
    {
        Debug.Log("[UI-Server] Client Connected to Server");
    }

    void HandleDisconnection()
    {
        Debug.Log("[UI-Server] Client Disconnect from Server");
    }
}