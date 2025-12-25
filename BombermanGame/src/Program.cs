// Program.cs - Ana Giriş Noktası
using System;
using BombermanGame.src.Core;
using BombermanGame.src.Database;

namespace BombermanGame.src
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Bomberman Multiplayer";
            Console.CursorVisible = false;

            // Database initialize
            DatabaseManager.Instance.Initialize();

            // Game Manager başlat
            GameManager gameManager = GameManager.Instance;

            // Ana menü göster
            MainMenu menu = new MainMenu();
            menu.Show(); 
        }
    }
}