using Microsoft.Xna.Framework;
using Survive_the_night.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using Math = System.Math; // Добавлено для Math.Max, если вы решите его использовать
using Game1 = Survive_the_night.Game1; // Добавлено для доступа к Game1.Random

namespace Survive_the_night.Managers
{
    public class SpawnManager
    {
        private List<Enemy> _enemies;
        private Player _player;

        // --- Поля для ОБЫЧНЫХ КРАСНЫХ ВРАГОВ ---
        private float _initialSpawnCooldown = 1.0f;
        private float _currentSpawnCooldown;
        private float _spawnTimer = 0f;

        // Масштабирование сложности
        private float _gameTimeElapsed = 0f;
        private float _difficultyMultiplier = 1.0f;
        private const float DifficultyInterval = 60f; // Увеличение каждые 60 секунд
        private const float DifficultyIncrease = 1.0f; // +100% частоты в минуту

        // --- Поля для ЭЛИТНЫХ СИНИХ ВРАГОВ ---
        private float _eliteSpawnTimer = 0f;
        private const float ELITE_SPAWN_COOLDOWN = 60f; // Например, 1 элитный враг в минуту

        // ------------------------------------

        public SpawnManager(List<Enemy> enemies, Player player)
        {
            _enemies = enemies;
            _player = player;
            _currentSpawnCooldown = _initialSpawnCooldown;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // --- 1. ЛОГИКА МАСШТАБИРОВАНИЯ СЛОЖНОСТИ (КРАСНЫЕ ВРАГИ) ---
            _gameTimeElapsed += deltaTime;
            if (_gameTimeElapsed >= DifficultyInterval)
            {
                // Увеличиваем множитель
                _difficultyMultiplier += DifficultyIncrease;
                _gameTimeElapsed -= DifficultyInterval;
            }

            // Расчет текущего кулдауна
            _currentSpawnCooldown = _initialSpawnCooldown / _difficultyMultiplier;
            _spawnTimer -= deltaTime;

            if (_spawnTimer <= 0f)
            {
                // Создание нового КРАСНОГО врага
                Vector2 spawnPos = GetSpawnPosition(_player);
                // Предполагается, что конструктор Enemy(Vector2, Player) существует
                Enemy newEnemy = new Enemy(spawnPos, _player);
                _enemies.Add(newEnemy);

                // Сброс таймера
                _spawnTimer = _currentSpawnCooldown;
            }

            // --- 2. ИСПРАВЛЕННАЯ ЛОГИКА СПАВНА ЭЛИТНОГО СИНЕГО ВРАГА (РАЗ В МИНУТУ) ---

            // !!! ИСПРАВЛЕНО: Прибавляем время к таймеру !!!
            _eliteSpawnTimer += deltaTime;

            // !!! ИСПРАВЛЕНО: Сравниваем с кулдауном !!!
            if (_eliteSpawnTimer >= ELITE_SPAWN_COOLDOWN)
            {
                // Используем GetSpawnPosition, так как GetRandomSpawnPosition у вас не определен
                Vector2 spawnPosition = GetSpawnPosition(_player);

                // 2. Создание EliteEnemy
                // Предполагается, что конструктор EliteEnemy(Vector2) существует
                _enemies.Add(new EliteEnemy(spawnPosition, _player));

                Debug.WriteLine("ЭЛИТНЫЙ ВРАГ ПОЯВИЛСЯ!");

                // Сброс таймера на 0 или вычитание кулдауна
                _eliteSpawnTimer = 0f;

                // Опционально: уменьшить кулдаун для следующего элитника
                // ELITE_SPAWN_COOLDOWN = Math.Max(30f, ELITE_SPAWN_COOLDOWN - 5f);
            }
        }

        // --- МЕТОД: Генерация позиции за пределами зоны игрока ---
        private Vector2 GetSpawnPosition(Player player)
        {
            const float MinSpawnDistance = 600f;
            const float MaxSpawnDistance = 900f;

            float spawnDistance = (float)Game1.Random.NextDouble() * (MaxSpawnDistance - MinSpawnDistance) + MinSpawnDistance;
            float angle = (float)Game1.Random.NextDouble() * MathHelper.TwoPi;

            Vector2 offset = new Vector2(
                (float)System.Math.Cos(angle) * spawnDistance,
                (float)System.Math.Sin(angle) * spawnDistance
            );

            Vector2 spawnPosition = player.Position + offset;

            // Проверка на выход за границы мира 
            spawnPosition.X = MathHelper.Clamp(spawnPosition.X, 0, Game1.WorldSize.X);
            spawnPosition.Y = MathHelper.Clamp(spawnPosition.Y, 0, Game1.WorldSize.Y);

            return spawnPosition;
        }

        // !!! ДОБАВЛЕНО: Определение GetRandomSpawnPosition, как было в вашем Debug.WriteLine
        // Но используем GetSpawnPosition, чтобы не дублировать код
        private Vector2 GetRandomSpawnPosition()
        {
            return GetSpawnPosition(_player);
        }
    }
}