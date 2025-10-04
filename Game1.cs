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

        // ЗАКОММЕНТИРОВАНО: Убираем шрифт, чтобы избежать ContentLoadException
        // private SpriteFont _font;

        private SpawnManager _spawnManager;
        private Camera _camera;

        private LevelUpMenu _levelUpMenu;
        private bool _isGameOver = false;

        private List<Enemy> _enemies = new List<Enemy>();

        private PlayingCards _playingCardsWeapon;
        private List<Weapon> _weapons = new List<Weapon>(); // ИСПРАВЛЕНИЕ: List<Weapon>

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

            _playingCardsWeapon = new PlayingCards(_player);
            _weapons.Add(_playingCardsWeapon);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            // ЗАКОММЕНТИРОВАНО: Удалили загрузку шрифта
            // _font = Content.Load<SpriteFont>("DefaultFont");

            // ИЗМЕНЕНИЕ: Убрали передачу шрифта в конструктор LevelUpMenu
            _levelUpMenu = new LevelUpMenu(_player, _playingCardsWeapon, GraphicsDevice, _debugTexture);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (!_player.IsAlive && !_isGameOver)
            {
                _isGameOver = true;
            }

            if (_isGameOver || _player.IsLevelUpPending)
            {
                _levelUpMenu.Update(gameTime);

                if (_isGameOver)
                {
                    return;
                }
                else
                {
                    return;
                }
            }

            _player.Update(gameTime);
            _camera.Follow();
            _spawnManager.Update(gameTime);

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                if (enemy.IsAlive)
                {
                    enemy.Update(gameTime);
                    if (enemy.GetBounds().Intersects(_player.GetBounds()))
                    {
                        _player.TakeDamage(10);
                    }
                }
                else
                {
                    _experienceOrbs.Add(new ExperienceOrb(enemy.Position, 1));
                    _enemies.RemoveAt(i);
                }
            }

            foreach (var weapon in _weapons)
            {
                weapon.Update(gameTime);
                weapon.Attack(gameTime, _enemies);
            }

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

            System.Diagnostics.Debug.WriteLine($"Уровень: {_player.Level}, Опыт: {_player.CurrentExperience}/{_player.ExperienceToNextLevel}");

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(transformMatrix: _camera.Transform);

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

            foreach (var orb in _experienceOrbs)
            {
                if (orb.IsActive)
                {
                    orb.Draw(_spriteBatch, _debugTexture, orb.Color);
                }
            }

            _spriteBatch.End();

            // ************************************************************

            _spriteBatch.Begin();

            DrawHealthBar(_spriteBatch);
            DrawExperienceBar(_spriteBatch);

            if (_player.IsLevelUpPending)
            {
                DrawLevelUpPendingScreen(_spriteBatch);
                // ИЗМЕНЕНИЕ: Вызываем Draw без аргумента font
                _levelUpMenu.Draw(_spriteBatch);
            }

            if (_isGameOver)
            {
                DrawGameOverScreen(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // ... (DrawExperienceBar, DrawHealthBar, DrawGameOverScreen, DrawLevelUpPendingScreen остаются прежними) ...

        private void DrawExperienceBar(SpriteBatch spriteBatch)
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            const int BarHeight = 10;
            const int BarX = 0;
            const int BarY = 720 - BarHeight;

            int barWidth = screenWidth;

            float experienceRatio = (float)_player.CurrentExperience / _player.ExperienceToNextLevel;
            int currentExperienceWidth = (int)(barWidth * experienceRatio);

            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, barWidth, BarHeight),
                Color.DarkSlateBlue
            );

            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, currentExperienceWidth, BarHeight),
                Color.Purple
            );
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            const int BarWidth = 200;
            const int BarHeight = 20;
            const int BarX = 10;
            const int BarY = 10;

            float healthRatio = (float)_player.CurrentHealth / _player.MaxHealth;
            int currentHealthWidth = (int)(BarWidth * healthRatio);

            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, BarWidth, BarHeight),
                Color.DarkGray
            );

            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, currentHealthWidth, BarHeight),
                Color.Lerp(Color.Red, Color.LimeGreen, healthRatio)
            );

            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY + BarHeight - 1, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, 1, BarHeight), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX + BarWidth - 1, BarY, 1, BarHeight), Color.White);
        }

        private void DrawGameOverScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.DarkRed * 0.8f
            );
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