namespace BombermanServer.Models.Messages
{
	public class CreateRoomRequest
	{
		public string RoomName { get; set; } = "";
		public string PlayerName { get; set; } = "";
		public string Theme { get; set; } = "Desert";
		public int MaxPlayers { get; set; } = 2;
	}

	public class JoinRoomRequest
	{
		public string RoomId { get; set; } = "";
		public string PlayerName { get; set; } = "";
	}

	public class PlayerMoveMessage
	{
		public string RoomId { get; set; } = "";
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class PlaceBombMessage
	{
		public string RoomId { get; set; } = "";
		public int X { get; set; }
		public int Y { get; set; }
		public int Range { get; set; }
	}

	public class ChatMessage
	{
		public string RoomId { get; set; } = "";
		public string Message { get; set; } = "";
	}
}