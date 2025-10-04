🌃 Survive the Night: Руководство для разработчиков
Это репозиторий проекта Survive the Night — минималистичный 2D-сурвайвор в стиле Vampire Survivors, разработанный на C# с использованием фреймворка MonoGame/XNA.

Данный проект предназначен для обучения базовым концепциям разработки игр: управление состоянием игры, коллизии, менеджеры сущностей (Entity Managers) и архитектура компонентов.

🎯 Архитектура проекта и основные принципы
Проект придерживается паттерна Component-Based Architecture (архитектура, основанная на компонентах) и принципов Single Responsibility Principle (принцип единственной ответственности) для легкой расширяемости.

1. Game State Management
Управление состоянием игры осуществляется через глобальное статическое перечисление Game1.CurrentState и локальную переменную _currentGameState в главном классе Game1.cs.

Состояние (GameState)	Описание	Классы-менеджеры
MainMenu	Стартовый экран.	MainMenu.cs
Playing	Основной игровой цикл (Update, Draw).	Game1.cs, SpawnManager
LevelUp	Пауза для выбора улучшения после получения уровня.	LevelUpMenu.cs
Roulette	Пауза для выбора награды после убийства Элитного врага.	RouletteManager.cs
GameOver	Финальный экран.	—

Экспортировать в Таблицы
Как это работает: Любой класс (например, MainMenu или RouletteManager) может изменить глобальное состояние, установив: Game1.CurrentState = GameState.Playing;.

2. Entities (Сущности)
Все интерактивные объекты в игре наследуются от базовых классов в папке Entities.

Класс	Родитель	Назначение
Player.cs	Entity	Управление вводом, здоровьем, опытом, инвентарём.
Enemy.cs	Entity	Базовый класс для всех врагов (логика движения и базового урона).
EliteEnemy.cs	Enemy	Расширяет Enemy, имеет больше здоровья/урона и запускает Рулетку.
ExperienceOrb.cs	Entity	Элементы, собираемые игроком для получения опыта.
HealthOrb.cs	Entity	Элементы, восстанавливающие здоровье.

Экспортировать в Таблицы
Ключевой момент: Враги (и, предположительно, игрок) имеют метод TakeDamage(int amount) и свойство IsAlive, что упрощает логику коллизий.

3. Weapons (Оружие)
Система оружия является модульной.

Все виды оружия должны наследоваться от абстрактного класса Weapon.cs.

Weapon имеет методы Update(GameTime) и Attack(GameTime, List<Enemy>), которые вызываются каждый кадр в Game1.cs.

Оружие управляет своими снарядами (проджектайлами) или областями воздействия (AoE).

🛠️ Как добавить новое оружие (The Fireball)
Предположим, мы хотим добавить новое оружие: Fireball (Огненный Шар), который запускает снаряд FireballProjectile.

Шаг 1: Создание класса Projectile
Создайте файл Projectiles/FireballProjectile.cs. Этот класс будет определять внешний вид, логику полета и коллизии самого огненного шара.

C#

// FireballProjectile.cs
public class FireballProjectile : Projectile
{
    // Константы
    public const int Size = 16; 

    // Конструктор: (позиция, направление, урон, цвет)
    public FireballProjectile(Vector2 position, Vector2 direction, int damage) 
        : base(position, direction, damage, 300f, Color.OrangeRed, Size)
    {
        // Дополнительная логика инициализации (например, установка текстуры)
    }

    public override void Update(GameTime gameTime, List<Enemy> enemies)
    {
        base.Update(gameTime, enemies); // Обновление базовой логики движения

        // Кастомная логика, например, замедленное вращение
        // Rotation += 0.1f;
    }

    // Если Fireball имеет особую логику поражения (например, AoE), её можно переопределить здесь.
}
Шаг 2: Создание класса Weapon
Создайте файл Weapons/Fireball.cs. Этот класс будет отвечать за тайминг, количество и характеристики создаваемых снарядов.

C#

// Fireball.cs
public class Fireball : Weapon
{
    private const float BaseCooldown = 1.5f; // Запускаем шары каждые 1.5 секунды

    public Fireball(Player player) 
        // base(player, damage, cooldown)
        : base(player, 5, BaseCooldown)
    {
        // Инициализация специфичных для оружия переменных
    }

    public override void Attack(GameTime gameTime, List<Enemy> enemies)
    {
        _timeSinceLastAttack += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_timeSinceLastAttack >= _cooldown)
        {
            // 1. Находим ближайшего врага
            Enemy target = FindNearestEnemy(enemies); 

            if (target != null)
            {
                // 2. Рассчитываем направление
                Vector2 direction = Vector2.Normalize(target.Position - _player.Position);

                // 3. Создаём и запускаем снаряд
                // Используем глобальный список Projectiles, если он есть, или собственный. 
                // В данном проекте, каждое оружие управляет своими проджектайлами
                
                // Добавьте FireballProjectile в список активных снарядов этого оружия (например, ActiveProjectiles)
                // (Предполагается, что у Weapon есть список `ActiveProjectiles` или `ActiveAreas` как у PlayingCards/Molotov)

                // Пример:
                // ActiveProjectiles.Add(new FireballProjectile(_player.Position, direction, CurrentDamage));

                _timeSinceLastAttack = 0f;
            }
        }

        // Обновление всех активных снарядов
        // foreach (var proj in ActiveProjectiles) { proj.Update(gameTime, enemies); }
    }
}
Примечание: Для работы этого класса нужно либо добавить в базовый Weapon список ActiveProjectiles, либо следовать архитектуре PlayingCards.cs/MolotovCocktail.cs.

Шаг 3: Интеграция в Game1
В классе Game1.cs добавьте новое оружие в список:

C#

// Game1.cs -> protected override void Initialize()

// ...
_playingCardsWeapon = new PlayingCards(_player);
_weapons.Add(_playingCardsWeapon);
_weapons.Add(new MolotovCocktail(_player)); 
// !!! ИНТЕГРАЦИЯ НОВОГО ОРУЖИЯ !!!
_weapons.Add(new Fireball(_player)); // <-- Добавили!
// ...
Благодаря тому, что Game1.Update() и Game1.DrawWorldObjects() содержат циклы:

C#

// Game1.Update()
foreach (var weapon in _weapons) { weapon.Update(gameTime); weapon.Attack(gameTime, _enemies); }

// Game1.DrawWorldObjects()
// Логика отрисовки должна быть добавлена здесь:
else if (weapon is Fireball fireball)
{
    foreach (var proj in fireball.ActiveProjectiles)
    {
        if (proj.IsActive) { proj.Draw(_spriteBatch, _debugTexture); }
    }
}
Новое оружие автоматически начнет работать!

📦 Основные папки и их назначение
Папка	Содержимое	Ответственность
Entities	Player, Enemy, Orb	Управление игровыми сущностями, их движением и базовым поведением.
Weapons	Weapon (Base), PlayingCards, Molotov	Логика тайминга атак и параметров оружия.
Projectiles	Projectile (Base), CardProjectile, MolotovArea	Непосредственное нанесение урона, коллизии с врагами и истечение срока жизни.
Managers	SpawnManager, RouletteManager	Геймплейная логика: появление врагов, управление улучшениями и рулеткой.
UI	MainMenu, LevelUpMenu	Отображение интерфейса и обработка ввода в неигровых состояниях.
Content	Fonts, Textures	Все ресурсы проекта (для Monogame).

Экспортировать в Таблицы







