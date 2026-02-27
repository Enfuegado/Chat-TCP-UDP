using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    public bool HasPayloadSizeLimit => true;
    public int MaxPayloadSize => 60 * 1024;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public bool isConnected { get; private set; }

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task ConnectToServer(string ipAddress, int port)
    {
        if (isConnected)
            return;

        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        isConnected = true;

        Debug.Log("[UDP Client] Ready to communicate with server");

        _ = ReceiveLoop();

        var connectPacket = new NetworkPacket(
            PacketType.Text,
            System.Text.Encoding.UTF8.GetBytes("CONNECT"),
            null
        );

        await SendMessageAsync(connectPacket);

        OnConnected?.Invoke();
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isConnected)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();

                NetworkPacket packet = PacketSerializer.Deserialize(result.Buffer);

                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (ObjectDisposedException)
        {

        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UDP Client] Receive loop stopped: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(NetworkPacket packet)
    {
        if (!isConnected || udpClient == null)
        {
            Debug.Log("[UDP Client] Not connected.");
            return;
        }

        byte[] data = PacketSerializer.Serialize(packet);

        await udpClient.SendAsync(data, data.Length, remoteEndPoint);
    }

    public void Disconnect()
    {
        if (!isConnected)
            return;

        isConnected = false;

        udpClient?.Close();
        udpClient = null;

        Debug.Log("[UDP Client] Disconnected");
        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}