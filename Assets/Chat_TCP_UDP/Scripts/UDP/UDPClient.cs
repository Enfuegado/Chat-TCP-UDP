using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public bool isConnected { get; private set; }

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task ConnectToServer(string ipAddress, int port)
    {
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        isConnected = true;
        _ = ReceiveLoop();

        // 🔹 Handshake usando NetworkPacket
        var connectPacket = new NetworkPacket(
            PacketType.Text,
            System.Text.Encoding.UTF8.GetBytes("CONNECT")
        );

        await SendMessageAsync(connectPacket);
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isConnected)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();

                NetworkPacket packet = DeserializePacket(result.Buffer);

                // 🔹 Handshake especial
                if (packet.Type == PacketType.Text)
                {
                    string text = System.Text.Encoding.UTF8.GetString(packet.Data);

                    if (text == "CONNECTED")
                    {
                        Debug.Log("[Client] UDP Server answered");
                        OnConnected?.Invoke();
                        continue;
                    }
                }

                OnPacketReceived?.Invoke(packet);
            }
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
        if (!isConnected)
        {
            Debug.Log("[Client] Not connected to server.");
            return;
        }

        byte[] data = PacketSerializer.Serialize(packet);
        await udpClient.SendAsync(data, data.Length, remoteEndPoint);
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
        if (!isConnected)
            return;

        isConnected = false;

        udpClient?.Close();
        udpClient?.Dispose();
        udpClient = null;

        Debug.Log("[Client] UDP Disconnected");
        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}