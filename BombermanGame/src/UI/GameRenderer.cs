// UI/GameRenderer.cs - Oyun render işlemleri
using System;
using System.Linq;
using BombermanGame.src.Core;
using BombermanGame.src.Models;

namespace BombermanGame.src.UI
{
    public class GameRenderer
    {
        private bool _isFirstRender = true;

        // Ana render metodu
        public void Render(GameManager gameManager)
        {
            if (gameManager.CurrentMap == null) return;

            // İlk render'da tüm ekranı çiz
            if (_isFirstRender)
            {
                Console.Clear();
                _isFirstRender = false;
            }

            Console.SetCursorPosition(0, 0);

            RenderHeader(gameManager);
            RenderMap(gameManager);
            RenderPlayerStats(gameManager);
            RenderEnemyStats(gameManager);
            RenderControls();
        }

        // Başlık render
        private void RenderHeader(GameManager gameManager)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ Theme: {gameManager.CurrentMap!.Theme.GetName(),-15} Players: {gameManager.Players.Count} Enemies: {gameManager.Enemies.Count(e => e.IsAlive),-3}║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        // Harita render
        private void RenderMap(GameManager gameManager)
        {
            for (int y = 0; y < gameManager.CurrentMap!.Height; y++)
            {
                for (int x = 0; x < gameManager.CurrentMap.Width; x++)
                {
                    RenderCell(gameManager, x, y);
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        // Tek bir hücre render
        private void RenderCell(GameManager gm, int x, int y)
        {
            char displayChar = GetDisplayChar(gm, x, y);
            ConsoleColor color = GetDisplayColor(gm, x, y, displayChar);

            Console.ForegroundColor = color;
            Console.Write(displayChar);
        }

        // Görüntülenecek karakteri belirle
        private char GetDisplayChar(GameManager gm, int x, int y)
        {
            // Patlama efekti
            var explosion = gm.Bombs.FirstOrDefault(b => b.HasExploded &&
                IsInExplosionRange(b, x, y));
            if (explosion != null) return '▓';

            // Oyuncu
            var player = gm.Players.FirstOrDefault(p =>
                p.IsAlive && p.Position.X == x && p.Position.Y == y);
            if (player != null) return player.Id == 1 ? '1' : '2';

            // Düşman
            var enemy = gm.Enemies.FirstOrDefault(e =>
                e.IsAlive && e.Position.X == x && e.Position.Y == y);
            if (enemy != null) return enemy.GetSymbol();

            // Bomba
            var bomb = gm.Bombs.FirstOrDefault(b =>
                !b.HasExploded && b.Position.X == x && b.Position.Y == y);
            if (bomb != null)
                return bomb.Timer > 0 ? char.Parse(bomb.Timer.ToString()) : '*';

            // Power-up
            var powerUp = gm.PowerUps.FirstOrDefault(p =>
                !p.IsCollected && p.Position.X == x && p.Position.Y == y);
            if (powerUp != null) return powerUp.GetSymbol();

            // Duvar
            return gm.CurrentMap!.GetTile(x, y).GetSymbol();
        }

        // Renk belirle
        private ConsoleColor GetDisplayColor(GameManager gm, int x, int y, char displayChar)
        {
            // Patlama efekti
            var explosion = gm.Bombs.FirstOrDefault(b => b.HasExploded &&
                IsInExplosionRange(b, x, y));
            if (explosion != null) return ConsoleColor.DarkRed;

            // Oyuncu
            if (displayChar == '1' || displayChar == '2')
                return ConsoleColor.Cyan;

            // Düşman
            if (displayChar == 'E' || displayChar == 'C' || displayChar == 'A')
                return ConsoleColor.Red;

            // Bomba
            if (char.IsDigit(displayChar) || displayChar == '*')
                return displayChar == '1' ? ConsoleColor.Red : ConsoleColor.Yellow;

            // Power-up
            if (displayChar == 'B' || displayChar == 'P' || displayChar == 'S')
                return ConsoleColor.Magenta;

            // Duvar
            if (displayChar == '#')
                return gm.CurrentMap!.Theme.GetUnbreakableWallColor();

            if (displayChar == '▒' || displayChar == '░' || displayChar == '▓')
                return gm.CurrentMap!.Theme.GetBreakableWallColor();

            return ConsoleColor.Gray;
        }

        // Oyuncu istatistikleri
        private void RenderPlayerStats(GameManager gameManager)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Players:");
            Console.ResetColor();

            foreach (var player in gameManager.Players)
            {
                string status = player.IsAlive ? "Alive" : "Dead";
                ConsoleColor statusColor = player.IsAlive ? ConsoleColor.Green : ConsoleColor.DarkRed;

                Console.Write($"  {player.Name}: ");
                Console.ForegroundColor = statusColor;
                Console.Write(status);
                Console.ResetColor();
                Console.WriteLine($" | {player.GetStats()}");
            }
        }

        // Düşman istatistikleri
        private void RenderEnemyStats(GameManager gameManager)
        {
            int aliveEnemies = gameManager.Enemies.Count(e => e.IsAlive);
            int totalEnemies = gameManager.Enemies.Count;

            Console.WriteLine();
            Console.Write("Enemies: ");
            Console.ForegroundColor = aliveEnemies > 0 ? ConsoleColor.Red : ConsoleColor.DarkGray;
            Console.Write($"{aliveEnemies}/{totalEnemies}");
            Console.ResetColor();
            Console.WriteLine();
        }

        // Kontroller
        private void RenderControls()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Controls: WASD/Arrows=Move | Space=Bomb | ESC=Exit");
            Console.ResetColor();
        }

        // Patlama menzilinde mi kontrol et
        private bool IsInExplosionRange(Bomb bomb, int x, int y)
        {
            if (bomb.Position.X == x && bomb.Position.Y == y)
                return true;

            // Yatay
            if (bomb.Position.Y == y &&
                Math.Abs(bomb.Position.X - x) <= bomb.Range)
                return true;

            // Dikey
            if (bomb.Position.X == x &&
                Math.Abs(bomb.Position.Y - y) <= bomb.Range)
                return true;

            return false;
        }

        // Game Over ekranı
        public void RenderGameOver(GameManager gameManager, bool playerWon)
        {
            Console.Clear();
            ConsoleUI.AddSpacing(5);

            if (playerWon)
            {
                ConsoleUI.WriteLineColored("╔════════════════════════════════════════════════════════╗", ConsoleColor.Green);
                ConsoleUI.WriteLineColored("║                                                        ║", ConsoleColor.Green);
                ConsoleUI.WriteLineColored("║                   🏆 VICTORY! 🏆                       ║", ConsoleColor.Yellow);
                ConsoleUI.WriteLineColored("║                                                        ║", ConsoleColor.Green);
                ConsoleUI.WriteLineColored("╚════════════════════════════════════════════════════════╝", ConsoleColor.Green);
            }
            else
            {
                ConsoleUI.WriteLineColored("╔════════════════════════════════════════════════════════╗", ConsoleColor.Red);
                ConsoleUI.WriteLineColored("║                                                        ║", ConsoleColor.Red);
                ConsoleUI.WriteLineColored("║                    GAME OVER                           ║", ConsoleColor.DarkRed);
                ConsoleUI.WriteLineColored("║                                                        ║", ConsoleColor.Red);
                ConsoleUI.WriteLineColored("╚════════════════════════════════════════════════════════╝", ConsoleColor.Red);
            }

            ConsoleUI.AddSpacing(2);

            // Oyuncu skorları
            Console.WriteLine("Final Scores:");
            foreach (var player in gameManager.Players)
            {
                string status = player.IsAlive ? "[ALIVE]" : "[DEAD]";
                ConsoleColor color = player.IsAlive ? ConsoleColor.Green : ConsoleColor.Red;

                Console.Write($"  {player.Name}: ");
                ConsoleUI.WriteColored(status, color);
                Console.WriteLine($" {player.GetStats()}");
            }

            ConsoleUI.AddSpacing();
            ConsoleUI.WaitForKey();
        }

        // Pause ekranı
        public void RenderPauseScreen()
        {
            Console.SetCursorPosition(0, Console.WindowHeight / 2 - 3);
            ConsoleUI.DrawBox(new string[]
            {
                "        GAME PAUSED        ",
                "                           ",
                "  Press ESC to resume      ",
                "  Press Q to quit          "
            });
        }

        // Countdown ekranı (oyun başlamadan önce)
        public void RenderCountdown(int seconds)
        {
            Console.Clear();
            ConsoleUI.AddSpacing(10);

            for (int i = seconds; i > 0; i--)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - 5, Console.WindowHeight / 2);

                ConsoleColor color = i switch
                {
                    3 => ConsoleColor.Green,
                    2 => ConsoleColor.Yellow,
                    1 => ConsoleColor.Red,
                    _ => ConsoleColor.White
                };

                ConsoleUI.WriteColored($"    {i}    ", color);
                Thread.Sleep(1000);
            }

