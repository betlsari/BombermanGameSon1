// MVC/Controllers/InputController.cs - TAMAMEN DÜZELTİLMİŞ VERSİYON
using System;
using System.Collections.Generic;
using BombermanGame.src.Patterns.Behavioral.Command;
using BombermanGame.src.Models;

namespace BombermanGame.src.MVC.Controllers
{
    /// <summary>
    /// Input yönetimi ve çoklu oyuncu kontrolü
    /// DÜZELTİLDİ: Her oyuncu kendi tuşlarıyla bomba koyabilir
    /// </summary>
    public class InputController
    {
        private Dictionary<int, PlayerControls> _playerControls;
        private CommandInvoker _commandInvoker;

        public InputController()
        {
            _commandInvoker = new CommandInvoker();
            _playerControls = new Dictionary<int, PlayerControls>();

            // Player 1 kontrolleri (WASD + Space)
            _playerControls[1] = new PlayerControls
            {
                Up = ConsoleKey.W,
                Down = ConsoleKey.S,
                Left = ConsoleKey.A,
                Right = ConsoleKey.D,
                PlaceBomb = ConsoleKey.Spacebar,
                AlternateUp = ConsoleKey.UpArrow,
                AlternateDown = ConsoleKey.DownArrow,
                AlternateLeft = ConsoleKey.LeftArrow,
                AlternateRight = ConsoleKey.RightArrow
            };

            // Player 2 kontrolleri (IJKL + Enter)
            _playerControls[2] = new PlayerControls
            {
                Up = ConsoleKey.I,
                Down = ConsoleKey.K,
                Left = ConsoleKey.J,
                Right = ConsoleKey.L,
                PlaceBomb = ConsoleKey.Enter
            };
        }

        /// <summary>
        /// Klavye girişini işle ve komut oluştur
        /// </summary>
        public ICommand? ProcessInput(ConsoleKeyInfo key, Player player, Map map)
        {
            if (!_playerControls.ContainsKey(player.Id))
                return null;

            var controls = _playerControls[player.Id];

            // Hareket komutları
            if (key.Key == controls.Up || key.Key == controls.AlternateUp)
            {
                return new MoveCommand(player, 0, -1, map);
            }
            else if (key.Key == controls.Down || key.Key == controls.AlternateDown)
            {
                return new MoveCommand(player, 0, 1, map);
            }
            else if (key.Key == controls.Left || key.Key == controls.AlternateLeft)
            {
                return new MoveCommand(player, -1, 0, map);
            }
            else if (key.Key == controls.Right || key.Key == controls.AlternateRight)
            {
                return new MoveCommand(player, 1, 0, map);
            }
            else if (key.Key == controls.PlaceBomb)
            {
                return new PlaceBombCommand(player);
            }

            return null;
        }

        /// <summary>
        /// Çoklu oyuncu için input işle - TAMAMEN DÜZELTİLDİ
        /// Her oyuncu kendi tuşlarına basınca işlem yapılır
        /// DÜZELTME: Her iki oyuncu için de kontrol yapılıyor ve ilk eşleşen işlem yapılıyor
        /// </summary>
        public void ProcessMultiplayerInput(ConsoleKeyInfo key, List<Player> players, Map map)
        {
            // Her oyuncu için ayrı ayrı kontrol et
            foreach (var player in players)
            {
                if (!player.IsAlive) continue;

                // Bu tuş bu oyuncuya ait mi kontrol et
                var command = ProcessInput(key, player, map);
                if (command != null)
                {
                    _commandInvoker.ExecuteCommand(command);
                    // İlk eşleşen oyuncu işlemi yaptı - diğerlerini kontrol etme
                    break;
                }
            }
        }

        /// <summary>
        /// Son komutu geri al
        /// </summary>
        public void UndoLastCommand()
        {
            _commandInvoker.UndoLastCommand();
        }

        /// <summary>
        /// Komut geçmişini temizle
        /// </summary>
        public void ClearHistory()
        {
            _commandInvoker.ClearHistory();
        }

        /// <summary>
        /// Oyuncu kontrol şemasını özelleştir
        /// </summary>
        public void SetPlayerControls(int playerId, PlayerControls controls)
        {
            _playerControls[playerId] = controls;
        }

        /// <summary>
        /// Kontrol şemasını al
        /// </summary>
        public PlayerControls? GetPlayerControls(int playerId)
        {
            return _playerControls.ContainsKey(playerId) ? _playerControls[playerId] : null;
        }
    }

    /// <summary>
    /// Oyuncu kontrol şeması
    /// </summary>
    public class PlayerControls
    {
        public ConsoleKey Up { get; set; }
        public ConsoleKey Down { get; set; }
        public ConsoleKey Left { get; set; }
        public ConsoleKey Right { get; set; }
        public ConsoleKey PlaceBomb { get; set; }

        // Alternatif tuşlar (opsiyonel)
        public ConsoleKey? AlternateUp { get; set; }
        public ConsoleKey? AlternateDown { get; set; }
        public ConsoleKey? AlternateLeft { get; set; }
        public ConsoleKey? AlternateRight { get; set; }

        public string GetControlsString()
        {
            return $"↑:{Up} ↓:{Down} ←:{Left} →:{Right} Bomb:{PlaceBomb}";
        }
    }
}