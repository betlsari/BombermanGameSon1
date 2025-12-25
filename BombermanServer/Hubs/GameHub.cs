using Microsoft.AspNetCore.SignalR;
using BombermanServer.Models;
using BombermanServer.Models.Messages;
using BombermanServer.Services;

namespace BombermanServer.Hubs
{
	public class GameHub : Hub
	{
		private readonly IRoomService _roomService;

		public GameHub(IRoomService roomService)
		{
			_roomService = roomService;
		}

		public override async Task OnConnectedAsync()
		{
			await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var rooms = _roomService.GetAllRooms();
			foreach (var room in rooms)
			{
				var player = room.GetPlayerByConnectionId(Context.ConnectionId);
				if (player != null)
				{
					await LeaveRoom(room.Id);
				}
			}

			await base.OnDisconnectedAsync(exception);
		}

		public async Task<RoomCreatedResponse> CreateRoom(CreateRoomRequest request)
		{
			try
			{
				var room = _roomService.CreateRoom(
					request.RoomName,
					Context.ConnectionId,
					request.PlayerName,
					request.Theme,
					request.MaxPlayers
				);

				await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);

				var roomInfo = new RoomInfoDto
				{
					Id = room.Id,
					Name = room.Name,
					CurrentPlayers = room.Players.Count,
					MaxPlayers = room.MaxPlayers,
					State = room.State.ToString(),
					Theme = room.Theme,
					PlayerNames = room.Players.Select(p => p.PlayerName).ToList()
				};

				return new RoomCreatedResponse
				{
					Success = true,
					RoomId = room.Id,
					RoomInfo = roomInfo
				};
			}
			catch (Exception ex)
			{
				return new RoomCreatedResponse
				{
					Success = false,
					ErrorMessage = ex.Message
				};
			}
		}

		public async Task<bool> JoinRoom(JoinRoomRequest request)
		{
			var success = _roomService.JoinRoom(request.RoomId, Context.ConnectionId, request.PlayerName);

			if (success)
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, request.RoomId);

				var room = _roomService.GetRoom(request.RoomId);
				if (room != null)
				{
					var roomInfo = new RoomInfoDto
					{
						Id = room.Id,
						Name = room.Name,
						CurrentPlayers = room.Players.Count,
						MaxPlayers = room.MaxPlayers,
						State = room.State.ToString(),
						Theme = room.Theme,
						PlayerNames = room.Players.Select(p => p.PlayerName).ToList()
					};

					await Clients.Group(request.RoomId).SendAsync("PlayerJoined", request.PlayerName, roomInfo);
				}
			}

			return success;
		}

		public async Task LeaveRoom(string roomId)
		{
			var room = _roomService.GetRoom(roomId);
			if (room == null) return;

			var player = room.GetPlayerByConnectionId(Context.ConnectionId);
			var playerName = player?.PlayerName ?? "Unknown";

			_roomService.LeaveRoom(roomId, Context.ConnectionId);
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

			await Clients.Group(roomId).SendAsync("PlayerLeft", playerName);
		}

		public async Task<RoomListResponse> GetRoomList()
		{
			var rooms = _roomService.GetAllRooms();

			var roomDtos = rooms.Select(r => new RoomInfoDto
			{
				Id = r.Id,
				Name = r.Name,
				CurrentPlayers = r.Players.Count,
				MaxPlayers = r.MaxPlayers,
				State = r.State.ToString(),
				Theme = r.Theme,
				PlayerNames = r.Players.Select(p => p.PlayerName).ToList()
			}).ToList();

			return new RoomListResponse
			{
				Rooms = roomDtos,
				TotalRooms = roomDtos.Count
			};
		}

		public async Task StartGame(string roomId)
		{
			var room = _roomService.GetRoom(roomId);
			if (room == null || room.HostConnectionId != Context.ConnectionId)
			{
				await Clients.Caller.SendAsync("Error", "Only host can start the game");
				return;
			}

			if (!room.CanStart)
			{
				await Clients.Caller.SendAsync("Error", "Need at least 2 players to start");
				return;
			}

			var success = _roomService.StartGame(roomId);
			if (success)
			{
				var startMessage = new GameStartedMessage
				{
					RoomId = roomId,
					MapSeed = room.MapSeed,
					Theme = room.Theme,
					Players = room.Players.Select(p => new PlayerStartInfo
					{
						ConnectionId = p.ConnectionId,
						Name = p.PlayerName,
						PlayerId = p.PlayerId,
						StartX = p.Position.X,
						StartY = p.Position.Y
					}).ToList()
				};

				await Clients.Group(roomId).SendAsync("GameStarted", startMessage);
			}
		}

		public async Task PlayerMove(PlayerMoveMessage message)
		{
			var room = _roomService.GetRoom(message.RoomId);
			if (room == null) return;

			var player = room.GetPlayerByConnectionId(Context.ConnectionId);
			if (player != null)
			{
				player.Position.X = message.X;
				player.Position.Y = message.Y;
			}

			await Clients.OthersInGroup(message.RoomId).SendAsync("PlayerMoved",
				Context.ConnectionId, message.X, message.Y);
		}

		public async Task PlaceBomb(PlaceBombMessage message)
		{
			await Clients.OthersInGroup(message.RoomId).SendAsync("BombPlaced",
				Context.ConnectionId, message.X, message.Y, message.Range);
		}

		public async Task UpdateGameState(string roomId, GameStateData gameState)
		{
			_roomService.UpdateGameState(roomId, gameState);
			await Clients.OthersInGroup(roomId).SendAsync("GameStateUpdated", gameState);
		}

		public async Task SendChatMessage(ChatMessage message)
		{
			var room = _roomService.GetRoom(message.RoomId);
			if (room == null) return;

			var player = room.GetPlayerByConnectionId(Context.ConnectionId);
			var playerName = player?.PlayerName ?? "Unknown";

			await Clients.Group(message.RoomId).SendAsync("ChatMessageReceived",
				playerName, message.Message, DateTime.UtcNow);
		}

		public async Task NotifyGameEnd(string roomId, string winnerId, int finalScore)
		{
			await Clients.Group(roomId).SendAsync("GameEnded", winnerId, finalScore);
		}
	}
}