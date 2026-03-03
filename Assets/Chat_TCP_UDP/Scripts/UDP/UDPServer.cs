using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    public bool HasPayloadSizeLimit => true;
    public int MaxPayloadSize => 60 * 1024;

    public bool IsConnected { get; private set; }

    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public Task StartServer(int port)
    {
        udpServer = new UdpClient(port);
        IsConnected = true;

        _ = ReceiveLoop();
        OnConnected?.Invoke();

        return Task.CompletedTask;
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (IsConnected)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                remoteEndPoint = result.RemoteEndPoint;

                NetworkPacket packet =
                    PacketSerializer.Deserialize(result.Buffer);

                if (packet.Type == PacketType.Text)
                {
                    string text =
                        System.Text.Encoding.UTF8.GetString(packet.Data);

                    if (text == "CONNECT")
                        continue;
                }

                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[UDP Server] Receive loop stopped: " + ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(NetworkPacket packet)
    {
        if (!IsConnected || remoteEndPoint == null)
            return;

        byte[] data = PacketSerializer.Serialize(packet);

        if (data.Length > MaxPayloadSize)
            return;

        await udpServer.SendAsync(data, data.Length, remoteEndPoint);
    }

    public void Disconnect()
    {
        if (!IsConnected)
            return;

        IsConnected = false;

        udpServer?.Close();
        udpServer?.Dispose();
        udpServer = null;

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}