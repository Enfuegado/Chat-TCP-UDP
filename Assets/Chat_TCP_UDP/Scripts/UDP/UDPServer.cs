using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    private const int MaxPayloadSize = 60 * 1024;

    public bool IsConnected { get; private set; }

    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    private bool isStarting;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public Task StartServer(int port)
    {
        if (udpServer != null || isStarting)
            return Task.CompletedTask;

        isStarting = true;

        try
        {
            udpServer = new UdpClient(port);
            _ = ReceiveLoop();
        }
        catch (Exception ex)
        {
            OnError?.Invoke("Failed to start UDP server.");
            Debug.LogWarning(ex.Message);
            Disconnect();
        }
        finally
        {
            isStarting = false;
        }

        return Task.CompletedTask;
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (udpServer != null)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                remoteEndPoint = result.RemoteEndPoint;

                NetworkPacket packet =
                    PacketSerializer.Deserialize(result.Buffer);

                if (packet.Type == PacketType.Connect)
                {
                    if (!IsConnected)
                    {
                        IsConnected = true;
                        OnConnected?.Invoke();
                    }

                    var ackPacket = new NetworkPacket(
                        PacketType.Connect,
                        Array.Empty<byte>()
                    );

                    byte[] ackData = PacketSerializer.Serialize(ackPacket);
                    await udpServer.SendAsync(ackData, ackData.Length, remoteEndPoint);

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
            OnError?.Invoke("Connection lost.");
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
            throw new InvalidOperationException("UDP server is not ready.");

        byte[] data = PacketSerializer.Serialize(packet);

        if (data.Length > MaxPayloadSize)
            throw new InvalidOperationException("File exceeds protocol size limit (60KB).");

        await udpServer.SendAsync(data, data.Length, remoteEndPoint);
    }

    public void Disconnect()
    {
        if (udpServer == null && !isStarting)
            return;

        bool wasConnected = IsConnected;

        IsConnected = false;
        isStarting = false;

        udpServer?.Close();
        udpServer?.Dispose();
        udpServer = null;

        if (wasConnected)
            OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}