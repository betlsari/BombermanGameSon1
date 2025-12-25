// MVC/Controllers/MultiplayerGameController.cs - TAMAMEN DÜZELTİLMİŞ
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BombermanGame.src.Core;
using BombermanGame.src.Models;
using BombermanGame.src.Patterns.Repository;
using BombermanGame.src.Patterns.Behavioral.Command;
using BombermanGame.src.Patterns.Behavioral.Observer;
using BombermanGame.src.Patterns.Creational.Factory;
using BombermanGame.src.UI;
using BombermanGame.src.Utils;

namespace BombermanGame.src.MVC.Controllers
{
    public class MultiplayerGameController
    {
        private GameManager _gameManager;
        private GameRenderer _gameRenderer;
        private NetworkManager _networkManager;
        private CommandInvoker _commandInvoker;
        private ScoreObserver _scoreObserver;
        private StatsObserver _statsObserver;
        private UIObserver _uiObserver;
        private bool _isRunning;
        private ScoreRepository _scoreRepository;
        private bool _isHost;
        private int _localPlayerId;
        private int _remotePlayerId;
        private int _mapSeed; // Aynı haritayı garanti etmek için seed

        public MultiplayerGameController()
        {
            _gameManager = GameManager.Instance;
            _gameRenderer = new GameRenderer();
            _networkManager = new NetworkManager();
            _commandInvoker = new CommandInvoker();
            _scoreRepository = new ScoreRepository();

            _scoreObserver = new ScoreObserver();
            _statsObserver = new StatsObserver();
            _uiObserver = new UIObserver();

            _gameManager.Attach(_scoreObserver);
            _gameManager.Attach(_statsObserver);
            _gameManager.Attach(_uiObserver);

            _networkManager.OnMessageReceived += HandleNetworkMessage;
            _networkManager.OnPlayerConnected += HandlePlayerConnected;
            _networkManager.OnPlayerDisconnected += HandlePlayerDisconnected;
        }

