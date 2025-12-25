// Patterns/Behavioral/Strategy/IMovementStrategy.cs - STRATEGY PATTERN

// Patterns/Behavioral/Strategy/IMovementStrategy.cs - STRATEGY PATTERN
using BombermanGame.src.Models;

namespace BombermanGame.src.Patterns.Behavioral.Strategy
{
    public interface IMovementStrategy
    {
        Position Move(Position currentPosition, Map map, Position? targetPosition);
    }
}