using System.Net.WebSockets;

namespace SharedLibrary
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public WebSocket Socket { get; set; }
    }
}
