namespace BombermanServer.Models
{
	public class GameStateData
	{
		public List<PlayerStateDto> Players { get; set; } = new();
		public List<BombStateDto> Bombs { get; set; } = new();
		public List<EnemyStateDto> Enemies { get; set; } = new();
		public List<PowerUpStateDto> PowerUps { get; set; } = new();
		public int GameTick { get; set; }
		public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
	}

	public class PlayerStateDto
	{
		public string ConnectionId { get; set; } = "";
		public string Name { get; set; } = "";
		public int PlayerId { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public bool IsAlive { get; set; }
		public int BombCount { get; set; }
		public int BombPower { get; set; }
		public int Speed { get; set; }
		public int Score { get; set; }
	}

	public class BombStateDto
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string OwnerId { get; set; } = "";
		public int X { get; set; }
		public int Y { get; set; }
		public int Timer { get; set; }
		public int Range { get; set; }
		public bool HasExploded { get; set; }
	}

	public class EnemyStateDto
	{
		public string Id { get; set; } = "";
		public string Type { get; set; } = "";
		public int X { get; set; }
		public int Y { get; set; }
		public bool IsAlive { get; set; }
		public int Health { get; set; }
	}

	public class PowerUpStateDto
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Type { get; set; } = "";
		public int X { get; set; }
		public int Y { get; set; }
		public bool IsCollected { get; set; }
	}
}