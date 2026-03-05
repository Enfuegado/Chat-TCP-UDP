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

            _ = ReceiveLoop();
            _ = ConnectionRetryLoop();
        }
        catch
        {
            OnError?.Invoke("Failed to start client.");
            Disconnect();
        }
    }

    private async Task ConnectionRetryLoop()
    {
        while (!IsConnected && udpClient != null)
        {
            try
            {
                var connectPacket = new NetworkPacket(
                    PacketType.Connect,
                    Array.Empty<byte>()
                );

                await SendRaw(connectPacket);
            }
            catch { }

            await Task.Delay(1000);
        }

        isConnecting = false;
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (udpClient != null)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();

                NetworkPacket packet =
                    PacketSerializer.Deserialize(result.Buffer);

                if (!IsConnected)
                {
                    IsConnected = true;
                    OnConnected?.Invoke();
                }

                OnPacketReceived?.Invoke(packet);
            }
        }
        catch (ObjectDisposedException)
        {
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
        if (!IsConnected || udpClient == null)
            throw new InvalidOperationException("UDP client is not connected.");

        await SendRaw(packet);
    }

    private async Task SendRaw(NetworkPacket packet)
    {
        byte[] data = PacketSerializer.Serialize(packet);

        if (data.Length > MaxPayloadSize)
            throw new InvalidOperationException("File exceeds protocol size limit (60KB).");

        await udpClient.SendAsync(data, data.Length, remoteEndPoint);
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