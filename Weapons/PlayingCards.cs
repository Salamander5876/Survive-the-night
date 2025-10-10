using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;

namespace Survive_the_night.Weapons
{
    public class PlayingCards : Weapon
    {
        public const string WeaponName = "Игровые карты";
        public int NumCards { get; private set; } = 1;
        public float Range { get; private set; } = 0.20f;
        public List<Projectile> ActiveProjectiles { get; private set; } = new List<Projectile>();
        public float ProjectileSpeed { get; private set; } = 250f;

        // НОВОЕ: Базовая перезарядка и улучшение перезарядки
        private float _baseCooldown = 1.5f;
        public float CurrentCooldown => _baseCooldown - (ReloadSpeedLevel * 0.1f);

        // Список текстур для случайного выбора
        private static List<Texture2D> _cardTextures = new List<Texture2D>();

        // Отдельные уровни прокачек
        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int ReloadSpeedLevel { get; private set; } = 0; // ИЗМЕНЕНО: Была SpeedLevel, теперь ReloadSpeedLevel

        // Параметры очереди выстрелов (используем CurrentCooldown вместо константы)
        private const float ShotIntervalSeconds = 0.1f;
        private float BurstCooldownSeconds => CurrentCooldown; // ИСПОЛЬЗУЕМ ТЕКУЩУЮ ПЕРЕЗАРЯДКУ
        private bool _isBurstActive = false;
        private int _shotsFiredInBurst = 0;
        private float _nextShotTimer = 0f;
        private float _burstCooldownTimer = 0f;

        // Список врагов, которые уже были поражены каждой картой
        private Dictionary<Projectile, List<Enemy>> _hitEnemies = new Dictionary<Projectile, List<Enemy>>();

        public PlayingCards(Player player) : base(player, 1.5f, 1) // Начальная перезарядка 1.5 секунды
        {
        }

        // Метод для добавления текстур карт
        public static void AddCardTexture(Texture2D texture)
        {
            if (texture != null && !_cardTextures.Contains(texture))
            {
                _cardTextures.Add(texture);
            }
        }

        // Метод для получения случайной текстуры карты
        public static Texture2D GetRandomCardTexture()
        {
            if (_cardTextures.Count == 0)
                return null;

            return _cardTextures[Game1.Random.Next(0, _cardTextures.Count)];
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

        // ИЗМЕНЕНО: Улучшение скорости перезарядки вместо скорости полета
        public void UpgradeReloadSpeed()
        {
            if (ReloadSpeedLevel >= 10) return;
            ReloadSpeedLevel++;
            // Перезарядка автоматически уменьшается через свойство CurrentCooldown
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
                    // Удаляем карту из словаря попаданий
                    if (_hitEnemies.ContainsKey(card))
                    {
                        _hitEnemies.Remove(card);
                    }
                    ActiveProjectiles.RemoveAt(i);
                }
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер перерыва между очередями (используем CurrentCooldown)
            if (_burstCooldownTimer > 0f)
            {
                _burstCooldownTimer -= deltaTime;
            }

            // Запуск новой очереди выстрелов, когда перерыв завершён
            if (_burstCooldownTimer <= 0f && !_isBurstActive)
            {
                _isBurstActive = true;
                _shotsFiredInBurst = 0;
                _nextShotTimer = 0f;
            }

            // Если очередь активна - выпускаем карты с интервалом
            if (_isBurstActive)
            {
                _nextShotTimer -= deltaTime;

                while (_nextShotTimer <= 0f && _shotsFiredInBurst < NumCards)
                {
                    Enemy target = FindClosestEnemyForCards(enemies);
                    if (target != null)
                    {
                        Vector2 offset = new Vector2(
                            (float)Game1.Random.NextDouble() * 10 - 5,
                            (float)Game1.Random.NextDouble() * 10 - 5
                        );

                        // Создаем карту с пробитием (всегда 3 врага) и случайной текстурой
                        var card = new PlayingCard(
                            Player.Position + offset,
                            16,
                            Color.White,
                            this.Damage,
                            this.ProjectileSpeed,
                            target.Position,
                            3, // Всегда пробивает 3 врагов
                            GetRandomCardTexture() // Случайная текстура
                        );
                        ActiveProjectiles.Add(card);

                        // Инициализируем список пораженных врагов для этой карты
                        _hitEnemies[card] = new List<Enemy>();

                        Game1.SFXCardDeal?.Play();
                    }

                    _shotsFiredInBurst++;
                    _nextShotTimer += ShotIntervalSeconds;
                }

                if (_shotsFiredInBurst >= NumCards)
                {
                    _isBurstActive = false;
                    _burstCooldownTimer = BurstCooldownSeconds; // Используем текущую перезарядку
                }
            }

            CheckProjectileCollisions(enemies);
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            for (int j = ActiveProjectiles.Count - 1; j >= 0; j--)
            {
                Projectile projectile = ActiveProjectiles[j];
                if (!projectile.IsActive) continue;

                // Получаем список врагов, которых уже поразила эта карта
                List<Enemy> alreadyHit = _hitEnemies.ContainsKey(projectile) ? _hitEnemies[projectile] : new List<Enemy>();

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = enemies[i];
                    if (!enemy.IsAlive) continue;

                    // Проверяем, не поражали ли уже этого врага
                    if (alreadyHit.Contains(enemy)) continue;

                    if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(projectile.Damage);

                        // Добавляем врага в список пораженных
                        alreadyHit.Add(enemy);
                        _hitEnemies[projectile] = alreadyHit;

                        // Логика пробивания - уменьшаем счетчик попаданий
                        projectile.HitsLeft--;

                        // Если достигли 0 попаданий - деактивируем
                        if (projectile.HitsLeft <= 0)
                        {
                            projectile.IsActive = false;
                            break; // Выходим из цикла по врагам для этой карты
                        }
                    }
                }
            }
        }

        private Enemy FindClosestEnemyForCards(List<Enemy> enemies)
        {
            float minDistance = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distance = Vector2.Distance(Player.Position, enemy.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        protected Enemy FindClosestEnemyInLimitedRange(List<Enemy> enemies, float maxRange)
        {
            float maxRangeSquared = maxRange * maxRange;
            float minDistanceSquared = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(Player.Position, enemy.Position);

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