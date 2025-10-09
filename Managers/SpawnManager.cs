using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using Game1 = Survive_the_night.Game1;

namespace Survive_the_night.Managers
{
    public class SpawnManager
    {
        private List<Enemy> _enemies;
        private Player _player;
        private Viewport _viewport;

        // --- Поля для ОБЫЧНЫХ ВРАГОВ ---
        private float _initialSpawnCooldown = 2.0f;
        private float _currentSpawnCooldown;
        private float _spawnTimer = 0f;

        // Масштабирование сложности
        private float _gameTimeElapsed = 0f;
        private float _difficultyMultiplier = 1.0f;
        private const float DifficultyInterval = 60f;
        private const float DifficultyIncrease = 0.5f;

        // --- Поля для ЭЛИТНЫХ ВРАГОВ ---
        private float _eliteSpawnTimer = 0f;
        private const float ELITE_SPAWN_COOLDOWN = 45f;

        public SpawnManager(List<Enemy> enemies, Player player)
        {
            _enemies = enemies;
            _player = player;
            _currentSpawnCooldown = _initialSpawnCooldown;
            _viewport = new Viewport(0, 0, 1280, 720); // Значения по умолчанию
        }

        public void SetViewport(Viewport viewport)
        {
            _viewport = viewport;
            Debug.WriteLine($"✅ Viewport установлен: {_viewport.Width}x{_viewport.Height}");
        }

        public void Update(GameTime gameTime)
        {
            if (_player == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // --- ЛОГИКА МАСШТАБИРОВАНИЯ СЛОЖНОСТИ ---
            _gameTimeElapsed += deltaTime;
            if (_gameTimeElapsed >= DifficultyInterval)
            {
                _difficultyMultiplier += DifficultyIncrease;
                _gameTimeElapsed = 0f;
                Debug.WriteLine($"📈 Сложность увеличена: множитель {_difficultyMultiplier:F1}");
            }

            // Расчет текущего кулдауна
            _currentSpawnCooldown = MathHelper.Clamp(_initialSpawnCooldown / _difficultyMultiplier, 0.3f, 5f);
            _spawnTimer -= deltaTime;

            if (_spawnTimer <= 0f)
            {
                SpawnEnemy();
                _spawnTimer = _currentSpawnCooldown;
            }

            // --- ЛОГИКА СПАВНА ЭЛИТНОГО ВРАГА ---
            _eliteSpawnTimer += deltaTime;
            if (_eliteSpawnTimer >= ELITE_SPAWN_COOLDOWN)
            {
                SpawnEliteEnemy();
                _eliteSpawnTimer = 0f;
            }
        }

        private void SpawnEnemy()
        {
            Vector2 spawnPos = CalculateSpawnPosition();
            Enemy newEnemy = new Enemy(spawnPos, _player);
            _enemies.Add(newEnemy);

            Debug.WriteLine($"🎯 Обычный враг создан на позиции: {spawnPos}");
            Debug.WriteLine($"   Игрок: {_player.Position}, Расстояние: {Vector2.Distance(_player.Position, spawnPos):F0}");
        }

        private void SpawnEliteEnemy()
        {
            Vector2 spawnPos = CalculateSpawnPosition();
            _enemies.Add(new EliteEnemy(spawnPos, _player));
            Debug.WriteLine($"👑 ЭЛИТНЫЙ ВРАГ создан на позиции: {spawnPos}");
        }

        private Vector2 CalculateSpawnPosition()
        {
            // Получаем текущие границы экрана относительно игрока
            Vector2 screenCenter = _player.Position;
            float screenLeft = screenCenter.X - _viewport.Width / 2;
            float screenRight = screenCenter.X + _viewport.Width / 2;
            float screenTop = screenCenter.Y - _viewport.Height / 2;
            float screenBottom = screenCenter.Y + _viewport.Height / 2;

            // Отступ от края экрана для спавна
            float spawnMargin = 150f;

            // Выбираем случайную сторону для спавна
            int side = Game1.Random.Next(0, 4);
            Vector2 spawnPosition = Vector2.Zero;

            switch (side)
            {
                case 0: // Сверху
                    spawnPosition = new Vector2(
                        Game1.Random.Next((int)(screenLeft + 50), (int)(screenRight - 50)),
                        screenTop - spawnMargin
                    );
                    break;
                case 1: // Справа
                    spawnPosition = new Vector2(
                        screenRight + spawnMargin,
                        Game1.Random.Next((int)(screenTop + 50), (int)(screenBottom - 50))
                    );
                    break;
                case 2: // Снизу
                    spawnPosition = new Vector2(
                        Game1.Random.Next((int)(screenLeft + 50), (int)(screenRight - 50)),
                        screenBottom + spawnMargin
                    );
                    break;
                case 3: // Слева
                    spawnPosition = new Vector2(
                        screenLeft - spawnMargin,
                        Game1.Random.Next((int)(screenTop + 50), (int)(screenBottom - 50))
                    );
                    break;
            }

            // Ограничиваем спавн в пределах мира
            spawnPosition.X = MathHelper.Clamp(spawnPosition.X, 100, Game1.WorldSize.X - 100);
            spawnPosition.Y = MathHelper.Clamp(spawnPosition.Y, 100, Game1.WorldSize.Y - 100);

            return spawnPosition;
        }

        // Старые методы для обратной совместимости
        private Vector2 GetSpawnPositionAroundScreen()
        {
            return CalculateSpawnPosition();
        }

        private Vector2 GetSpawnPosition(Player player)
        {
            return CalculateSpawnPosition();
        }

        private Vector2 GetRandomSpawnPosition()
        {
            return CalculateSpawnPosition();
        }
    }
}