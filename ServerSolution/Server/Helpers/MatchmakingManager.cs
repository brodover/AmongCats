using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Server.Hubs;
using SharedLibrary;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using static Server.Helpers.ServerCommon;

namespace Server.Helpers
{
    public class MatchmakingManager
    {
        // completed rooms
        public IReadOnlyList<Room> ActiveRooms => _activeRooms;
        private readonly List<Room> _activeRooms = new();

        private ConcurrentQueue<Player> WaitingHumans = new(); // humans in queue
        private ConcurrentQueue<Player> WaitingCats = new(); // cats in queue
        private ConcurrentQueue<Player> WaitingRandoms = new(); // randoms in queue

        private readonly IHubContext<GameHub> _hubContext;

        public MatchmakingManager(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public bool AddToQueue(Player player)
        {
            RemoveFromQueue(player);

            switch (player.Role)
            {
                case Common.Role.Human:
                    lock (WaitingHumans)
                    {
                        WaitingHumans.Enqueue(player);
                        Logger.Default.LogTrace($"AddToQueue: {player.Role}, {WaitingHumans.Count}");
                    }
                    break;
                case Common.Role.Cat:
                    lock (WaitingCats)
                    {
                        WaitingCats.Enqueue(player);
                        Logger.Default.LogTrace($"AddToQueue: {player.Role}, {WaitingCats.Count}");
                    }
                    break;
                case Common.Role.Random:
                    lock (WaitingRandoms)
                    {
                        WaitingRandoms.Enqueue(player);
                        Logger.Default.LogTrace($"AddToQueue: {player.Role}, {WaitingRandoms.Count}");
                    }
                    break;
            }

            var players = TryMatchPlayers();
            if (players != null)
                TryCreateRoom(players);

            return true;
        }

        public void RemoveFromQueue(Player player)
        {
            switch (player.Role)
            {
                case Common.Role.Human:
                    lock (WaitingHumans)
                    {
                        WaitingHumans = new ConcurrentQueue<Player>(WaitingHumans.Where(p => p.Id != player.Id));
                        Logger.Default.LogTrace($"RemoveFromQueue: {player.Role}, {WaitingHumans.Count}");
                    }
                    break;
                case Common.Role.Cat:
                    lock (WaitingCats)
                    {
                        WaitingCats = new ConcurrentQueue<Player>(WaitingCats.Where(p => p.Id != player.Id));
                        Logger.Default.LogTrace($"RemoveFromQueue: {player.Role}, {WaitingCats.Count}");
                    }
                    break;
                case Common.Role.Random:
                    lock (WaitingRandoms)
                    {
                        WaitingRandoms = new ConcurrentQueue<Player>(WaitingRandoms.Where(p => p.Id != player.Id));
                        Logger.Default.LogTrace($"RemoveFromQueue: {player.Role}, {WaitingRandoms.Count}");
                    }
                    break;
            }
        }

        // called when WaitingPlayers is locked already
        private List<Player> TryMatchPlayers()
        {
            Logger.Default.LogDebug("TryMatchPlayers");

            if (WaitingRandoms.Count > 0)
            {
                if (WaitingHumans.Count > WaitingCats.Count && WaitingRandoms.TryDequeue(out Player cat1))
                {
                    if (WaitingHumans.TryDequeue(out Player human1))
                    {
                        cat1.Role = Common.Role.Cat;
                        human1.Role = Common.Role.Human;
                        Logger.Default.LogDebug("TryMatchPlayers 1 human, 1 random");
                        return new List<Player> { human1, cat1 };
                    }
                    WaitingRandoms.Enqueue(cat1); // not ideal, shld replace in front
                }
                else if (WaitingCats.Count > 0 && WaitingRandoms.TryDequeue(out Player human2))
                {
                    if (WaitingCats.TryDequeue(out Player cat2))
                    {
                        human2.Role = Common.Role.Human;
                        cat2.Role = Common.Role.Cat;
                        Logger.Default.LogDebug("TryMatchPlayers 1 cat, 1 random");
                        return new List<Player> { human2, cat2 };
                    }
                    WaitingRandoms.Enqueue(human2); // not ideal, shld replace in front
                }
                else if (WaitingRandoms.Count > 1)
                {
                    if (WaitingRandoms.TryDequeue(out Player human3))
                    {
                        if (WaitingRandoms.TryDequeue(out Player cat3))
                        {
                            human3.Role = Common.Role.Human;
                            cat3.Role = Common.Role.Cat;
                            Logger.Default.LogDebug("TryMatchPlayers 2 random");
                            return new List<Player> { human3, cat3 };
                        }
                        WaitingRandoms.Enqueue(human3); // not ideal, shld replace in front
                    }
                }
            }
            else
            {
                if (WaitingHumans.Count > 0 && WaitingCats.Count > 0)
                {
                    if (WaitingHumans.TryDequeue(out Player human4))
                    {
                        if (WaitingCats.TryDequeue(out Player cat4))
                        {
                            human4.Role = Common.Role.Human;
                            cat4.Role = Common.Role.Cat;
                            Logger.Default.LogDebug("TryMatchPlayers 1 human, 1 cat");
                            return new List<Player> { human4, cat4 };
                        }
                        WaitingHumans.Enqueue(human4); // not ideal, shld replace in front
                    }
                }
            }

            Logger.Default.LogDebug($"TryMatchPlayers not matched, {WaitingHumans.Count}, {WaitingCats.Count}, {WaitingRandoms.Count}");
            return null;
        }

        // called when WaitingPlayers is locked already
        private async void TryCreateRoom(List<Player> players)
        {
            Logger.Default.LogDebug("TryCreateRoom");
            Room room = new Room { Id = Guid.NewGuid().ToString(), Players = players };

            // add each player to game and notify players
            if (room?.Players.Count >= Room.MaxPlayers)
            {
                foreach (var player in room.Players)
                {
                    player.RoomId = room.Id;
                    await _hubContext.Groups.AddToGroupAsync(player.Id, room.Id);
                }

                AddRoom(room);

                Logger.Default.LogDebug("MatchCreated");
                await _hubContext.Clients.Group(room.Id).SendAsync(Common.HubMsg.ToClient.MatchCreated, room);
            }
            else
            {
                // re-add players back to queue
                Logger.Default.LogDebug("TryCreateRoom not enough players");
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
