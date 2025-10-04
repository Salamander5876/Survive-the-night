using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System; // Для Random

using Survive_the_night.Entities;
using Survive_the_night.Weapons;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;

namespace Survive_the_night
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Game State Management
        private GameState _currentGameState;
        private MainMenu _mainMenu;

        // Game World Entities
        private Player _player;
        private SpawnManager _spawnManager;
        private Camera _camera;
        private List<Enemy> _enemies = new List<Enemy>();
        private List<ExperienceOrb> _experienceOrbs = new List<ExperienceOrb>();
        // НОВОЕ ПОЛЕ: Список для хилок
        private List<HealthOrb> _healthOrbs = new List<HealthOrb>();
        // Для логики выпадения
        private System.Random _random = new System.Random();

        // HUD Data
        private float _survivalTime = 0f;
        private int _killCount = 0; // Счетчик убитых врагов

        // Weapons and Upgrades
        private PlayingCards _playingCardsWeapon;
        private List<Weapon> _weapons = new List<Weapon>();
        private LevelUpMenu _levelUpMenu;

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
            _currentGameState = GameState.MainMenu;

            Vector2 initialPlayerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
            );
            _player = new Player(initialPlayerPosition);

            _spawnManager = new SpawnManager(_player, _enemies, GraphicsDevice.Viewport);
            _camera = new Camera(_player, GraphicsDevice.Viewport);

            _playingCardsWeapon = new PlayingCards(_player);
            _weapons.Add(_playingCardsWeapon);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            _font = Content.Load<SpriteFont>("Fonts/Default");

            _mainMenu = new MainMenu(GraphicsDevice, _debugTexture, _font);
            _levelUpMenu = new LevelUpMenu(_player, _playingCardsWeapon, GraphicsDevice, _debugTexture, _font);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (_currentGameState)
            {
                case GameState.MainMenu:
                    _currentGameState = _mainMenu.Update(gameTime);
                    break;

                case GameState.Playing:
                    _survivalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // --- ПРОВЕРКИ СОСТОЯНИЯ ---
                    if (!_player.IsAlive)
                    {
                        _currentGameState = GameState.GameOver;
                        return;
                    }
                    if (_player.IsLevelUpPending)
                    {
                        _currentGameState = GameState.LevelUp;
                        return;
                    }

                    // --- ИГРОВАЯ ЛОГИКА ---
                    _player.Update(gameTime);
                    _camera.Follow();
                    _spawnManager.Update(gameTime);

                    // Обновление врагов
                    for (int i = _enemies.Count - 1; i >= 0; i--)
                    {
                        var enemy = _enemies[i];
                        if (enemy.IsAlive)
                        {
                            enemy.Update(gameTime);
                            // Проверка столкновения враг-игрок
                            if (enemy.GetBounds().Intersects(_player.GetBounds()))
                            {
                                _player.TakeDamage(10);
                            }
                        }
                        else
                        {
                            _killCount++;
                            _experienceOrbs.Add(new ExperienceOrb(enemy.Position, 1));

                            // НОВАЯ ЛОГИКА: Выпадение хилки с вероятностью 15%
                            if (_random.NextDouble() < 0.15)
                            {
                                // 0.20f = 20% от максимального здоровья
                                _healthOrbs.Add(new HealthOrb(enemy.Position, 0.20f));
                            }

                            _enemies.RemoveAt(i);
                        }
                    }

                    // Обновление оружия
                    foreach (var weapon in _weapons)
                    {
                        weapon.Update(gameTime);
                        weapon.Attack(gameTime, _enemies);
                    }

                    // Обновление орбов опыта
                    for (int i = _experienceOrbs.Count - 1; i >= 0; i--)
                    {
                        var orb = _experienceOrbs[i];
                        if (orb.IsActive)
                        {
                            orb.Update(gameTime, _player);
                        }

                        if (!orb.IsActive)
                        {
                            _player.GainExperience(orb.Value);
                            _experienceOrbs.RemoveAt(i);
                        }
                    }

                    // НОВАЯ ЛОГИКА: Обновление и сбор хилок
                    for (int i = _healthOrbs.Count - 1; i >= 0; i--)
                    {
                        var orb = _healthOrbs[i];
                        if (orb.IsActive)
                        {
                            orb.Update(gameTime, _player);
                        }

                        if (!orb.IsActive)
                        {
                            _healthOrbs.RemoveAt(i);
                        }
                    }

                    Debug.WriteLine($"Уровень: {_player.Level}, Опыт: {_player.CurrentExperience}/{_player.ExperienceToNextLevel}");
                    break;

                case GameState.LevelUp:
                    _levelUpMenu.Update(gameTime);
                    if (!_player.IsLevelUpPending)
                    {
                        _currentGameState = GameState.Playing;
                    }
                    break;

                case GameState.GameOver:
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
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    enemy.Draw(_spriteBatch, _debugTexture);
                }
            }

            Color playerTint = Color.White;
            if (_player.IsInvulnerable)
            {
                playerTint = Color.White * 0.5f;
            }
            _player.Draw(_spriteBatch, _debugTexture, playerTint);

            foreach (var weapon in _weapons)
            {
                PlayingCards cards = weapon as PlayingCards;

                if (cards != null)
                {
                    foreach (var card in cards.ActiveProjectiles)
                    {
                        if (card.IsActive)
                        {
                            card.Draw(_spriteBatch, _debugTexture);
                        }
                    }
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

            // НОВОЕ: Отрисовка хилок
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
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.Black * 0.6f
            );
        }
    }
}