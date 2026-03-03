using System;
using System.Threading.Tasks;

public interface IChatConnection
{
    event Action<NetworkPacket> OnPacketReceived;
    event Action OnConnected;
    event Action OnDisconnected;
    bool IsConnected { get; }

    Task SendMessageAsync(NetworkPacket packet);
    void Disconnect();

    bool HasPayloadSizeLimit { get; }
    int MaxPayloadSize { get; }
}