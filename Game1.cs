using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

// Убедитесь, что все эти пространства имен существуют
using Survive_the_night.Entities;
using Survive_the_night.Weapons;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;
using Survive_the_night.Interfaces;

namespace Survive_the_night
{
    /// <summary>
    /// Перечисление состояний игры. Определено ТОЛЬКО здесь, в корневом пространстве имен.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        StartMenu,
        Playing,
        LevelUp,
        GameOver,
        Roulette
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // --- СТАТИЧЕСКИЕ ПОЛЯ ДЛЯ ГЛОБАЛЬНОГО ДОСТУПА ---
        public static System.Random Random { get; private set; } = new System.Random();
        public static Vector2 WorldSize { get; private set; }
        public static List<Enemy> CurrentEnemies { get; private set; }

        // Звуки (только общие, не связанные с оружиями)
        public static SoundEffect SFXGoldenSword;
        public static SoundEffect SFXThrowMolotov;
        public static SoundEffect SFXFireBurn;
        public static SoundEffect SFXCasinoChips;

        /// <summary>
        /// Глобальное статическое поле, которое используется для управления состоянием игры из других классов.
        /// </summary>
        public static GameState CurrentState = GameState.MainMenu;
        // ------------------------------------------------

        // Game State Management
        private GameState _currentGameState;
        private MainMenuRenderer _mainMenu;
        private StartMenu _startMenu;

        // Game World Entities
        private Player _player;
        private SpawnManager _spawnManager;
        private Camera _camera;
        private List<Enemy> _enemies = new List<Enemy>();
        private List<ExperienceOrb> _experienceOrbs = new List<ExperienceOrb>();
        private List<BaseHealthOrb> _healthOrbs = new List<BaseHealthOrb>();

        // World Generation
        private WorldGeneration _worldGeneration;

        // HUD Data
        private float _survivalTime = 0f;
        private int _killCount = 0;

        // Weapons and Upgrades
        private List<Weapon> _weapons = new List<Weapon>();
        private LevelUpMenu _levelUpMenu;
        private LevelUpMenuRenderer _levelUpMenuRenderer;
        private RouletteManager _rouletteManager;

        // Content
        private Texture2D _debugTexture;
        private SpriteFont _font;
        // Textures
        private Texture2D _heartTexture;
        private Texture2D _goldenHeartTexture;

        // Стартовое оружие
        private WeaponName _selectedStartingWeapon = WeaponName.PlayingCards;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Синхронизация локального и статического состояния
            _currentGameState = GameState.MainMenu;
            Game1.CurrentState = GameState.MainMenu;

