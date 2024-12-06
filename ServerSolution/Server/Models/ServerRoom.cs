using Microsoft.AspNetCore.SignalR;
using Server.Helpers;
using Server.Hubs;
using SharedLibrary;

namespace Server.Models
{
    public class ServerRoom : Room
    {
        public DateTime LastMove { get; set; }
        public DateTime LastUpdate { get; set; }

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private const int UPDATE_DELAY_MILISECOND = 500; // 100ms = 10hz

        public void StartRoom(IHubContext<GameHub> hubContext)
        {
            LastMove = DateTime.UtcNow;
            LastUpdate = DateTime.UtcNow;

            var token = _cancellationTokenSource.Token;

            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (LastUpdate > LastMove)
                        continue;

                    try
                    {
                        _ = hubContext.Clients.Group(Id).SendAsync(Common.HubMsg.ToClient.PlayerMoveUpdated, Players);
                        LastUpdate = DateTime.UtcNow;
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

        public void StopRoom()
        {
            _cancellationTokenSource.Cancel(); // Signal the task to stop
        }

        public void UpdateRoom()
        {
            LastMove = DateTime.UtcNow;
        }
    }
}
