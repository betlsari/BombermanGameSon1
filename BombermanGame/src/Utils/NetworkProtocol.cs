// Utils/NetworkProtocol.cs - TAMAMEN DÜZELTİLMİŞ VERSİYON
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BombermanGame.src.Core;
using BombermanGame.src.Models;

namespace BombermanGame.src.Utils
{
    // Message türleri
    public enum MessageType
    {
        Connect,
        Disconnect,
        PlayerMove,
        PlaceBomb,
        GameState,
        PlayerJoin,
        PlayerLeave,
        GameStart,
        GameEnd,
        Ping,
        Pong
    }

    // Network mesajı base class
    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string SenderId { get; set; } = "";
        public long Timestamp { get; set; }
        public string Data { get; set; } = "";

        public NetworkMessage()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    // Oyuncu hareket mesajı
    public class PlayerMoveMessage
    {
        public string PlayerId { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
    }

    // Bomba yerleştirme mesajı
    public class PlaceBombMessage
    {
        public string PlayerId { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int Range { get; set; }
        public int Timer { get; set; }
    }

    // Oyun durumu mesajı
    public class GameStateMessage
    {
        public List<PlayerState> Players { get; set; } = new List<PlayerState>();
        public List<BombState> Bombs { get; set; } = new List<BombState>();
        public List<EnemyState> Enemies { get; set; } = new List<EnemyState>();
        public List<PowerUpState> PowerUps { get; set; } = new List<PowerUpState>();
        public int GameTick { get; set; }
    }

    // Oyuncu durumu
    public class PlayerState
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsAlive { get; set; }
        public int Health { get; set; }
        public int Score { get; set; }
        public int BombCount { get; set; }
        public int BombRange { get; set; }
        public int Speed { get; set; }
    }

    // Bomba durumu
    public class BombState
    {
        public string Id { get; set; } = "";
        public string OwnerId { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int Timer { get; set; }
        public int Range { get; set; }
        public bool HasExploded { get; set; }
    }

    // Düşman durumu
    public class EnemyState
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsAlive { get; set; }
        public int Health { get; set; }
    }

    // Power-up durumu
    public class PowerUpState
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsCollected { get; set; }
    }

    // Network protokol yöneticisi
    public static class NetworkProtocol
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        // Mesaj serialize
        public static string SerializeMessage(NetworkMessage message)
        {
            try
            {
                return JsonSerializer.Serialize(message, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PROTOCOL] Serialization error: {ex.Message}");
                return "";
            }
        }

