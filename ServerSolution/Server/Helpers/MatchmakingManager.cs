using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using SharedLibrary;
using System.Net.WebSockets;
using static Server.Helpers.ServerCommon;

namespace Server.Helpers
{
    public class MatchmakingManager
    {
        // completed rooms
        public IReadOnlyList<Room> ActiveRooms => _activeRooms;
        private readonly List<Room> _activeRooms = new();

        private readonly PriorityQueue<Player, PlayerPriority> WaitingPlayers = new(); // players in queue

        private readonly IHubContext<GameHub> _hubContext;

        public MatchmakingManager(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public bool AddToQueue(Player player)
        {
            lock (WaitingPlayers)
            {
                WaitingPlayers.Enqueue(player, PlayerPriority.Normal);
                TryCreateRoom();
            }

            return true;
        }

        public void RemoveFromQueue(Player player)
        {
            lock (WaitingPlayers)
            {
                WaitingPlayers.Enqueue(player, PlayerPriority.Canceled);
            }
        }

        // called when WaitingPlayers is locked already
        private async void TryCreateRoom()
        {
            Room room = null;

            lock (WaitingPlayers)
            {
                if (WaitingPlayers.Count >= Room.MaxPlayers)
                {
                    room = new Room { Id = Guid.NewGuid().ToString() };


                    // add waiting players but not if they canceled
                    while (room.Players.Count < Room.MaxPlayers && WaitingPlayers.TryDequeue(out var player, out var priority))
                    {
                        if (priority == PlayerPriority.Canceled)
                        {
                            continue;
                        }

                        room.Players.Add(player);
                    }

                    if (room.Players.Count < Room.MaxPlayers)
                    {
                        foreach (var player in room.Players)
                        {
                            WaitingPlayers.Enqueue(player, PlayerPriority.High);
                        }
                        return;
                    }
                }
            }

            // add each player to game and notify players
            if (room?.Players.Count >= Room.MaxPlayers)
            {
                /*if (_hubContext != null)
                {*/
                foreach (var player in room.Players)
                {
                    player.RoomId = room.Id;
                    await _hubContext.Groups.AddToGroupAsync(player.Id, room.Id);
                }

                AddRoom(room);

                await _hubContext.Clients.Group(room.Id).SendAsync(Common.HubMsg.ToClient.MatchCreated, room);
                   
                /*}
                else
                {
                    throw new InvalidOperationException("HubContext is not initialized.");
                }*/
            }
        }

        public void AddRoom(Room room)
        {
            lock (_activeRooms)
            {
                if (!_activeRooms.Contains(room))
                    _activeRooms.Add(room);
            }
        }

        public void RemoveRoom(Room room)
        {
            lock (_activeRooms)
            {
                if (!_activeRooms.Contains(room))
                    _activeRooms.Add(room);
            }
        }
    }

}
