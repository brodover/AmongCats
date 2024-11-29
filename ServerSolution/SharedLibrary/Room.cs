using System.Collections.Generic;

namespace SharedLibrary
{
    public class Room
    {
        public string Id { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public bool IsFull => Players.Count >= MaxPlayers;
        public const int MaxPlayers = 2;
    }
}
