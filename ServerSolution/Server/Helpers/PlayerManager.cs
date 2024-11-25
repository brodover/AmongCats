using System.Collections.Concurrent;
using SharedLibrary;

namespace Server.Helpers
{
    public class PlayerManager
    {
        public IReadOnlyDictionary<string, Player> Players => _players;

        private readonly ConcurrentDictionary<string, Player> _players = new();

        public void AddPlayer(string id, Player player)
        {
            _players.TryAdd(id, player);
        }

        public Player RemovePlayer(string id)
        {
            lock (_players)
            {
                _players.Remove(id, out var player);
                return player;
            }
        }
    }
}
