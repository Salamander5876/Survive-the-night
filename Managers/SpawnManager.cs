using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game1 = Survive_the_night.Game1;

namespace Survive_the_night.Managers
{
    public class SpawnManager
    {
        private List<Enemy> _enemies;
        private Player _player;
        private GameBoundaries _boundaries;
        private Camera _camera; // ДОБАВЛЕНО: поле для камеры

        // --- ЛИМИТЫ ВРАГОВ ---
        private const int MAX_REGULAR_ENEMIES = 100;
        private const int MAX_ELITE_ENEMIES = 1;
        private const int MAX_TOTAL_ENEMIES_ON_SCREEN = 150;

        // --- ТАЙМЕРЫ И КУЛДАУНЫ ---
        private float _gameTimeTotal = 0f;

        // Обычные враги
        private float _regularSpawnTimer = 0f;
        private float _currentRegularSpawnCooldown = 2.0f;
        private float _minSpawnCooldown = 0.3f;

        // Элитные враги
        private float _eliteSpawnTimer = 0f;
        private const float ELITE_SPAWN_INTERVAL = 300f; // 5 минут

        // Прогрессия сложности
        private int _difficultyWave = 0;
        private const float DIFFICULTY_INTERVAL = 30f; // Увеличиваем сложность каждые 30 секунд
        private float _difficultyTimer = 0f;

        // Статистика
        private int _totalEnemiesSpawned = 0;
        private int _totalElitesSpawned = 0;

        public SpawnManager(List<Enemy> enemies, Player player, Camera camera, Viewport viewport)
        {
            _enemies = enemies;
            _player = player;
            _camera = camera; // ДОБАВЛЕНО: сохраняем камеру
            _boundaries = new GameBoundaries(camera, viewport);

            Debug.WriteLine("✅ SpawnManager инициализирован с системой границ");
        }

        public void Update(GameTime gameTime)
        {
            if (_player == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _gameTimeTotal += deltaTime;
            _difficultyTimer += deltaTime;

            // Обновляем границы каждый кадр
            _boundaries.UpdateBounds();

            // --- ПРОГРЕССИЯ СЛОЖНОСТИ ---
            if (_difficultyTimer >= DIFFICULTY_INTERVAL)
            {
                IncreaseDifficulty();
                _difficultyTimer = 0f;
            }

            // --- СПАВН ОБЫЧНЫХ ВРАГОВ ---
            _regularSpawnTimer += deltaTime;
            if (_regularSpawnTimer >= _currentRegularSpawnCooldown)
            {
                if (CanSpawnRegularEnemy())
                {
                    SpawnRegularEnemy();
                }
                _regularSpawnTimer = 0f;
            }

            // --- СПАВН ЭЛИТНЫХ ВРАГОВ ---
            _eliteSpawnTimer += deltaTime;
            if (_eliteSpawnTimer >= ELITE_SPAWN_INTERVAL)
            {
                if (CanSpawnEliteEnemy())
                {
                    SpawnEliteEnemy();
                }
                _eliteSpawnTimer = 0f;
            }

            // Очистка мертвых врагов (опционально, для производительности)
            CleanDeadEnemies();
        }

        private void IncreaseDifficulty()
        {
            _difficultyWave++;

            // Уменьшаем кулдаун спавна (но не ниже минимума)
            float newCooldown = _currentRegularSpawnCooldown * 0.9f;
            _currentRegularSpawnCooldown = MathHelper.Max(newCooldown, _minSpawnCooldown);

            Debug.WriteLine($"📈 Волна сложности {_difficultyWave}. Кулдаун спавна: {_currentRegularSpawnCooldown:F2}с");
        }

        private bool CanSpawnRegularEnemy()
        {
            // Проверяем лимиты
            int regularCount = _enemies.Count(e => e.IsAlive && !(e is EliteEnemy));
            int totalCount = _enemies.Count(e => e.IsAlive);

            return regularCount < MAX_REGULAR_ENEMIES && totalCount < MAX_TOTAL_ENEMIES_ON_SCREEN;
        }

        private bool CanSpawnEliteEnemy()
        {
            // Проверяем лимиты и время появления (только каждые 5 минут)
            int eliteCount = _enemies.Count(e => e.IsAlive && e is EliteEnemy);
            int totalCount = _enemies.Count(e => e.IsAlive);

            return eliteCount < MAX_ELITE_ENEMIES &&
                   totalCount < MAX_TOTAL_ENEMIES_ON_SCREEN &&
                   _gameTimeTotal >= (_totalElitesSpawned + 1) * ELITE_SPAWN_INTERVAL;
        }

        private void SpawnRegularEnemy()
        {
            Vector2 spawnPos = CalculateSpawnPosition();
            Enemy newEnemy = new Enemy(spawnPos, _player);
            _enemies.Add(newEnemy);
            _totalEnemiesSpawned++;

            Debug.WriteLine($"🎯 Обычный враг #{_totalEnemiesSpawned} создан. Всего живых: {_enemies.Count(e => e.IsAlive)}");
        }

        private void SpawnEliteEnemy()
        {
            Vector2 spawnPos = CalculateSpawnPosition();
            _enemies.Add(new EliteEnemy(spawnPos, _player));
            _totalElitesSpawned++;

            int minutes = (int)(_gameTimeTotal / 60);
            int seconds = (int)(_gameTimeTotal % 60);
            Debug.WriteLine($"👑 ЭЛИТНЫЙ ВРАГ #{_totalElitesSpawned} создан в {minutes:00}:{seconds:00}");
        }

        private Vector2 CalculateSpawnPosition()
        {
            // Спавним ЗА ПРЕДЕЛАМИ видимой области, но в зоне спавна
            Vector2 spawnPosition;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                // Выбираем случайную сторону для спавна
                int side = Game1.Random.Next(0, 4);
                spawnPosition = GetPositionOnSide(side);
                attempts++;

            } while (_boundaries.IsInsideScreen(spawnPosition) && attempts < maxAttempts);

            // Если не удалось найти позицию вне экрана, используем последнюю найденную
            return spawnPosition;
        }

