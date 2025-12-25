namespace BombermanServer.Models.Messages
{
	public class RoomCreatedResponse
	{
		public bool Success { get; set; }
		public string? RoomId { get; set; }
		public string? ErrorMessage { get; set; }
		public RoomInfoDto? RoomInfo { get; set; }
	}

	public class RoomInfoDto
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public int CurrentPlayers { get; set; }
		public int MaxPlayers { get; set; }
		public string State { get; set; } = "";
		public string Theme { get; set; } = "";
		public List<string> PlayerNames { get; set; } = new();
	}

	public class RoomListResponse
	{
		public List<RoomInfoDto> Rooms { get; set; } = new();
		public int TotalRooms { get; set; }
	}

	public class ErrorResponse
	{
		public string ErrorCode { get; set; } = "";
		public string Message { get; set; } = "";
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	}

	public class GameStartedMessage
	{
		public string RoomId { get; set; } = "";
		public int MapSeed { get; set; }
		public string Theme { get; set; } = "";
		public List<PlayerStartInfo> Players { get; set; } = new();
	}

	public class PlayerStartInfo
	{
		public string ConnectionId { get; set; } = "";
		public string Name { get; set; } = "";
		public int PlayerId { get; set; }
		public int StartX { get; set; }
		public int StartY { get; set; }
	}
}