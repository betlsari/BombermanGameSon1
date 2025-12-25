using BombermanServer.Models;
using BombermanServer.Models.Enums;
using System.Collections.Concurrent;

namespace BombermanServer.Services
{
	public interface IRoomService
	{
		GameRoom CreateRoom(string roomName, string hostConnectionId, string playerName, string theme, int maxPlayers);
		GameRoom? GetRoom(string roomId);
		List<GameRoom> GetAllRooms();
		bool JoinRoom(string roomId, string connectionId, string playerName);
		bool LeaveRoom(string roomId, string connectionId);
		bool StartGame(string roomId);
		bool UpdateGameState(string roomId, GameStateData gameState);
		void RemoveRoom(string roomId);
		void CleanupAbandonedRooms();
	}

	public class RoomService : IRoomService
	{
		private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
		private readonly object _lock = new();

		public GameRoom CreateRoom(string roomName, string hostConnectionId, string playerName, string theme, int maxPlayers)
		{
			var room = new GameRoom
			{
				Name = roomName,
				HostConnectionId = hostConnectionId,
				Theme = theme,
				MaxPlayers = maxPlayers,
				MapSeed = new Random().Next()
			};

			var hostPlayer = new RoomPlayer
			{
				ConnectionId = hostConnectionId,
				PlayerName = playerName,
				PlayerId = 1,
				IsHost = true,
				IsReady = false,
				Position = new PlayerPosition { X = 1, Y = 1 }
			};

			room.AddPlayer(hostPlayer);
			_rooms[room.Id] = room;

			return room;
		}

		public GameRoom? GetRoom(string roomId)
		{
			_rooms.TryGetValue(roomId, out var room);
			return room;
		}

		public List<GameRoom> GetAllRooms()
		{
			return _rooms.Values
				.Where(r => r.State == GameRoomState.Waiting && !r.IsFull)
				.OrderByDescending(r => r.CreatedAt)
				.ToList();
		}

		public bool JoinRoom(string roomId, string connectionId, string playerName)
		{
			var room = GetRoom(roomId);
			if (room == null || room.IsFull || room.State != GameRoomState.Waiting)
			{
				return false;
			}

			lock (_lock)
			{
				if (room.IsFull)
				{
					return false;
				}

				var playerId = room.Players.Count + 1;
				var startX = playerId == 2 ? 19 : 1;
				var startY = playerId == 2 ? 13 : 1;

				var player = new RoomPlayer
				{
					ConnectionId = connectionId,
					PlayerName = playerName,
					PlayerId = playerId,
					IsHost = false,
					IsReady = false,
					Position = new PlayerPosition { X = startX, Y = startY }
				};

				room.AddPlayer(player);
				return true;
			}
		}

		public bool LeaveRoom(string roomId, string connectionId)
		{
			var room = GetRoom(roomId);
			if (room == null)
			{
				return false;
			}

			lock (_lock)
			{
				room.RemovePlayer(connectionId);

				if (room.Players.Count == 0)
				{
					RemoveRoom(roomId);
				}
				else if (room.HostConnectionId == connectionId && room.Players.Any())
				{
					room.HostConnectionId = room.Players.First().ConnectionId;
					room.Players.First().IsHost = true;
				}

				return true;
			}
		}

		public bool StartGame(string roomId)
		{
			var room = GetRoom(roomId);
			if (room == null || !room.CanStart)
			{
				return false;
			}

			lock (_lock)
			{
				room.State = GameRoomState.InProgress;
				return true;
			}
		}

		public bool UpdateGameState(string roomId, GameStateData gameState)
		{
			var room = GetRoom(roomId);
			if (room == null)
			{
				return false;
			}

			room.CurrentGameState = gameState;
			return true;
		}

		public void RemoveRoom(string roomId)
		{
			_rooms.TryRemove(roomId, out _);
		}

		public void CleanupAbandonedRooms()
		{
			var abandonedRooms = _rooms.Values
				.Where(r => r.Players.Count == 0 ||
						   (r.State == GameRoomState.Waiting &&
							DateTime.UtcNow - r.CreatedAt > TimeSpan.FromMinutes(30)))
				.ToList();

			foreach (var room in abandonedRooms)
			{
				RemoveRoom(room.Id);
			}
		}
	}
}