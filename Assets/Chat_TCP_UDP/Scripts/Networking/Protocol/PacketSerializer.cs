using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public static class PacketSerializer
{
    public static byte[] Serialize(NetworkPacket packet)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write((byte)packet.Type);

        if (packet.FileName == null)
        {
            writer.Write(0);
        }
        else
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(packet.FileName);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
        }

        writer.Write(packet.Data.Length);
        writer.Write(packet.Data);

        return ms.ToArray();
    }

    public static async Task<NetworkPacket> DeserializeAsync(Stream stream)
    {
        byte[] typeBuffer = await ReadExactAsync(stream, 1);
        PacketType type = (PacketType)typeBuffer[0];

        byte[] nameLengthBuffer = await ReadExactAsync(stream, 4);
        int nameLength = BitConverter.ToInt32(nameLengthBuffer, 0);

        string fileName = null;

        if (nameLength > 0)
        {
            byte[] nameBytes = await ReadExactAsync(stream, nameLength);
            fileName = Encoding.UTF8.GetString(nameBytes);
        }

        byte[] dataLengthBuffer = await ReadExactAsync(stream, 4);
        int dataLength = BitConverter.ToInt32(dataLengthBuffer, 0);

        byte[] data = await ReadExactAsync(stream, dataLength);

        return new NetworkPacket(type, data, fileName);
    }

    private static async Task<byte[]> ReadExactAsync(Stream stream, int size)
    {
        byte[] buffer = new byte[size];
        int totalRead = 0;

        while (totalRead < size)
        {
            int read = await stream.ReadAsync(buffer, totalRead, size - totalRead);

            if (read == 0)
                throw new EndOfStreamException();

            totalRead += read;
        }

        return buffer;
    }
    public static NetworkPacket Deserialize(byte[] buffer)
{
    using var ms = new MemoryStream(buffer);
    using var reader = new BinaryReader(ms);

    PacketType type = (PacketType)reader.ReadByte();

    int nameLength = reader.ReadInt32();
    string fileName = null;

    if (nameLength > 0)
    {
        byte[] nameBytes = reader.ReadBytes(nameLength);
        fileName = Encoding.UTF8.GetString(nameBytes);
    }

    int dataLength = reader.ReadInt32();
    byte[] data = reader.ReadBytes(dataLength);

    return new NetworkPacket(type, data, fileName);
}
}