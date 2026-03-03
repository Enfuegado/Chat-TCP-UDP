using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    public bool HasPayloadSizeLimit => true;
    public int MaxPayloadSize => 60 * 1024;

    public bool IsConnected { get; private set; }

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task ConnectToServer(string ipAddress, int port)
    {
        if (IsConnected)
            return;

        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        IsConnected = true;

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
            while (IsConnected)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();

                NetworkPacket packet =
                    PacketSerializer.Deserialize(result.Buffer);

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
        if (!IsConnected || udpClient == null)
            return;

        byte[] data = PacketSerializer.Serialize(packet);

        if (data.Length > MaxPayloadSize)
            return;

        await udpClient.SendAsync(data, data.Length, remoteEndPoint);
    }

    public void Disconnect()
    {
        if (!IsConnected)
            return;

        IsConnected = false;

        udpClient?.Close();
        udpClient = null;

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}