# 📊 Bomberman Multiplayer - UML Diyagramları

**Proje Adı**: Bomberman Multiplayer  
**Tarih**: 18 Aralık 2025  
**Versiyon**: 1.0 Final

---

## İçindekiler

1. [Factory Method Pattern - Class Diagram](#1-factory-method-pattern)
2. [Singleton Pattern - Class Diagram](#2-singleton-pattern)
3. [Decorator Pattern - Class Diagram](#3-decorator-pattern)
4. [Adapter Pattern - Class Diagram](#4-adapter-pattern)
5. [Strategy Pattern - Class Diagram](#5-strategy-pattern)
6. [Observer Pattern - Class Diagram](#6-observer-pattern)
7. [State Pattern - Class Diagram](#7-state-pattern)
8. [Command Pattern - Class Diagram](#8-command-pattern)
9. [Repository Pattern - Class Diagram](#9-repository-pattern)
10. [MVC Pattern - Component Diagram](#10-mvc-pattern)
11. [Sequence Diagrams](#11-sequence-diagrams)

---

## 1. Factory Method Pattern

### Class Diagram

```
┌─────────────────────────────────────────────────────────┐
│                  <<interface>>                          │
│                  IEnemyFactory                          │
├─────────────────────────────────────────────────────────┤
│ + CreateEnemy(id: int, position: Position): Enemy      │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ implements
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│StaticEnemy  │ │ChaseEnemy   │ │SmartEnemy   │
│Factory      │ │Factory      │ │Factory      │
├─────────────┤ ├─────────────┤ ├─────────────┤
│+CreateEnemy│ │+CreateEnemy │ │+CreateEnemy │
└─────────────┘ └─────────────┘ └─────────────┘
        │            │            │
        │creates     │creates     │creates
        ▼            ▼            ▼
    ┌─────────────────────────────────┐
    │           Enemy                 │
    ├─────────────────────────────────┤
    │- id: int                        │
    │- position: Position             │
    │- type: EnemyType                │
    │- movementStrategy: IMovementStr │
    │- isAlive: bool                  │
    ├─────────────────────────────────┤
    │+ Move(map: Map, target: Pos)    │
    │+ GetSymbol(): char              │
    └─────────────────────────────────┘

┌─────────────────────────────────────────┐
│      EnemyFactoryProvider               │
│      (Static Factory)                   │
├─────────────────────────────────────────┤
│+ GetFactory(type: string): IEnemyFact   │
└─────────────────────────────────────────┘
```

**Açıklama**:
- `IEnemyFactory`: Factory interface
- `StaticEnemyFactory`, `ChaseEnemyFactory`, `SmartEnemyFactory`: Concrete factories
- `EnemyFactoryProvider`: Factory provider (Simple Factory)
- Her factory kendi düşman tipini ve hareket stratejisini ayarlar

---

## 2. Singleton Pattern

### Class Diagram

```
┌──────────────────────────────────────────────────┐
│              GameManager                         │
│              <<singleton>>                       │
├──────────────────────────────────────────────────┤
│- _instance: GameManager (static)                │
│- _lock: object (static, readonly)               │
│- _observers: List<IObserver>                    │
│- currentMap: Map                                 │
│- players: List<Player>                           │
│- bombs: List<Bomb>                               │
│- enemies: List<Enemy>                            │
│- powerUps: List<PowerUp>                         │
│- isGameRunning: bool                             │
│- currentUserId: int                              │
├──────────────────────────────────────────────────┤
│- GameManager() (private constructor)             │
│+ Instance: GameManager (static property)         │
│+ Attach(observer: IObserver): void               │
│+ Detach(observer: IObserver): void               │
│+ Notify(event: GameEvent): void                  │
│+ ResetGame(): void                               │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│            DatabaseManager                       │
│            <<singleton>>                         │
├──────────────────────────────────────────────────┤
│- _instance: DatabaseManager (static)            │
│- _lock: object (static, readonly)               │
│- _connection: SQLiteConnection                  │
│- DatabaseFile: string (const)                   │
├──────────────────────────────────────────────────┤
│- DatabaseManager() (private constructor)         │
│+ Instance: DatabaseManager (static property)     │
│+ Initialize(): void                              │
│+ GetConnection(): SQLiteConnection              │
│+ ExecuteNonQuery(query: string): void           │
│+ Close(): void                                   │
│- CreateTables(): void                            │
└──────────────────────────────────────────────────┘
```

**Açıklama**:
- Private constructor ile dışarıdan instance oluşturma engellendi
- Static `Instance` property ile global erişim sağlandı
- Double-check locking ile thread safety
- Lazy initialization

---

## 3. Decorator Pattern

### Class Diagram

```
                  ┌────────────────────────┐
                  │    <<interface>>       │
                  │       IPlayer          │
                  ├────────────────────────┤
                  │+ Id: int               │
                  │+ Name: string          │
                  │+ Position: Position    │
                  │+ BombCount: int        │
                  │+ BombPower: int        │
                  │+ Speed: int            │
                  │+ IsAlive: bool         │
                  ├────────────────────────┤
                  │+ Move(dx, dy, map)     │
                  │+ PlaceBomb()           │
                  │+ GetStats(): string    │
                  └───────┬────────────────┘
                          │
                          │ implements
           ┌──────────────┼──────────────┐
           │              │              │
           ▼              ▼              ▼
    ┌─────────┐  ┌─────────────────┐
    │ Player  │  │PlayerDecorator  │
    │(Concrete)│  │  (Abstract)     │
    └─────────┘  ├─────────────────┤
                 │# _decoratedPlayer│
                 ├─────────────────┤
                 │+ PlayerDecorator│
                 │  (player: IPlayer)│
                 │+ virtual GetStats│
                 └────────┬────────┘
                          │ extends
          ┌───────────────┼───────────────┐
          │               │               │
          ▼               ▼               ▼
┌─────────────────┐ ┌─────────────┐ ┌──────────────┐
│BombCount        │ │BombPower    │ │SpeedBoost    │
│Decorator        │ │Decorator    │ │Decorator     │
├─────────────────┤ ├─────────────┤ ├──────────────┤
│- _additionalBombs│ │- _additionalPow│ │- _speedBoost │
├─────────────────┤ ├─────────────┤ ├──────────────┤
│+ BombCount: int │ │+ BombPower: int│ │+ Speed: int  │
│+ GetStats()     │ │+ GetStats()    │ │+ GetStats()  │
└─────────────────┘ └─────────────┘ └──────────────┘
```

**Kullanım Örneği**:
```csharp
IPlayer player = new Player(1, "Hero", position);
player = new BombCountDecorator(player, +2);
player = new SpeedBoostDecorator(player, +1);
player = new BombPowerDecorator(player, +1);
```

**Açıklama**:
- `IPlayer`: Component interface
- `Player`: Concrete component
- `PlayerDecorator`: Base decorator (abstract)
- `BombCountDecorator`, `BombPowerDecorator`, `SpeedBoostDecorator`: Concrete decorators
- Runtime'da zincirleme decorator uygulanabilir

---

## 4. Adapter Pattern

### Class Diagram

```
                    ┌──────────────────────┐
                    │  <<interface>>       │
                    │      ITheme          │
                    ├──────────────────────┤
                    │+ GetName(): string   │
                    │+ GetUnbreakableWallColor(): ConsoleColor│
                    │+ GetBreakableWallColor(): ConsoleColor  │
                    │+ GetHardWallColor(): ConsoleColor       │
                    │+ GetGroundColor(): ConsoleColor         │
                    │+ GetUnbreakableWallChar(): char         │
                    │+ GetBreakableWallChar(): char           │
                    │+ GetHardWallChar(): char                │
                    └───────────┬──────────┘
                                │ implements
                ┌───────────────┼───────────────┐
                │               │               │
                ▼               ▼               ▼
    ┌─────────────────┐ ┌─────────────┐ ┌─────────────┐
    │DesertTheme      │ │ForestTheme  │ │CityTheme    │
    │Adapter          │ │Adapter      │ │Adapter      │
    ├─────────────────┤ ├─────────────┤ ├─────────────┤
    │- _desertTheme   │ │- _forestTheme│ │- _cityTheme │
    ├─────────────────┤ ├─────────────┤ ├─────────────┤
    │+ GetName()      │ │+ GetName()   │ │+ GetName()  │
    │+ Get*Color()    │ │+ Get*Color() │ │+ Get*Color()│
    │+ Get*Char()     │ │+ Get*Char()  │ │+ Get*Char() │
    └────────┬────────┘ └──────┬───────┘ └──────┬──────┘
             │                 │                │
             │ adapts          │ adapts         │ adapts
             ▼                 ▼                ▼
    ┌─────────────┐   ┌─────────────┐  ┌─────────────┐
    │DesertTheme  │   │ForestTheme  │  │CityTheme    │
    │(Adaptee)    │   │(Adaptee)    │  │(Adaptee)    │
    ├─────────────┤   ├─────────────┤  ├─────────────┤
    │+ Name       │   │+ Name       │  │+ Name       │
    │+ SandColor  │   │+ GrassColor │  │+ ConcreteColor│
    │+ StoneColor │   │+ TreeColor  │  │+ BrickColor │
    │+ RockColor  │   │+ LogColor   │  │+ MetalColor │
    ├─────────────┤   ├─────────────┤  ├─────────────┤
    │+ GetSandChar│   │+ GetGrassChar│ │+ GetConcreteChar│
    │+ GetStoneChar│  │+ GetTreeChar│  │+ GetBrickChar│
    │+ GetRockChar│   │+ GetLogChar │  │+ GetMetalChar│
    └─────────────┘   └─────────────┘  └─────────────┘

┌───────────────────────────────────┐
│      ThemeFactory                 │
│      (Static Factory)             │
├───────────────────────────────────┤
│+ GetTheme(name: string): ITheme   │
└───────────────────────────────────┘
```

**Açıklama**:
- `ITheme`: Target interface
- `DesertTheme`, `ForestTheme`, `CityTheme`: Adaptee sınıflar (uyarlanacak)
- Adapter sınıflar: Adaptee'leri ITheme'e uyarlayan sınıflar
- `ThemeFactory`: Tema seçimi için factory

---

## 5. Strategy Pattern

### Class Diagram

```
            ┌─────────────────────────────────┐
            │      <<interface>>              │
            │    IMovementStrategy            │
            ├─────────────────────────────────┤
            │+ Move(currentPos: Position,     │
            │       map: Map,                 │
            │       target: Position): Position│
            └───────────┬─────────────────────┘
                        │ implements
        ┌───────────────┼───────────────┬──────────┐
        │               │               │          │
        ▼               ▼               ▼          ▼
┌──────────────┐ ┌─────────────┐ ┌──────────┐ ┌─────────────┐
│Static        │ │Random       │ │Chase     │ │Pathfinding  │
│Movement      │ │Movement     │ │Movement  │ │Movement     │
│Strategy      │ │Strategy     │ │Strategy  │ │Strategy     │
├──────────────┤ ├─────────────┤ ├──────────┤ ├─────────────┤
│+ Move()      │ │- _random    │ │+ Move()  │ │+ Move()     │
│  returns     │ ├─────────────┤ │  (simple │ │  (uses A*)  │
│  currentPos  │ │+ Move()     │ │   chase) │ └─────────────┘
└──────────────┘ │  (random dir)│ └──────────┘
                 └─────────────┘

                        │
                        │ uses
                        ▼
              ┌──────────────────┐
              │      Enemy       │
              ├──────────────────┤
              │- id: int         │
              │- position: Position│
              │- type: EnemyType │
              │- movementStrategy: IMovementStrategy│
              │- isAlive: bool   │
              ├──────────────────┤
              │+ Move(map, target)│
              │+ GetSymbol(): char│
              └──────────────────┘

              ┌──────────────────────────┐
              │         AStar            │
              │      (Utility Class)     │
              ├──────────────────────────┤
              │+ FindPath(start, end, map)│
              │- GetHeuristic(a, b)      │
              │- GetNeighbors(pos, map)  │
              │- ReconstructPath(node)   │
              └──────────────────────────┘
```

**Açıklama**:
- `IMovementStrategy`: Strategy interface
- 4 concrete strategies:
  - `StaticMovementStrategy`: Hareket etmez
  - `RandomMovementStrategy`: Rastgele hareket
  - `ChaseMovementStrategy`: Basit takip
  - `PathfindingMovementStrategy`: A* ile akıllı takip (BONUS)
- `Enemy`: Context sınıfı
- `AStar`: A* pathfinding utility (BONUS)

---

## 6. Observer Pattern

### Class Diagram

```
┌─────────────────────────────────────────┐
│         <<interface>>                   │
│           ISubject                      │
├─────────────────────────────────────────┤
│+ Attach(observer: IObserver): void     │
│+ Detach(observer: IObserver): void     │
│+ Notify(event: GameEvent): void        │
└────────────┬────────────────────────────┘
             │
             │ implements
             ▼
┌─────────────────────────────────────────┐
│         GameManager                     │
│         <<singleton>>                   │
├─────────────────────────────────────────┤
│- _observers: List<IObserver>           │
├─────────────────────────────────────────┤
│+ Attach(observer: IObserver)           │
│+ Detach(observer: IObserver)           │
│+ Notify(event: GameEvent)              │
└─────────────────┬───────────────────────┘
                  │
                  │ notifies
                  ▼
        ┌──────────────────────┐
        │  <<interface>>       │
        │     IObserver        │
        ├──────────────────────┤
        │+ Update(GameEvent)   │
        └──────┬───────────────┘
               │ implements
    ┌──────────┼──────────┐
    │          │          │
    ▼          ▼          ▼
┌─────────┐ ┌──────────┐ ┌──────────┐
│Score    │ │Stats     │ │UI        │
│Observer │ │Observer  │ │Observer  │
├─────────┤ ├──────────┤ ├──────────┤
│-_currentScore│-_wallsDestroyed│+ Update()│
│         │ │-_enemiesKilled │    │
├─────────┤ │-_powerUpsCollected│  │
│+ Update()│ ├──────────┤      │
│+ GetScore()│+ Update()│       │
│+ ResetScore()│-SaveGameStats()│  │
└─────────┘ │+ Reset() │       │
            └──────────┘       │
                               └──────────┘

┌─────────────────────────────────┐
│         GameEvent               │
├─────────────────────────────────┤
│+ Type: EventType (enum)         │
│+ Data: object                   │
│+ Timestamp: DateTime            │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│         EventType               │
│         <<enumeration>>         │
├─────────────────────────────────┤
│ BombExploded                    │
│ PlayerDied                      │
│ PowerUpCollected                │
│ WallDestroyed                   │
│ GameEnded                       │
│ EnemyKilled                     │
└─────────────────────────────────┘
```

**Açıklama**:
- `ISubject`: Subject interface (GameManager tarafından implement edilir)
- `IObserver`: Observer interface
- `GameManager`: Concrete subject (Singleton)
- 3 concrete observers:
  - `ScoreObserver`: Skor takibi
  - `StatsObserver`: İstatistik takibi ve veritabanına kaydetme
  - `UIObserver`: UI mesajları
- `GameEvent`: Event data object
- `EventType`: Event türleri

---

## 7. State Pattern

### Class Diagram

```
               ┌─────────────────────────┐
               │   <<interface>>         │
               │    IPlayerState         │
               ├─────────────────────────┤
               │+ Move(player, dx, dy, map)│
               │+ PlaceBomb(player)      │
               │+ TakeDamage(player)     │
               │+ GetStateName(): string │
               └──────────┬──────────────┘
                          │ implements
          ┌───────────────┼───────────────┐
          │               │               │
          ▼               ▼               ▼
    ┌─────────┐     ┌──────────┐   ┌──────────┐
    │Alive    │     │Dead      │   │Winner    │
    │State    │     │State     │   │State     │
    ├─────────┤     ├──────────┤   ├──────────┤
    │+ Move() │     │+ Move()  │   │+ Move()  │
    │  (normal)│     │  (denied)│   │  (denied)│
    │+ PlaceBomb│   │+ PlaceBomb│  │+ PlaceBomb│
    │  (allowed)│   │  (denied)│   │  (denied)│
    │+ TakeDamage│  │+ TakeDamage│ │+ TakeDamage│
    │  (→Dead)│     │  (no-op) │   │  (no-op) │
    └─────────┘     └──────────┘   └──────────┘
          │
          │ used by
          ▼
    ┌──────────────────┐
    │      Player      │
    ├──────────────────┤
    │- id: int         │
    │- name: string    │
    │- position: Position│
    │- state: IPlayerState│
    │- isAlive: bool   │
    ├──────────────────┤
    │+ Move(dx, dy, map)│
    │+ PlaceBomb()     │
    │+ TakeDamage()    │
    └──────────────────┘
```

**State Transitions**:
```
   [AliveState] ──TakeDamage()──> [DeadState]
        │
        └──────Win()──────> [WinnerState]
```

**Açıklama**:
- `IPlayerState`: State interface
- 3 concrete states:
  - `AliveState`: Normal oyun durumu
  - `DeadState`: Oyuncu öldü
  - `WinnerState`: Oyuncu kazandı
- `Player`: Context sınıfı (state'i tutar)
- Her state farklı davranış sergiler

---

## 8. Command Pattern

### Class Diagram

```
         ┌──────────────────────────┐
         │    <<interface>>         │
         │      ICommand            │
         ├──────────────────────────┤
         │+ Execute(): void         │
         │+ Undo(): void            │
         │+ GetDescription(): string│
         └────────┬─────────────────┘
                  │ implements
       ┌──────────┼──────────┐
       │          │          │
       ▼          ▼          ▼
┌─────────────┐ ┌──────────────┐
│Move         │ │PlaceBomb     │
│Command      │ │Command       │
├─────────────┤ ├──────────────┤
│- _player    │ │- _player     │
│- _dx, _dy   │ │- _placedBomb │
│- _map       │ ├──────────────┤
│- _previousPos││+ Execute()   │
├─────────────┤ │  (place bomb)│
│+ Execute()  │ │+ Undo()      │
│  (move)     │ │  (remove bomb)│
│+ Undo()     │ │+ GetDescription()│
│  (restore pos)│└──────────────┘
│+ GetDescription()│
└─────────────┘

       │
       │ uses
       ▼
┌──────────────────────────┐
│   CommandInvoker         │
├──────────────────────────┤
│- _commandHistory: Stack  │
│- _maxHistorySize: int    │
├──────────────────────────┤
│+ ExecuteCommand(ICommand)│
│+ UndoLastCommand(): void │
│+ ClearHistory(): void    │
│+ GetHistorySize(): int   │
└──────────────────────────┘

       │
       │ used by
       ▼
┌──────────────────────────┐
│   InputController        │
├──────────────────────────┤
│- _commandInvoker         │
│- _playerControls: Dict   │
├──────────────────────────┤
│+ ProcessInput(key, player)│
│+ ProcessMultiplayerInput()│
│+ UndoLastCommand()       │
└──────────────────────────┘
```

**Açıklama**:
- `ICommand`: Command interface
- 2 concrete commands:
  - `MoveCommand`: Hareket komutu (undo ile geri alma)
  - `PlaceBombCommand`: Bomba yerleştirme (undo ile kaldırma)
- `CommandInvoker`: Command yöneticisi (history stack)
- `InputController`: Client (commands'ları oluşturur ve execute eder)
- Undo/Redo desteği

---

## 9. Repository Pattern

### Class Diagram

```
                ┌───────────────────────────┐
                │   <<interface>>           │
                │   IRepository<T>          │
                ├───────────────────────────┤
                │+ GetById(id: int): T      │
                │+ GetAll(): IEnumerable<T> │
                │+ Add(entity: T): void     │
                │+ Update(entity: T): void  │
                │+ Delete(id: int): void    │
                └────────┬──────────────────┘
                         │ implements
         ┌───────────────┼───────────────┬──────────┐
         │               │               │          │
         ▼               ▼               ▼          ▼
┌────────────┐  ┌─────────────┐ ┌──────────┐ ┌──────────┐
│User        │  │GameStatistic│ │HighScore │ │Player    │
│Repository  │  │Repository   │ │Repository│ │Preference│
├────────────┤  ├─────────────┤ ├──────────┤ │Repository│
│+ GetById() │  │+ GetById()  │ │+ GetById()│ ├──────────┤
│+ GetAll()  │  │+ GetAll()   │ │+ GetAll()│ │+ GetById()│
│+ Add()     │  │+ Add()      │ │+ Add()   │ │+ GetAll()│
│+ Update()  │  │+ Update()   │ │+ Update()│ │+ Add()   │
│+ Delete()  │  │+ Delete()   │ │+ Delete()│ │+ Update()│
│+ GetByUsername│+ GetByUserId│+ GetTopScores│+ Delete()│
│+ UsernameExists│+ IncrementWins│+ GetUserScores│+ GetByUserId│
└────────────┘  │+ IncrementLosses│└──────────┘└──────────┘
                └─────────────┘

         │
         │ uses
         ▼
┌──────────────────────────┐
│   DatabaseManager        │
│   <<singleton>>          │
├──────────────────────────┤
│+ GetConnection():        │
│  SQLiteConnection        │
└──────────────────────────┘

         │
         │ queries
         ▼
┌──────────────────────────┐
│   SQLite Database        │
├──────────────────────────┤
│ Tables:                  │
│ - Users                  │
│ - GameStatistics         │
│ - HighScores             │
│ - PlayerPreferences      │
└──────────────────────────┘
```

**Açıklama**:
- `IRepository<T>`: Generic repository interface
- 4 concrete repositories:
  - `UserRepository`: Kullanıcı işlemleri
  - `StatsRepository`: İstatistik işlemleri
  - `ScoreRepository`: Skor işlemleri
  - `PreferencesRepository`: Tercih işlemleri
- Dapper ORM kullanılır
- DatabaseManager singleton ile bağlantı yönetimi

---

## 10. MVC Pattern

### Component Diagram

```
┌─────────────────────────────────────────────────────────┐
│                      MVC ARCHITECTURE                   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────┐
│       USER          │
│   (Console Input)   │
└──────────┬──────────┘
           │ input
           ▼
┌───────────────────────────────────────┐
│         CONTROLLER LAYER              │
│  (MVC/Controllers/)                   │
├───────────────────────────────────────┤
│ ┌──────────────────────────────────┐ │
│ │   GameController                 │ │
│ │ - Manages game loop              │ │
│ │ - Handles input                  │ │
│ │ - Coordinates Model & View       │ │
│ └──────────────────────────────────┘ │
│                                       │
│ ┌──────────────────────────────────┐ │
│ │   MultiplayerGameController      │ │
│ │ - Network game management        │ │
│ │ - Client/Server logic            │ │
│ └──────────────────────────────────┘ │
│                                       │
│ ┌──────────────────────────────────┐ │
│ │   InputController                │ │
│ │ - Keyboard input processing      │ │
│ │ - Command creation               │ │
│ └──────────────────────────────────┘ │
└──────────┬────────────────┬───────────┘
           │                │
           │ updates        │ queries
           ▼                │
┌───────────────────────────▼───────────┐
│          MODEL LAYER                  │
│  (Models/, Core/)                     │
├───────────────────────────────────────┤
│ ┌──────────────────────────────────┐ │
│ │   GameManager (Singleton)        │ │
│ │ - Game state                     │ │
│ │ - Players, Bombs, Enemies        │ │
│ │ - Observer pattern hub           │ │
│ └──────────────────────────────────┘ │
│                                       │
│ ┌──────────────────────────────────┐ │
│ │   Domain Models                  │ │
│ │ - Player, Bomb, Enemy            │ │
│ │ - Map, Position, PowerUp         │ │
│ │ - Wall implementations           │ │
│ └──────────────────────────────────┘ │
│                                       │
│ ┌──────────────────────────────────┐ │
│ │   Database Models                │ │
│ │ - User, GameStatistic            │ │
│ │ - HighScore, PlayerPreference    │ │
│ └──────────────────────────────────┘ │
│                                       │
│ ┌──────────────────────────────────┐ │
│ │   Repositories                   │ │
│ │ - UserRepository                 │ │
│ │ - StatsRepository                │ │
│ │ - ScoreRepository                │ │
│ └──────────────────────────────────┘ │
└──────────┬────────────────────────────┘
           │
           │ notifies
           ▼
┌───────────────────────────────────────┐
│          VIEW LAYER                   │
│  (UI/)                                │
├───────────────────────────────────────┤
│ ┌──────────────────────────────────┐ │
│ │   GameRenderer                   │ │
│ │ - Renders game board             │ │
│ │ - Player/Enemy rendering         │ │
│ │ - Bomb/Explosion effects         │ │
│ └──────────────────────────────────┘ │
│                                       │
│ ┌──────────────────────────────────┐ │
│ │   MenuDisplay                    │ │
│ │ - Main menu                      │ │
│ │ - Settings menu                  │ │
│ │ - Leaderboard display            │ │
│ └──────────────────────────────────┘ │
│                                       │
│ ┌──────────────────────────────────┐ │
│ │   ConsoleUI                      │ │
│ │ - UI utilities                   │ │
│ │ - Color management               │ │
│ │ - Formatting helpers             │ │
│ └──────────────────────────────────┘ │
└──────────┬────────────────────────────┘
           │
           │ displays
           ▼
┌─────────────────────┐
│       USER          │
│  (Console Output)   │
└─────────────────────┘
```

**Data Flow**:
```
User Input → Controller → Model → View → User Output
     ↑                                      │
     └──────────────────────────────────────┘
           (Feedback Loop)
```

**Açıklama**:
- **Model**: İş mantığı ve veri (Models/, Core/, Database/)
- **View**: Görsel sunum (UI/)
- **Controller**: Akış kontrolü (MVC/Controllers/)
- Observer pattern ile Model → View bildirim
- Separation of Concerns prensibi

---

## 11. Sequence Diagrams

### 11.1 Bomba Patlaması Sequence Diagram

```
User    GameController  GameManager  Bomb  Map  Player  Observer
 │            │             │         │     │      │       │
 │  [Space]   │             │         │     │      │       │
 ├───────────>│             │         │     │      │       │
 │            │ PlaceBomb() │         │     │      │       │
 │            ├────────────>│         │     │      │       │
 │            │             │ new Bomb│     │      │       │
 │            │             ├────────>│     │      │       │
 │            │             │<────────┤     │      │       │
 │            │             │ Add(bomb)     │      │       │
 │            │             │         │     │      │       │
 │            │    [3 seconds pass]   │     │      │       │
 │            │             │         │     │      │       │
 │            │ Update()    │         │     │      │       │
 │            ├────────────>│         │     │      │       │
 │            │             │ Update()│     │      │       │
 │            │             ├────────>│     │      │       │
 │            │             │ Timer--│     │      │       │
 │            │             │         │     │      │       │
 │            │             │ ShouldExplode?│      │       │
 │            │             ├────────>│     │      │       │
 │            │             │<────YES─┤     │      │       │
 │            │             │         │     │      │       │
 │            │ ExplodeBomb(bomb)    │      │       │
 │            ├────────────>│         │     │      │       │
 │            │             │ GetExplosionArea()   │       │
 │            │             ├──────────────>│      │       │
 │            │             │<─────positions│      │       │
 │            │             │         │     │      │       │
 │            │             │ Notify(BombExploded)│        │
 │            │             ├──────────────────────────────>│
 │            │             │         │     │      │       │
 │            │             │ DamageWall(pos)      │       │
 │            │             ├──────────────>│      │       │
 │            │             │         │     │      │       │
 │            │             │ CheckPlayerHit(pos)  │       │
 │            │             ├─────────────────────>│       │
 │            │             │         │     │ TakeDamage() │
 │            │             │         │     │<─────┤       │
 │            │             │         │     │ State│       │
 │            │             │         │     │ →Dead│       │
 │            │             │         │     │      │       │
 │            │             │ Notify(PlayerDied)   │       │
 │            │             ├──────────────────────────────>│
 │            │             │         │     │      │       │
 │            │             │ Remove(bomb)  │      │       │
 │            │             │         │     │      │       │
 │            │<────────────┤         │     │      │       │
 │            │ Render()    │         │     │      │       │
 │<───────────┤             │         │     │      │       │
```

### 11.2 Factory Kullanımı Sequence Diagram

```
GameController  EnemyFactoryProvider  SmartEnemyFactory  Enemy  PathfindingStrategy
     │                  │                    │             │            │
     │ SpawnEnemies()   │                    │             │            │
     ├──────────────────>│                   │             │            │
     │                  │ GetFactory("smart")│             │            │
     │                  ├───────────────────>│             │            │
     │                  │<──────────────────┤             │            │
     │                  │                    │             │            │
     │ CreateEnemy(id, pos)                 │             │            │
     ├──────────────────────────────────────>│            │            │
     │                  │                    │ new Enemy() │            │
     │                  │                    ├────────────>│            │
     │                  │                    │<────────────┤            │
     │                  │                    │             │            │
     │                  │                    │ new PathfindingStrategy()│
     │                  │                    ├─────────────────────────>│
     │                  │                    │<─────────────────────────┤
     │                  │                    │             │            │
     │                  │                    │ SetStrategy(strategy)    │
     │                  │                    ├────────────>│            │
     │                  │                    │             │ strategy = PathfindingStrategy
     │                  │                    │             │            │
     │                  │                    │<────enemy───┤            │
     │<─────────────────────────────────────┤             │            │
     │                  │                    │             │            │
     │ Add(enemy)       │                    │             │            │
     ├──────────────────>GameManager        │             │            │
     │                  │                    │             │            │
     │                  │                    │             │            │
     │ [Game Loop]      │                    │             │            │
     │ enemy.Move(map, playerPos)           │             │            │
     ├──────────────────────────────────────────────────>│            │
     │                  │                    │             │ Move(currentPos, map, target)
     │                  │                    │             ├───────────>│
     │                  │                    │             │ AStar.FindPath()
     │                  │                    │             │            │
     │                  │                    │             │<───path────┤
     │                  │                    │             │ newPos = path[1]
     │<─────────────────────────────────────────────newPos┤            │
     │                  │                    │             │            │
```

### 11.3 Power-up Collection Sequence Diagram

```
User  GameController  Player  PowerUp  Decorator  Observer
 │          │           │        │         │         │
 │ [Move]   │           │        │         │         │
 ├─────────>│           │        │         │         │
 │          │ Move()    │        │         │         │
 │          ├──────────>│        │         │         │
 │          │           │ newPos │         │         │
 │          │           │        │         │         │
 │          │ CheckPowerUpCollection()     │         │
 │          ├──────────>│        │         │         │
 │          │           │ At(newPos)?      │         │
 │          │           ├───────>│         │         │
 │          │           │<───YES─┤         │         │
 │          │           │        │         │         │
 │          │           │ Collect()        │         │
 │          │           ├───────>│         │         │
 │          │           │        │ IsCollected=true  │
 │          │           │        │         │         │
 │          │ ApplyPowerUp(player, powerUp)│         │
 │          ├──────────────────────────────>│        │
 │          │           │        │  new BombCountDecorator(player)
 │          │           │        │         │         │
 │          │<──decoratedPlayer────────────┤         │
 │          │           │        │         │         │
 │          │ Notify(PowerUpCollected)     │         │
 │          ├────────────────────────────────────────>│
 │          │           │        │         │  Update()│
 │          │           │        │         │  +25 Score
 │          │           │        │         │         │
 │          │ Render()  │        │         │         │
 │<─────────┤           │        │         │         │
 │  ⭐ +Bomb │           │        │         │         │
```

---

## Özet

Bu dokümanda Bomberman Multiplayer projesinde kullanılan **10 tasarım kalıbı** için UML diyagramları sunulmuştur:

1. ✅ **Factory Method** - Düşman yaratma
2. ✅ **Singleton** - GameManager, DatabaseManager
3. ✅ **Decorator** - Power-up sistemi
4. ✅ **Adapter** - Tema sistemi
5. ✅ **Strategy** - Düşman hareket algoritmaları
6. ✅ **Observer** - Event notification
7. ✅ **State** - Oyuncu durumları
8. ✅ **Command** - Aksiyon kapsülleme
9. ✅ **Repository** - Data access
10. ✅ **MVC** - Mimari pattern

Ayrıca **3 sequence diagram** ile dinamik davranışlar gösterilmiştir.

---

**Son Güncelleme**: 18 Aralık 2025  
**Versiyon**: 1.0 Final