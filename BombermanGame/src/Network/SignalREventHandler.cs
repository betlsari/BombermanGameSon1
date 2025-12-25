using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BombermanGame.src.Core;
using BombermanGame.src.Models;
using BombermanGame.src.Patterns.Behavioral.Observer;
using BombermanGame.src.Patterns.Creational.Factory;

namespace BombermanGame.src.Network
{
	/// <summary>
	/// SignalR server event'lerini yönetir ve GameManager ile UI'a iletir
	/// AŞAMA 1.2: SignalR Event Handler Implementation
	/// </summary>
	public class SignalREventHandler
	{
		private readonly SignalRClient _client;
		private readonly GameManager _gameManager;

		// Event delegates for UI updates
		public event Action<string>? OnConnectionStatusChanged;
		public event Action<string, object>? OnRoomInfoUpdated;
		public event Action<string>? OnGameStateChanged;
		public event Action<string>? OnErrorOccurred;
		public event Action<string>? OnPlayerStatusChanged;
		public event Action? OnGameReady;

		// Current game state
		private string _currentRoomId = "";
		private bool _isGameActive = false;
		private Dictionary<string, PlayerInfo> _remotePlayers = new();

		public bool IsGameActive => _isGameActive;
		public string CurrentRoomId => _currentRoomId;

		public SignalREventHandler(SignalRClient client)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_gameManager = GameManager.Instance;

			RegisterEventHandlers();
		}

		/// <summary>
		/// SignalR client'tan gelen tüm event'leri kaydet
		/// </summary>
		private void RegisterEventHandlers()
		{
			// Connection events
			_client.OnConnected += HandleConnected;
			_client.OnDisconnected += HandleDisconnected;
			_client.OnReconnecting += HandleReconnecting;
			_client.OnReconnected += HandleReconnected;
			_client.OnConnectionError += HandleConnectionError;

			// Room events
			_client.OnRoomCreated += HandleRoomCreated;
			_client.OnPlayerJoined += HandlePlayerJoined;
			_client.OnPlayerLeft += HandlePlayerLeft;

			// Game events
			_client.OnGameStarted += HandleGameStarted;
			_client.OnPlayerMoved += HandlePlayerMoved;
			_client.OnBombPlaced += HandleBombPlaced;
			_client.OnGameStateUpdated += HandleGameStateUpdated;
			_client.OnGameEnded += HandleGameEnded;

			// Error handling
			_client.OnError += HandleError;
		}

		/// <summary>
		/// Event handler'ları temizle
		/// </summary>
		public void UnregisterEventHandlers()
		{
			_client.OnConnected -= HandleConnected;
			_client.OnDisconnected -= HandleDisconnected;
			_client.OnReconnecting -= HandleReconnecting;
			_client.OnReconnected -= HandleReconnected;
			_client.OnConnectionError -= HandleConnectionError;
			_client.OnRoomCreated -= HandleRoomCreated;
			_client.OnPlayerJoined -= HandlePlayerJoined;
			_client.OnPlayerLeft -= HandlePlayerLeft;
			_client.OnGameStarted -= HandleGameStarted;
			_client.OnPlayerMoved -= HandlePlayerMoved;
			_client.OnBombPlaced -= HandleBombPlaced;
			_client.OnGameStateUpdated -= HandleGameStateUpdated;
			_client.OnGameEnded -= HandleGameEnded;
			_client.OnError -= HandleError;
		}

		#region Connection Event Handlers

		private void HandleConnected(string connectionId)
		{
			Console.WriteLine($"[HANDLER] ✅ Connected to server: {connectionId}");
			OnConnectionStatusChanged?.Invoke($"Connected: {connectionId.Substring(0, 8)}...");
		}

		private void HandleDisconnected(string reason)
		{
			Console.WriteLine($"[HANDLER] ❌ Disconnected: {reason}");
			OnConnectionStatusChanged?.Invoke($"Disconnected: {reason}");

			// Clean up game state on disconnect
			if (_isGameActive)
			{
				_isGameActive = false;
				OnGameStateChanged?.Invoke("Game interrupted - connection lost");
			}
		}

