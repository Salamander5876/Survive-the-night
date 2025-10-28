using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Entities.Enemies.Regular;
using Survive_the_night.Entities.Enemies.Elite;
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

        // В SpawnManager добавьте поле:
        private LevelManager _levelManager;

        public SpawnManager(List<Enemy> enemies, Player player, Camera camera, Viewport viewport, LevelManager levelManager)
        {
            _enemies = enemies;
            _player = player;
            _camera = camera;
            _levelManager = levelManager;
            _boundaries = new GameBoundaries(camera, viewport);
            _levelManager.EliteKilled();

            // Проверка условия победы
            if (_levelManager.CurrentLevel == 8 && _levelManager.ElitesKilled >= 2)
            {
                // Активируем экран победы
                // Это будет обработано в Game1.Update()
            }

            // Отладочная информация
            if (_levelManager == null)
            {
                Debug.WriteLine("❌❌❌ CRITICAL: LevelManager is NULL in SpawnManager constructor!");
            }
            else
            {
                Debug.WriteLine($"✅ SpawnManager инициализирован с LevelManager. Текущий уровень: {_levelManager.CurrentLevel}");
            }
        }

        // В SpawnManager добавьте метод:
        public void SetLevelManager(LevelManager levelManager)
        {
            _levelManager = levelManager;
            Debug.WriteLine($"🔄 LevelManager установлен в SpawnManager. Уровень: {_levelManager?.CurrentLevel}");
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
            // Проверяем лимиты
            int eliteCount = _enemies.Count(e => e.IsAlive && e is EliteEnemy);
            int totalCount = _enemies.Count(e => e.IsAlive);

            // Спавним по одному элитному врагу каждые 5 минут
            float nextSpawnTime = (_totalElitesSpawned + 1) * 300f; // 300 секунд = 5 минут

            bool canSpawn = eliteCount < MAX_ELITE_ENEMIES &&
                   totalCount < MAX_TOTAL_ENEMIES_ON_SCREEN &&
                   _gameTimeTotal >= nextSpawnTime;

            if (canSpawn)
            {
                Debug.WriteLine($"Можно спавнить элитного врага! Время: {_gameTimeTotal}, след. спавн: {nextSpawnTime}");
            }

            return canSpawn;
        }

        private void SpawnRegularEnemy()
        {
            Vector2 spawnPos = CalculateSpawnPosition();
            Enemy newEnemy = CreateRegularEnemyForStage(spawnPos);
            _enemies.Add(newEnemy);
            _totalEnemiesSpawned++;

            Debug.WriteLine($"{newEnemy.GetType().Name} создан на этапе {newEnemy.CurrentStage}. HP: {newEnemy.MaxHealth}");
        }

        private Enemy CreateRegularEnemyForStage(Vector2 position)
        {
            // Защита от null
            if (_levelManager == null)
            {
                Debug.WriteLine("❌ LevelManager is null! Using default BasicEnemy");
                return new BasicEnemy(position, _player, 1);
            }

            int currentStage = _levelManager.CurrentLevel;
            float randomValue = (float)Game1.Random.NextDouble();

            Debug.WriteLine($"🔄 Создание врага на этапе {currentStage}. Random: {randomValue:F2}");

            switch (currentStage)
            {
                case 1:
                    // Только BasicEnemy на 1 этапе
                    return new BasicEnemy(position, _player, currentStage);

                case 2:
                    // 70% BasicEnemy, 30% TankEnemy
                    if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   → BasicEnemy (70%)");
                        return new BasicEnemy(position, _player, currentStage);
                    }
                    else
                    {
                        Debug.WriteLine("   → TankEnemy (30%)");
                        return new TankEnemy(position, _player, currentStage);
                    }

                case 3:
                    // 50% BasicEnemy, 25% TankEnemy, 25% FastEnemy
                    if (randomValue < 0.5f)
                    {
                        Debug.WriteLine("   → BasicEnemy (50%)");
                        return new BasicEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.75f)
                    {
                        Debug.WriteLine("   → TankEnemy (25%)");
                        return new TankEnemy(position, _player, currentStage);
                    }
                    else
                    {
                        Debug.WriteLine("   → FastEnemy (25%)");
                        return new FastEnemy(position, _player, currentStage);
                    }

                case 4:
                    // 40% BasicEnemy, 20% TankEnemy, 20% FastEnemy, 20% StrongEnemy
                    if (randomValue < 0.4f)
                    {
                        Debug.WriteLine("   → BasicEnemy (40%)");
                        return new BasicEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.6f)
                    {
                        Debug.WriteLine("   → TankEnemy (20%)");
                        return new TankEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.8f)
                    {
                        Debug.WriteLine("   → FastEnemy (20%)");
                        return new FastEnemy(position, _player, currentStage);
                    }
                    else
                    {
                        Debug.WriteLine("   → StrongEnemy (20%)");
                        return new StrongEnemy(position, _player, currentStage);
                    }

                case 5:
                    // 30% BasicEnemy, 20% TankEnemy, 20% FastEnemy, 15% StrongEnemy, 15% VampireEnemy
                    if (randomValue < 0.3f)
                    {
                        Debug.WriteLine("   → BasicEnemy (30%)");
                        return new BasicEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.5f)
                    {
                        Debug.WriteLine("   → TankEnemy (20%)");
                        return new TankEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   → FastEnemy (20%)");
                        return new FastEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.85f)
                    {
                        Debug.WriteLine("   → StrongEnemy (15%)");
                        return new StrongEnemy(position, _player, currentStage);
                    }
                    else
                    {
                        Debug.WriteLine("   → VampireEnemy (15%)");
                        return new VampireEnemy(position, _player, currentStage);
                    }

                case 6:
                    // 25% BasicEnemy, 15% TankEnemy, 15% FastEnemy, 15% StrongEnemy, 15% VampireEnemy, 15% RangedEnemy
                    if (randomValue < 0.25f)
                    {
                        Debug.WriteLine("   → BasicEnemy (25%)");
                        return new BasicEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.4f)
                    {
                        Debug.WriteLine("   → TankEnemy (15%)");
                        return new TankEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.55f)
                    {
                        Debug.WriteLine("   → FastEnemy (15%)");
                        return new FastEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   → StrongEnemy (15%)");
                        return new StrongEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.85f)
                    {
                        Debug.WriteLine("   → VampireEnemy (15%)");
                        return new VampireEnemy(position, _player, currentStage);
                    }
                    else
                    {
                        Debug.WriteLine("   → RangedEnemy (15%)");
                        return new RangedEnemy(position, _player, currentStage);
                    }

                case 7:
                case 8:
                    // На высоких уровнях все типы врагов
                    // 20% BasicEnemy, 10% TankEnemy, 10% FastEnemy, 10% StrongEnemy, 10% VampireEnemy, 10% RangedEnemy, 30% UndyingEnemy
                    if (randomValue < 0.2f)
                    {
                        Debug.WriteLine("   → BasicEnemy (20%)");
                        return new BasicEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.3f)
                    {
                        Debug.WriteLine("   → TankEnemy (10%)");
                        return new TankEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.4f)
                    {
                        Debug.WriteLine("   → FastEnemy (10%)");
                        return new FastEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.5f)
                    {
                        Debug.WriteLine("   → StrongEnemy (10%)");
                        return new StrongEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.6f)
                    {
                        Debug.WriteLine("   → VampireEnemy (10%)");
                        return new VampireEnemy(position, _player, currentStage);
                    }
                    else if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   → RangedEnemy (10%)");
                        return new RangedEnemy(position, _player, currentStage);
                    }
                    else
                    {
                        Debug.WriteLine("   → UndyingEnemy (30%)");
                        return new UndyingEnemy(position, _player, currentStage);
                    }

                default:
                    Debug.WriteLine("   → BasicEnemy (default)");
                    return new BasicEnemy(position, _player, currentStage);
            }
        }

        private void SpawnEliteEnemy()
        {
            Vector2 spawnPos = CalculateSpawnPosition();

            // Защита от null
            int stage = _levelManager?.CurrentLevel ?? 1;

            // Определяем какой тип элитного врага спавнить
            // На нечетных спавнах (1, 3, 5...) - Type1, на четных (2, 4, 6...) - Type2
            EliteEnemy.EliteType type = (_totalElitesSpawned % 2 == 0) ?
                EliteEnemy.EliteType.Type1 : EliteEnemy.EliteType.Type2;

            var eliteEnemy = new EliteEnemy(spawnPos, _player, stage, type);
            _enemies.Add(eliteEnemy);
            _totalElitesSpawned++;

            int minutes = (int)(_gameTimeTotal / 60);
            int seconds = (int)(_gameTimeTotal % 60);
            Debug.WriteLine($"👑 ЭЛИТНЫЙ ВРАГ #{_totalElitesSpawned} ({type}) создан в {minutes:00}:{seconds:00}. HP: {eliteEnemy.MaxHealth}");
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

        private Vector2 GetNearbyPosition(Vector2 basePosition)
        {
            // Создаем позицию рядом с базовой
            return basePosition + new Vector2(60, 60); // Смещение на 60 пикселей
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