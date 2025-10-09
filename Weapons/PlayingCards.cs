// Weapons/PlayingCards.cs

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;

namespace Survive_the_night.Weapons
{
    public class PlayingCards : Weapon
    {
        public int NumCards { get; private set; } = 1;
        public bool IsPiercing { get; private set; } = false;
        // !!! НОВОЕ СВОЙСТВО: Дальность атаки !!!
        public float Range { get; private set; } = 0.20f; // Начальная дальность в 300 единиц
        public List<Projectile> ActiveProjectiles { get; private set; } = new List<Projectile>();

        // Базовая скорость полёта пули
        public float ProjectileSpeed { get; private set; } = 500f;

        // Отдельные уровни прокачек (только для отображения и ограничений)
        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int SpeedLevel { get; private set; } = 0;

        // Параметры очереди выстрелов (пуль)
        private const float ShotIntervalSeconds = 0.1f; // задержка между выстрелами в очереди
        private const float BurstCooldownSeconds = 1.0f; // задержка между очередями
        private bool _isBurstActive = false;
        private int _shotsFiredInBurst = 0;
        private float _nextShotTimer = 0f;
        private float _burstCooldownTimer = 0f;

        public PlayingCards(Player player) : base(player, 0.5f, 1)
        {
        }

        public override void LevelUp() { }

        // --- ТРИ ЯВНЫЕ ПРОКАЧКИ ---
        public void UpgradeCount()
        {
            if (CountLevel >= 10) return;
            NumCards += 1;
            CountLevel++;
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 10) return;
            Damage += 1;
            DamageLevel++;
        }

        public void UpgradeSpeed()
        {
            if (SpeedLevel >= 10) return;
            ProjectileSpeed += 50f; // шаг ускорения
            SpeedLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var card = ActiveProjectiles[i];
                if (card.IsActive)
                {
                    card.Update(gameTime);
                }
                else
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер перерыва между очередями
            if (_burstCooldownTimer > 0f)
            {
                _burstCooldownTimer -= deltaTime;
            }

            // Запуск новой очереди выстрелов, когда перерыв завершён
            if (_burstCooldownTimer <= 0f && !_isBurstActive)
            {
                _isBurstActive = true;
                _shotsFiredInBurst = 0;
                _nextShotTimer = 0f; // первый выстрел мгновенно
            }

            // Если очередь активна — выпускаем пули с интервалом 0.2с
            if (_isBurstActive)
            {
                _nextShotTimer -= deltaTime;

                while (_nextShotTimer <= 0f && _shotsFiredInBurst < NumCards)
                {
                    Enemy target = FindClosestEnemy(enemies);
                    if (target != null)
                    {
                        Vector2 offset = new Vector2(
                            (float)Game1.Random.NextDouble() * 10 - 5,
                            (float)Game1.Random.NextDouble() * 10 - 5
                        );
                        var card = new PlayingCard(
                            Player.Position + offset,
                            16,
                            Color.White,
                            this.Damage,
                            this.ProjectileSpeed,
                            target.Position,
                            this.IsPiercing ? int.MaxValue : 1
                        );
                        ActiveProjectiles.Add(card);
                        Game1.SFXGunShooting?.Play();
                    }

                    _shotsFiredInBurst++;
                    _nextShotTimer += ShotIntervalSeconds; // планируем следующий кадр очереди
                }

                if (_shotsFiredInBurst >= NumCards)
                {
                    _isBurstActive = false; // очередь завершена
                    _burstCooldownTimer = BurstCooldownSeconds; // стартуем перерыв между очередями
                }
            }

            CheckProjectileCollisions(enemies);
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                if (!enemy.IsAlive) continue;

                for (int j = ActiveProjectiles.Count - 1; j >= 0; j--)
                {
                    // Предполагаем, что PlayingCard является Projectile
                    Projectile projectile = ActiveProjectiles[j];

                    if (!projectile.IsActive) continue;

                    if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(projectile.Damage);

                        // Логика пробивания (projectile.HitsLeft - 1)
                        if (IsPiercing)
                        {
                            // Если пробивает, projectile.HitsLeft остается > 0
                        }
                        else
                        {
                            projectile.IsActive = false; // Иначе удаляем
                        }

                        if (!projectile.IsActive) break; // Если карта удалена, переходим к следующему врагу
                    }
                }
            }
        }
        // !!! НОВЫЙ МЕТОД: Поиск ближайшего врага с ограничением дальности !!!
        protected Enemy FindClosestEnemyInRange(List<Enemy> enemies, float maxRange)
        {
            float maxRangeSquared = maxRange * maxRange;
            float minDistanceSquared = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(Player.Position, enemy.Position);

                // Если враг ближе, чем максимальная дальность И ближе, чем предыдущий найденный
                if (distanceSquared < maxRangeSquared && distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }
    }
}