		private void HandleReconnecting(string message)
		{
			Console.WriteLine($"[HANDLER] 🔄 Reconnecting: {message}");
			OnConnectionStatusChanged?.Invoke($"Reconnecting... {message}");
		}

		private void HandleReconnected(string connectionId)
		{
			Console.WriteLine($"[HANDLER] ✅ Reconnected: {connectionId}");
			OnConnectionStatusChanged?.Invoke($"Reconnected: {connectionId.Substring(0, 8)}...");
		}

		private void HandleConnectionError(Exception error)
		{
			Console.WriteLine($"[HANDLER] ❌ Connection Error: {error.Message}");
			OnErrorOccurred?.Invoke($"Connection error: {error.Message}");
		}

		#endregion

		#region Room Event Handlers

		private void HandleRoomCreated(string roomId, object roomInfo)
		{
			try
			{
				Console.WriteLine($"[HANDLER] 🏠 Room created: {roomId}");
				_currentRoomId = roomId;

				// Parse room info
				var jsonElement = (JsonElement)roomInfo;
				var roomName = jsonElement.GetProperty("Name").GetString() ?? "Unknown Room";
				var theme = jsonElement.GetProperty("Theme").GetString() ?? "Desert";
				var maxPlayers = jsonElement.GetProperty("MaxPlayers").GetInt32();

				Console.WriteLine($"  ├─ Name: {roomName}");
				Console.WriteLine($"  ├─ Theme: {theme}");
				Console.WriteLine($"  └─ Max Players: {maxPlayers}");

				OnRoomInfoUpdated?.Invoke(roomId, roomInfo);
				OnGameStateChanged?.Invoke($"Room created: {roomName}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error parsing room info: {ex.Message}");
				OnErrorOccurred?.Invoke($"Failed to parse room info: {ex.Message}");
			}
		}

		private void HandlePlayerJoined(string playerName, object roomInfo)
		{
			try
			{
				Console.WriteLine($"[HANDLER] 👤 Player joined: {playerName}");

				// Parse room info for player list
				var jsonElement = (JsonElement)roomInfo;
				var playerList = jsonElement.GetProperty("PlayerNames");

				int playerCount = 0;
				foreach (var player in playerList.EnumerateArray())
				{
					playerCount++;
					Console.WriteLine($"  ├─ Player {playerCount}: {player.GetString()}");
				}

				OnRoomInfoUpdated?.Invoke(_currentRoomId, roomInfo);
				OnPlayerStatusChanged?.Invoke($"{playerName} joined ({playerCount} players)");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error handling player join: {ex.Message}");
			}
		}

		private void HandlePlayerLeft(string playerName)
		{
			Console.WriteLine($"[HANDLER] 🚪 Player left: {playerName}");

			// Remove from remote players if exists
			var playerId = _remotePlayers.FirstOrDefault(p => p.Value.Name == playerName).Key;
			if (!string.IsNullOrEmpty(playerId))
			{
				_remotePlayers.Remove(playerId);
			}

			OnPlayerStatusChanged?.Invoke($"{playerName} left the game");
		}

		#endregion

		#region Game Event Handlers

