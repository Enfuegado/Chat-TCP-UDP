using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TCPClient : MonoBehaviour, IClient
{
    private TcpClient tcpClient;
    private NetworkStream networkStream;

    private bool isConnecting;

    public bool IsConnected { get; private set; }

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public async Task ConnectToServer(string ip, int port)
    {
        if (IsConnected || isConnecting)
            return;

        isConnecting = true;

        try
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);

            networkStream = tcpClient.GetStream();
            IsConnected = true;

            Debug.Log("[Client] Connected to server");
            OnConnected?.Invoke();

            _ = ReceiveLoop();
        }
        catch (Exception ex)
        {
            OnError?.Invoke("Connection failed.");
            Debug.LogWarning(ex.Message);
            Cleanup();
        }
        finally
        {
            isConnecting = false;
        }
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (IsConnected && tcpClient != null && tcpClient.Connected)
            {
                NetworkPacket packet =
                    await PacketSerializer.DeserializeAsync(networkStream);

                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke("Connection lost.");
            Debug.LogWarning($"[Client] Receive loop stopped: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(NetworkPacket packet)
    {
        if (!IsConnected || networkStream == null)
            throw new InvalidOperationException("TCP client is not connected.");

        try
        {
            byte[] data = PacketSerializer.Serialize(packet);
            await networkStream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            OnError?.Invoke("Failed to send message.");
            Debug.LogWarning(ex.Message);
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (!IsConnected && tcpClient == null)
            return;

        IsConnected = false;
        isConnecting = false;

        Cleanup();

        Debug.Log("[Client] Disconnected");
        OnDisconnected?.Invoke();
    }

    private void Cleanup()
    {
        try { networkStream?.Close(); } catch { }
        try { tcpClient?.Close(); } catch { }

        networkStream = null;
        tcpClient = null;
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}