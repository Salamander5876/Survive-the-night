# 🌃 Survive the Night: Руководство для разработчиков

Это репозиторий проекта **Survive the Night** — минималистичный 2D-сурвайвор в стиле **Vampire Survivors**, разработанный на **C#** с использованием фреймворка **MonoGame/XNA**.

Данный проект предназначен для обучения базовым концепциям разработки игр: управление состоянием игры, коллизии, менеджеры сущностей (Entity Managers) и архитектура компонентов. Проект использует простую, но масштабируемую структуру, чтобы вы могли легко добавлять новые механики, такие как оружие, враги или улучшения.


## 📋 Требования и установка

### Зависимости
- **MonoGame** (версия 3.8.1 или выше) — основной фреймворк для рендеринга и игрового цикла.
- **.NET SDK** (версия 6.0 или выше) для компиляции.
- Встроенные библиотеки: `Microsoft.Xna.Framework`, `Microsoft.Xna.Framework.Content`.

### Установка
1. Клонируйте репозиторий:
   ```
   git clone https://github.com/yourusername/survive-the-night.git
   cd survive-the-night
   ```
2. Откройте проект в Visual Studio или VS Code с расширением C#.
3. Восстановите пакеты:
   ```
   dotnet restore
   ```
4. Запустите проект:
   ```
   dotnet run
   ```
   Или нажмите F5 в IDE.

### Структура папок
```
Survive-the-night/
├── Content/          # Ассеты (спрайты, звуки)
├── Entities/         # Базовые классы сущностей (Player, Enemy и т.д.)
├── Projectiles/      # Снаряды для оружия
├── Weapons/          # Классы оружия
├── Managers/         # Менеджеры (SpawnManager, RouletteManager)
├── States/           # Менеджеры состояний (MainMenu, LevelUpMenu)
├── Game1.cs          # Основной класс игры
└── Program.cs        # Точка входа
```

---

## 🎯 Архитектура проекта и основные принципы

Проект построен на принципах **модульности** и **разделения ответственности** (Single Responsibility Principle) для обеспечения легкой расширяемости. Каждая сущность отвечает только за свою логику, а глобальные системы (например, коллизии) обрабатываются централизованно в `Game1.cs`.

### 1. Управление состоянием игры (Game State Management)

Состояние игры (меню, игра, пауза) управляется глобальным статическим перечислением **`GameState.CurrentState`** в классе `Game1.cs`. Это позволяет переключаться между экранами без сложных иерархий.

| Состояние (`GameState`) | Описание | Классы-менеджеры | Ключевые методы |
|------------------------|----------|------------------|-----------------|
| `MainMenu` | Стартовый экран с кнопками "Начать игру", "Выход". | `MainMenuManager.cs` | `Update()`, `Draw()` — обработка кликов мыши/клавиатуры. |
| `Playing` | Основной игровой цикл: спавн врагов, обновление сущностей, атаки. | `Game1.cs`, `SpawnManager.cs` | `Update(gameTime)`, `Draw(gameTime)` — полный цикл игры. |
| `Paused` | Пауза игры (ESC). | `PauseMenu.cs` | `HandleInput()` — возврат в `Playing` или выход в меню. |
| `LevelUp` | Экран выбора улучшения после набора опыта. | `LevelUpMenu.cs` | `ShowOptions()`, `SelectUpgrade()` — случайный выбор из 3-4 опций. |
| `GameOver` | Экран поражения (здоровье = 0). | `GameOverMenu.cs` | `Restart()` — сброс состояния и возврат в `MainMenu`. |
| `Roulette` | Пауза для выбора награды после убийства элитного врага. | `RouletteManager.cs` | `SpinWheel()`, `AwardPrize()` — анимация рулетки с рандомными наградами. |

**Переключение состояний**: В `Game1.Update()` проверяется `CurrentState` и вызывается соответствующий менеджер. Например:
```csharp
switch (CurrentState)
{
    case GameState.Playing:
        UpdatePlaying(gameTime);
        break;
    case GameState.LevelUp:
        levelUpMenu.Update(gameTime);
        if (levelUpMenu.IsSelected) CurrentState = GameState.Playing;
        break;
}
```

### 2. Структура сущностей (Entities)

Все игровые объекты наследуются от базового класса `Entity` (содержит `Position`, `Velocity`, `IsAlive`). Сущности хранятся в списках в `Game1.cs` (например, `List<Enemy> _enemies`).

