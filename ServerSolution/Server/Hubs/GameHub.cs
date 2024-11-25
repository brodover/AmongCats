using Microsoft.AspNetCore.SignalR;
using Server.Helpers;
using SharedLibrary;

namespace Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly PlayerManager _playerM;
        private readonly MatchmakingManager _matchmakingM;
        private readonly ILogger<GameHub> _logger;

        public GameHub(PlayerManager manager, MatchmakingManager matchmakingM, ILogger<GameHub> logger)
        {
            _playerM = manager;
            _matchmakingM = matchmakingM;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);
            _playerM.AddPlayer(Context.ConnectionId, new Player() { Id = Context.ConnectionId });
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error");
            }

            //remove player from game/queue and server if disconnect
            await LeaveGame();
            LeaveQueue();
            _playerM.RemovePlayer(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public Response JoinQueue(string role)
        {
            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(Common.ServerError.NoPlayer);

            if (player.RoomId != string.Empty)
                return Response.Fail(Common.ServerError.AlrInMatch);

            player.Role = role;

            _matchmakingM.AddToQueue(player);
            return Response.Succeed();
        }

        public Response LeaveQueue()
        {
            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(Common.ServerError.NoPlayer);

            _matchmakingM.RemoveFromQueue(player);

            return Response.Succeed();
        }

        public async Task<Response> LeaveGame()
        {
            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(Common.ServerError.NoPlayer);

            if (player.RoomId == string.Empty)
                return Response.Fail(Common.ServerError.NoRoomId);

            player.RoomId = string.Empty;

            var room = _matchmakingM.ActiveRooms.First(x => x.Id == player.RoomId);
            if (room == null)
                return Response.Fail(Common.ServerError.NoActiveRoom);

            foreach (var roomPlayer in room.Players)
            {
                roomPlayer.RoomId = string.Empty;
                await Groups.RemoveFromGroupAsync(roomPlayer.Id, player.RoomId);
            }

            _matchmakingM.RemoveRoom(room);

            return Response.Succeed();
        }
    }
}
