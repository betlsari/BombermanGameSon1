# 📐 Bomberman Multiplayer - Design Document (Tasarım Dokümanı)

**Proje Adı**: Bomberman Multiplayer  
**Ders**: Design Patterns - 2025  
**Öğretim Görevlisi**: Prof. Dr. Doğan Aydın  
**Tarih**: 18 Aralık 2025  
**Versiyon**: 1.0 Final

---

## İçindekiler

1. [Giriş](#1-giriş)
2. [Mimari Genel Bakış](#2-mimari-genel-bakış)
3. [Tasarım Kalıpları Detaylı Açıklama](#3-tasarım-kalıpları-detaylı-açıklama)
4. [UML Diyagramları](#4-uml-diyagramları)
5. [Sequence Diyagramlar](#5-sequence-diyagramlar)
6. [Veritabanı Tasarımı](#6-veritabanı-tasarımı)
7. [Network Protokolü](#7-network-protokolü)
8. [Kod Kalitesi ve Standartlar](#8-kod-kalitesi-ve-standartlar)
9. [Test Stratejisi](#9-test-stratejisi)
10. [Sonuç ve Değerlendirme](#10-sonuç-ve-değerlendirme)

---

## 1. Giriş

### 1.1 Proje Amacı

Bu proje, tasarım kalıplarının gerçek bir yazılım projesinde nasıl uygulandığını göstermek için geliştirilmiş **konsol tabanlı bir Bomberman oyunudur**. Proje, **10 farklı tasarım kalıbı** kullanarak yazılım geliştirmede best practice'leri uygulamayı hedeflemektedir.

### 1.2 Tasarım Prensipleri

Proje geliştirilirken aşağıdaki prensiplere uyulmuştur:

#### SOLID Prensipleri:
- ✅ **S**ingle Responsibility: Her sınıf tek bir sorumluluğa sahip
- ✅ **O**pen/Closed: Genişletmeye açık, değişikliğe kapalı
- ✅ **L**iskov Substitution: Alt sınıflar üst sınıfların yerini alabilir
- ✅ **I**nterface Segregation: Küçük ve özel interface'ler
- ✅ **D**ependency Inversion: Soyutlamalara bağımlılık

#### DRY (Don't Repeat Yourself):
- Kod tekrarı minimize edilmiştir
- Ortak fonksiyonlar utility sınıflarında toplanmıştır

#### KISS (Keep It Simple, Stupid):
- Karmaşık çözümler yerine basit ve anlaşılır kod
- Over-engineering'den kaçınılmıştır

### 1.3 Kullanılan Teknolojiler

| Teknoloji | Versiyon | Amaç |
|-----------|----------|------|
| C# | .NET 7.0 | Ana programlama dili |
| SQLite | 3.x | Veritabanı |
| Dapper | 2.1.66 | ORM (Object-Relational Mapping) |
| BCrypt.Net | 4.0.3 | Şifre hash'leme |
| TCP Sockets | Native | Network multiplayer |

---

## 2. Mimari Genel Bakış

### 2.1 Katmanlı Mimari

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (UI/, MVC/Views/)                      │
│  - GameRenderer                         │
│  - MenuDisplay                          │
│  - ConsoleUI                            │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Application Layer               │
│  (MVC/Controllers/)                     │
│  - GameController                       │
│  - InputController                      │
│  - MultiplayerGameController            │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Business Logic Layer            │
│  (Core/, Models/)                       │
│  - GameManager (Singleton)              │
│  - Player, Bomb, Enemy, Map             │
│  - Game Rules & Logic                   │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Data Access Layer               │
│  (Patterns/Repository/)                 │
│  - UserRepository                       │
│  - StatsRepository                      │
│  - ScoreRepository                      │
│  - PreferencesRepository                │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Database Layer                  │
│  (Database/)                            │
│  - DatabaseManager (Singleton)          │
│  - SQLite Database                      │
└─────────────────────────────────────────┘
```

---

## 3. Tasarım Kalıpları Detaylı Açıklama

### 3.1 Factory Method Pattern (Creational) ⭐

#### 3.1.1 Problem Statement

Oyunda farklı davranışlara sahip düşman türleri oluşturmak gerekiyor:
- **Static Enemy**: Hareketsiz düşman
- **Chase Enemy**: Oyuncuyu takip eden düşman
- **Smart Enemy**: A* algoritması ile akıllı düşman

Her düşman tipinin kendine özel hareket stratejisi var ve yeni düşman tipleri eklenebilmeli.

#### 3.1.2 Solution Approach

Factory Method Pattern kullanarak her düşman tipi için ayrı factory sınıfları oluşturuldu. Factory Provider sınıfı ile istenilen factory'ye kolayca erişilebiliyor.

#### 3.1.3 Implementation Details

**Interface:**
```csharp
public interface IEnemyFactory
{
    Enemy CreateEnemy(int id, Position position);
}
```

**Concrete Factories:**
```csharp
public class StaticEnemyFactory : IEnemyFactory
{
    public Enemy CreateEnemy(int id, Position position)
    {
        var enemy = new Enemy(id, position, EnemyType.Static);
        enemy.MovementStrategy = new StaticMovementStrategy();
        return enemy;
    }
}

public class SmartEnemyFactory : IEnemyFactory
{
    public Enemy CreateEnemy(int id, Position position)
    {
        var enemy = new Enemy(id, position, EnemyType.Smart);
        enemy.MovementStrategy = new PathfindingMovementStrategy();
        return enemy;
    }
}
```

#### 3.1.4 Advantages

✅ **Open/Closed Principle**: Yeni düşman tipleri eklemek için mevcut kodu değiştirmeye gerek yok  
✅ **Encapsulation**: Düşman yaratma mantığı factory'lerde izole edilmiş  
✅ **Single Responsibility**: Her factory kendi düşman tipinden sorumlu  
✅ **Flexibility**: Runtime'da farklı düşman tipleri oluşturulabilir

---

### 3.2 Singleton Pattern (Creational) ⭐

#### 3.2.1 Problem Statement

Oyun boyunca tek bir `GameManager` ve tek bir `DatabaseManager` instance'ı olmalı. Birden fazla instance veri tutarsızlığına ve kaynak israfına sebep olur.

#### 3.2.2 Solution Approach

Thread-safe Singleton Pattern kullanılarak (Double-Check Locking) her sınıftan sadece bir instance oluşturulması garantilendi.

#### 3.2.3 Implementation Details

**GameManager:**
```csharp
public sealed class GameManager : ISubject
{
    private static GameManager? _instance;
    private static readonly object _lock = new object();
    
    private GameManager() { }
    
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
}
```

#### 3.2.4 Advantages

✅ **Single Instance**: Garanti edilen tek instance  
✅ **Thread-safe**: Çoklu thread ortamında güvenli  
✅ **Lazy Initialization**: İlk kullanımda oluşturulur  
✅ **Global Access**: Her yerden erişilebilir

---

### 3.3 Decorator Pattern (Structural) ⭐

#### 3.3.1 Problem Statement

Oyuncular oyun sırasında power-up'lar topladıkça dinamik olarak yeni özellikler kazanıyor. Bu özellikleri runtime'da eklemek ve çıkarmak gerekiyor.

#### 3.3.2 Implementation Details

```csharp
public interface IPlayer
{
    int BombCount { get; }
    int BombPower { get; }
    int Speed { get; }
}

public abstract class PlayerDecorator : IPlayer
{
    protected IPlayer _decoratedPlayer;
    
    public PlayerDecorator(IPlayer player)
    {
        _decoratedPlayer = player;
    }
    
    public virtual int BombCount => _decoratedPlayer.BombCount;
}

public class BombCountDecorator : PlayerDecorator
{
    private int _additionalBombs;
    
    public BombCountDecorator(IPlayer player, int additionalBombs = 1) 
        : base(player)
    {
        _additionalBombs = additionalBombs;
    }
    
    public override int BombCount => _decoratedPlayer.BombCount + _additionalBombs;
}
```

**Usage:**
```csharp
IPlayer player = new Player(1, "Hero", position);
player = new BombCountDecorator(player, +2);
player = new SpeedBoostDecorator(player, +1);
```

---

### 3.4 Adapter Pattern (Structural) ⭐

#### 3.4.1 Problem Statement

Oyunda 3 farklı tema var ve her temanın kendi renk ve karakter sistemi var. Bu farklı sistemleri ortak bir interface'den kullanmak gerekiyor.

#### 3.4.2 Implementation Details

```csharp
public interface ITheme
{
    string GetName();
    ConsoleColor GetUnbreakableWallColor();
    char GetUnbreakableWallChar();
}

public class DesertThemeAdapter : ITheme
{
    private readonly DesertTheme _desertTheme;
    
    public DesertThemeAdapter()
    {
        _desertTheme = new DesertTheme();
    }
    
    public string GetName() => _desertTheme.Name;
    public ConsoleColor GetUnbreakableWallColor() => _desertTheme.RockColor;
    public char GetUnbreakableWallChar() => _desertTheme.GetRockChar();
}
```

---

### 3.5 Strategy Pattern (Behavioral) ⭐

#### 3.5.1 Problem Statement

Düşmanların farklı hareket algoritmaları var ve bu algoritmalar runtime'da değişebilmeli.

#### 3.5.2 Implementation Details

```csharp
public interface IMovementStrategy
{
    Position Move(Position currentPosition, Map map, Position? targetPosition);
}

public class PathfindingMovementStrategy : IMovementStrategy
{
    public Position Move(Position currentPosition, Map map, Position? targetPosition)
    {
        if (targetPosition == null) return currentPosition;
        
        List<Position> path = AStar.FindPath(currentPosition, targetPosition, map);
        
        if (path != null && path.Count > 1)
        {
            return path[1];
        }
        
        return currentPosition;
    }
}
```

---

### 3.6 Observer Pattern (Behavioral) ⭐

#### 3.6.1 Problem Statement

Oyun olayları (bomba patlaması, oyuncu ölümü, skor değişimi) gerçekleştiğinde birden fazla bileşenin haberdar olması gerekiyor.

#### 3.6.2 Implementation Details

```csharp
public interface IObserver
{
    void Update(GameEvent gameEvent);
}

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify(GameEvent gameEvent);
}

public class ScoreObserver : IObserver
{
    private int _currentScore = 0;
    
    public void Update(GameEvent gameEvent)
    {
        switch (gameEvent.Type)
        {
            case EventType.WallDestroyed:
                _currentScore += 10;
                break;
            case EventType.EnemyKilled:
                _currentScore += 50;
                break;
        }
    }
}
```

---

### 3.7 State Pattern (Behavioral) ⭐

#### 3.7.1 Problem Statement

Oyuncunun durumuna göre (canlı, ölü, kazanan) farklı davranışlar sergilemesi gerekiyor.

#### 3.7.2 Implementation Details

```csharp
public interface IPlayerState
{
    void Move(Player player, int dx, int dy, Map map);
    void PlaceBomb(Player player);
    void TakeDamage(Player player);
}

public class AliveState : IPlayerState
{
    public void Move(Player player, int dx, int dy, Map map)
    {
        int newX = player.Position.X + dx;
        int newY = player.Position.Y + dy;
        
        if (map.IsWalkable(newX, newY))
        {
            player.Position.X = newX;
            player.Position.Y = newY;
        }
    }
    
    public void TakeDamage(Player player)
    {
        player.IsAlive = false;
        player.State = new DeadState();
    }
}

public class DeadState : IPlayerState
{
    public void Move(Player player, int dx, int dy, Map map)
    {
        Console.WriteLine($"[{player.Name}] cannot move - player is dead!");
    }
}
```

---

### 3.8 Command Pattern (Behavioral) ⭐

#### 3.8.1 Problem Statement

Oyuncu aksiyonlarını kapsüllemek ve undo/redo desteği sağlamak gerekiyor.

#### 3.8.2 Implementation Details

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
    string GetDescription();
}

public class MoveCommand : ICommand
{
    private readonly Player _player;
    private readonly int _dx, _dy;
    private readonly Map _map;
    private Position? _previousPosition;
    
    public MoveCommand(Player player, int dx, int dy, Map map)
    {
        _player = player;
        _dx = dx;
        _dy = dy;
        _map = map;
    }
    
    public void Execute()
    {
        _previousPosition = new Position(_player.Position.X, _player.Position.Y);
        _player.Move(_dx, _dy, _map);
    }
    
    public void Undo()
    {
        if (_previousPosition != null)
        {
            _player.Position = _previousPosition;
        }
    }
}

public class CommandInvoker
{
    private Stack<ICommand> _commandHistory = new Stack<ICommand>();
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _commandHistory.Push(command);
    }
    
    public void UndoLastCommand()
    {
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
        }
    }
}
```

---

### 3.9 Repository Pattern (Data Access) ⭐

#### 3.9.1 Problem Statement

Veritabanı erişimini soyutlamak ve iş mantığını veri erişiminden ayırmak gerekiyor.

#### 3.9.2 Implementation Details

```csharp
public interface IRepository<T> where T : class
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

public class UserRepository : IRepository<User>
{
    public User? GetById(int id)
    {
        var connection = DatabaseManager.Instance.GetConnection();
        return connection.QueryFirstOrDefault<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }
    
    public void Add(User entity)
    {
        var connection = DatabaseManager.Instance.GetConnection();
        var sql = @"INSERT INTO Users (Username, PasswordHash, CreatedAt) 
                   VALUES (@Username, @PasswordHash, @CreatedAt)";
        connection.Execute(sql, entity);
    }
}
```

---

### 3.10 MVC Pattern (Architectural) ⭐

#### 3.10.1 Problem Statement

Uygulama katmanlarını ayırmak ve Separation of Concerns prensibi uygulamak gerekiyor.

#### 3.10.2 Implementation Details

**Model (Models/):**
- Player, Enemy, Bomb, Map
- Business logic

**View (UI/):**
- GameRenderer
- MenuDisplay
- ConsoleUI

**Controller (MVC/Controllers/):**
- GameController
- InputController
- MultiplayerGameController

```csharp
public class GameController
{
    private GameManager _gameManager;  // Model
    private GameRenderer _gameRenderer; // View
    
    public void StartGame(string theme, bool isSinglePlayer)
    {
        // Controller logic
        _gameManager.ResetGame();
        _gameRenderer.Render(_gameManager);
    }
}
```

---

## 4. UML Diyagramları

UML diyagramları `UML_DIAGRAMS.md` dosyasında detaylı olarak sunulmuştur. Her pattern için:
- Class Diagram
- İlişkiler ve bağımlılıklar
- Kullanım örnekleri

---

## 5. Sequence Diyagramlar

### 5.1 Bomba Patlaması Sequence

1. Oyuncu bomba koyar
2. 3 saniye timer başlar
3. Timer bitince patlama gerçekleşir
4. Patlama alanı hesaplanır
5. Duvarlar hasar alır
6. Oyuncular kontrol edilir
7. Observer'lar bilgilendirilir

### 5.2 Factory Kullanımı

1. GameController düşman oluşturma isteği gönderir
2. EnemyFactoryProvider uygun factory'yi döner
3. Factory düşman oluşturur
4. MovementStrategy atanır
5. Düşman oyuna eklenir

---

## 6. Veritabanı Tasarımı

### 6.1 Tablolar

**Users:**
- Id (PK, AUTOINCREMENT)
- Username (UNIQUE)
- PasswordHash (BCrypt)
- CreatedAt (DATETIME)

**GameStatistics:**
- Id (PK)
- UserId (FK → Users.Id)
- Wins, Losses, TotalGames

**HighScores:**
- Id (PK)
- UserId (FK)
- Score
- GameDate

**PlayerPreferences:**
- Id (PK)
- UserId (FK, UNIQUE)
- Theme
- SoundEnabled

---

## 7. Network Protokolü

### 7.1 Mesaj Türleri

- **Connect/Disconnect**: Bağlantı yönetimi
- **PlayerMove**: Oyuncu hareketi
- **PlaceBomb**: Bomba yerleştirme
- **GameState**: Oyun durumu senkronizasyonu
- **Ping/Pong**: Latency ölçümü

### 7.2 JSON Serialization

```json
{
  "Type": "PlayerMove",
  "SenderId": "player-uuid",
  "Timestamp": 1703000000000,
  "Data": "{\"X\":5,\"Y\":3}"
}
```

---

## 8. Kod Kalitesi ve Standartlar

### 8.1 SOLID Principles

- ✅ Single Responsibility
- ✅ Open/Closed
- ✅ Liskov Substitution
- ✅ Interface Segregation
- ✅ Dependency Inversion

### 8.2 Best Practices

- ✅ DRY (Don't Repeat Yourself)
- ✅ KISS (Keep It Simple)
- ✅ Clean Code
- ✅ Meaningful Names
- ✅ Error Handling
- ✅ Documentation

---

## 9. Test Stratejisi

### 9.1 Manuel Testler

- ✅ Single player mode
- ✅ Two player local mode
- ✅ Multiplayer online mode
- ✅ Database operations
- ✅ Theme switching
- ✅ Power-up collection
- ✅ Enemy AI behavior
- ✅ Undo/Redo functionality

---

## 10. Sonuç ve Değerlendirme

### 10.1 Başarılar

- ✅ 10 tasarım kalıbı başarıyla uygulandı
- ✅ SOLID prensipleri takip edildi
- ✅ Temiz ve sürdürülebilir kod yazıldı
- ✅ Kapsamlı dokümantasyon hazırlandı
- ✅ Bonus özellikler eklendi (A*, Multiplayer, UI/UX)

### 10.2 Öğrenilen Dersler

1. **Factory Pattern**: Nesne yaratma esnekliği
2. **Singleton**: Kaynak yönetimi
3. **Decorator**: Runtime özellik ekleme
4. **Adapter**: Interface uyarlama
5. **Strategy**: Algoritma değişimi
6. **Observer**: Loose coupling
7. **State**: Durum yönetimi
8. **Command**: Aksiyon kapsülleme ve undo
9. **Repository**: Data access abstraction
10. **MVC**: Mimari organizasyon

### 10.3 Gelecek İyileştirmeler

- [ ] Unit testler ekleme
- [ ] Sound effects
- [ ] Map editor
- [ ] Replay system
- [ ] AI difficulty levels

---

**Son Güncelleme**: 18 Aralık 2025  
**Versiyon**: 1.0 Final  
**Durum**: ✅ TAMAMLANDI

---

## Referanslar

[1] Eric Freeman et al. Head First Design Patterns. O'Reilly Media, 2004.  
[2] Erich Gamma et al. Design Patterns: Elements of Reusable Object-Oriented Software. 1995.  
[3] Robert Nystrom. Game Programming Patterns. 2014.  
[4] Refactoring.Guru. Design Patterns. 2025.  
[5] Microsoft C# Documentation.  
[6] SQLite Documentation.