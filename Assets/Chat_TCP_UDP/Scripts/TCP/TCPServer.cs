using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TCPServer : MonoBehaviour, IServer
{
    public bool HasPayloadSizeLimit => false;
    public int MaxPayloadSize => int.MaxValue;
    public bool IsConnected { get; private set; }

    private TcpListener tcpListener;
    private TcpClient connectedClient;
    private NetworkStream networkStream;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task StartServer(int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        Debug.Log("[Server] TCP Server started, waiting for connections...");

        connectedClient = await tcpListener.AcceptTcpClientAsync();
        networkStream = connectedClient.GetStream();

        IsConnected = true;
        OnConnected?.Invoke();

        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (IsConnected && connectedClient != null && connectedClient.Connected)
            {
                NetworkPacket packet =
                    await PacketSerializer.DeserializeAsync(networkStream);

                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TCP Server] Receive loop stopped: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(NetworkPacket packet)
    {
        if (!IsConnected || networkStream == null)
            return;

        byte[] data = PacketSerializer.Serialize(packet);
        await networkStream.WriteAsync(data, 0, data.Length);
    }

    public void Disconnect()
    {
        if (!IsConnected && tcpListener == null)
            return;

        IsConnected = false; 

        try { networkStream?.Close(); } catch { }
        try { connectedClient?.Close(); } catch { }
        try { tcpListener?.Stop(); } catch { }

        networkStream = null;
        connectedClient = null;
        tcpListener = null;

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}