using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    private const int MaxPayloadSize = 60 * 1024;

    public bool IsConnected { get; private set; }

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    private bool isConnecting;

    public event Action<NetworkPacket> OnPacketReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public async Task ConnectToServer(string ipAddress, int port)
    {
        if (IsConnected || isConnecting)
            return;

        isConnecting = true;

        try
        {
            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            IsConnected = true;

            _ = ReceiveLoop();

            var connectPacket = new NetworkPacket(
                PacketType.Connect,
                Array.Empty<byte>()
            );

            await SendMessageAsync(connectPacket);

            OnConnected?.Invoke();
        }
        catch (Exception ex)
        {
            OnError?.Invoke("Failed to connect.");
            Debug.LogWarning(ex.Message);
            Disconnect();
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
            while (IsConnected)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();

                NetworkPacket packet =
                    PacketSerializer.Deserialize(result.Buffer);

                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            OnError?.Invoke("Connection lost.");
            Debug.LogWarning($"[UDP Client] Receive loop stopped: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public async Task SendMessageAsync(NetworkPacket packet)
    {
        if (!IsConnected || udpClient == null)
            throw new InvalidOperationException("UDP client is not connected.");

        byte[] data = PacketSerializer.Serialize(packet);

        if (data.Length > MaxPayloadSize)
            throw new InvalidOperationException("File exceeds protocol size limit (60KB).");

        try
        {
            await udpClient.SendAsync(data, data.Length, remoteEndPoint);
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
        if (!IsConnected && !isConnecting)
            return;

        IsConnected = false;
        isConnecting = false;

        udpClient?.Close();
        udpClient = null;

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}