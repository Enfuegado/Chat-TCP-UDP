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

        connection.OnConnected += HandleConnected;
        connection.OnDisconnected += HandleDisconnected;

        view.SetConnectionStatus(ConnectionStatus.Disconnected);
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
            bool amClient = connection is IClient;
            view.DisplayText(message, amClient ? "[client]" : "[server]", true);
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
            bool amClient = connection is IClient;
            view.DisplayImage(data, amClient ? "[client]" : "[server]", true);
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
            bool amClient = connection is IClient;
            view.DisplayFile(data, Path.GetFileName(path), amClient ? "[client]" : "[server]", true);
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
            if (view == null)
                return;

            bool amClient = connection is IClient;

            switch (packet.Type)
            {
                case PacketType.Text:
                    view.DisplayText(Encoding.UTF8.GetString(packet.Data), amClient ? "[server]" : "[client]", false);
                    break;

                case PacketType.Image:
                    view.DisplayImage(packet.Data, amClient ? "[server]" : "[client]", false);
                    break;

                case PacketType.File:
                    view.DisplayFile(packet.Data, packet.FileName, amClient ? "[server]" : "[client]", false);
                    break;
            }
        });
    }

    private void HandleError(string message)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (view == null)
                return;

            view.ShowError(message);
        });
    }

    private void HandleConnected()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (view == null)
                return;

            view.SetConnectionStatus(ConnectionStatus.Connected);
        });
    }

    private void HandleDisconnected()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (view == null)
                return;

            view.SetConnectionStatus(ConnectionStatus.Disconnected);
        });
    }

    public void Dispose()
    {
        connection.OnPacketReceived -= HandlePacketReceived;
        connection.OnError -= HandleError;
        connection.OnConnected -= HandleConnected;
        connection.OnDisconnected -= HandleDisconnected;
    }
}