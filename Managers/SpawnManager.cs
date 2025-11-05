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
        private Camera _camera;
        private LevelManager _levelManager;
        private DifficultyManager _difficultyManager;

        private const int MAX_REGULAR_ENEMIES = 100;
        private const int MAX_ELITE_ENEMIES = 1;
        private const int MAX_TOTAL_ENEMIES_ON_SCREEN = 150;

        private float _gameTimeTotal = 0f;
        private float _regularSpawnTimer = 0f;
        private float _currentRegularSpawnCooldown = 2.0f;
        private float _minSpawnCooldown = 0.3f;

        private float _eliteSpawnTimer = 0f;
        private const float ELITE_SPAWN_INTERVAL = 300f;

        private int _difficultyWave = 0;
        private const float DIFFICULTY_INTERVAL = 30f;
        private float _difficultyTimer = 0f;

        private int _totalEnemiesSpawned = 0;
        private int _totalElitesSpawned = 0;

        public SpawnManager(List<Enemy> enemies, Player player, Camera camera, Viewport viewport, LevelManager levelManager, DifficultyManager difficultyManager)
        {
            _enemies = enemies;
            _player = player;
            _camera = camera;
            _levelManager = levelManager;
            _difficultyManager = difficultyManager;
            _boundaries = new GameBoundaries(camera, viewport);

            if (_levelManager == null)
            {
                Debug.WriteLine("CRITICAL: LevelManager is NULL in SpawnManager constructor!");
            }
            else
            {
                Debug.WriteLine($"SpawnManager инициализирован с LevelManager. Текущий уровень: {_levelManager.CurrentLevel}");
            }
        }

        public void SetLevelManager(LevelManager levelManager)
        {
            _levelManager = levelManager;
            Debug.WriteLine($"LevelManager установлен в SpawnManager. Уровень: {_levelManager?.CurrentLevel}");
        }

        public void Update(GameTime gameTime)
        {
            if (_player == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _gameTimeTotal += deltaTime;
            _difficultyTimer += deltaTime;

            _boundaries.UpdateBounds();

            if (_difficultyTimer >= DIFFICULTY_INTERVAL)
            {
                IncreaseDifficulty();
                _difficultyTimer = 0f;
            }

            _regularSpawnTimer += deltaTime * GetSpawnRateMultiplier();
            if (_regularSpawnTimer >= _currentRegularSpawnCooldown)
            {
                if (CanSpawnRegularEnemy())
                {
                    SpawnRegularEnemy();
                }
                _regularSpawnTimer = 0f;
            }

            _eliteSpawnTimer += deltaTime;
            if (_eliteSpawnTimer >= ELITE_SPAWN_INTERVAL)
            {
                if (CanSpawnEliteEnemy())
                {
                    SpawnEliteEnemy();
                    _eliteSpawnTimer = 0f;
                }
            }

            CleanDeadEnemies();
        }

        private float GetSpawnRateMultiplier()
        {
            return _difficultyManager?.SpawnRateMultiplier ?? 1.0f;
        }

        private void IncreaseDifficulty()
        {
            _difficultyWave++;

            float newCooldown = _currentRegularSpawnCooldown * 0.9f;
            _currentRegularSpawnCooldown = MathHelper.Max(newCooldown, _minSpawnCooldown);

            Debug.WriteLine($"Волна сложности {_difficultyWave}. Кулдаун спавна: {_currentRegularSpawnCooldown:F2}с");
        }

        private bool CanSpawnRegularEnemy()
        {
            int regularCount = _enemies.Count(e => e.IsAlive && !(e is EliteEnemy));
            int totalCount = _enemies.Count(e => e.IsAlive);

            return regularCount < MAX_REGULAR_ENEMIES && totalCount < MAX_TOTAL_ENEMIES_ON_SCREEN;
        }

        private bool CanSpawnEliteEnemy()
        {
            int eliteCount = _enemies.Count(e => e.IsAlive && e is EliteEnemy);
            int totalCount = _enemies.Count(e => e.IsAlive);

            float nextSpawnTime = (_totalElitesSpawned + 1) * ELITE_SPAWN_INTERVAL;

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
            if (_levelManager == null)
            {
                Debug.WriteLine("LevelManager is null! Using default BasicEnemy");
                return new BasicEnemy(position, _player, 1, _difficultyManager);
            }

            int currentStage = _levelManager.CurrentLevel;
            float randomValue = (float)Game1.Random.NextDouble();

            Debug.WriteLine($"Создание врага на этапе {currentStage}. Random: {randomValue:F2}");

            switch (currentStage)
            {
                case 1:
                    return new BasicEnemy(position, _player, currentStage, _difficultyManager);

                case 2:
                    if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   -> BasicEnemy (70%)");
                        return new BasicEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else
                    {
                        Debug.WriteLine("   -> TankEnemy (30%)");
                        return new TankEnemy(position, _player, currentStage, _difficultyManager);
                    }

                case 3:
                    if (randomValue < 0.5f)
                    {
                        Debug.WriteLine("   -> BasicEnemy (50%)");
                        return new BasicEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.75f)
                    {
                        Debug.WriteLine("   -> TankEnemy (25%)");
                        return new TankEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else
                    {
                        Debug.WriteLine("   -> FastEnemy (25%)");
                        return new FastEnemy(position, _player, currentStage, _difficultyManager);
                    }

                case 4:
                    if (randomValue < 0.4f)
                    {
                        Debug.WriteLine("   -> BasicEnemy (40%)");
                        return new BasicEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.6f)
                    {
                        Debug.WriteLine("   -> TankEnemy (20%)");
                        return new TankEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.8f)
                    {
                        Debug.WriteLine("   -> FastEnemy (20%)");
                        return new FastEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else
                    {
                        Debug.WriteLine("   -> StrongEnemy (20%)");
                        return new StrongEnemy(position, _player, currentStage, _difficultyManager);
                    }

                case 5:
                    if (randomValue < 0.3f)
                    {
                        Debug.WriteLine("   -> BasicEnemy (30%)");
                        return new BasicEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.5f)
                    {
                        Debug.WriteLine("   -> TankEnemy (20%)");
                        return new TankEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   -> FastEnemy (20%)");
                        return new FastEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.85f)
                    {
                        Debug.WriteLine("   -> StrongEnemy (15%)");
                        return new StrongEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else
                    {
                        Debug.WriteLine("   -> VampireEnemy (15%)");
                        return new VampireEnemy(position, _player, currentStage, _difficultyManager);
                    }

                case 6:
                    if (randomValue < 0.25f)
                    {
                        Debug.WriteLine("   -> BasicEnemy (25%)");
                        return new BasicEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.4f)
                    {
                        Debug.WriteLine("   -> TankEnemy (15%)");
                        return new TankEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.55f)
                    {
                        Debug.WriteLine("   -> FastEnemy (15%)");
                        return new FastEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   -> StrongEnemy (15%)");
                        return new StrongEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.85f)
                    {
                        Debug.WriteLine("   -> VampireEnemy (15%)");
                        return new VampireEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else
                    {
                        Debug.WriteLine("   -> RangedEnemy (15%)");
                        return new RangedEnemy(position, _player, currentStage, _difficultyManager);
                    }

                case 7:
                case 8:
                    if (randomValue < 0.2f)
                    {
                        Debug.WriteLine("   -> BasicEnemy (20%)");
                        return new BasicEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.3f)
                    {
                        Debug.WriteLine("   -> TankEnemy (10%)");
                        return new TankEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.4f)
                    {
                        Debug.WriteLine("   -> FastEnemy (10%)");
                        return new FastEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.5f)
                    {
                        Debug.WriteLine("   -> StrongEnemy (10%)");
                        return new StrongEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.6f)
                    {
                        Debug.WriteLine("   -> VampireEnemy (10%)");
                        return new VampireEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else if (randomValue < 0.7f)
                    {
                        Debug.WriteLine("   -> RangedEnemy (10%)");
                        return new RangedEnemy(position, _player, currentStage, _difficultyManager);
                    }
                    else
                    {
                        Debug.WriteLine("   -> UndyingEnemy (30%)");
                        return new UndyingEnemy(position, _player, currentStage, _difficultyManager);
                    }

                default:
                    Debug.WriteLine("   -> BasicEnemy (default)");
                    return new BasicEnemy(position, _player, currentStage, _difficultyManager);
            }
        }

        public void SpawnEliteEnemy()
        {
            Vector2 spawnPos = CalculateSpawnPosition();
            int stage = _levelManager?.CurrentLevel ?? 1;

            EliteEnemy.EliteType type;
            if (_levelManager?.ShouldSpawnBothEliteTypes() == false)
            {
                type = EliteEnemy.EliteType.Type1;
            }
            else
            {
                type = (_totalElitesSpawned % 2 == 0) ? EliteEnemy.EliteType.Type1 : EliteEnemy.EliteType.Type2;
            }

            var eliteEnemy = new EliteEnemy(spawnPos, _player, stage, type, _difficultyManager);
            _enemies.Add(eliteEnemy);
            _totalElitesSpawned++;

            int minutes = (int)(_gameTimeTotal / 60);
            int seconds = (int)(_gameTimeTotal % 60);
            Debug.WriteLine($"ЭЛИТНЫЙ ВРАГ #{_totalElitesSpawned} ({type}) создан в {minutes:00}:{seconds:00}. HP: {eliteEnemy.MaxHealth}");
        }

        private Vector2 CalculateSpawnPosition()
        {
            Vector2 spawnPosition;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                int side = Game1.Random.Next(0, 4);
                spawnPosition = GetPositionOnSide(side);
                attempts++;

            } while (_boundaries.IsInsideScreen(spawnPosition) && attempts < maxAttempts);

            return spawnPosition;
        }

        private Vector2 GetPositionOnSide(int side)
        {
            float margin = 50f;
            Vector2 position = Vector2.Zero;

            switch (side)
            {
                case 0:
                    position = new Vector2(
                        Game1.Random.Next((int)(_boundaries.SpawnLeft + margin), (int)(_boundaries.SpawnRight - margin)),
                        _boundaries.SpawnTop + margin
                    );
                    break;
                case 1:
                    position = new Vector2(
                        _boundaries.SpawnRight - margin,
                        Game1.Random.Next((int)(_boundaries.SpawnTop + margin), (int)(_boundaries.SpawnBottom - margin))
                    );
                    break;
                case 2:
                    position = new Vector2(
                        Game1.Random.Next((int)(_boundaries.SpawnLeft + margin), (int)(_boundaries.SpawnRight - margin)),
                        _boundaries.SpawnBottom - margin
                    );
                    break;
                case 3:
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
            if (_gameTimeTotal % 1f < 0.016f)
            {
                int removed = _enemies.RemoveAll(e => !e.IsAlive);
                if (removed > 0)
                {
                    Debug.WriteLine($"Удалено {removed} мертвых врагов. Осталось: {_enemies.Count(e => e.IsAlive)}");
                }
            }
        }

        private Vector2 GetNearbyPosition(Vector2 basePosition)
        {
            return basePosition + new Vector2(60, 60);
        }

        public void UpdateViewport(Viewport viewport)
        {
            _boundaries = new GameBoundaries(_camera, viewport);
        }

        public int GetAliveRegularCount() => _enemies.Count(e => e.IsAlive && !(e is EliteEnemy));
        public int GetAliveEliteCount() => _enemies.Count(e => e.IsAlive && e is EliteEnemy);
        public int GetTotalAliveCount() => _enemies.Count(e => e.IsAlive);
        public float GetNextEliteSpawnTime() => MathHelper.Max(0, (_totalElitesSpawned + 1) * ELITE_SPAWN_INTERVAL - _gameTimeTotal);
    }
}