using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Server.Helpers;
using SharedLibrary;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Server.Hubs
{
    public class GameUpdate
    {

        private readonly IHubContext<GameHub> _gameHubContext;
        private readonly PlayerManager _playerM;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private string _roomId;
        private List<Player> _players;

        private const int UPDATE_DELAY_MILISECOND = 50; // 50s = 20hz

        public GameUpdate(string roomId)
        {
            _roomId = roomId;
            Start();
        }

        public void Start()
        {
            var token = _cancellationTokenSource.Token;

            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        _players.Clear();
                        _players.AddRange(_playerM.Players.Values.Where(player => player.RoomId == _roomId));

                        _ = _gameHubContext.Clients.Group(_roomId).SendAsync(Common.HubMsg.ToClient.PlayerMoveUpdated, _players);
                        await Task.Delay(UPDATE_DELAY_MILISECOND);
                    }
                    catch (OperationCanceledException)
                    {
                        // Task was cancelled
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Handle unexpected exceptions (log, etc.)
                        Console.WriteLine($"Error in GameUpdate: {ex.Message}");
                    }
                }
            }, token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel(); // Signal the task to stop
        }
    }
}
