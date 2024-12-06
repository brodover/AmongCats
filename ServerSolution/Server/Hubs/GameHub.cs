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
            _logger.LogDebug("Receive JoinQueue: {role}", role);

            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer).LogFail();

            if (player.RoomId != string.Empty)
                return Response.Fail(ServerError.AlrInMatch).LogFail();

            player.Role = role;

            _matchmakingM.AddToQueue(player, _playerM);

            return Response.Succeed();
        }

        public Response LeaveQueue()
        {
            _logger.LogDebug("Receive LeaveQueue");

            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer).LogFail();

            _matchmakingM.RemoveFromQueue(player);

            return Response.Succeed();
        }

        public async Task<Response> LeaveGame()
        {
            _logger.LogDebug("Receive LeaveGame");

            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer).LogFail();

            if (player.RoomId == string.Empty)
                return Response.Fail(ServerError.NoRoomId).LogFail();

            var room = _matchmakingM.ActiveRooms.FirstOrDefault(x => x.Id == player.RoomId);
            if (room == null)
                return Response.Fail(ServerError.NoActiveRoom).LogFail();

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

        public Response MovePlayer(Player uPlayer)
        {
            _logger.LogDebug($"Receive MovePlayer {Context.ConnectionId}: {uPlayer.Position.X}");

            if (!_playerM.Players.TryGetValue(Context.ConnectionId, out var player))
                return Response.Fail(ServerError.NoPlayer).LogFail();

            var room = _matchmakingM.ActiveRooms.FirstOrDefault(x => x.Id == player.RoomId);
            if (room == null)
                return Response.Fail(ServerError.NoActiveRoom).LogFail();

            var rplayer = room.Players.FirstOrDefault(x => x.Id == uPlayer.Id);
            if (rplayer == null)
                return Response.Fail(ServerError.NoPlayer).LogFail();

            rplayer.Position = new Vector3(uPlayer.Position.X, uPlayer.Position.Y, uPlayer.Position.Z);
            rplayer.IsFaceRight = uPlayer.IsFaceRight;
            room.UpdateRoom();

            return Response.Succeed();
        }

        public async Task SendMessage(string message)
        {
            _logger.LogDebug($"Receive SendMessage: {message}");

            await Clients.All.SendAsync(HubMsg.ToClient.MessageReceived, message);
        }

    }
}
