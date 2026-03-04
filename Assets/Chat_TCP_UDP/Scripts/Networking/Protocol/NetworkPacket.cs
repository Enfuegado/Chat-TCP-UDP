using System;
public class NetworkPacket
{
    public PacketType Type { get; }
    public string FileName { get; }
    public byte[] Data { get; }

    public NetworkPacket(PacketType type, byte[] data, string fileName = null)
    {
        if (!Enum.IsDefined(typeof(PacketType), type))
            throw new ArgumentException("Invalid packet type.", nameof(type));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        Type = type;
        Data = data;
        FileName = fileName;
    }
}