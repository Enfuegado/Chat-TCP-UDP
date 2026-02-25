public class NetworkPacket
{
    public PacketType Type;
    public string FileName; // null para texto
    public byte[] Data;

    public NetworkPacket(PacketType type, byte[] data, string fileName = null)
    {
        Type = type;
        Data = data;
        FileName = fileName;
    }
}