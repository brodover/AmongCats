using System.Numerics;

namespace SharedLibrary
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public string RoomId { get; set; }
        public Vector3 Position { get; set; } = Vector3.Zero;
        public bool IsFaceLeft { get; set; } = true;
    }
}