            Console.SetCursorPosition(Console.WindowWidth / 2 - 5, Console.WindowHeight / 2);
            ConsoleUI.WriteColored("   GO!   ", ConsoleColor.Cyan);
            Thread.Sleep(500);
        }

        // Skor tablosu render
        public void RenderScoreBoard(GameManager gameManager, int elapsedTime)
        {
            Console.SetCursorPosition(gameManager.CurrentMap!.Width + 3, 4);
            Console.WriteLine("╔═══════════════════╗");

            Console.SetCursorPosition(gameManager.CurrentMap.Width + 3, 5);
            Console.WriteLine("║   SCORE BOARD     ║");

            Console.SetCursorPosition(gameManager.CurrentMap.Width + 3, 6);
            Console.WriteLine("╠═══════════════════╣");

            int row = 7;
            foreach (var player in gameManager.Players)
            {
                Console.SetCursorPosition(gameManager.CurrentMap.Width + 3, row);
                Console.Write($"║ {player.Name,-8}      ║");
                row++;
            }

            Console.SetCursorPosition(gameManager.CurrentMap.Width + 3, row);
            Console.WriteLine("╠═══════════════════╣");
            row++;

            Console.SetCursorPosition(gameManager.CurrentMap.Width + 3, row);
            Console.WriteLine($"║ Time: {elapsedTime / 60:D2}:{elapsedTime % 60:D2}      ║");
            row++;

            Console.SetCursorPosition(gameManager.CurrentMap.Width + 3, row);
            Console.WriteLine("╚═══════════════════╝");
        }

        // Render'ı sıfırla (yeni oyun için)
        public void ResetRenderer()
        {
            _isFirstRender = true;
        }
    }
}