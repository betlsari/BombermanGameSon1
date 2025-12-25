// Patterns/Creational/Factory/IEnemyFactory.cs - FACTORY PATTERN

// Patterns/Creational/Factory/IEnemyFactory.cs - FACTORY PATTERN
using BombermanGame.src.Models;

namespace BombermanGame.src.Patterns.Creational.Factory
{
    public interface IEnemyFactory
    {
        Enemy CreateEnemy(int id, Position position);
    }
}