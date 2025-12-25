namespace BombermanServer.Models
{
	public class RoomPlayer
	{
		public string ConnectionId { get; set; } = "";
		public string PlayerName { get; set; } = "";
		public int PlayerId { get; set; }
		public bool IsHost { get; set; }
		public bool IsReady { get; set; }
		public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
		public PlayerPosition Position { get; set; } = new();
		public PlayerStats Stats { get; set; } = new();
		public bool IsAlive { get; set; } = true;
	}

	public class PlayerPosition
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class PlayerStats
	{
		public int BombCount { get; set; } = 1;
		public int BombPower { get; set; } = 1;
		public int Speed { get; set; } = 1;
		public int Score { get; set; } = 0;
	}
}