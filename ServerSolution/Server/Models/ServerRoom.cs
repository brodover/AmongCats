using Microsoft.AspNetCore.SignalR;
using Server.Helpers;
using Server.Hubs;
using SharedLibrary;

namespace Server.Models
{
    public class ServerRoom : Room
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private const int UPDATE_DELAY_MILISECOND = 200; // 100ms = 10hz

        public void StartRoom(IHubContext<GameHub> hubContext)
        {
            var token = _cancellationTokenSource.Token;

            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Logger.Default.LogDebug("ServerRoom Sending PlayerMoveUpdated");
                        _ = hubContext.Clients.Group(Id).SendAsync(Common.HubMsg.ToClient.PlayerMoveUpdated, Players);
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
    }
}
