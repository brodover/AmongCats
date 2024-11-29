using System.Numerics;

namespace SharedLibrary
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Common.Role Role { get; set; }

        public string RoomId { get; set; } = string.Empty;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public bool IsFaceLeft { get; set; } = true;
    }
}
