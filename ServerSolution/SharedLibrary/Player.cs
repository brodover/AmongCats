using System.Numerics;

namespace SharedLibrary
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Common.Role Role { get; set; }

        public string RoomId { get; set; } = string.Empty;
        public SerializableVector3 Position { get; set; }
        public bool IsFaceRight { get; set; }
    }
}
