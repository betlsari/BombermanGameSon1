// Core/LobbySystem.cs - MULTIPLAYER LOBBY SYSTEM (BONUS +5)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BombermanGame.src.Utils;

namespace BombermanGame.src.Core
{
    /// <summary>
    /// Multiplayer lobi sistemi - oyuncuların bekleme odası
    /// BONUS FEATURE: Lobby System (+5 puan)
    /// </summary>
    public class LobbySystem
    {
        private TcpListener? _lobbyServer;
        private List<LobbyPlayer> _players;
        private bool _isRunning;
        private int _maxPlayers = 4;
        private string _lobbyName;
        private Thread? _listenerThread;

        public event Action<LobbyPlayer>? OnPlayerJoined;
        public event Action<LobbyPlayer>? OnPlayerLeft;
        public event Action? OnGameStarting;

        public int PlayerCount => _players.Count;
        public bool IsFull => _players.Count >= _maxPlayers;
        public List<LobbyPlayer> Players => _players.ToList();

        public LobbySystem(string lobbyName = "Bomberman Lobby", int maxPlayers = 4)
        {
            _lobbyName = lobbyName;
            _maxPlayers = maxPlayers;
            _players = new List<LobbyPlayer>();
        }

        /// <summary>
        /// Lobi sunucusunu başlat
        /// </summary>
        public async Task StartLobby(int port = 9999)
        {
            try
            {
                _isRunning = true;
                _lobbyServer = new TcpListener(IPAddress.Any, port);
                _lobbyServer.Start();

                Console.WriteLine($"╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║              LOBBY CREATED: {_lobbyName,-35}║");
                Console.WriteLine($"╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine($"\n🌐 Lobby Address: {GetLocalIPAddress()}:{port}");
                Console.WriteLine($"👥 Max Players: {_maxPlayers}");
                Console.WriteLine($"📊 Status: Waiting for players...\n");

                // Oyuncu kabul döngüsü
                while (_isRunning && !IsFull)
                {
                    try
                    {
                        var client = await _lobbyServer.AcceptTcpClientAsync();
                        await HandleNewPlayer(client);
                    }
                    catch (Exception ex)
                    {
                        if (_isRunning)
                        {
                            Console.WriteLine($"[LOBBY] Connection error: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOBBY] Error starting lobby: {ex.Message}");
            }
        }

        /// <summary>
        /// Lobiye bağlan
        /// </summary>
        public async Task<bool> JoinLobby(string hostIP, int port, string playerName)
        {
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(hostIP, port);

                Console.WriteLine($"\n✅ Connected to lobby at {hostIP}:{port}");

                // Oyuncu bilgilerini gönder
                var joinMessage = new Dictionary<string, string>
                {
                    { "action", "join" },
                    { "playerName", playerName }
                };

                var stream = client.GetStream();
                var data = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(joinMessage) + "\n");
                await stream.WriteAsync(data, 0, data.Length);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOBBY] Failed to join: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Yeni oyuncuyu işle
        /// </summary>
        private async Task HandleNewPlayer(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[4096];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(message);

                    if (data != null && data.ContainsKey("action") && data["action"] == "join")
                    {
                        var playerName = data.ContainsKey("playerName") ? data["playerName"] : "Player";
                        var playerId = Guid.NewGuid().ToString();

                        var lobbyPlayer = new LobbyPlayer
                        {
                            Id = playerId,
                            Name = playerName,
                            Client = client,
                            IsReady = false,
                            JoinedAt = DateTime.Now
                        };

                        _players.Add(lobbyPlayer);

                        Console.WriteLine($"✅ Player joined: {playerName} ({_players.Count}/{_maxPlayers})");

                        OnPlayerJoined?.Invoke(lobbyPlayer);

                        // Lobideki tüm oyunculara bildir
                        await BroadcastLobbyState();

                        // Lobi doldu mu kontrol et
                        if (IsFull)
                        {
                            await StartGame();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOBBY] Error handling player: {ex.Message}");
            }
        }

        /// <summary>
        /// Lobi durumunu tüm oyunculara gönder
        /// </summary>
        private async Task BroadcastLobbyState()
        {
            var lobbyState = new Dictionary<string, object>
            {
                { "action", "lobbyUpdate" },
                { "playerCount", _players.Count },
                { "maxPlayers", _maxPlayers },
                { "players", _players.Select(p => new { p.Id, p.Name, p.IsReady }).ToList() }
            };

            var message = System.Text.Json.JsonSerializer.Serialize(lobbyState) + "\n";
            var data = Encoding.UTF8.GetBytes(message);

            foreach (var player in _players)
            {
                try
                {
                    var stream = player.Client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOBBY] Failed to send to {player.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Oyunu başlat
        /// </summary>
        private async Task StartGame()
        {
            Console.WriteLine("\n🎮 Starting game...");

            // Countdown
            for (int i = 3; i > 0; i--)
            {
                Console.WriteLine($"  {i}...");
                await Task.Delay(1000);
            }

            Console.WriteLine("  GO!");

            OnGameStarting?.Invoke();

            // Tüm oyunculara oyun başlıyor mesajı gönder
            var startMessage = new Dictionary<string, string>
            {
                { "action", "gameStarting" }
            };

            var message = System.Text.Json.JsonSerializer.Serialize(startMessage) + "\n";
            var data = Encoding.UTF8.GetBytes(message);

            foreach (var player in _players)
            {
                try
                {
                    var stream = player.Client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                }
                catch { }
            }

            _isRunning = false;
        }

        /// <summary>
        /// Oyuncuyu lobiden çıkar
        /// </summary>
        public void RemovePlayer(string playerId)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                _players.Remove(player);
                player.Client.Close();

                Console.WriteLine($"❌ Player left: {player.Name}");
                OnPlayerLeft?.Invoke(player);

                _ = BroadcastLobbyState();
            }
        }

        /// <summary>
        /// Oyuncunun hazır durumunu değiştir
        /// </summary>
        public async Task SetPlayerReady(string playerId, bool isReady)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsReady = isReady;
                Console.WriteLine($"{(isReady ? "✅" : "⏸️")} {player.Name} is {(isReady ? "ready" : "not ready")}");
                await BroadcastLobbyState();

                // Tüm oyuncular hazır mı?
                if (_players.All(p => p.IsReady) && _players.Count >= 2)
                {
                    await StartGame();
                }
            }
        }

        /// <summary>
        /// Lobi durumunu göster
        /// </summary>
        public void DisplayLobbyState()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║              LOBBY: {_lobbyName,-42}║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine($"👥 Players: {_players.Count}/{_maxPlayers}\n");

            if (_players.Count > 0)
            {
                Console.WriteLine("Player Name          | Status   | Joined");
                Console.WriteLine("─────────────────────┼──────────┼────────────────");

                foreach (var player in _players)
                {
                    var status = player.IsReady ? "✅ Ready" : "⏸️  Waiting";
                    Console.WriteLine($"{player.Name,-20} | {status,-8} | {player.JoinedAt:HH:mm:ss}");
                }
            }
            else
            {
                Console.WriteLine("No players in lobby yet...");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Lobi kapat
        /// </summary>
        public void CloseLobby()
        {
            _isRunning = false;

            foreach (var player in _players)
            {
                player.Client.Close();
            }

            _players.Clear();
            _lobbyServer?.Stop();

            Console.WriteLine("Lobby closed.");
        }

        /// <summary>
        /// Local IP adresini al
        /// </summary>
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// Lobideki oyuncu bilgileri
    /// </summary>
    public class LobbyPlayer
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public TcpClient Client { get; set; } = null!;
        public bool IsReady { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}