        // Mesaj deserialize
        public static NetworkMessage? DeserializeMessage(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<NetworkMessage>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PROTOCOL] Deserialization error: {ex.Message}");
                return null;
            }
        }

        // Oyuncu hareket mesajı oluştur
        public static NetworkMessage CreateMoveMessage(string playerId, Position position)
        {
            var moveData = new PlayerMoveMessage
            {
                PlayerId = playerId,
                X = position.X,
                Y = position.Y
            };

            return new NetworkMessage
            {
                Type = MessageType.PlayerMove,
                SenderId = playerId,
                Data = JsonSerializer.Serialize(moveData, _jsonOptions)
            };
        }

        // Bomba yerleştirme mesajı oluştur
        public static NetworkMessage CreatePlaceBombMessage(string playerId, Bomb bomb)
        {
            var bombData = new PlaceBombMessage
            {
                PlayerId = playerId,
                X = bomb.Position.X,
                Y = bomb.Position.Y,
                Range = bomb.Power,
                Timer = bomb.Timer
            };

            return new NetworkMessage
            {
                Type = MessageType.PlaceBomb,
                SenderId = playerId,
                Data = JsonSerializer.Serialize(bombData, _jsonOptions)
            };
        }

        // Oyun durumu mesajı oluştur
        public static NetworkMessage CreateGameStateMessage(GameManager gameManager)
        {
            var gameState = new GameStateMessage
            {
                Players = ConvertPlayers(gameManager.Players),
                Bombs = ConvertBombs(gameManager.Bombs),
                Enemies = ConvertEnemies(gameManager.Enemies),
                PowerUps = ConvertPowerUps(gameManager.PowerUps),
                GameTick = Environment.TickCount
            };

            return new NetworkMessage
            {
                Type = MessageType.GameState,
                SenderId = "server",
                Data = JsonSerializer.Serialize(gameState, _jsonOptions)
            };
        }

        // Bağlantı mesajı oluştur
        public static NetworkMessage CreateConnectMessage(string playerId, string playerName)
        {
            var connectData = new Dictionary<string, string>
            {
                { "playerId", playerId },
                { "playerName", playerName }
            };

            return new NetworkMessage
            {
                Type = MessageType.Connect,
                SenderId = playerId,
                Data = JsonSerializer.Serialize(connectData, _jsonOptions)
            };
        }

        // Bağlantı kesme mesajı oluştur
        public static NetworkMessage CreateDisconnectMessage(string playerId)
        {
            return new NetworkMessage
            {
                Type = MessageType.Disconnect,
                SenderId = playerId,
                Data = ""
            };
        }

        // Ping mesajı oluştur
        public static NetworkMessage CreatePingMessage(string playerId)
        {
            return new NetworkMessage
            {
                Type = MessageType.Ping,
                SenderId = playerId,
                Data = ""
            };
        }

        // Pong mesajı oluştur
        public static NetworkMessage CreatePongMessage(string playerId)
        {
            return new NetworkMessage
            {
                Type = MessageType.Pong,
                SenderId = playerId,
                Data = ""
            };
        }

        // Oyun başlama mesajı oluştur - DÜZELTİLDİ
        public static NetworkMessage CreateGameStartMessage(string theme, int mapSeed)
        {
            var startData = new Dictionary<string, object>
            {
                { "theme", theme },
                { "mapSeed", mapSeed }
            };

            return new NetworkMessage
            {
                Type = MessageType.GameStart,
                SenderId = "server",
                Data = JsonSerializer.Serialize(startData, _jsonOptions)
            };
        }

        // Map seed parse et - YENİ EKLEME
        public static int ParseMapSeed(NetworkMessage message)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message.Data, _jsonOptions);
                if (data != null && data.ContainsKey("mapSeed"))
                {
                    return data["mapSeed"].GetInt32();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PROTOCOL] ParseMapSeed error: {ex.Message}");
            }
            return 12345; // Default fallback
        }

        // Tema parse et - YENİ EKLEME
        public static string ParseTheme(NetworkMessage message)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message.Data, _jsonOptions);
                if (data != null && data.ContainsKey("theme"))
                {
                    return data["theme"].GetString() ?? "Desert";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PROTOCOL] ParseTheme error: {ex.Message}");
            }
            return "Desert"; // Default fallback
        }

        // Oyun bitiş mesajı oluştur
        public static NetworkMessage CreateGameEndMessage(string winnerId, int finalScore)
        {
            var endData = new Dictionary<string, object>
            {
                { "winnerId", winnerId },
                { "finalScore", finalScore }
            };

            return new NetworkMessage
            {
                Type = MessageType.GameEnd,
                SenderId = "server",
                Data = JsonSerializer.Serialize(endData, _jsonOptions)
            };
        }

        // Helper metodlar - Dönüşümler
        private static List<PlayerState> ConvertPlayers(List<Player> players)
        {
            var result = new List<PlayerState>();
            foreach (var player in players)
            {
                result.Add(new PlayerState
                {
                    Id = player.Id.ToString(),
                    Name = player.Name,
                    X = player.Position.X,
                    Y = player.Position.Y,
                    IsAlive = player.IsAlive,
                    Health = player.Health,
                    Score = player.Score,
                    BombCount = player.BombCount,
                    BombRange = player.BombPower,
                    Speed = player.Speed
                });
            }
            return result;
        }

        private static List<BombState> ConvertBombs(List<Bomb> bombs)
        {
            var result = new List<BombState>();
            foreach (var bomb in bombs)
            {
                result.Add(new BombState
                {
                    Id = Guid.NewGuid().ToString(),
                    OwnerId = bomb.OwnerId.ToString(),
                    X = bomb.Position.X,
                    Y = bomb.Position.Y,
                    Timer = bomb.Timer,
                    Range = bomb.Power,
                    HasExploded = bomb.HasExploded
                });
            }
            return result;
        }

        private static List<EnemyState> ConvertEnemies(List<Enemy> enemies)
        {
            var result = new List<EnemyState>();
            foreach (var enemy in enemies)
            {
                result.Add(new EnemyState
                {
                    Id = enemy.Id.ToString(),
                    Type = enemy.Type.ToString() + "Enemy",
                    X = enemy.Position.X,
                    Y = enemy.Position.Y,
                    IsAlive = enemy.IsAlive,
                    Health = enemy.Health
                });
            }
            return result;
        }

        private static List<PowerUpState> ConvertPowerUps(List<PowerUp> powerUps)
        {
            var result = new List<PowerUpState>();
            foreach (var powerUp in powerUps)
            {
                result.Add(new PowerUpState
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = powerUp.Type.ToString(),
                    X = powerUp.Position.X,
                    Y = powerUp.Position.Y,
                    IsCollected = powerUp.IsCollected
                });
            }
            return result;
        }

        // Mesaj validasyonu
        public static bool ValidateMessage(NetworkMessage message)
        {
            if (message == null) return false;
            if (string.IsNullOrEmpty(message.SenderId)) return false;
            if (message.Timestamp <= 0) return false;

            // Eski mesajları reddet (5 saniyeden eski)
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (currentTime - message.Timestamp > 5000) return false;

            return true;
        }

        // Mesaj sıkıştırma (opsiyonel, büyük veri için)
        public static byte[] CompressMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var gzipStream = new System.IO.Compression.GZipStream(
                    memoryStream, System.IO.Compression.CompressionMode.Compress, true))
                {
                    gzipStream.Write(buffer, 0, buffer.Length);
                }
                return memoryStream.ToArray();
            }
        }

        // Mesaj açma
        public static string DecompressMessage(byte[] compressedData)
        {
            using (var memoryStream = new System.IO.MemoryStream(compressedData))
            {
                using (var gzipStream = new System.IO.Compression.GZipStream(
                    memoryStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    using (var reader = new System.IO.StreamReader(gzipStream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}