using System;
using System.Threading.Tasks;

public interface IChatConnection
{
    event Action<NetworkPacket> OnPacketReceived;
    event Action OnConnected;
    event Action OnDisconnected;

    Task SendMessageAsync(NetworkPacket packet);
    void Disconnect();
}