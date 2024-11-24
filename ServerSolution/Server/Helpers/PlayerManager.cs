using SharedLibrary;

namespace Server.Helpers
{
    public class PlayerManager
    {
        public IReadOnlyDictionary<string, Player> Players => _players;

        private readonly Dictionary<string, Player> _players = new();

        public void AddPlayer(string id, Player player)
        {
            lock (_players)
            {
                if (!_players.ContainsKey(id))
                    _players.Add(id, player);
            }
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
