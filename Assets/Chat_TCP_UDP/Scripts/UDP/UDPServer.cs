using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    public bool HasPayloadSizeLimit => true;
    public int MaxPayloadSize => 60 * 1024;
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public bool isServerRunning { get; private set; }

    public Task StartServer(int port)
    {
        udpServer = new UdpClient(port);
        isServerRunning = true;

        Debug.Log("[UDP Server] Started on port " + port);

        _ = ReceiveLoop();
        return Task.CompletedTask;
    }

private async Task ReceiveLoop()
{
    try
    {
        while (isServerRunning)
        {
            UdpReceiveResult result = await udpServer.ReceiveAsync();
            remoteEndPoint = result.RemoteEndPoint;

            NetworkPacket packet = PacketSerializer.Deserialize(result.Buffer);

            if (packet.Type == PacketType.Text)
            {
                string text = System.Text.Encoding.UTF8.GetString(packet.Data);

                if (text == "CONNECT")
                {
                    Debug.Log("[UDP Server] Client registered: " + remoteEndPoint);
                    continue;
                }
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
        if (!isServerRunning || remoteEndPoint == null)
            return;

        byte[] data = PacketSerializer.Serialize(packet);
        await udpServer.SendAsync(data, data.Length, remoteEndPoint);
    }

    private NetworkPacket DeserializePacket(byte[] buffer)
    {
        return PacketSerializer.Deserialize(buffer);
    }

    public void Disconnect()
    {
        if (!isServerRunning)
            return;

        isServerRunning = false;

        udpServer?.Close();
        udpServer?.Dispose();
        udpServer = null;

        Debug.Log("[UDP Server] Disconnected");
        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}