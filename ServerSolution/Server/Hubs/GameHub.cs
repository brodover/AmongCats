using Microsoft.AspNetCore.SignalR;
using Server.Helpers;
using SharedLibrary;

namespace Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly PlayerManager _playerM;
        private readonly MatchmakingManager _matchmakingM;

        public GameHub(PlayerManager manager, MatchmakingManager matchmakingM)
        {
            _playerM = manager;
            _matchmakingM = matchmakingM;
        }

        public override Task OnConnectedAsync()
        {
            _playerM.AddPlayer(Context.ConnectionId, new Player() { Id = Context.ConnectionId });
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //remove player from game/queue and server if disconnect
            await LeaveGame();
            //await LeaveQueue();
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
