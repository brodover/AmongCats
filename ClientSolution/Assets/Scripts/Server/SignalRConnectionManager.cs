using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using static Common;

public class SignalRConnectionManager
{
    private static SignalRConnectionManager _instance;
    public static SignalRConnectionManager Instance => _instance ??= new SignalRConnectionManager();

    private List<PlayerConnection> _playerConnections = new List<PlayerConnection>();
    private string _myId = "";

    private HubConnection _connection;

    public async Task InitializeConnection()
    {
        if (_connection != null)
            return;

        _myId = Guid.NewGuid().ToString();
        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(Common.ServerGameHub)
                //.WithAutomaticReconnect()
                .Build();

            _connection.Closed += async (error) =>
            {
                Console.WriteLine($"Connection closed: {error?.Message}. Reconnecting...");
                await Task.Delay(5000);
                await StartConnection();
            };

            _connection.On<string>("ReceiveMessage", message =>
            {
                Debug.Log($"Message from server: {message}");
            });

            _connection.On<string>("MatchCreated", message =>
            {
                Debug.Log($"MatchCreated: {message}");
            });

            await StartConnection();
        }
        catch (Exception ex)
        {
            Debug.LogError($"SignalR connection failed: {ex.Message}");
        }
    }

    private async Task StartConnection()
    {
        try
        {
            await _connection.StartAsync();
            Console.WriteLine("SignalR connected.");

            // Send a test message
            await _connection.InvokeAsync("SendMessage", "Hello from Unity!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to SignalR: {ex.Message}");
        }
    }

    public async Task Disconnect()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async Task PlayerJoinQueue(Role role)
    {
        await _connection.InvokeAsync("JoinQueue", role.ToString());
    }

    public async Task PlayerLeaveQueue()
    {
        await _connection.InvokeAsync("LeaveQueue");
    }
}
