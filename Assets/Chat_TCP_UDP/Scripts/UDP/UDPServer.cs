using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public bool isServerRunning { get; private set; }

    public Task StartServer(int port)
    {
        udpServer = new UdpClient(port);
        Debug.Log("[Server] UDP Server started. Waiting for messages...");
        isServerRunning = true;

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

                // 🔹 Deserializar paquete
                NetworkPacket packet = DeserializePacket(result.Buffer);

                // 🔹 Handshake especial solo para texto CONNECT
                if (packet.Type == PacketType.Text)
                {
                    string text = System.Text.Encoding.UTF8.GetString(packet.Data);

                    if (text == "CONNECT")
                    {
                        Debug.Log("[Server] Client connected: " + remoteEndPoint);

                        var responsePacket = new NetworkPacket(
                            PacketType.Text,
                            System.Text.Encoding.UTF8.GetBytes("CONNECTED")
                        );

                        await SendMessageAsync(responsePacket);
                        OnConnected?.Invoke();
                        continue;
                    }
                }

                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UDP Server] Receive loop stopped: {ex.Message}");
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
        using (var stream = new System.IO.MemoryStream(buffer))
        {
            return PacketSerializer.DeserializeAsync(stream).Result;
        }
    }

    public void Disconnect()
    {
        if (!isServerRunning)
            return;

        isServerRunning = false;

        udpServer?.Close();
        udpServer?.Dispose();
        udpServer = null;

        Debug.Log("[Server] UDP Disconnected");
        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}