        public async Task StartAsHost(string theme, int port = 9999)
        {
            _isHost = true;
            _localPlayerId = 1;
            _remotePlayerId = 2;

            // Rastgele seed oluştur
            _mapSeed = new Random().Next();

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              HOSTING MULTIPLAYER GAME                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            await _networkManager.StartServer(port);

            if (!_networkManager.IsConnected)
            {
                Console.WriteLine("\n❌ Failed to start server!");
                Console.WriteLine("Press any key to return to menu...");
                Console.ReadKey();
                return;
            }

            // Oyuncu bağlanana kadar bekle
            int waitCount = 0;
            while (!_networkManager.IsConnected && waitCount < 60)
            {
                await Task.Delay(500);
                waitCount++;
            }

            if (!_networkManager.IsConnected)
            {
                Console.WriteLine("\n❌ No player connected (timeout)");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\n✅ Opponent connected!");

            // Oyun ayarlarını gönder (tema ve map seed)
            await SendGameSetup(theme);

            await Task.Delay(2000);

            await InitializeMultiplayerGame(theme);
            await GameLoop();
        }

        public async Task ConnectToHost(string theme, string hostIP, int port = 9999)
        {
            _isHost = false;
            _localPlayerId = 2;
            _remotePlayerId = 1;

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              JOINING MULTIPLAYER GAME                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine($"Connecting to {hostIP}:{port}...");

            await _networkManager.ConnectToServer(hostIP, port);

            if (!_networkManager.IsConnected)
            {
                Console.WriteLine("\n❌ Failed to connect to server!");
                Console.WriteLine("Press any key to return to menu...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\n✅ Connected to server!");

            long latency = await _networkManager.MeasureLatencyAsync();
            if (latency > 0)
            {
                Console.WriteLine($"🌐 Latency: {latency}ms");
            }

            // Host'tan oyun ayarlarını bekle
            Console.WriteLine("\n⏳ Waiting for game setup...");
            await Task.Delay(3000);

            // Eğer map seed alınmadıysa varsayılan kullan
            if (_mapSeed == 0)
            {
                _mapSeed = 12345; // Fallback
            }

            await InitializeMultiplayerGame(theme);
            await GameLoop();
        }

        // Oyun ayarlarını gönder (sadece host)
        private async Task SendGameSetup(string theme)
        {
            var setupMsg = NetworkProtocol.CreateGameStartMessage(theme, _mapSeed);
            await _networkManager.SendMessageAsync(setupMsg);
            Console.WriteLine($"[HOST] 📤 Game setup sent (seed: {_mapSeed})");
        }

        private async Task InitializeMultiplayerGame(string theme)
        {
            _isRunning = true;
            _gameManager.ResetGame();

            // AYNI HARİTAYI OLUŞTUR (seed kullanarak)
            var themeAdapter = Patterns.Structural.Adapter.ThemeFactory.GetTheme(theme);
            _gameManager.CurrentMap = new Map(21, 15, themeAdapter, _mapSeed);

            // Oyuncuları oluştur
            var localPlayer = new Player(_localPlayerId,
                _isHost ? "Host" : "Client",
                _isHost ? new Position(1, 1) : new Position(19, 13));

            var remotePlayer = new Player(_remotePlayerId,
                _isHost ? "Client" : "Host",
                _isHost ? new Position(19, 13) : new Position(1, 1));

            _gameManager.Players.Add(localPlayer);
            _gameManager.Players.Add(remotePlayer);

            // Düşmanlar (sadece host oluşturur)
            if (_isHost)
            {
                SpawnEnemies();
            }

            Console.Clear();
            Console.WriteLine($"╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  YOU: {(_isHost ? "Host (P1)" : "Client (P2)"),-52} ║");
            Console.WriteLine($"║  Map Seed: {_mapSeed,-47} ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine("\nGame starting in...");
            for (int i = 3; i > 0; i--)
            {
                Console.WriteLine($"  {i}");
                await Task.Delay(1000);
            }
            Console.WriteLine("  GO!");
            await Task.Delay(500);
        }

        private async Task GameLoop()
        {
            int frameCount = 0;
            int syncInterval = _isHost ? 3 : 100; // Host her 3 frame'de sync, client sadece render

            while (_isRunning && _networkManager.CheckConnection())
            {
                Console.Clear();

                // Oyuncu bilgilerini göster
                var localPlayer = _gameManager.Players.FirstOrDefault(p => p.Id == _localPlayerId);
                var remotePlayer = _gameManager.Players.FirstOrDefault(p => p.Id == _remotePlayerId);

                Console.WriteLine($"YOU ({(_isHost ? "Host" : "Client")}): {localPlayer?.Position.X},{localPlayer?.Position.Y} | " +
                                $"OPPONENT: {remotePlayer?.Position.X},{remotePlayer?.Position.Y}");

                _gameRenderer.Render(_gameManager);

                // Input kontrolü
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    await HandleMultiplayerInput(key);
                }

                frameCount++;

                // Host: Oyun mantığını güncelle ve state'i gönder
                if (_isHost && frameCount % syncInterval == 0)
                {
                    UpdateBombs();
                    UpdateEnemies();
                    CheckCollisions();

                    // Oyun durumunu gönder
                    await _networkManager.SendGameState(_gameManager);
                }

                // Oyun bitti mi kontrol et
                if (CheckGameOver())
                {
                    _isRunning = false;
                    await EndMultiplayerGame();
                }

                await Task.Delay(100);
            }

            if (!_networkManager.IsConnected)
            {
                Console.Clear();
                Console.WriteLine("\n❌ Connection lost!");
                Console.WriteLine("Press any key to return to menu...");
                Console.ReadKey();
            }
        }

        private async Task HandleMultiplayerInput(ConsoleKeyInfo key)
        {
            var localPlayer = _gameManager.Players.FirstOrDefault(p => p.Id == _localPlayerId);
            if (localPlayer == null || !localPlayer.IsAlive) return;

            ICommand? command = null;
            bool moved = false;

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    command = new MoveCommand(localPlayer, 0, -1, _gameManager.CurrentMap!);
                    moved = true;
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    command = new MoveCommand(localPlayer, 0, 1, _gameManager.CurrentMap!);
                    moved = true;
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    command = new MoveCommand(localPlayer, -1, 0, _gameManager.CurrentMap!);
                    moved = true;
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    command = new MoveCommand(localPlayer, 1, 0, _gameManager.CurrentMap!);
                    moved = true;
                    break;
                case ConsoleKey.Spacebar:
                    command = new PlaceBombCommand(localPlayer);
                    break;
                case ConsoleKey.Escape:
                    _isRunning = false;
                    return;
            }

            if (command != null)
            {
                // Komutu lokal olarak uygula
                _commandInvoker.ExecuteCommand(command);

                // Hareketi network'e gönder
                if (moved)
                {
                    await _networkManager.SendPlayerMove(localPlayer);
                    Console.WriteLine($"[LOCAL] 📤 Move sent: ({localPlayer.Position.X},{localPlayer.Position.Y})");
                }
                else if (command is PlaceBombCommand)
                {
                    var bomb = _gameManager.Bombs.LastOrDefault(b => b.OwnerId == localPlayer.Id);
                    if (bomb != null)
                    {
                        await _networkManager.SendPlaceBomb(bomb);
                        Console.WriteLine($"[LOCAL] 💣 Bomb placed and sent");
                    }
                }

                CheckPowerUpCollection(localPlayer);
            }
        }

        private void HandleNetworkMessage(NetworkMessage message)
        {
            try
            {
                switch (message.Type)
                {
                    case MessageType.PlayerMove:
                        HandleRemotePlayerMove(message);
                        break;

                    case MessageType.PlaceBomb:
                        HandleRemotePlaceBomb(message);
                        break;

                    case MessageType.GameState:
                        HandleGameStateUpdate(message);
                        break;

                    case MessageType.GameStart:
                        HandleGameSetup(message);
                        break;

                    case MessageType.GameEnd:
                        HandleGameEnd(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Message handling: {ex.Message}");
            }
        }

        private void HandleGameSetup(NetworkMessage message)
        {
            try
            {
                // NetworkProtocol helper metodunu kullan
                _mapSeed = NetworkProtocol.ParseMapSeed(message);
                Console.WriteLine($"[CLIENT] 📥 Received map seed: {_mapSeed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Setup handling: {ex.Message}");
                _mapSeed = 12345; // Fallback
            }
        }

        private void HandleRemotePlayerMove(NetworkMessage message)
        {
            try
            {
                var moveData = System.Text.Json.JsonSerializer.Deserialize<PlayerMoveMessage>(message.Data);
                if (moveData == null) return;

                var remotePlayer = _gameManager.Players.FirstOrDefault(p => p.Id == _remotePlayerId);
                if (remotePlayer != null)
                {
                    remotePlayer.Position.X = moveData.X;
                    remotePlayer.Position.Y = moveData.Y;
                    Console.WriteLine($"[REMOTE] 📥 Move received: ({moveData.X},{moveData.Y})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Move handling: {ex.Message}");
            }
        }

        private void HandleRemotePlaceBomb(NetworkMessage message)
        {
            try
            {
                var bombData = System.Text.Json.JsonSerializer.Deserialize<PlaceBombMessage>(message.Data);
                if (bombData == null) return;

                var bomb = new Bomb(
                    new Position(bombData.X, bombData.Y),
                    bombData.Range,
                    int.Parse(bombData.PlayerId)
                );
                bomb.Timer = bombData.Timer;

                _gameManager.Bombs.Add(bomb);
                Console.WriteLine($"[REMOTE] 💣 Bomb placed at ({bombData.X},{bombData.Y})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Bomb handling: {ex.Message}");
            }
        }

        private void HandleGameStateUpdate(NetworkMessage message)
        {
            if (_isHost) return; // Host kendi state'ini oluşturur

            try
            {
                var gameState = System.Text.Json.JsonSerializer.Deserialize<GameStateMessage>(message.Data);
                if (gameState == null) return;

                // Bombaları güncelle
                _gameManager.Bombs.Clear();
                foreach (var bombState in gameState.Bombs)
                {
                    if (!bombState.HasExploded)
                    {
                        var bomb = new Bomb(
                            new Position(bombState.X, bombState.Y),
                            bombState.Range,
                            int.Parse(bombState.OwnerId)
                        );
                        bomb.Timer = bombState.Timer;
                        bomb.HasExploded = bombState.HasExploded;
                        _gameManager.Bombs.Add(bomb);
                    }
                }

                // Düşmanları güncelle
                _gameManager.Enemies.Clear();
                foreach (var enemyState in gameState.Enemies)
                {
                    if (enemyState.IsAlive)
                    {
                        var enemyType = Enum.Parse<EnemyType>(enemyState.Type.Replace("Enemy", ""));
                        var enemy = new Enemy(
                            int.Parse(enemyState.Id),
                            new Position(enemyState.X, enemyState.Y),
                            enemyType
                        );
                        enemy.IsAlive = enemyState.IsAlive;
                        enemy.Health = enemyState.Health;
                        _gameManager.Enemies.Add(enemy);
                    }
                }

                // Power-up'ları güncelle
                _gameManager.PowerUps.Clear();
                foreach (var powerUpState in gameState.PowerUps)
                {
                    if (!powerUpState.IsCollected)
                    {
                        var powerUpType = Enum.Parse<PowerUpType>(powerUpState.Type);
                        var powerUp = new PowerUp(
                            new Position(powerUpState.X, powerUpState.Y),
                            powerUpType
                        );
                        powerUp.IsCollected = powerUpState.IsCollected;
                        _gameManager.PowerUps.Add(powerUp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GameState handling: {ex.Message}");
            }
        }

        private void HandleGameEnd(NetworkMessage message)
        {
            _isRunning = false;
        }

        private void HandlePlayerConnected(string playerId)
        {
            Console.WriteLine($"[MULTIPLAYER] Player {playerId.Substring(0, 8)}... joined!");
        }

        private void HandlePlayerDisconnected(string playerId)
        {
            _isRunning = false;
            Console.WriteLine($"[MULTIPLAYER] Player {playerId.Substring(0, 8)}... left the game!");
        }

        private async Task EndMultiplayerGame()
        {
            var winner = _gameManager.Players.FirstOrDefault(p => p.IsAlive);
            string winnerName = winner?.Name ?? "No one";

            _gameManager.Notify(new GameEvent(EventType.GameEnded, winnerName));

            if (_isHost && _networkManager.IsConnected)
            {
                var endMsg = NetworkProtocol.CreateGameEndMessage(
                    winner?.Id.ToString() ?? "0",
                    _scoreObserver.GetScore()
                );
                await _networkManager.SendMessageAsync(endMsg);
            }

            Console.Clear();
            if (winner?.Id == _localPlayerId)
            {
                ConsoleUI.WriteLineColored("\n🏆 YOU WIN! 🏆\n", ConsoleColor.Green);
            }
            else if (winner != null)
            {
                ConsoleUI.WriteLineColored("\n💀 YOU LOSE 💀\n", ConsoleColor.Red);
            }
            else
            {
                ConsoleUI.WriteLineColored("\n🤝 DRAW 🤝\n", ConsoleColor.Yellow);
            }

            Console.WriteLine($"Final Score: {_scoreObserver.GetScore()}");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();

            _networkManager.Disconnect();
        }

        // Helper metodlar
        private void SpawnEnemies()
        {
            var staticFactory = EnemyFactoryProvider.GetFactory("static");
            var chaseFactory = EnemyFactoryProvider.GetFactory("chase");
            var smartFactory = EnemyFactoryProvider.GetFactory("smart");

            _gameManager.Enemies.Add(staticFactory.CreateEnemy(1, new Position(10, 7)));
            _gameManager.Enemies.Add(chaseFactory.CreateEnemy(2, new Position(15, 5)));
            _gameManager.Enemies.Add(smartFactory.CreateEnemy(3, new Position(5, 10)));
        }

        private void UpdateBombs()
        {
            var bombsToExplode = new List<Bomb>();

            foreach (var bomb in _gameManager.Bombs)
            {
                bomb.Update();
                if (bomb.ShouldExplode())
                {
                    bombsToExplode.Add(bomb);
                }
            }

            foreach (var bomb in bombsToExplode)
            {
                ExplodeBomb(bomb);
            }
        }

        private void ExplodeBomb(Bomb bomb)
        {
            if (_gameManager.CurrentMap == null) return;

            bomb.HasExploded = true;
            var explosionArea = _gameManager.CurrentMap.GetExplosionArea(bomb.Position, bomb.Power);

            _gameManager.Notify(new GameEvent(EventType.BombExploded, bomb));

            foreach (var pos in explosionArea)
            {
                if (_gameManager.CurrentMap.IsDestructible(pos.X, pos.Y))
                {
                    _gameManager.CurrentMap.DamageWall(pos.X, pos.Y);
                    _gameManager.Notify(new GameEvent(EventType.WallDestroyed));

                    if (new Random().Next(100) < 30)
                    {
                        SpawnPowerUp(pos);
                    }
                }

                foreach (var player in _gameManager.Players)
                {
                    if (player.IsAlive && player.Position.X == pos.X && player.Position.Y == pos.Y)
                    {
                        player.State?.TakeDamage(player);
                        _gameManager.Notify(new GameEvent(EventType.PlayerDied, player.Name));
                    }
                }

                foreach (var enemy in _gameManager.Enemies)
                {
                    if (enemy.IsAlive && enemy.Position.X == pos.X && enemy.Position.Y == pos.Y)
                    {
                        enemy.IsAlive = false;
                        _gameManager.Notify(new GameEvent(EventType.EnemyKilled));
                    }
                }
            }

            _gameManager.Bombs.Remove(bomb);
        }

        private void SpawnPowerUp(Position position)
        {
            var types = Enum.GetValues(typeof(PowerUpType));
            var randomType = (PowerUpType)types.GetValue(new Random().Next(types.Length))!;
            _gameManager.PowerUps.Add(new PowerUp(position, randomType));
        }

        private void CheckPowerUpCollection(Player player)
        {
            var powerUp = _gameManager.PowerUps.FirstOrDefault(p =>
                !p.IsCollected && p.Position.X == player.Position.X && p.Position.Y == player.Position.Y);

            if (powerUp != null)
            {
                powerUp.IsCollected = true;
                ApplyPowerUp(player, powerUp);
                _gameManager.Notify(new GameEvent(EventType.PowerUpCollected, powerUp.Type));
                _gameManager.PowerUps.Remove(powerUp);
            }
        }

        private void ApplyPowerUp(Player player, PowerUp powerUp)
        {
            switch (powerUp.Type)
            {
                case PowerUpType.BombCount:
                    player.IncreaseBombCount();
                    break;
                case PowerUpType.BombPower:
                    player.IncreaseBombPower();
                    break;
                case PowerUpType.SpeedBoost:
                    player.IncreaseSpeed();
                    break;
            }
        }

        private void UpdateEnemies()
        {
            var alivePlayer = _gameManager.Players.FirstOrDefault(p => p.IsAlive);
            if (alivePlayer == null || _gameManager.CurrentMap == null) return;

            foreach (var enemy in _gameManager.Enemies.Where(e => e.IsAlive))
            {
                enemy.Move(_gameManager.CurrentMap, alivePlayer.Position);
            }
        }

        private void CheckCollisions()
        {
            foreach (var player in _gameManager.Players.Where(p => p.IsAlive))
            {
                foreach (var enemy in _gameManager.Enemies.Where(e => e.IsAlive))
                {
                    if (player.Position.X == enemy.Position.X && player.Position.Y == enemy.Position.Y)
                    {
                        player.State?.TakeDamage(player);
                        _gameManager.Notify(new GameEvent(EventType.PlayerDied, player.Name));
                    }
                }
            }
        }

        private bool CheckGameOver()
        {
            int alivePlayers = _gameManager.Players.Count(p => p.IsAlive);
            return alivePlayers <= 1;
        }
    }
}