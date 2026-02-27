using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TCPClient : MonoBehaviour, IClient
{
    public bool HasPayloadSizeLimit => false;
    public int MaxPayloadSize => int.MaxValue;
    private TcpClient tcpClient;
    private NetworkStream networkStream;

    public bool isConnected { get; private set; }

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task ConnectToServer(string ip, int port)
    {
        tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(ip, port);

        networkStream = tcpClient.GetStream();
        isConnected = true;

        Debug.Log("[Client] Connected to server");
        OnConnected?.Invoke();

        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (tcpClient != null && tcpClient.Connected)
            {
                NetworkPacket packet = await PacketSerializer.DeserializeAsync(networkStream);
                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Client] Receive loop stopped: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(NetworkPacket packet)
    {
        if (!isConnected || networkStream == null)
        {
            Debug.Log("[Client] Not connected to server");
            return;
        }

        byte[] data = PacketSerializer.Serialize(packet);
        await networkStream.WriteAsync(data, 0, data.Length);
    }

    public void Disconnect()
    {
        if (!isConnected)
            return;

        isConnected = false;

        networkStream?.Close();
        tcpClient?.Close();

        networkStream = null;
        tcpClient = null;

        Debug.Log("[Client] Disconnected");
        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}