            Vector2 initialPlayerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
            );

            _player = new Player(initialPlayerPosition);
            _camera = new Camera(_player, GraphicsDevice.Viewport);

            // Спавн менеджер с границами
            _spawnManager = new SpawnManager(_enemies, _player, _camera, GraphicsDevice.Viewport);

            // Оружие будет инициализировано после выбора в StartMenu
            CurrentEnemies = _enemies;

            base.Initialize();
        }

        protected override void LoadContent()
        {

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            // Загрузка шрифта
            _font = Content.Load<SpriteFont>("Fonts/Default");

            // Загрузка текстуры пола казино и инициализация генерации мира
            Texture2D casinoFloorTexture = Content.Load<Texture2D>("Sprites/CasinoFloor");
            _worldGeneration = new WorldGeneration(casinoFloorTexture, _camera, GraphicsDevice.Viewport);

            // --- ЗАГРУЗКА ТЕКСТУР И ЗВУКОВ ДЛЯ ОРУЖИЙ ЧЕРЕЗ WEAPON MANAGER ---

            // Игральные карты
            var cardTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/Card1");
            var cardTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/Card2");
            var cardTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/Card3");
            var cardTexture4 = Content.Load<Texture2D>("Sprites/Projectiles/Card4");
            var cardSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXCardDeal");
            WeaponManager.LoadWeaponTextures(WeaponName.PlayingCards, cardTexture1, cardTexture2, cardTexture3, cardTexture4);
            WeaponManager.LoadWeaponSound(WeaponName.PlayingCards, cardSound);

            // Золотые пули
            var bulletTexture = Content.Load<Texture2D>("Sprites/Projectiles/Bullet");
            var gunSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXGunShooting");
            WeaponManager.LoadWeaponTextures(WeaponName.GoldenBullet, bulletTexture);
            WeaponManager.LoadWeaponSound(WeaponName.GoldenBullet, gunSound);

            // Фишки казино
            var chipTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsBlue");
            var chipTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsGreen");
            var chipTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsRed");
            var chipsSound = Content.Load<SoundEffect>("Sounds/Weapons/SFCCasinoChips");
            WeaponManager.LoadWeaponTextures(WeaponName.CasinoChips, chipTexture1, chipTexture2, chipTexture3);
            WeaponManager.LoadWeaponSound(WeaponName.CasinoChips, chipsSound);

            // Золотой меч
            var swordTexture = Content.Load<Texture2D>("Sprites/Projectiles/GoldenSword");
            SFXGoldenSword = Content.Load<SoundEffect>("Sounds/Weapons/SFXGoldenSword");
            WeaponManager.LoadWeaponTextures(WeaponName.GoldenSword, swordTexture);
            WeaponManager.LoadWeaponSound(WeaponName.GoldenSword, SFXGoldenSword);

            // Молотов
            var molotovTexture = Content.Load<Texture2D>("Sprites/Projectiles/Molotov");
            var molotovFireTexture = Content.Load<Texture2D>("Sprites/Projectiles/MolotovFire");
            SFXThrowMolotov = Content.Load<SoundEffect>("Sounds/Weapons/SFXThrowMolotov");
            SFXFireBurn = Content.Load<SoundEffect>("Sounds/Weapons/SFXFireBurn");
            MolotovCocktail.SetTextures(molotovTexture, molotovFireTexture);
            WeaponManager.LoadWeaponTextures(WeaponName.MolotovCocktail, molotovTexture);
            WeaponManager.LoadWeaponSound(WeaponName.MolotovCocktail, SFXThrowMolotov);

            // Установка текстур по умолчанию для проектов
            PlayingCard.SetDefaultTexture(cardTexture1);
            GoldenBulletProjectile.SetDefaultTexture(bulletTexture);
            GoldenSwordProjectile.SetDefaultTexture(swordTexture);
            CasinoChip.SetDefaultTexture(chipTexture1);

            _heartTexture = Content.Load<Texture2D>("Sprites/Heart");
            _goldenHeartTexture = Content.Load<Texture2D>("Sprites/GoldenHeart");

            // Загрузка текстур для StartMenu
            var weaponCellTexture = Content.Load<Texture2D>("Sprites/GUI/CellWeapon");
            var upButtonTexture = Content.Load<Texture2D>("Sprites/GUI/UpButton");
            var downButtonTexture = Content.Load<Texture2D>("Sprites/GUI/DownButton");

            // Инициализация менеджеров и меню
            _mainMenu = new MainMenuRenderer(GraphicsDevice, _debugTexture, _font);
            _startMenu = new StartMenu(GraphicsDevice, _debugTexture, _font);
            _startMenu.LoadContent(weaponCellTexture, upButtonTexture, downButtonTexture);

            _levelUpMenu = new LevelUpMenu(_player, _weapons, GraphicsDevice, _debugTexture, _font);
            _levelUpMenuRenderer = new LevelUpMenuRenderer(_levelUpMenu, GraphicsDevice, _debugTexture, _font);
            _rouletteManager = new RouletteManager(_levelUpMenu);
        }

        // Метод для инициализации выбранного оружия
        private void InitializePlayerWeapon()
        {
            _weapons.Clear();
            var selectedWeapon = WeaponManager.CreateWeapon(_selectedStartingWeapon, _player);
            _weapons.Add(selectedWeapon);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Обновляем приватное состояние из статического для работы switch'а
            _currentGameState = Game1.CurrentState;

            switch (_currentGameState)
            {
                case GameState.MainMenu:
                    Game1.CurrentState = _mainMenu.Update(gameTime);
                    break;

                case GameState.StartMenu:
                    var newState = _startMenu.Update(gameTime);
                    if (newState == GameState.Playing)
                    {
                        // Сохраняем выбранное оружие и инициализируем игрока
                        _selectedStartingWeapon = _startMenu.SelectedWeapon;
                        InitializePlayerWeapon();
                    }
                    Game1.CurrentState = newState;
                    break;

                case GameState.Playing:
                    _survivalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Проверки состояния
                    if (!_player.IsAlive)
                    {
                        Game1.CurrentState = GameState.GameOver;
                        return;
                    }

                    // Обычный LevelUp
                    if (_player.IsLevelUpPending)
                    {
                        Game1.CurrentState = GameState.LevelUp;
                        return;
                    }

                    // Игровая логика
                    _player.Update(gameTime);
                    _camera.Follow();
                    _spawnManager.Update(gameTime);

                    for (int i = _enemies.Count - 1; i >= 0; i--)
                    {
                        Enemy enemy = _enemies[i];
                        enemy.Update(gameTime);

                        if (!enemy.IsAlive)
                        {
                            _killCount++;

                            // Дроп элитного врага: запуск рулетки
                            if (enemy is EliteEnemy)
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    _experienceOrbs.Add(new ExperienceOrb(enemy.Position, 1));
                                }

                                _rouletteManager.StartRoulette();
                            }
                            else
                            {
                                // Обычный дроп опыта
                                _experienceOrbs.Add(new ExperienceOrb(enemy.Position, 1));

                                // Шанс дропа сердца 2%
                                if (Game1.Random.NextDouble() < 0.02)
                                {
                                    _healthOrbs.Add(new HealthOrb(enemy.Position, 0.25f));
                                }
                                // Шанс дропа золотого сердца 1%
                                if (Game1.Random.NextDouble() < 0.01)
                                {
                                    _healthOrbs.Add(new GoldenHealthOrb(enemy.Position, 0.5f));
                                }
                            }

                            _enemies.RemoveAt(i);
                        }
                    }

                    // Обработка коллизий с врагами
                    if (_player.IsAlive)
                    {
                        var playerBounds = GetBounds(_player);
                        var collidingEnemies = _enemies.Where(enemy =>
                            enemy.IsAlive &&
                            GetBounds(enemy).Intersects(playerBounds)
                        ).ToList();

                        if (collidingEnemies.Any())
                        {
                            if (!_player.IsInvulnerable)
                            {
                                _player.TakeDamage(collidingEnemies.First().Damage);
                            }
                        }
                    }

                    // Обновление оружия, орбов опыта и хилок
                    foreach (var weapon in _weapons)
                    {
                        weapon.Update(gameTime);
                        weapon.Attack(gameTime, _enemies);
                    }

                    for (int i = _experienceOrbs.Count - 1; i >= 0; i--)
                    {
                        var orb = _experienceOrbs[i];
                        if (orb.IsActive) { orb.Update(gameTime, _player); }
                        if (!orb.IsActive) { _player.GainExperience(orb.Value); _experienceOrbs.RemoveAt(i); }
                    }

                    // Обновление и сбор хилок
                    for (int i = _healthOrbs.Count - 1; i >= 0; i--)
                    {
                        var orb = _healthOrbs[i];
                        if (orb is GoldenHealthOrb golden)
                        {
                            if (golden.IsActive) { golden.Update(gameTime, _player); }
                            if (!golden.IsActive) { _player.Heal(_player.GetGoldenHeartHealAmount(golden.HealAmount)); _healthOrbs.RemoveAt(i); }
                        }
                        else if (orb is HealthOrb health)
                        {
                            if (health.IsActive) { health.Update(gameTime, _player); }
                            if (!health.IsActive) { _player.Heal((health.HealAmount + _player.HeartHealBonusPercent) * _player.MaxHealth); _healthOrbs.RemoveAt(i); }
                        }
                    }

                    break;

                case GameState.LevelUp:
                    if (_levelUpMenu.CurrentOptions.Count == 0) { _levelUpMenu.GenerateOptions(); }
                    _levelUpMenu.Update(gameTime);
                    if (!_player.IsLevelUpPending)
                    {
                        Game1.CurrentState = GameState.Playing;
                        _levelUpMenu.CurrentOptions.Clear();
                    }
                    break;

                case GameState.Roulette:
                    _rouletteManager.Update(gameTime);
                    break;

                case GameState.GameOver:
                    // Ничего не делаем, ждем, пока пользователь выйдет
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (_currentGameState)
            {
                case GameState.MainMenu:
                    _spriteBatch.Begin();
                    _mainMenu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.StartMenu:
                    _spriteBatch.Begin();
                    _startMenu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.Playing:
                case GameState.LevelUp:
                case GameState.GameOver:
                case GameState.Roulette:
                    // Отрисовка игрового мира (с камерой)
                    _spriteBatch.Begin(transformMatrix: _camera.Transform);
                    DrawWorldObjects();
                    _spriteBatch.End();

                    // Отрисовка HUD и UI (без камеры)
                    _spriteBatch.Begin();
                    DrawHUD();

                    if (_currentGameState == GameState.LevelUp)
                    {
                        DrawLevelUpPendingScreen(_spriteBatch);
                        _levelUpMenuRenderer.Draw(_spriteBatch);
                    }

                    if (_currentGameState == GameState.Roulette)
                    {
                        DrawLevelUpPendingScreen(_spriteBatch);
                        _rouletteManager.Draw(
                            _spriteBatch,
                            _font,
                            _debugTexture,
                            GraphicsDevice.Viewport.Width,
                            GraphicsDevice.Viewport.Height
                        );
                    }

                    if (_currentGameState == GameState.GameOver)
                    {
                        DrawGameOverScreen(_spriteBatch);
                    }

                    _spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ DRAW ---

        private void DrawWorldObjects()
        {
            // Отрисовка фона мира
            _worldGeneration.Draw(_spriteBatch);

            // Визуализация зоны спавна (для отладки)
            Vector2 screenCenter = _player.Position;
            float screenLeft = screenCenter.X - GraphicsDevice.Viewport.Width / 2;
            float screenRight = screenCenter.X + GraphicsDevice.Viewport.Width / 2;
            float screenTop = screenCenter.Y - GraphicsDevice.Viewport.Height / 2;
            float screenBottom = screenCenter.Y + GraphicsDevice.Viewport.Height / 2;

            // Рисуем границы экрана
            Rectangle screenBounds = new Rectangle(
                (int)screenLeft, (int)screenTop,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height
            );
            _spriteBatch.Draw(_debugTexture, screenBounds, Color.Green * 0.1f);

            // Отрисовка огненных областей первыми (под всеми)
            foreach (var weapon in _weapons)
            {
                if (weapon is MolotovCocktail molotov)
                {
                    molotov.DrawProjectiles(_spriteBatch);
                }
            }

            // Отрисовка орбов опыта
            foreach (var orb in _experienceOrbs)
            {
                if (orb.IsActive)
                {
                    orb.Draw(_spriteBatch, _debugTexture, orb.Color);
                }
            }

            // Отрисовка хилок
            foreach (var orb in _healthOrbs)
            {
                if (orb.IsActive)
                {
                    if (orb is HealthOrb)
                        HealthOrb.SetTexture(_heartTexture);
                    else if (orb is GoldenHealthOrb)
                        GoldenHealthOrb.SetTexture(_goldenHeartTexture);
                    orb.Draw(_spriteBatch, _debugTexture, orb.Color);
                }
            }

            // Отрисовка врагов
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    enemy.Draw(_spriteBatch, _debugTexture);
                }
            }

            // Отрисовка игрока (САМЫЙ ВЕРХНИЙ СЛОЙ)
            Color playerTint = Color.White;
            if (_player.IsInvulnerable)
            {
                if ((int)(_survivalTime * 10) % 2 == 0)
                {
                    playerTint = Color.Red * 0.5f;
                }
            }
            _player.Draw(_spriteBatch, _debugTexture, playerTint);

            // Отрисовка остального оружия (НАД ВСЕМИ)
            foreach (var weapon in _weapons)
            {
                if (weapon is PlayingCards cards)
                {
                    foreach (var card in cards.ActiveProjectiles)
                    {
                        if (card.IsActive)
                        {
                            card.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is GoldenBullet goldenBullet)
                {
                    foreach (var bullet in goldenBullet.ActiveProjectiles)
                    {
                        if (bullet.IsActive)
                        {
                            bullet.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is GoldenSword sword)
                {
                    foreach (var projectile in sword.ActiveProjectiles)
                    {
                        if (projectile.IsActive)
                        {
                            projectile.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is CasinoChips casinoChips)
                {
                    foreach (var chip in casinoChips.ActiveProjectiles)
                    {
                        if (chip.IsActive)
                        {
                            chip.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }
            }
        }

        private void DrawHUD()
        {
            DrawHealthBar(_spriteBatch);
            DrawExperienceBar(_spriteBatch);

            int screenWidth = GraphicsDevice.Viewport.Width;

            // Отрисовка УРОВНЯ (слева вверху)
            _spriteBatch.DrawString(_font, $"УРОВЕНЬ: {_player.Level}", new Vector2(10, 35), Color.White);

            // Отрисовка ТАЙМЕРА (по центру вверху)
            int minutes = (int)(_survivalTime / 60);
            int seconds = (int)(_survivalTime % 60);
            string timeString = $"{minutes:00}:{seconds:00}";

            Vector2 timeSize = _font.MeasureString(timeString);
            Vector2 timePosition = new Vector2(
                (screenWidth - timeSize.X) / 2,
                10
            );

            _spriteBatch.DrawString(_font, timeString, timePosition, Color.Yellow);

            // Отрисовка СЧЕТЧИКА КИЛЛОВ (справа вверху)
            string killString = $"КИЛЛЫ: {_killCount}";
            Vector2 killSize = _font.MeasureString(killString);
            Vector2 killPosition = new Vector2(
                screenWidth - killSize.X - 10,
                10
            );

            _spriteBatch.DrawString(_font, killString, killPosition, Color.Red);
        }

        private void DrawExperienceBar(SpriteBatch spriteBatch)
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            const int BarHeight = 10;
            const int BarX = 0;
            const int BarY = 720 - BarHeight;

            int barWidth = screenWidth;

            float experienceRatio = (float)_player.CurrentExperience / _player.ExperienceToNextLevel;
            int currentExperienceWidth = (int)(barWidth * experienceRatio);

            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, barWidth, BarHeight), Color.DarkSlateBlue);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, currentExperienceWidth, BarHeight), Color.Purple);

            // Отрисовка текста XP
            string xpText = $"ОПЫТ: {_player.CurrentExperience} / {_player.ExperienceToNextLevel}";
            Vector2 textSize = _font.MeasureString(xpText);

            Vector2 textPosition = new Vector2(
                (screenWidth - textSize.X) / 2,
                BarY - textSize.Y - 5
            );

            spriteBatch.DrawString(_font, xpText, textPosition, Color.White);
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            const int BarWidth = 200;
            const int BarHeight = 20;
            const int BarX = 10;
            const int BarY = 10;

            float healthRatio = (float)_player.CurrentHealth / _player.MaxHealth;
            int currentHealthWidth = (int)(BarWidth * healthRatio);

            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, BarWidth, BarHeight), Color.DarkGray);
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, currentHealthWidth, BarHeight),
                Color.Lerp(Color.Red, Color.LimeGreen, healthRatio)
            );

            // Рамка
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY + BarHeight - 1, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, 1, BarHeight), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX + BarWidth - 1, BarY, 1, BarHeight), Color.White);

            // Отрисовка текста HP
            string healthText = $"HP: {_player.CurrentHealth}/{_player.MaxHealth}";
            Vector2 textSize = _font.MeasureString(healthText);

            Vector2 textPosition = new Vector2(
                BarX + (BarWidth - textSize.X) / 2,
                BarY + (BarHeight - textSize.Y) / 2
            );

            spriteBatch.DrawString(_font, healthText, textPosition, Color.White);
        }

        private void DrawGameOverScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.DarkRed * 0.8f
            );

            string text = "ИГРА ОКОНЧЕНА";
            Vector2 size = _font.MeasureString(text);
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width - size.X) / 2,
                (GraphicsDevice.Viewport.Height - size.Y) / 2
            );
            spriteBatch.DrawString(_font, text, position, Color.White);
        }

        private void DrawLevelUpPendingScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.Black * 0.6f
            );
        }

        private Rectangle GetBounds(object obj)
        {
            const int Size = 24;
            Vector2 position = Vector2.Zero;

            if (obj is Player p)
            {
                position = p.Position;
            }
            else if (obj is Enemy e)
            {
                position = e.Position;
            }
            else
            {
                return Rectangle.Empty;
            }

            return new Rectangle(
                (int)(position.X - Size),
                (int)(position.Y - Size),
                Size * 2,
                Size * 2
            );
        }
    }
}