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
        connection.OnError += HandleError;
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
            Encoding.UTF8.GetBytes(message)
        );

        try
        {
            await connection.SendMessageAsync(packet);
            view.DisplayText(message);
        }
        catch (Exception ex)
        {
            view.ShowError(ex.Message);
        }
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

        var packet = new NetworkPacket(
            PacketType.Image,
            data,
            Path.GetFileName(path)
        );

        try
        {
            await connection.SendMessageAsync(packet);
            view.DisplayImage(data);
        }
        catch (Exception ex)
        {
            view.ShowError(ex.Message);
        }
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

        var packet = new NetworkPacket(
            PacketType.File,
            data,
            Path.GetFileName(path)
        );

        try
        {
            await connection.SendMessageAsync(packet);
            view.DisplayFile(data, Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            view.ShowError(ex.Message);
        }
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

    private void HandleError(string message)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            view.ShowError(message);
        });
    }
    public void Dispose()
    {
        connection.OnPacketReceived -= HandlePacketReceived;
        connection.OnError -= HandleError;
    }
}