| Класс | Родитель | Ответственность | Ключевые поля/методы |
|-------|----------|-----------------|----------------------|
| `Entity.cs` (базовый) | — | Общие свойства: позиция, скорость, здоровье. | `Update()`, `Draw()`, `TakeDamage()`. |
| `Player.cs` | `Entity` | Обработка ввода (WASD/стрелки), здоровья, опыта, инвентаря. | `HandleInput()`, `GainExperience()`, `EquipWeapon(Weapon w)`. |
| `Enemy.cs` (абстрактный) | `Entity` | Логика преследования игрока, базовое здоровье и урон. | `ChasePlayer()`, `OnDeath()` — дроп опыта/наград. |
| `BasicEnemy.cs` | `Enemy` | Простой враг с прямолинейным движением. | `UpdatePathfinding()`. |
| `EliteEnemy.cs` | `Enemy` | Усиленный враг с большим HP и спавном миньонов. | `SpawnMinions()`. |
| `Weapon.cs` (абстрактный) | — | Тайминг атаки, управление списком активных снарядов/зон. | `Attack(gameTime, enemies)`, `Update(gameTime)`. |
| `Projectile.cs` (абстрактный) | `Entity` | Логика движения снаряда, проверка коллизий с врагами и нанесение урона. | `Move()`, `CheckCollisions(enemies)`, `OnHit(Enemy e)`. |

**Коллизии**: Используется простой AABB (Axis-Aligned Bounding Box) в `Projectile.CheckCollisions()`. Для оптимизации — проверка только ближайших сущностей (используя расстояние).

### 3. Менеджеры и системы

- **SpawnManager.cs**: Спавнит врагов волнами на основе времени/этапа. Использует `Random` для позиций по краю экрана.
- **ExperienceManager.cs**: Отслеживает XP игрока, триггерит `LevelUp` при порогах (100, 200, 400 XP).
- **InventoryManager.cs**: Хранит список оружия игрока, позволяет экипировку при левел-апе.

### 4. Как работает игровой цикл (`Game1.cs`)

`Game1.cs` — сердце проекта. Он управляет общим циклом, вызывая `Update()` и `Attack()` для **ВСЕГО** списка оружия (`List<Weapon> _weapons`). Это ключевой принцип, позволяющий легко добавлять новое оружие без изменения основного цикла.

```csharp
protected override void Update(GameTime gameTime)
{
    switch (CurrentState)
    {
        case GameState.Playing:
            _player.Update(gameTime);
            SpawnManager.Update(gameTime, _enemies);
            
            // Обновление всех врагов
            foreach (var enemy in _enemies) enemy.Update(gameTime, _player.Position);
            
            // Обновление всех оружий
            foreach (var weapon in _weapons) weapon.Update(gameTime);
            
            // Атаки: передаем список врагов для коллизий
            foreach (var weapon in _weapons) weapon.Attack(gameTime, _enemies);
            
            // Коллизии игрока с врагами
            CheckPlayerCollisions();
            break;
    }
    base.Update(gameTime);
}

private void DrawWorldObjects(SpriteBatch spriteBatch)
{
    // Рисуем игрока
    _player.Draw(spriteBatch);
    
    // Рисуем врагов
    foreach (var enemy in _enemies.Where(e => e.IsAlive)) enemy.Draw(spriteBatch);
    
    // Рисуем снаряды/зоны для каждого оружия
    foreach (var weapon in _weapons)
    {
        weapon.Draw(spriteBatch); // Абстрактный метод в Weapon
    }
}
```

**Производительность**: Для 100+ врагов используйте пулы объектов (Object Pooling) в `SpawnManager` для переиспользования снарядов/врагов.

---

## 🛠️ Руководство по расширению: Добавление нового оружия

Чтобы добавить новый тип оружия (например, **Fireball** — огненный шар, летящий к ближайшему врагу), создайте три компонента: **снаряд**, **класс оружия** и **интеграцию в основной цикл**. Это займет ~30 минут.

### Шаг 1: Создание снаряда (Projectile)

Создайте класс в папке `Projectiles`. Например, `FireballProjectile.cs`. Он наследует `Projectile.cs` и добавляет уникальную логику (например, эффект горения).

```csharp
// Projectiles/FireballProjectile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

public class FireballProjectile : Projectile
{
    private float _burnDuration = 2f; // Длительность горения

    public FireballProjectile(Vector2 position, Vector2 direction, int damage) 
        : base(position, direction, damage, 300f /* speed */, Color.OrangeRed, 16 /* radius */)
    {
        // Уникальная инициализация: добавляем эффект частиц (если есть ParticleSystem)
    }

    public override void Update(GameTime gameTime, List<Enemy> enemies)
    {
        base.Update(gameTime, enemies); // Базовая логика: движение, коллизии
        
        // Уникальная логика: если попал, наноси DoT (Damage over Time)
        if (HasHit)
        {
            // Логика горения реализуется в Enemy.TakeDamage() с таймером
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
    {
        // Отрисовка: круг или спрайт с свечением
        spriteBatch.Draw(debugTexture, Position - new Vector2(Radius), null, Color, 0f, Vector2.Zero, Radius * 2 / debugTexture.Width, SpriteEffects.None, 0f);
    }
}
```

