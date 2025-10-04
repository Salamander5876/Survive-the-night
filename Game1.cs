using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

// Подключаем новые пространства имен для наших папок:
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

        private Player _player;
        private Texture2D _debugTexture;

        private SpriteFont _font;

        private SpawnManager _spawnManager;
        private Camera _camera;

        private LevelUpMenu _levelUpMenu;
        private bool _isGameOver = false;

        private List<Enemy> _enemies = new List<Enemy>();

        // ИСПРАВЛЕНИЕ: Мы хотим List<Weapon>, а не List<List<Weapon>>.
        // ИСПОЛЬЗУЕМ PlayingCards напрямую для передачи в LevelUpMenu
        private PlayingCards _playingCardsWeapon;
        private List<Weapon> _weapons = new List<Weapon>(); // <-- ИСПРАВЛЕНО ЗДЕСЬ

        // Список для хранения активных сфер опыта
        private List<ExperienceOrb> _experienceOrbs = new List<ExperienceOrb>();


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
            Vector2 initialPlayerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
            );
            _player = new Player(initialPlayerPosition);

            _spawnManager = new SpawnManager(_player, _enemies, GraphicsDevice.Viewport);
            _camera = new Camera(_player, GraphicsDevice.Viewport);

            // ИНИЦИАЛИЗАЦИЯ ИНТЕГРИРОВАННОГО ОРУЖИЯ
            _playingCardsWeapon = new PlayingCards(_player);
            _weapons.Add(_playingCardsWeapon); // Добавляем в общий список оружия (теперь тип совпадает)

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            // НОВОЕ: Загрузка шрифта
            _font = Content.Load<SpriteFont>("DefaultFont");

            // НОВОЕ: Инициализация меню улучшений с передачей оружия!
            _levelUpMenu = new LevelUpMenu(_player, _playingCardsWeapon, GraphicsDevice, _debugTexture, _font);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // ЛОГИКА СМЕРТИ/GAME OVER
            if (!_player.IsAlive && !_isGameOver)
            {
                _isGameOver = true;
            }

            // Проверяем флаг LevelUpPending, чтобы остановить игру при повышении уровня
            // Если игра окончена ИЛИ ждем выбора улучшения, ничего не обновляем, КРОМЕ МЕНЮ
            if (_isGameOver || _player.IsLevelUpPending)
            {
                // НОВОЕ: Мы должны обновлять меню, даже если игра на паузе!
                _levelUpMenu.Update(gameTime);

                if (_isGameOver)
                {
                    return;
                }
                else
                {
                    // Если пауза только для выбора улучшения, пропускаем остальное
                    return;
                }
            }

            // Если игра не на паузе, продолжаем обновление:

            // 1. Обновляем логику игрока
            _player.Update(gameTime);

            // Камера следит за игроком
            _camera.Follow();

            // 2. ОБНОВЛЕНИЕ МЕНЕДЖЕРА СПАУНА
            _spawnManager.Update(gameTime);

            // 3. ОБНОВЛЕНИЕ ЛОГИКИ ВРАГОВ
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                if (enemy.IsAlive)
                {
                    enemy.Update(gameTime);

                    // Проверка столкновения врага с игроком
                    if (enemy.GetBounds().Intersects(_player.GetBounds()))
                    {
                        _player.TakeDamage(10);
                    }
                }
                else
                {
                    // Когда враг умирает, создаем сферу опыта
                    _experienceOrbs.Add(new ExperienceOrb(enemy.Position, 1));

                    _enemies.RemoveAt(i); // Удаляем мертвого врага!
                }
            }

            // 4. Обновляем логику оружия (перезарядка и атака)
            // Теперь weapon имеет правильный тип Weapon, и мы можем вызывать его методы
            foreach (var weapon in _weapons)
            {
                weapon.Update(gameTime);
                weapon.Attack(gameTime, _enemies);
            }

            // 5. ОБНОВЛЕНИЕ СФЕР ОПЫТА
            for (int i = _experienceOrbs.Count - 1; i >= 0; i--)
            {
                var orb = _experienceOrbs[i];
                if (orb.IsActive)
                {
                    // Обновляем позицию сферы
                    orb.Update(gameTime, _player);
                }

                // Проверка, была ли сфера собрана (IsActive стало false)
                if (!orb.IsActive)
                {
                    // ИГРОК ПОЛУЧАЕТ ОПЫТ!
                    _player.GainExperience(orb.Value);

                    _experienceOrbs.RemoveAt(i);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Уровень: {_player.Level}, Опыт: {_player.CurrentExperience}/{_player.ExperienceToNextLevel}");

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // ПЕРВЫЙ SpriteBatch: Отрисовка ИГРОВОГО МИРА (с матрицей камеры)
            _spriteBatch.Begin(transformMatrix: _camera.Transform);

            // 1. Отрисовываем врагов
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    enemy.Draw(_spriteBatch, _debugTexture);
                }
            }

            // 2. Отрисовываем игрока
            Color playerTint = Color.White;
            if (_player.IsInvulnerable)
            {
                // Мерцающий эффект
                playerTint = Color.White * 0.5f;
            }
            _player.Draw(_spriteBatch, _debugTexture, playerTint);

            // 3. Отрисовка снарядов
            // Поскольку мы знаем, что _playingCardsWeapon в _weapons, мы можем сделать
            // прямое приведение, или оставить цикл, как есть.
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

            // 4. Отрисовываем сферы опыта
            foreach (var orb in _experienceOrbs)
            {
                if (orb.IsActive)
                {
                    orb.Draw(_spriteBatch, _debugTexture, orb.Color);
                }
            }

            _spriteBatch.End(); // Конец отрисовки игрового мира

            // ************************************************************

            // ВТОРОЙ SpriteBatch: Отрисовка HUD (БЕЗ матрицы, привязано к экрану)
            _spriteBatch.Begin();

            // Отображение HUD
            DrawHealthBar(_spriteBatch);
            DrawExperienceBar(_spriteBatch);

            // НОВОЕ: Отрисовка меню улучшений
            if (_player.IsLevelUpPending)
            {
                // 1. Рисуем затемняющий фон
                DrawLevelUpPendingScreen(_spriteBatch);

                // 2. Рисуем само меню (передаем шрифт для текста)
                if (_font != null)
                {
                    _levelUpMenu.Draw(_spriteBatch, _font);
                }
                else
                {
                    // Если шрифт не загрузился, рисуем хотя бы прямоугольники
                    _levelUpMenu.Draw(_spriteBatch);
                }
            }

            // Экран Game Over
            if (_isGameOver)
            {
                DrawGameOverScreen(_spriteBatch);
            }

            _spriteBatch.End(); // Конец отрисовки HUD

            base.Draw(gameTime);
        }

        // МЕТОД: Отрисовка полоски опыта (XP Bar)
        private void DrawExperienceBar(SpriteBatch spriteBatch)
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            const int BarHeight = 10;
            const int BarX = 0;
            const int BarY = 720 - BarHeight;

            int barWidth = screenWidth;

            float experienceRatio = (float)_player.CurrentExperience / _player.ExperienceToNextLevel;
            int currentExperienceWidth = (int)(barWidth * experienceRatio);

            // 1. Отрисовываем фон (темно-фиолетовый/синий прямоугольник)
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, barWidth, BarHeight),
                Color.DarkSlateBlue
            );

            // 2. Отрисовываем текущий опыт (яркий фиолетовый/желтый)
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, currentExperienceWidth, BarHeight),
                Color.Purple
            );
        }

        // МЕТОД: Отрисовка полоски здоровья (HP Bar)
        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            const int BarWidth = 200;
            const int BarHeight = 20;
            const int BarX = 10;
            const int BarY = 10;

            float healthRatio = (float)_player.CurrentHealth / _player.MaxHealth;
            int currentHealthWidth = (int)(BarWidth * healthRatio);

            // 1. Отрисовываем фон (черный прямоугольник)
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, BarWidth, BarHeight),
                Color.DarkGray
            );

            // 2. Отрисовываем текущее здоровье (цвет меняется от красного до зеленого)
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, currentHealthWidth, BarHeight),
                Color.Lerp(Color.Red, Color.LimeGreen, healthRatio)
            );

            // 3. Отрисовываем рамку (опционально)
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY + BarHeight - 1, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, 1, BarHeight), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX + BarWidth - 1, BarY, 1, BarHeight), Color.White);
        }

        // МЕТОД: Отрисовка экрана Game Over
        private void DrawGameOverScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.DarkRed * 0.8f
            );
        }

        // МЕТОД: Временный экран, пока ждем выбора улучшения
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