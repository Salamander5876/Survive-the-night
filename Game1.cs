using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;

// Убедитесь, что все эти пространства имен существуют
using Survive_the_night.Entities;
using Survive_the_night.Weapons;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;

namespace Survive_the_night
{
    /// <summary>
    /// Перечисление состояний игры. Определено ТОЛЬКО здесь, в корневом пространстве имен.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        LevelUp,
        GameOver,
        Roulette // Состояние для Рулетки-Автомата
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // --- СТАТИЧЕСКИЕ ПОЛЯ ДЛЯ ГЛОБАЛЬНОГО ДОСТУПА ---
        public static System.Random Random { get; private set; } = new System.Random();
        public static Vector2 WorldSize { get; private set; }
        public static List<Enemy> CurrentEnemies { get; private set; }

        /// <summary>
        /// Глобальное статическое поле, которое используется для управления состоянием игры из других классов.
        /// </summary>
        public static GameState CurrentState = GameState.MainMenu;
        // ------------------------------------------------

        // Game State Management
        private GameState _currentGameState; // Локальная переменная для использования в switch
        private MainMenu _mainMenu;

        // Game World Entities
        private Player _player;
        private SpawnManager _spawnManager;
        private Camera _camera;
        private List<Enemy> _enemies = new List<Enemy>();
        private List<ExperienceOrb> _experienceOrbs = new List<ExperienceOrb>();
        private List<HealthOrb> _healthOrbs = new List<HealthOrb>();
        // private System.Random _random = new System.Random(); // Используем статический Game1.Random

        // HUD Data
        private float _survivalTime = 0f;
        private int _killCount = 0;

        // Weapons and Upgrades
        private PlayingCards _playingCardsWeapon;
        private List<Weapon> _weapons = new List<Weapon>();
        private LevelUpMenu _levelUpMenu;
        private RouletteManager _rouletteManager; // МЕНЕДЖЕР РУЛЕТКИ


        // Content
        private Texture2D _debugTexture;
        private SpriteFont _font;

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

            // Предполагается, что эти классы существуют и корректно инициализируются
            _player = new Player(initialPlayerPosition);
            _spawnManager = new SpawnManager(_enemies, _player);
            _camera = new Camera(_player, GraphicsDevice.Viewport);

            // Инициализация оружия
            _playingCardsWeapon = new PlayingCards(_player);

            _weapons.Add(_playingCardsWeapon);
            // !!! ДОБАВЛЕНО: MolotovCocktail для тестирования !!!
            

            WorldSize = new Vector2(3000, 3000);
            CurrentEnemies = _enemies; // Инициализация статического списка врагов

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            // Загрузка шрифта
            _font = Content.Load<SpriteFont>("Fonts/Default");

            // Инициализация менеджеров и меню
            _mainMenu = new MainMenu(GraphicsDevice, _debugTexture, _font);
            _levelUpMenu = new LevelUpMenu(_player, _weapons, GraphicsDevice, _debugTexture, _font);
            _rouletteManager = new RouletteManager(_levelUpMenu); // ИНИЦИАЛИЗАЦИЯ РУЛЕТКИ
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
                    // MainMenu.Update должен возвращать новое GameState
                    Game1.CurrentState = (GameState)_mainMenu.Update(gameTime);
                    break;

                case GameState.Playing:
                    _survivalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // --- ПРОВЕРКИ СОСТОЯНИЯ ---
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

                    // --- ИГРОВАЯ ЛОГИКА ---
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

                            // !!! ДРОП ЭЛИТНОГО ВРАГА: ЗАПУСК РУЛЕТКИ !!!
                            if (enemy is EliteEnemy)
                            {
                                // Дроп 10 орбов опыта
                                for (int j = 0; j < 10; j++) // Предполагаем 10, или используем EliteEnemy.ExperienceOrbCount
                                {
                                    _experienceOrbs.Add(new ExperienceOrb(enemy.Position, 1));
                                }

                                // 2. Запуск Рулетки-Автомата (меняет Game1.CurrentState на Roulette)
                                _rouletteManager.StartRoulette();
                            }
                            else
                            {
                                // Обычный дроп
                                _experienceOrbs.Add(new ExperienceOrb(enemy.Position, 1));
                            }