### Шаг 2: Создание класса оружия (Weapon)

Создайте класс в папке `Weapons`. Например, `Fireball.cs`. Он наследует `Weapon.cs` и управляет своим списком снарядов. Обновление снарядов происходит в `Attack()` для доступа к `enemies`.

```csharp
// Weapons/Fireball.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using System.Collections.Generic;
using System.Linq;

public class Fireball : Weapon
{
    private const float BaseCooldown = 1.5f; // Секунд между выстрелами
    
    public List<FireballProjectile> ActiveProjectiles { get; private set; } = new List<FireballProjectile>();

    public Fireball(Player player) : base(player, 5 /* base damage */, BaseCooldown)
    {
        // Инициализация: уровень апгрейда увеличивает урон/скорость
    }

    public override void Update(GameTime gameTime)
    {
        // Легкое обновление: только очистка неактивных (полное — в Attack)
        ActiveProjectiles.RemoveAll(p => !p.IsActive);
    }

    public override void Attack(GameTime gameTime, List<Enemy> enemies)
    {
        // 1. Обновляем таймер
        _timeSinceLastAttack += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // 2. Создание нового снаряда, если кулдаун прошел
        if (_timeSinceLastAttack >= _cooldown)
        {
            Enemy target = FindNearestEnemy(enemies); 
            if (target != null)
            {
                Vector2 direction = Vector2.Normalize(target.Position - _player.Position);
                ActiveProjectiles.Add(new FireballProjectile(_player.Position, direction, CurrentDamage));
                _timeSinceLastAttack = 0f;
            }
        }
        
        // 3. Обновление существующих снарядов (движение, коллизии)
        foreach (var proj in ActiveProjectiles.ToList()) // ToList() для безопасного удаления
        {
            if (proj.IsActive)
            {
                proj.Update(gameTime, enemies);
            }
            else
            {
                ActiveProjectiles.Remove(proj);
            }
        }
    }

    // Вспомогательный метод (можно вынести в базовый Weapon)
    protected Enemy FindNearestEnemy(List<Enemy> enemies)
    {
        return enemies
            .Where(e => e.IsAlive)
            .OrderBy(e => Vector2.Distance(_player.Position, e.Position))
            .FirstOrDefault();
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
    {
        // Отрисовка всех активных снарядов
        foreach (var proj in ActiveProjectiles.Where(p => p.IsActive))
        {
            proj.Draw(spriteBatch, debugTexture);
        }
    }
}
```

### Шаг 3: Интеграция в Game1.cs

1. **Добавьте в список оружия** в `Initialize()` (или при левел-апе):
   ```csharp
   // Game1.cs -> protected override void Initialize()
   _weapons.Add(new MolotovCocktail(_player)); 
   _weapons.Add(new Fireball(_player)); // <-- Новое оружие
   ```

2. **Добавьте отрисовку** в `DrawWorldObjects()` (используйте `is` для полиморфизма):
   ```csharp
   // Game1.cs -> private void DrawWorldObjects(SpriteBatch spriteBatch, Texture2D debugTexture)
   foreach (var weapon in _weapons)
   {
       weapon.Draw(spriteBatch, debugTexture); // Автоматическая отрисовка через override
   }
   // Нет нужды в else if — полиморфизм обрабатывает всё!
   ```

### Шаг 4: Тестирование и баланс
- Запустите игру и проверьте: снаряды летят к врагам? Коллизии работают? Кулдаун правильный?
- Баланс: Увеличьте `BaseCooldown` для новичков, добавьте апгрейды в `LevelUpMenu` (например, "+1 урон Fireball").
- Расширение: Добавьте звук/частицы в `FireballProjectile.Draw()`.

---

## 🚀 Дополнительные расширения

### Добавление нового врага
1. Создайте `NewEnemy.cs` наследуя `Enemy`.
2. Добавьте в `SpawnManager.SpawnEnemy()` случайный выбор типа.
3. Обновите `Draw()` в `Game1` (полиморфно).

### Улучшения (Upgrades)
В `LevelUpMenu` добавьте опции: `UpgradeWeapon(Weapon w, int level)` — увеличивает damage/speed.

### Оптимизации
- **Пулы объектов**: Для снарядов/врагов — переиспользуйте вместо new/delete.
- **Звуки**: Интегрируйте `SoundEffect` в `Attack()`.
- **UI**: Используйте `SpriteFont` для HUD (HP, XP).

## 🐛 Отладка и советы
- **DebugTexture**: Белый круг для коллизий — включите в `Draw()` для визуализации.
- **Ошибки**: Проверьте NullReference в коллизиях — всегда фильтруйте `IsAlive`.
- **Производительность**: Используйте `Profiler` MonoGame для bottleneck'ов.

Если у вас вопросы — создайте issue на GitHub! Happy coding! 🎮