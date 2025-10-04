using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics; // НОВОЕ: Для Viewport
using Survive_the_night.Entities;
using System;
using System.Collections.Generic;

// Удаляем 'using SharpDX;'
// Используем 'using Microsoft.Xna.Framework;'

namespace Survive_the_night.Managers
{
    // Класс, отвечающий за логику спауна врагов
    public class SpawnManager
    {
        private Player _player;
        private List<Enemy> _enemies;
        // ИСПРАВЛЕНО: Явно указываем тип Viewport из MonoGame
        private Viewport _viewport;

        private Random _random = new Random();

        private float _spawnTimer = 0f;
        private const float BaseSpawnInterval = 1.0f;

        // Зона за пределами экрана, где могут появляться враги
        private const float SpawnDistance = 100f;

        // Конструктор: ИСПРАВЛЕНО - явно указываем тип Viewport в аргументах
        public SpawnManager(Player player, List<Enemy> enemies, Viewport viewport)
        {
            _player = player;
            _enemies = enemies;
            _viewport = viewport;
        }

        // Главный метод обновления
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _spawnTimer -= deltaTime;

            if (_spawnTimer <= 0f)
            {
                SpawnEnemy();
                _spawnTimer = BaseSpawnInterval;
            }
        }

        private void SpawnEnemy()
        {
            // 1. Определяем случайное место спауна за экраном
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // 2. Создаем нового врага и добавляем его в список
            Enemy newEnemy = new Enemy(spawnPosition, _player);
            _enemies.Add(newEnemy);
        }

        private Vector2 GetRandomSpawnPosition()
        {
            float screenWidth = _viewport.Width;
            float screenHeight = _viewport.Height;

            int side = _random.Next(4);

            float x = 0;
            float y = 0;

            // Расстояние от края экрана. _player.Size теперь доступен (public get)
            float offset = _player.Size + SpawnDistance;

            switch (side)
            {
                case 0: // Сверху
                    // ИСПРАВЛЕНО: Теперь NextFloat должен быть доступен
                    x = _random.NextFloat(-offset, screenWidth + offset);
                    y = -offset;
                    break;
                case 1: // Снизу
                    x = _random.NextFloat(-offset, screenWidth + offset);
                    y = screenHeight + offset;
                    break;
                case 2: // Слева
                    x = -offset;
                    y = _random.NextFloat(-offset, screenHeight + offset);
                    break;
                case 3: // Справа
                    x = screenWidth + offset;
                    y = _random.NextFloat(-offset, screenHeight + offset);
                    break;
            }

            return new Vector2(x, y);
        }
    }
}