		private void HandleGameStarted(object startMessage)
		{
			try
			{
				Console.WriteLine($"[HANDLER] 🎮 Game starting...");

				var jsonElement = (JsonElement)startMessage;
				var roomId = jsonElement.GetProperty("RoomId").GetString() ?? "";
				var mapSeed = jsonElement.GetProperty("MapSeed").GetInt32();
				var theme = jsonElement.GetProperty("Theme").GetString() ?? "Desert";

				Console.WriteLine($"  ├─ Room: {roomId}");
				Console.WriteLine($"  ├─ Map Seed: {mapSeed}");
				Console.WriteLine($"  └─ Theme: {theme}");

				// Initialize game state
				_isGameActive = true;
				_gameManager.ResetGame();

				// Create map with same seed
				var themeAdapter = Patterns.Structural.Adapter.ThemeFactory.GetTheme(theme);
				_gameManager.CurrentMap = new Map(21, 15, themeAdapter, mapSeed);

				// Parse players
				var playersArray = jsonElement.GetProperty("Players");
				var myConnectionId = _client.ConnectionId;

				foreach (var playerElement in playersArray.EnumerateArray())
				{
					var connectionId = playerElement.GetProperty("ConnectionId").GetString() ?? "";
					var name = playerElement.GetProperty("Name").GetString() ?? "Unknown";
					var playerId = playerElement.GetProperty("PlayerId").GetInt32();
					var startX = playerElement.GetProperty("StartX").GetInt32();
					var startY = playerElement.GetProperty("StartY").GetInt32();

					var player = new Player(playerId, name, new Position(startX, startY));
					_gameManager.Players.Add(player);

					Console.WriteLine($"  ├─ Player {playerId}: {name} at ({startX},{startY})");

					// Track remote players
					if (connectionId != myConnectionId)
					{
						_remotePlayers[connectionId] = new PlayerInfo
						{
							ConnectionId = connectionId,
							Name = name,
							PlayerId = playerId
						};
					}
				}

				// Spawn enemies (only if we're the host)
				// Note: In SignalR implementation, server should handle enemies
				// But for compatibility, we'll spawn them locally
				SpawnEnemies();

				OnGameReady?.Invoke();
				OnGameStateChanged?.Invoke("Game started!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error starting game: {ex.Message}");
				OnErrorOccurred?.Invoke($"Failed to start game: {ex.Message}");
			}
		}