                            _enemies.RemoveAt(i);
                        }
                    }

                    // --------------------------------------------------------
                    // !!! ИСПРАВЛЕНИЕ: ОБРАБОТКА КОЛЛИЗИЙ И УРОНА ОТ ВРАГОВ !!!
                    if (_player.IsAlive)
                    {
                        var playerBounds = GetBounds(_player);

                        // Находим всех живых врагов, которые пересекаются с игроком
                        var collidingEnemies = _enemies.Where(enemy =>
                            enemy.IsAlive &&
                            GetBounds(enemy).Intersects(playerBounds)
                        ).ToList();

                        if (collidingEnemies.Any())
                        {
                            // Наносим урон игроку, если он не неуязвим (по умолчанию 1 ед. урона)
                            // Предполагается, что Player.IsInvulnerable и Player.TakeDamage(int) существуют.
                            if (!_player.IsInvulnerable)
                            {
                                _player.TakeDamage(1);
                            }
                        }
                    }
                    // --------------------------------------------------------

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

                    for (int i = _healthOrbs.Count - 1; i >= 0; i--)
                    {
                        var orb = _healthOrbs[i];
                        if (orb.IsActive) { orb.Update(gameTime, _player); }
                        if (!orb.IsActive) { _player.Heal(orb.HealAmount * _player.MaxHealth); _healthOrbs.RemoveAt(i); }
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

                case GameState.Roulette: // ОБНОВЛЕНИЕ РУЛЕТКИ
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

                case GameState.Playing:
                case GameState.LevelUp:
                case GameState.GameOver:
                case GameState.Roulette:
                    // --- Отрисовка игрового мира (с камерой) ---
                    _spriteBatch.Begin(transformMatrix: _camera.Transform);
                    DrawWorldObjects();
                    _spriteBatch.End();

                    // --- Отрисовка HUD и UI (без камеры) ---
                    _spriteBatch.Begin();
                    DrawHUD();

                    if (_currentGameState == GameState.LevelUp)
                    {
                        DrawLevelUpPendingScreen(_spriteBatch);
                        _levelUpMenu.Draw(_spriteBatch, _font);
                    }

                    if (_currentGameState == GameState.Roulette) // ОТРИСОВКА РУЛЕТКИ
                    {
                        DrawLevelUpPendingScreen(_spriteBatch); // Затемнение
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
            // Отрисовка врагов
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    enemy.Draw(_spriteBatch, _debugTexture);
                }
            }

            // Отрисовка игрока
            Color playerTint = Color.White;
            if (_player.IsInvulnerable)
            {
                if ((int)(_survivalTime * 10) % 2 == 0)
                {
                    playerTint = Color.Red * 0.5f;
                }
            }
            _player.Draw(_spriteBatch, _debugTexture, playerTint);

            // --- ОТРИСОВКА ОРУЖИЯ ---
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

                // !!! ИСПРАВЛЕНИЕ: ОТРИСОВКА МОЛОТОВА !!!
                else if (weapon is MolotovCocktail molotov)
                {
                    foreach (var area in molotov.ActiveAreas)
                    {
                        if (area.IsActive)
                        {
                            area.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }
                // ---------------------------------
            }
            // ------------------------

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
                    orb.Draw(_spriteBatch, _debugTexture, orb.Color);
                }
            }
        }

        private void DrawHUD()
        {
            DrawHealthBar(_spriteBatch);
            DrawExperienceBar(_spriteBatch);

            int screenWidth = GraphicsDevice.Viewport.Width;

            // 1. Отрисовка УРОВНЯ (слева вверху)
            _spriteBatch.DrawString(_font, $"УРОВЕНЬ: {_player.Level}", new Vector2(10, 35), Color.White);

            // 2. Отрисовка ТАЙМЕРА (по центру вверху)
            int minutes = (int)(_survivalTime / 60);
            int seconds = (int)(_survivalTime % 60);
            string timeString = $"{minutes:00}:{seconds:00}";

            Vector2 timeSize = _font.MeasureString(timeString);
            Vector2 timePosition = new Vector2(
                (screenWidth - timeSize.X) / 2,
                10
            );

            _spriteBatch.DrawString(_font, timeString, timePosition, Color.Yellow);

            // 3. Отрисовка СЧЕТЧИКА КИЛЛОВ (справа вверху)
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
            // Затемняет экран, используется для LevelUp и Roulette
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.Black * 0.6f
            );
        }

        // --- НОВЫЙ ВСПОМОГАТЕЛЬНЫЙ МЕТОД: ПОЛУЧЕНИЕ ГРАНИЦ ДЛЯ КОЛЛИЗИИ ---
        /// <summary>
        /// Возвращает прямоугольные границы для проверки коллизии. 
        /// Предполагается, что Player и Enemy имеют свойство Position и размер 48x48 (радиус 24).
        /// </summary>
        private Rectangle GetBounds(object obj)
        {
            // Размер сущности (радиус)
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
                // Если тип объекта неизвестен, возвращаем пустой прямоугольник
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