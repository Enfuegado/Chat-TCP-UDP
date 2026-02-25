using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TCPServer : MonoBehaviour, IServer
{
    private TcpListener tcpListener;
    private TcpClient connectedClient;
    private NetworkStream networkStream;

    public bool isServerRunning { get; private set; }

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task StartServer(int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        Debug.Log("[Server] TCP Server started, waiting for connections...");
        isServerRunning = true;

        connectedClient = await tcpListener.AcceptTcpClientAsync();
        Debug.Log("[Server] Client connected: " + connectedClient.Client.RemoteEndPoint);
        OnConnected?.Invoke();

        networkStream = connectedClient.GetStream();
        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (connectedClient != null && connectedClient.Connected)
            {
                NetworkPacket packet = await PacketSerializer.DeserializeAsync(networkStream);

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
        if (networkStream == null || connectedClient == null || !connectedClient.Connected)
        {
            Debug.Log("[Server] No client connected");
            return;
        }

        byte[] data = PacketSerializer.Serialize(packet);
        await networkStream.WriteAsync(data, 0, data.Length);
    }

    public void Disconnect()
    {
        networkStream?.Close();
        connectedClient?.Close();
        tcpListener?.Stop();

        networkStream = null;
        connectedClient = null;
        tcpListener = null;

        Debug.Log("[Server] TCP Disconnected");
        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}