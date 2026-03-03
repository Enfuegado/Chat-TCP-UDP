using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TCPClient : MonoBehaviour, IClient
{
    public bool HasPayloadSizeLimit => false;
    public int MaxPayloadSize => int.MaxValue;

    private TcpClient tcpClient;
    private NetworkStream networkStream;

    public bool IsConnected { get; private set; }

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public async Task ConnectToServer(string ip, int port)
    {
        if (IsConnected)
            return;

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
        {
            OnError?.Invoke("You must connect before sending a message.");
            return;
        }

        try
        {
            byte[] data = PacketSerializer.Serialize(packet);
            await networkStream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Client] Send failed: {ex.Message}");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (!IsConnected)
            return;

        IsConnected = false;

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