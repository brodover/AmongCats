using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public class SignalrConnection : MonoBehaviour
{
    private List<PlayerConnection> _playerConnections = new List<PlayerConnection>();
    private string _myId = "";

    private HubConnection _connection;

    private const string _HUB_URL = Common.ServerUrl + Common.ServerGameHub;

    async void Start()
    {
        await StartConnection();
    }

    private async Task StartConnection()
    {
        _myId = Guid.NewGuid().ToString();
        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_HUB_URL)
                //.WithAutomaticReconnect()
                .Build();

            _connection.On<string>("ReceiveMessage", message =>
            {
                Debug.Log($"Message from server: {message}");
            });

            // Start the connection
            await _connection.StartAsync();
            Debug.Log("Connected to SignalR hub!");

            // Send a test message
            await _connection.InvokeAsync("SendMessage", "Hello from Unity!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"SignalR connection failed: {ex.Message}");
        }
    }

    private async void OnApplicationQuit()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
    }
}
