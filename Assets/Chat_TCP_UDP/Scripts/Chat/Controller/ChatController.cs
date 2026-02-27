using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

public class ChatController
{
    private readonly IChatConnection connection;
    private readonly IChatView view;

    public ChatController(IChatConnection connection, IChatView view)
    {
        this.connection = connection;
        this.view = view;

        connection.OnPacketReceived += HandlePacketReceived;
    }

    public async Task Connect(string ip, int port)
    {
        if (connection is IClient client)
        {
            await client.ConnectToServer(ip, port);
        }
    }

    public async Task SendTextMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var packet = new NetworkPacket(
            PacketType.Text,
            Encoding.UTF8.GetBytes(message),
            null
        );

        view.DisplayText(message);
        await connection.SendMessageAsync(packet);
    }

    public async Task SendImage(string path)
    {
        if (!File.Exists(path))
            return;

        byte[] data = await File.ReadAllBytesAsync(path);

        if (connection.HasPayloadSizeLimit && data.Length > connection.MaxPayloadSize)
        {
            Debug.LogWarning("Payload is too large for this protocol.");
            return;
        }

        string fileName = Path.GetFileName(path);

        var packet = new NetworkPacket(PacketType.Image, data, fileName);

        view.DisplayImage(data);

        await connection.SendMessageAsync(packet);
    }

    public async Task SendFile(string path)
    {
        if (!File.Exists(path))
            return;

        byte[] data = await File.ReadAllBytesAsync(path);

        if (connection.HasPayloadSizeLimit && data.Length > connection.MaxPayloadSize)
        {
            Debug.LogWarning("Payload is too large for this protocol.");
            return;
        }

        string fileName = Path.GetFileName(path);

        var packet = new NetworkPacket(PacketType.File, data, fileName);

        view.DisplayFile(data, fileName);

        await connection.SendMessageAsync(packet);
    }

    private void HandlePacketReceived(NetworkPacket packet)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            switch (packet.Type)
            {
                case PacketType.Text:
                    string text = Encoding.UTF8.GetString(packet.Data);
                    view.DisplayText(text);
                    break;

                case PacketType.Image:
                    view.DisplayImage(packet.Data);
                    break;

                case PacketType.File:
                    view.DisplayFile(packet.Data, packet.FileName);
                    break;

            }
        });
    }
}