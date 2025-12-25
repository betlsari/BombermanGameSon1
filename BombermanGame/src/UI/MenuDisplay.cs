// UI/MenuDisplay.cs - Menü gösterim sınıfı
using System;
using System.Collections.Generic;

namespace BombermanGame.src.UI
{
    public class MenuDisplay
    {
        // Ana menü logosu
        public void DisplayLogo()
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

        // Menü seçeneklerini göster
        public void DisplayMenu(string title, List<string> options, int selectedIndex = -1)
        {
            ConsoleUI.DrawTitle(title);
            Console.WriteLine();

            for (int i = 0; i < options.Count; i++)
            {
                if (selectedIndex == i)
                {
                    ConsoleUI.WriteColored($"► {i + 1}. {options[i]}\n", ConsoleColor.Yellow);
                }
                else
                {
                    Console.WriteLine($"  {i + 1}. {options[i]}");
                }
            }
        }

        // Kullanıcı input al (menü seçimi)
        public int GetMenuChoice(int maxOption)
        {
            Console.Write("\nSelect option: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= maxOption)
            {
                return choice;
            }

            return -1; // Geçersiz seçim
        }

        // Ok tuşları ile menü navigasyonu
        public int NavigateMenu(List<string> options, int currentIndex = 0)
        {
            ConsoleKeyInfo key;
            int selectedIndex = currentIndex;

            do
            {
                Console.Clear();
                DisplayMenu("MENU", options, selectedIndex);

                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        selectedIndex = (selectedIndex - 1 + options.Count) % options.Count;
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        selectedIndex = (selectedIndex + 1) % options.Count;
                        break;

                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                        return selectedIndex;

                    case ConsoleKey.Escape:
                        return -1;
                }

            } while (true);
        }

        // İstatistik tablosu göster
        public void DisplayStats(Dictionary<string, string> stats)
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      STATISTICS                            ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");

            foreach (var stat in stats)
            {
                Console.WriteLine($"║ {stat.Key,-25} : {stat.Value,-28} ║");
            }

            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        }

        // Leaderboard göster
        public void DisplayLeaderboard(List<(string Username, int Score, DateTime Date)> scores)
        {
            Console.Clear();
            ConsoleUI.DrawTitle("GLOBAL LEADERBOARD");
            Console.WriteLine();

            if (scores.Count == 0)
            {
                ConsoleUI.ShowInfo("No scores yet. Be the first to play!");
                return;
            }

            Console.WriteLine("Rank | Player            | Score    | Date");
            Console.WriteLine("─────┼───────────────────┼──────────┼────────────────");

            for (int i = 0; i < scores.Count; i++)
            {
                var (username, score, date) = scores[i];

                ConsoleColor color = i switch
                {
                    0 => ConsoleColor.Yellow,   // Gold
                    1 => ConsoleColor.Gray,     // Silver
                    2 => ConsoleColor.DarkYellow, // Bronze
                    _ => ConsoleColor.White
                };

                Console.ForegroundColor = color;
                Console.WriteLine($"{i + 1,4} | {username,-17} | {score,8} | {date:yyyy-MM-dd}");
                Console.ResetColor();
            }
        }

        // Form input al
        public string GetInput(string prompt, bool isPassword = false)
        {
            Console.Write($"{prompt}: ");

            if (isPassword)
            {
                return ConsoleUI.ReadPassword();
            }

            return Console.ReadLine() ?? "";
        }

        // Tema seçim menüsü
        public string SelectTheme(string currentTheme)
        {
            Console.Clear();
            ConsoleUI.DrawTitle("SELECT THEME");
            Console.WriteLine();

            var themes = new List<string>
            {
                "Desert Theme",
                "Forest Theme",
                "City Theme",
                $"Use Current ({currentTheme})"
            };

            DisplayMenu("THEMES", themes);
            int choice = GetMenuChoice(themes.Count);

            return choice switch
            {
                1 => "Desert",
                2 => "Forest",
                3 => "City",
                _ => currentTheme
            };
        }

        // Oyun modu seçimi
        public bool SelectGameMode()
        {
            Console.Clear();
            ConsoleUI.DrawTitle("SELECT GAME MODE");
            Console.WriteLine();

            var modes = new List<string>
            {
                "Single Player (vs AI)",
                "Two Players (Local Multiplayer)"
            };

            DisplayMenu("GAME MODE", modes);
            int choice = GetMenuChoice(modes.Count);

            return choice == 1; // true = single player
        }

