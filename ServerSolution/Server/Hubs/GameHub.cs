using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using Server.Helpers;
using SharedLibrary;
using static SharedLibrary.Common;

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

        public async override Task OnConnectedAsync()
        {
            _logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);
            var player = new Player() { Id = Context.ConnectionId };
            _playerM.AddPlayer(Context.ConnectionId, player);
            await base.OnConnectedAsync();
            await Clients.Client(Context.ConnectionId).SendAsync(HubMsg.ToClient.PlayerConnected, player);
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

        public Response JoinQueue(Common.Role role)
        {
            _logger.LogDebug("JoinQueue: {role}", role);

            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer);

            if (player.RoomId != string.Empty)
                return Response.Fail(ServerError.AlrInMatch);

            player.Role = role;

            _matchmakingM.AddToQueue(player, _playerM);

            return Response.Succeed();
        }

        public Response LeaveQueue()
        {
            _logger.LogDebug("LeaveQueue");

            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer);

            _matchmakingM.RemoveFromQueue(player);

            return Response.Succeed();
        }

        public async Task<Response> LeaveGame()
        {
            _logger.LogDebug("LeaveGame");

            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer);

            if (player.RoomId == string.Empty)
                return Response.Fail(ServerError.NoRoomId);

            var room = _matchmakingM.ActiveRooms.FirstOrDefault(x => x.Id == player.RoomId);
            if (room == null)
                return Response.Fail(ServerError.NoActiveRoom);

            await Clients.Group(player.RoomId).SendAsync(HubMsg.ToClient.MatchClosed, string.Empty);

            foreach (var roomPlayer in room.Players)
            {
                roomPlayer.RoomId = string.Empty;
                roomPlayer.Role = Role.None;
                _playerM.UpdatePlayer(roomPlayer.Id, roomPlayer);
                await Groups.RemoveFromGroupAsync(roomPlayer.Id, player.RoomId);
            }

            _matchmakingM.RemoveRoom(room);

            return Response.Succeed();
        }

        public Response Move(Player rPlayer)
        {
            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer);

            player.Position = new Vector3(rPlayer.Position.X, rPlayer.Position.Y, rPlayer.Position.Z);
            player.IsFaceLeft = rPlayer.IsFaceLeft;

            return Response.Succeed();
        }

        public async Task SendMessage(string message)
        {
            _logger.LogDebug("SendMessage: {message}", message);

            await Clients.All.SendAsync(HubMsg.ToClient.MessageReceived, message);
        }

    }
}
