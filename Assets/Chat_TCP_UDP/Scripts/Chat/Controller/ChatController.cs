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
            await client.ConnectToServer(ip, port);
    }

    public async Task SendTextMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (!connection.IsConnected)
        {
            view.ShowError("You must connect before sending a message.");
            return;
        }

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

        if (!connection.IsConnected)
        {
            view.ShowError("You must connect before sending an image.");
            return;
        }

        byte[] data = await File.ReadAllBytesAsync(path);

        if (connection.HasPayloadSizeLimit && data.Length > connection.MaxPayloadSize)
        {
            view.ShowError("File exceeds protocol size limit.");
            return;
        }

        var packet = new NetworkPacket(
            PacketType.Image,
            data,
            Path.GetFileName(path)
        );

        view.DisplayImage(data);
        await connection.SendMessageAsync(packet);
    }

    public async Task SendFile(string path)
    {
        if (!File.Exists(path))
            return;

        if (!connection.IsConnected)
        {
            view.ShowError("You must connect before sending a file.");
            return;
        }

        byte[] data = await File.ReadAllBytesAsync(path);

        if (connection.HasPayloadSizeLimit && data.Length > connection.MaxPayloadSize)
        {
            view.ShowError("File exceeds protocol size limit.");
            return;
        }

        var packet = new NetworkPacket(
            PacketType.File,
            data,
            Path.GetFileName(path)
        );

        view.DisplayFile(data, Path.GetFileName(path));
        await connection.SendMessageAsync(packet);
    }

    private void HandlePacketReceived(NetworkPacket packet)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            switch (packet.Type)
            {
                case PacketType.Text:
                    view.DisplayText(Encoding.UTF8.GetString(packet.Data));
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