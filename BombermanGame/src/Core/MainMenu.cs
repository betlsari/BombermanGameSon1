// Core/MainMenu.cs - DÜZELTİLMİŞ MULTIPLAYER BÖLÜMÜ
using System;
using System.Linq;
using System.Threading;
using BombermanGame.src.MVC.Controllers;
using BombermanGame.src.UI;
using BombermanGame.src.Models.Entities;
using BombermanGame.src.Patterns.Repository;
using BombermanGame.src.Utils;

namespace BombermanGame.src.Core
{
    public class MainMenu
    {
        private UserRepository _userRepository;
        private PreferencesRepository _preferencesRepository;
        private ScoreRepository _scoreRepository;
        private StatsRepository _statsRepository;

        public MainMenu()
        {
            _userRepository = new UserRepository();
            _preferencesRepository = new PreferencesRepository();
            _scoreRepository = new ScoreRepository();
            _statsRepository = new StatsRepository();
        }

        public void Show()
        {
            while (true)
            {
                Console.Clear();
                DisplayLogo();

                Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                         MAIN MENU                            ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine("\n1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. View Leaderboard");
                Console.WriteLine("4. Exit");
                Console.Write("\nSelect option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        LoginMenu();
                        break;
                    case "2":
                        RegisterMenu();
                        break;
                    case "3":
                        ShowLeaderboard();
                        break;
                    case "4":
                        Console.WriteLine("\nThank you for playing!");
                        return;
                    default:
                        Console.WriteLine("\nInvalid option!");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void DisplayLogo()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
    ╔══════════════════════════════════════════════════════════════╗
    ║  ____                  _                                     ║
    ║ |  _ \                | |                                    ║
    ║ | |_) | ___  _ __ ___ | |__   ___ _ __ _ __ ___   __ _ _ __  ║
    ║ |  _ < / _ \| '_ ` _ \| '_ \ / _ \ '__| '_ ` _ \ / _` | '_ \ ║
    ║ | |_) | (_) | | | | | | |_) |  __/ |  | | | | | | (_| | | | |║
    ║ |____/ \___/|_| |_| |_|_.__/ \___|_|  |_| |_| |_|\__,_|_| |_|║
    ║                                                                ║
    ╚══════════════════════════════════════════════════════════════╝
            ");
            Console.ResetColor();
        }

        private void LoginMenu()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                           LOGIN                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            Console.Write("Username: ");
            string? username = Console.ReadLine();

            Console.Write("Password: ");
            string? password = ReadPassword();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("\nUsername and password cannot be empty!");
                Console.ReadKey();
                return;
            }