		private void HandlePlayerMoved(string connectionId, int x, int y)
		{
			try
			{
				// Find the player by connection ID
				if (_remotePlayers.TryGetValue(connectionId, out var playerInfo))
				{
					var player = _gameManager.Players.FirstOrDefault(p => p.Id == playerInfo.PlayerId);
					if (player != null)
					{
						player.Position.X = x;
						player.Position.Y = y;

						// Don't log every movement - too spammy
						// Console.WriteLine($"[HANDLER] 🎮 Player {playerInfo.Name} moved to ({x},{y})");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error handling player move: {ex.Message}");
			}
		}

		private void HandleBombPlaced(string connectionId, int x, int y, int range)
		{
			try
			{
				if (_remotePlayers.TryGetValue(connectionId, out var playerInfo))
				{
					Console.WriteLine($"[HANDLER] 💣 Bomb placed by {playerInfo.Name} at ({x},{y})");

					var bomb = new Bomb(new Position(x, y), range, playerInfo.PlayerId);
					_gameManager.Bombs.Add(bomb);

					// Notify observers
					_gameManager.Notify(new GameEvent(EventType.BombExploded, bomb));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error handling bomb placement: {ex.Message}");
			}
		}

		private void HandleGameStateUpdated(object gameState)
		{
			try
			{
				// Parse game state from server
				var jsonElement = (JsonElement)gameState;

				// Update bombs
				if (jsonElement.TryGetProperty("Bombs", out var bombsElement))
				{
					_gameManager.Bombs.Clear();
					foreach (var bombElement in bombsElement.EnumerateArray())
					{
						var x = bombElement.GetProperty("X").GetInt32();
						var y = bombElement.GetProperty("Y").GetInt32();
						var range = bombElement.GetProperty("Range").GetInt32();
						var timer = bombElement.GetProperty("Timer").GetInt32();
						var ownerId = bombElement.GetProperty("OwnerId").GetString() ?? "0";
						var hasExploded = bombElement.GetProperty("HasExploded").GetBoolean();

						if (!hasExploded)
						{
							var bomb = new Bomb(new Position(x, y), range, int.Parse(ownerId))
							{
								Timer = timer,
								HasExploded = hasExploded
							};
							_gameManager.Bombs.Add(bomb);
						}
					}
				}

				// Update enemies
				if (jsonElement.TryGetProperty("Enemies", out var enemiesElement))
				{
					_gameManager.Enemies.Clear();
					foreach (var enemyElement in enemiesElement.EnumerateArray())
					{
						var id = int.Parse(enemyElement.GetProperty("Id").GetString() ?? "0");
						var type = enemyElement.GetProperty("Type").GetString() ?? "StaticEnemy";
						var x = enemyElement.GetProperty("X").GetInt32();
						var y = enemyElement.GetProperty("Y").GetInt32();
						var isAlive = enemyElement.GetProperty("IsAlive").GetBoolean();

						if (isAlive)
						{
							var enemyType = type.Replace("Enemy", "");
							var parsedType = Enum.Parse<EnemyType>(enemyType);
							var enemy = new Enemy(id, new Position(x, y), parsedType)
							{
								IsAlive = isAlive
							};
							_gameManager.Enemies.Add(enemy);
						}
					}
				}

				// Update power-ups
				if (jsonElement.TryGetProperty("PowerUps", out var powerUpsElement))
				{
					_gameManager.PowerUps.Clear();
					foreach (var powerUpElement in powerUpsElement.EnumerateArray())
					{
						var type = powerUpElement.GetProperty("Type").GetString() ?? "BombCount";
						var x = powerUpElement.GetProperty("X").GetInt32();
						var y = powerUpElement.GetProperty("Y").GetInt32();
						var isCollected = powerUpElement.GetProperty("IsCollected").GetBoolean();

						if (!isCollected)
						{
							var parsedType = Enum.Parse<PowerUpType>(type);
							var powerUp = new PowerUp(new Position(x, y), parsedType)
							{
								IsCollected = isCollected
							};
							_gameManager.PowerUps.Add(powerUp);
						}
					}
				}

				// Console.WriteLine($"[HANDLER] 🔄 Game state updated");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error updating game state: {ex.Message}");
			}
		}

		private void HandleGameEnded(string winnerId, int finalScore)
		{
			try
			{
				Console.WriteLine($"[HANDLER] 🏆 Game ended!");
				Console.WriteLine($"  ├─ Winner: {winnerId}");
				Console.WriteLine($"  └─ Score: {finalScore}");

				_isGameActive = false;

				// Determine winner name
				var winnerName = "No one";
				if (!string.IsNullOrEmpty(winnerId) && winnerId != "0")
				{
					var winner = _gameManager.Players.FirstOrDefault(p => p.Id.ToString() == winnerId);
					winnerName = winner?.Name ?? "Unknown";
				}

				_gameManager.Notify(new GameEvent(EventType.GameEnded, winnerName));
				OnGameStateChanged?.Invoke($"Game ended! Winner: {winnerName}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error handling game end: {ex.Message}");
			}
		}

		#endregion

		#region Error Handling

		private void HandleError(string errorMessage)
		{
			Console.WriteLine($"[HANDLER] ❌ Server Error: {errorMessage}");
			OnErrorOccurred?.Invoke(errorMessage);
		}

		#endregion

		#region Helper Methods

		private void SpawnEnemies()
		{
			try
			{
				var staticFactory = EnemyFactoryProvider.GetFactory("static");
				var chaseFactory = EnemyFactoryProvider.GetFactory("chase");
				var smartFactory = EnemyFactoryProvider.GetFactory("smart");

				_gameManager.Enemies.Add(staticFactory.CreateEnemy(1, new Position(10, 7)));
				_gameManager.Enemies.Add(chaseFactory.CreateEnemy(2, new Position(15, 5)));
				_gameManager.Enemies.Add(smartFactory.CreateEnemy(3, new Position(5, 10)));

				Console.WriteLine("[HANDLER] 👾 Enemies spawned");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[HANDLER] ❌ Error spawning enemies: {ex.Message}");
			}
		}

		public void Reset()
		{
			_currentRoomId = "";
			_isGameActive = false;
			_remotePlayers.Clear();
			Console.WriteLine("[HANDLER] 🔄 Event handler reset");
		}

		#endregion
	}

	/// <summary>
	/// Remote oyuncu bilgileri
	/// </summary>
	public class PlayerInfo
	{
		public string ConnectionId { get; set; } = "";
		public string Name { get; set; } = "";
		public int PlayerId { get; set; }
	}
}