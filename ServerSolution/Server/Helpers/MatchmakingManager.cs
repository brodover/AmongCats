using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Server.Models;
using SharedLibrary;
using System.Net.WebSockets;

namespace Server.Helpers
{
    public class MatchmakingManager
    {
        // completed rooms
        public IReadOnlyList<Room> ActiveRooms => _activeRooms;
        private readonly List<Room> _activeRooms = new();

        private readonly Queue<Player> WaitingPlayers = new();  // players in queue
        private readonly HashSet<string> CanceledPlayers = new(); // players who canceled queue
        private readonly Queue<Room> WaitingRooms = new();  // incomplete rooms

        private readonly IHubContext<GameHub> _hubContext;

        public MatchmakingManager(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public bool AddToQueue(Player player)
        {
            lock (CanceledPlayers)
            {
                if (CanceledPlayers.Contains(player.Id))
                {
                    return false;
                }
            }

            lock (WaitingPlayers)
            {
                WaitingPlayers.Enqueue(player);
                TryCreateRoom();
            }

            return true;
        }

        // don't call this yet. or else, canceled players will have to wait till
        // someone else joins queue to get unblocked from joining
        public void RemoveFromQueue(Player player)
        {
            lock (CanceledPlayers)
            {
                if (!CanceledPlayers.Contains(player.Id))
                {
                    CanceledPlayers.Add(player.Id);
                }
            }
        }

        // called when WaitingPlayers is locked already
        private async void TryCreateRoom()
        {
            Room room = new Room();

            lock (CanceledPlayers)
            {
                var canceledPlayersToRemove = new List<string>();

                if (WaitingPlayers.Count - CanceledPlayers.Count >= Room.MaxPlayers)
                {
                    // if existing half created room exists, use it
                    if (WaitingRooms.Count > 0)
                        room = WaitingRooms.Dequeue();
                    else
                        room = new Room { Id = Guid.NewGuid().ToString() };

                    // add waiting players but not if they canceled
                    var i = room.Players.Count;
                    while (i < Room.MaxPlayers)
                    {
                        var player = WaitingPlayers.Dequeue();

                        if (CanceledPlayers.Contains(player.Id))
                        {
                            canceledPlayersToRemove.Add(player.Id);
                            continue;
                        }

                        room.Players.Add(player);
                        i++;
                    }

                    if (room.Players.Count < Room.MaxPlayers)
                        WaitingRooms.Enqueue(room);

                    foreach (var item in canceledPlayersToRemove)
                    {
                        CanceledPlayers.Remove(item);
                    }
                }
            }

            // add each player to game and notify players
            if (room.Players.Count >= Room.MaxPlayers)
            {
                if (_hubContext != null)
                {
                    foreach (var player in room.Players)
                    {
                        player.RoomId = room.Id;
                        await _hubContext.Groups.AddToGroupAsync(player.Id, room.Id);
                    }
                    
                    await _hubContext.Clients.Group(room.Id).SendAsync(Common.HubMessage.MatchCreated, room.Id);
                   
                    AddRoom(room);
                }
                else
                {
                    throw new InvalidOperationException("HubContext is not initialized.");
                }
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
