using Server.Hubs;
using SharedLibrary;

namespace Server.Models
{
    public class ServerRoom : Room
    {
        private GameUpdate _gameUpdate;

        public void StartRoom()
        {
            _gameUpdate = new GameUpdate(Id);
        }

        public void StopRoom()
        {
            _gameUpdate.Stop();
        }
    }
}
