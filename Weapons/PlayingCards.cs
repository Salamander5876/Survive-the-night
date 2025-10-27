using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;

namespace Survive_the_night.Weapons
{
    public class PlayingCards : Weapon
    {
        public int NumCards { get; private set; } = 1;
        public float Range { get; private set; } = 0.20f;
        public List<Projectile> ActiveProjectiles { get; private set; } = new List<Projectile>();
        public float ProjectileSpeed { get; private set; } = 300f; // УВЕЛИЧЕНО с 250 до 300

        private float _baseCooldown = 1.5f;
        public float CurrentCooldown => _baseCooldown - (ReloadSpeedLevel * 0.1f);

        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int ReloadSpeedLevel { get; private set; } = 0;

        private const float ShotIntervalSeconds = 0.1f;
        private float BurstCooldownSeconds => CurrentCooldown;
        private bool _isBurstActive = false;
        private int _shotsFiredInBurst = 0;
        private float _nextShotTimer = 0f;
        private float _burstCooldownTimer = 0f;

        private Dictionary<Projectile, List<Enemy>> _hitEnemies = new Dictionary<Projectile, List<Enemy>>();

        public PlayingCards(Player player) : base(player, WeaponType.Regular, WeaponName.PlayingCards, 1.5f, 2) // УРОН УВЕЛИЧЕН с 1 до 2
        {
        }

        public override void LevelUp() { }

        public void UpgradeCount()
        {
            if (CountLevel >= 10) return;
            NumCards += 1;
            CountLevel++;
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 10) return;
            Damage += 2; // УВЕЛИЧЕНО с +1 до +2 за уровень
            DamageLevel++;
        }

        public void UpgradeReloadSpeed()
        {
            if (ReloadSpeedLevel >= 10) return;
            ReloadSpeedLevel++;
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

            if (_burstCooldownTimer > 0f)
            {
                _burstCooldownTimer -= deltaTime;
            }

            if (_burstCooldownTimer <= 0f && !_isBurstActive)
            {
                _isBurstActive = true;
                _shotsFiredInBurst = 0;
                _nextShotTimer = 0f;
            }

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

                        var card = new PlayingCard(
                            Player.Position + offset,
                            0, // размер определится автоматически из текстуры
                            Color.White,
                            this.Damage, // Теперь урон 2 (базовый)
                            this.ProjectileSpeed, // Теперь скорость 300
                            target.Position,
                            3,
                            WeaponManager.GetRandomWeaponTexture(WeaponName.PlayingCards)
                        );
                        ActiveProjectiles.Add(card);

                        _hitEnemies[card] = new List<Enemy>();

                        WeaponManager.GetWeaponSound(WeaponName.PlayingCards)?.Play();
                    }

                    _shotsFiredInBurst++;
                    _nextShotTimer += ShotIntervalSeconds;
                }

                if (_shotsFiredInBurst >= NumCards)
                {
                    _isBurstActive = false;
                    _burstCooldownTimer = BurstCooldownSeconds;
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

                List<Enemy> alreadyHit = _hitEnemies.ContainsKey(projectile) ? _hitEnemies[projectile] : new List<Enemy>();

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = enemies[i];
                    if (!enemy.IsAlive) continue;

                    if (alreadyHit.Contains(enemy)) continue;

                    if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(projectile.Damage);

                        alreadyHit.Add(enemy);
                        _hitEnemies[projectile] = alreadyHit;

                        projectile.HitsLeft--;

                        if (projectile.HitsLeft <= 0)
                        {
                            projectile.IsActive = false;
                            break;
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
    }
}