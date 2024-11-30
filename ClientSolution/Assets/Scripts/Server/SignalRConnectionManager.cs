using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static ClientCommon;
using static SharedLibrary.Common;

public class SignalRConnectionManager
{
    private static SignalRConnectionManager _instance;
    public static SignalRConnectionManager Instance => _instance ??= new SignalRConnectionManager();

    public event Action OnMatchCreated;

    private Player _myPlayer = null;
    private Room _myRoom = null;

    private HubConnection _connection;

    public async Task InitializeConnection()
    {
        if (_connection != null)
            return;

        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(ClientCommon.ServerGameHub)
                //.WithAutomaticReconnect()
                .Build();

            _connection.Closed += async (error) =>
            {
                Console.WriteLine($"Connection closed: {error?.Message}. Reconnecting...");
                await Task.Delay(5000);
                await StartConnection();
            };

            _connection.On<string>(HubMsg.ToClient.ReceiveMessage, message =>
            {
                Debug.Log($"Message from server: {message}");
            });

            _connection.On<Player>(HubMsg.ToClient.PlayerConnected, player =>
            {
                Debug.Log($"PlayerConnected: {player.Id}");
                _myPlayer = player;
            });

            _connection.On<Room>(HubMsg.ToClient.MatchCreated, room =>
            {
                Debug.Log($"MatchCreated: {room.Id}, {room.Players.Count}");

                _myRoom = room;
                OnMatchCreated?.Invoke();
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
        await _connection.InvokeAsync(HubMsg.ToServer.JoinQueue, role);
    }

    public async Task PlayerLeaveQueue()
    {
        await _connection.InvokeAsync(HubMsg.ToServer.LeaveQueue);
    }
}
