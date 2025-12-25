// Patterns/Behavioral/Observer/IObserver.cs
namespace BombermanGame.src.Patterns.Behavioral.Observer
{
    public interface IObserver
    {
        void Update(GameEvent gameEvent);
    }
}
