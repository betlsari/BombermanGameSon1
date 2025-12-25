
// Core/GameManager.cs - SINGLETON PATTERN
using System;
using System.Collections.Generic;
using BombermanGame.src.Models;
using BombermanGame.src.Patterns.Behavioral.Observer;

namespace BombermanGame.src.Core
{//deneme
    public sealed class GameManager : ISubject
    {
        private static GameManager? _instance;
        private static readonly object _lock = new object();
        private List<IObserver> _observers = new List<IObserver>();

        public Map? CurrentMap { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Bomb> Bombs { get; set; } = new List<Bomb>();
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public List<PowerUp> PowerUps { get; set; } = new List<PowerUp>();
        public bool IsGameRunning { get; set; }
        public int CurrentUserId { get; set; }

        // Private constructor
        private GameManager() { }

        // Thread-safe Singleton instance
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GameManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Attach(IObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(GameEvent gameEvent)
        {
            foreach (var observer in _observers)
            {
                observer.Update(gameEvent);
            }
        }

        public void ResetGame()
        {
            Players.Clear();
            Bombs.Clear();
            Enemies.Clear();
            PowerUps.Clear();
            IsGameRunning = false;
        }
    }
}