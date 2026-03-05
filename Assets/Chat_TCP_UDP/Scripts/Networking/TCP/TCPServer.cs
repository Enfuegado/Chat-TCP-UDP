using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TCPServer : MonoBehaviour, IServer
{
    public bool IsConnected { get; private set; }

    private TcpListener tcpListener;
    private TcpClient connectedClient;
    private NetworkStream networkStream;

    private bool isStarting;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public async Task StartServer(int port)
    {
        if (IsConnected || isStarting)
            return;

        isStarting = true;

        try
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();

            connectedClient = await tcpListener.AcceptTcpClientAsync();
            networkStream = connectedClient.GetStream();

            IsConnected = true;
            OnConnected?.Invoke();

            _ = ReceiveLoop();
        }
        catch
        {
            OnError?.Invoke("Server failed to start.");
            Disconnect();
        }
        finally
        {
            isStarting = false;
        }
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
        catch
        {
            if (IsConnected)
                OnError?.Invoke("Connection lost.");
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(NetworkPacket packet)
    {
        if (!IsConnected || networkStream == null)
            throw new InvalidOperationException("TCP server is not connected.");

        try
        {
            byte[] data = PacketSerializer.Serialize(packet);
            await networkStream.WriteAsync(data, 0, data.Length);
        }
        catch
        {
            OnError?.Invoke("Failed to send message.");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (!IsConnected && tcpListener == null && !isStarting)
            return;

        IsConnected = false;
        isStarting = false;

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