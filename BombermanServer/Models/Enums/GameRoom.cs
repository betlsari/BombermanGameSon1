namespace BombermanServer.Models.Enums
{
	public class GameRoom
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string HostConnectionId { get; set; } = "";
        public List<RoomPlayer> Players { get; set; } = new();
        public GameRoomState State { get; set; } = GameRoomState.Waiting;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int MaxPlayers { get; set; } = 2;
        public string Theme { get; set; } = "Desert";
        public int MapSeed { get; set; }
        public GameStateData? CurrentGameState { get; set; }

        public bool IsFull => Players.Count >= MaxPlayers;
        public bool CanStart => Players.Count >= 2 && State == GameRoomState.Waiting;

        public RoomPlayer? GetPlayerByConnectionId(string connectionId)
        {
            return Players.FirstOrDefault(p => p.ConnectionId == connectionId);
        }

        public void AddPlayer(RoomPlayer player)
        {
            if (!IsFull && !Players.Any(p => p.ConnectionId == player.ConnectionId))
            {
                Players.Add(player);
            }
        }

        public void RemovePlayer(string connectionId)
        {
            Players.RemoveAll(p => p.ConnectionId == connectionId);
        }


    }
}