        private Vector2 GetPositionOnSide(int side)
        {
            float margin = 50f; // Отступ от границы спавна
            Vector2 position = Vector2.Zero;

            switch (side)
            {
                case 0: // Сверху
                    position = new Vector2(
                        Game1.Random.Next((int)(_boundaries.SpawnLeft + margin), (int)(_boundaries.SpawnRight - margin)),
                        _boundaries.SpawnTop + margin
                    );
                    break;
                case 1: // Справа
                    position = new Vector2(
                        _boundaries.SpawnRight - margin,
                        Game1.Random.Next((int)(_boundaries.SpawnTop + margin), (int)(_boundaries.SpawnBottom - margin))
                    );
                    break;
                case 2: // Снизу
                    position = new Vector2(
                        Game1.Random.Next((int)(_boundaries.SpawnLeft + margin), (int)(_boundaries.SpawnRight - margin)),
                        _boundaries.SpawnBottom - margin
                    );
                    break;
                case 3: // Слева
                    position = new Vector2(
                        _boundaries.SpawnLeft + margin,
                        Game1.Random.Next((int)(_boundaries.SpawnTop + margin), (int)(_boundaries.SpawnBottom - margin))
                    );
                    break;
            }

            return position;
        }

        private void CleanDeadEnemies()
        {
            // Удаляем мертвых врагов для оптимизации (раз в секунду)
            if (_gameTimeTotal % 1f < 0.016f) // Примерно раз в секунду
            {
                int removed = _enemies.RemoveAll(e => !e.IsAlive);
                if (removed > 0)
                {
                    Debug.WriteLine($"🧹 Удалено {removed} мертвых врагов. Осталось: {_enemies.Count(e => e.IsAlive)}");
                }
            }
        }

        public void UpdateViewport(Viewport viewport)
        {
            // ИСПРАВЛЕНО: используем сохраненную камеру
            _boundaries = new GameBoundaries(_camera, viewport);
        }

        // Методы для получения статистики (можно использовать в HUD)
        public int GetAliveRegularCount() => _enemies.Count(e => e.IsAlive && !(e is EliteEnemy));
        public int GetAliveEliteCount() => _enemies.Count(e => e.IsAlive && e is EliteEnemy);
        public int GetTotalAliveCount() => _enemies.Count(e => e.IsAlive);
        public float GetNextEliteSpawnTime() => MathHelper.Max(0, (_totalElitesSpawned + 1) * ELITE_SPAWN_INTERVAL - _gameTimeTotal);
    }
}