            var user = _userRepository.GetByUsername(username);
            if (user != null && PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                Console.WriteLine("\n✓ Login successful!");
                GameManager.Instance.CurrentUserId = user.Id;
                Thread.Sleep(1000);
                GameMenu(user);
            }
            else
            {
                Console.WriteLine("\n✗ Invalid username or password!");
                Console.ReadKey();
            }
        }

        private void RegisterMenu()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        REGISTER                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            Console.Write("Username: ");
            string? username = Console.ReadLine();

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("\nUsername cannot be empty!");
                Console.ReadKey();
                return;
            }

            if (_userRepository.UsernameExists(username))
            {
                Console.WriteLine("\n✗ Username already exists!");
                Console.ReadKey();
                return;
            }

            Console.Write("Password: ");
            string? password = ReadPassword();

            Console.Write("\nConfirm Password: ");
            string? confirmPassword = ReadPassword();

            if (password != confirmPassword)
            {
                Console.WriteLine("\n\n✗ Passwords do not match!");
                Console.ReadKey();
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                Console.WriteLine("\n\n✗ Password must be at least 6 characters!");
                Console.ReadKey();
                return;
            }

            var user = new User
            {
                Username = username,
                PasswordHash = PasswordHelper.HashPassword(password),
                CreatedAt = DateTime.Now
            };

            _userRepository.Add(user);

            _preferencesRepository.Add(new PlayerPreference
            {
                UserId = user.Id,
                Theme = "Desert",
                SoundEnabled = true
            });

            _statsRepository.Add(new GameStatistic
            {
                UserId = user.Id,
                Wins = 0,
                Losses = 0,
                TotalGames = 0
            });

            Console.WriteLine("\n\n✓ Registration successful!");
            Console.ReadKey();
        }

        private void GameMenu(User user)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║ Welcome, {user.Username,-50} ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

                var stats = _statsRepository.GetByUserId(user.Id);
                if (stats != null)
                {
                    Console.WriteLine($"Games Played: {stats.TotalGames} | Wins: {stats.Wins} | Losses: {stats.Losses}");
                    if (stats.TotalGames > 0)
                    {
                        double winRate = (double)stats.Wins / stats.TotalGames * 100;
                        Console.WriteLine($"Win Rate: {winRate:F1}%\n");
                    }
                }

                Console.WriteLine("1. Start Single Player Game");
                Console.WriteLine("2. Start Two Player Game (Local)");
                Console.WriteLine("3. Multiplayer (Online)");
                Console.WriteLine("4. View My Scores");
                Console.WriteLine("5. Settings");
                Console.WriteLine("6. Logout");
                Console.Write("\nSelect option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        StartGame(user, true);
                        break;
                    case "2":
                        StartGame(user, false);
                        break;
                    case "3":
                        StartMultiplayerMenu(user);
                        break;
                    case "4":
                        ShowMyScores(user);
                        break;
                    case "5":
                        SettingsMenu(user);
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("\nInvalid option!");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void StartMultiplayerMenu(User user)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              MULTIPLAYER MODE (ONLINE)                       ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

                Console.WriteLine("⚠️  ÖNEMLİ NOTLAR:");
                Console.WriteLine("   • Her iki oyuncu da aynı ağda olmalı");
                Console.WriteLine("   • Windows Firewall izin vermelidir");
                Console.WriteLine("   • Port 9999 açık olmalıdır\n");

                Console.WriteLine("1. Host Game (Sunucu Oluştur)");
                Console.WriteLine("2. Join Game (Sunucuya Bağlan)");
                Console.WriteLine("3. Network Test (Bağlantı Testi)");
                Console.WriteLine("4. Back");
                Console.Write("\nSelect option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        HostMultiplayerGame(user);
                        break;
                    case "2":
                        JoinMultiplayerGame(user);
                        break;
                    case "3":
                        NetworkTest();
                        break;
                    case "4":
                        return;
                }
            }
        }

        private void NetworkTest()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    NETWORK TEST                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            try
            {
                // IP adreslerini göster
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                Console.WriteLine("📡 Local IP Addresses:");
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"   • {ip}");
                    }
                }

                Console.WriteLine("\n🔧 Firewall Kontrolü:");
                Console.WriteLine("   Windows Defender Firewall'da bu uygulamaya izin verilmeli.");
                Console.WriteLine("   Ayarlar → Güvenlik → Windows Güvenliği → Güvenlik Duvarı");

                Console.WriteLine("\n🌐 Port Kontrolü:");
                Console.WriteLine("   Port 9999 açık olmalı");
                Console.WriteLine("   Test için: telnet <ip> 9999");

                Console.WriteLine("\n💡 Sorun Giderme İpuçları:");
                Console.WriteLine("   1. Her iki bilgisayar da aynı ağa bağlı mı?");
                Console.WriteLine("   2. Firewall kapalı mı veya izin veriliyor mu?");
                Console.WriteLine("   3. Antivirus engelliyor mu?");
                Console.WriteLine("   4. VPN aktif mi? (Kapatın)");
                Console.WriteLine("   5. Host önce sunucu başlatmalı");
                Console.WriteLine("   6. Client doğru IP adresini girmeli");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Test hatası: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private void HostMultiplayerGame(User user)
        {
            var preferences = _preferencesRepository.GetByUserId(user.Id);
            string theme = preferences?.Theme ?? "Desert";

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      HOST SETUP                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("Port numarası (varsayılan 9999):");
            Console.Write("Port (Enter for default): ");
            string? portInput = Console.ReadLine();

            int port = 9999;
            if (!string.IsNullOrEmpty(portInput) && int.TryParse(portInput, out int customPort))
            {
                port = customPort;
            }

            Console.WriteLine($"\n✅ Port: {port}");
            Console.WriteLine("📡 Arkadaşınıza IP adresinizi verin");
            Console.WriteLine("\n🔄 Sunucu başlatılıyor...\n");

            Thread.Sleep(1000);

            var multiplayerController = new MultiplayerGameController();
            multiplayerController.StartAsHost(theme, port).Wait();
        }

        private void JoinMultiplayerGame(User user)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      JOIN SETUP                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("Host IP adresini girin:");
            Console.WriteLine("(Örnek: 192.168.1.100)\n");
            Console.Write("IP Address: ");
            string? hostIP = Console.ReadLine();

            if (string.IsNullOrEmpty(hostIP))
            {
                Console.WriteLine("❌ IP adresi boş olamaz!");
                Thread.Sleep(2000);
                return;
            }

            Console.Write("\nPort (varsayılan 9999, Enter for default): ");
            string? portInput = Console.ReadLine();

            int port = 9999;
            if (!string.IsNullOrEmpty(portInput) && int.TryParse(portInput, out int customPort))
            {
                port = customPort;
            }

            var preferences = _preferencesRepository.GetByUserId(user.Id);
            string theme = preferences?.Theme ?? "Desert";

            Console.WriteLine($"\n✅ Hedef: {hostIP}:{port}");
            Console.WriteLine("🔄 Bağlanılıyor...\n");

            Thread.Sleep(1000);

            var multiplayerController = new MultiplayerGameController();
            multiplayerController.ConnectToHost(theme, hostIP, port).Wait();
        }

        private void StartGame(User user, bool singlePlayer)
        {
            var preferences = _preferencesRepository.GetByUserId(user.Id);
            string theme = preferences?.Theme ?? "Desert";

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      SELECT THEME                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine("1. Desert Theme");
            Console.WriteLine("2. Forest Theme");
            Console.WriteLine("3. City Theme");
            Console.WriteLine($"4. Use Default ({theme})");
            Console.Write("\nSelect theme: ");

            string? themeChoice = Console.ReadLine();
            theme = themeChoice switch
            {
                "1" => "Desert",
                "2" => "Forest",
                "3" => "City",
                _ => theme
            };

            Console.WriteLine("\nStarting game...");
            Thread.Sleep(1000);

            var gameController = new GameController();
            gameController.StartGame(theme, singlePlayer);
        }

        private void ShowMyScores(User user)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       MY HIGH SCORES                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            var scores = _scoreRepository.GetUserScores(user.Id);
            int rank = 1;

            if (!scores.Any())
            {
                Console.WriteLine("No scores yet. Play a game to set your first score!");
            }
            else
            {
                foreach (var score in scores.Take(10))
                {
                    Console.WriteLine($"{rank}. Score: {score.Score} | Date: {score.GameDate:yyyy-MM-dd HH:mm}");
                    rank++;
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private void SettingsMenu(User user)
        {
            var preferences = _preferencesRepository.GetByUserId(user.Id);
            if (preferences == null)
            {
                preferences = new PlayerPreference
                {
                    UserId = user.Id,
                    Theme = "Desert",
                    SoundEnabled = true
                };
                _preferencesRepository.Add(preferences);
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                         SETTINGS                             ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
                Console.WriteLine($"Current Theme: {preferences.Theme}");
                Console.WriteLine($"Sound: {(preferences.SoundEnabled ? "Enabled" : "Disabled")}\n");
                Console.WriteLine("1. Change Theme");
                Console.WriteLine("2. Toggle Sound");
                Console.WriteLine("3. Back");
                Console.Write("\nSelect option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\n1. Desert\n2. Forest\n3. City");
                        Console.Write("Select theme: ");
                        string? themeChoice = Console.ReadLine();
                        preferences.Theme = themeChoice switch
                        {
                            "1" => "Desert",
                            "2" => "Forest",
                            "3" => "City",
                            _ => preferences.Theme
                        };
                        _preferencesRepository.Update(preferences);
                        Console.WriteLine("✓ Theme updated!");
                        Thread.Sleep(1000);
                        break;
                    case "2":
                        preferences.SoundEnabled = !preferences.SoundEnabled;
                        _preferencesRepository.Update(preferences);
                        Console.WriteLine($"✓ Sound {(preferences.SoundEnabled ? "enabled" : "disabled")}!");
                        Thread.Sleep(1000);
                        break;
                    case "3":
                        return;
                }
            }
        }

        private void ShowLeaderboard()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    GLOBAL LEADERBOARD                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            var topScores = _scoreRepository.GetTopScores(10);
            int rank = 1;

            if (!topScores.Any())
            {
                Console.WriteLine("No scores yet. Be the first to play!");
            }
            else
            {
                Console.WriteLine("Rank | Player            | Score    | Date");
                Console.WriteLine("─────┼───────────────────┼──────────┼────────────────");
                foreach (var score in topScores)
                {
                    Console.WriteLine($"{rank,4} | {score.Username,-17} | {score.Score,8} | {score.GameDate:yyyy-MM-dd}");
                    rank++;
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            return password;
        }
    }
}