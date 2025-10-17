using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;

namespace Survive_the_night.Weapons
{
    public class GoldenBullet : Weapon
    {
        public int NumBullets { get; private set; } = 1;
        public float ProjectileSpeed { get; private set; } = 500f;
        public List<Projectile> ActiveProjectiles { get; private set; } = new List<Projectile>();

        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int SpeedLevel { get; private set; } = 0;

        private const float ShotIntervalSeconds = 0.2f;
        private const float BurstCooldownSeconds = 1.0f;
        private bool _isBurstActive = false;
        private int _shotsFiredInBurst = 0;
        private float _nextShotTimer = 0f;
        private float _burstCooldownTimer = 0f;

        public GoldenBullet(Player player) : base(player, WeaponType.Regular, WeaponName.GoldenBullet, 1.5f, 1)
        {
        }

        public override void LevelUp() { }

        public void UpgradeCount()
        {
            if (CountLevel >= 10) return;
            NumBullets += 1;
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
            ProjectileSpeed += 50f;
            SpeedLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var bullet = ActiveProjectiles[i];
                if (bullet.IsActive)
                {
                    bullet.Update(gameTime);
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

                while (_nextShotTimer <= 0f && _shotsFiredInBurst < NumBullets)
                {
                    Enemy target = FindClosestEnemyForBullets(enemies);
                    if (target != null)
                    {
                        Vector2 offset = new Vector2(
                            (float)Game1.Random.NextDouble() * 10 - 5,
                            (float)Game1.Random.NextDouble() * 10 - 5
                        );

                        var bullet = new GoldenBulletProjectile(
                            Player.Position + offset,
                            12,
                            Color.Gold,
                            this.Damage,
                            this.ProjectileSpeed,
                            target.Position,
                            WeaponManager.GetRandomWeaponTexture(WeaponName.GoldenBullet)
                        );
                        ActiveProjectiles.Add(bullet);

                        WeaponManager.GetWeaponSound(WeaponName.GoldenBullet)?.Play();
                    }

                    _shotsFiredInBurst++;
                    _nextShotTimer += ShotIntervalSeconds;
                }

                if (_shotsFiredInBurst >= NumBullets)
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
                Projectile bullet = ActiveProjectiles[j];
                if (!bullet.IsActive) continue;

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = enemies[i];
                    if (!enemy.IsAlive) continue;

                    if (bullet.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(bullet.Damage);
                        bullet.IsActive = false;
                        break;
                    }
                }
            }
        }

        private Enemy FindClosestEnemyForBullets(List<Enemy> enemies)
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