        // Ayarlar menüsü
        public void DisplaySettings(string theme, bool soundEnabled)
        {
            Console.Clear();
            ConsoleUI.DrawTitle("SETTINGS");
            Console.WriteLine();

            var settings = new Dictionary<string, string>
            {
                { "Current Theme", theme },
                { "Sound Effects", soundEnabled ? "Enabled" : "Disabled" },
                { "Controls", "WASD / Arrow Keys" }
            };

            foreach (var setting in settings)
            {
                Console.WriteLine($"{setting.Key,-20} : {setting.Value}");
            }

            Console.WriteLine();
            Console.WriteLine("1. Change Theme");
            Console.WriteLine("2. Toggle Sound");
            Console.WriteLine("3. Back to Menu");
        }

        // Hoşgeldin mesajı
        public void DisplayWelcome(string username, int wins, int losses, int totalGames)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ Welcome back, {username,-46} ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (totalGames > 0)
            {
                double winRate = (double)wins / totalGames * 100;
                Console.WriteLine($"Games Played: {totalGames}");
                Console.WriteLine($"Record: {wins}W - {losses}L");
                Console.WriteLine($"Win Rate: {winRate:F1}%");
                Console.WriteLine();
            }
            else
            {
                ConsoleUI.ShowInfo("No games played yet. Start your first game!");
                Console.WriteLine();
            }
        }

        // Yükleniyor animasyonu
        public void ShowLoadingScreen(string message)
        {
            Console.Clear();
            ConsoleUI.AddSpacing(10);

            string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

            for (int i = 0; i < 20; i++)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - message.Length / 2, Console.WindowHeight / 2);
                Console.Write($"{frames[i % frames.Length]} {message}");
                Thread.Sleep(100);
            }
        }

        // Başarı mesajı animasyonu
        public void ShowSuccessAnimation(string message)
        {
            Console.Clear();
            ConsoleUI.AddSpacing(10);

            for (int i = 0; i < 3; i++)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - message.Length / 2, Console.WindowHeight / 2);
                ConsoleUI.WriteColored($"✓ {message}", ConsoleColor.Green);
                Thread.Sleep(200);

                Console.SetCursorPosition(Console.WindowWidth / 2 - message.Length / 2, Console.WindowHeight / 2);
                Console.Write(new string(' ', message.Length + 2));
                Thread.Sleep(200);
            }

            Console.SetCursorPosition(Console.WindowWidth / 2 - message.Length / 2, Console.WindowHeight / 2);
            ConsoleUI.WriteLineColored($"✓ {message}", ConsoleColor.Green);
            Thread.Sleep(1000);
        }

        // Hata mesajı popup
        public void ShowErrorPopup(string message)
        {
            Console.WriteLine();
            ConsoleUI.DrawBox(new string[]
            {
                "        ERROR!        ",
                "                      ",
                $" {message,-20} "
            });
            Thread.Sleep(2000);
        }

        // Onay dialogu
        public bool ShowConfirmDialog(string message)
        {
            Console.WriteLine();
            ConsoleUI.DrawBox(new string[]
            {
                "     CONFIRMATION     ",
                "                      ",
                $" {message,-20} ",
                "                      ",
                "  Y = Yes  |  N = No  "
            });

            var key = Console.ReadKey(true);
            return key.Key == ConsoleKey.Y;
        }

        // Oyun bitişi özeti
        public void DisplayGameSummary(int score, int enemiesKilled, int duration)
        {
            Console.Clear();
            ConsoleUI.AddSpacing(5);

            ConsoleUI.DrawTitle("GAME SUMMARY");
            Console.WriteLine();

            var summary = new Dictionary<string, string>
            {
                { "Final Score", score.ToString() },
                { "Enemies Defeated", enemiesKilled.ToString() },
                { "Game Duration", $"{duration}s" },
                { "Performance", GetPerformanceRating(score) }
            };

            DisplayStats(summary);
            ConsoleUI.AddSpacing();
            ConsoleUI.WaitForKey();
        }

        // Performans değerlendirmesi
        private string GetPerformanceRating(int score)
        {
            if (score >= 1000) return "★★★★★ Legendary!";
            if (score >= 750) return "★★★★☆ Excellent!";
            if (score >= 500) return "★★★☆☆ Good";
            if (score >= 250) return "★★☆☆☆ Average";
            return "★☆☆☆☆ Keep trying!";
        }
    }
}