using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;

namespace Survive_the_night.Weapons
{
    public class CasinoChips : Weapon
    {
        public int NumChips { get; private set; } = 1;
        public float Range { get; private set; } = 0.20f;
        public List<Projectile> ActiveProjectiles { get; private set; } = new List<Projectile>();
        public float ProjectileSpeed { get; private set; } = 500f;

        private float _baseCooldown = 2.5f;
        public float CurrentCooldown => _baseCooldown - (ReloadSpeedLevel * 0.02f);

        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int ReloadSpeedLevel { get; private set; } = 0;
        public int BounceLevel { get; private set; } = 0;

        private const float ShotIntervalSeconds = 0.15f;
        private float BurstCooldownSeconds => CurrentCooldown;
        private bool _isBurstActive = false;
        private int _shotsFiredInBurst = 0;
        private float _nextShotTimer = 0f;
        private float _burstCooldownTimer = 0f;

        private Dictionary<Projectile, List<Enemy>> _hitEnemies = new Dictionary<Projectile, List<Enemy>>();

        public CasinoChips(Player player) : base(player, WeaponType.Regular, WeaponName.CasinoChips, 2.0f, 1)
        {
        }

        public override void LevelUp() { }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 10) return;
            Damage += 1;
            DamageLevel++;
        }

        public void UpgradeReloadSpeed()
        {
            if (ReloadSpeedLevel >= 10) return;
            ReloadSpeedLevel++;
        }

        public void UpgradeBounceCount()
        {
            if (BounceLevel >= 10) return;
            BounceLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var chip = ActiveProjectiles[i];
                if (chip.IsActive)
                {
                    chip.Update(gameTime);
                }
                else
                {
                    if (_hitEnemies.ContainsKey(chip))
                    {
                        _hitEnemies.Remove(chip);
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

                while (_nextShotTimer <= 0f && _shotsFiredInBurst < NumChips)
                {
                    Enemy target = FindClosestEnemyForChips(enemies);
                    if (target != null)
                    {
                        Vector2 offset = new Vector2(
                            (float)Game1.Random.NextDouble() * 10 - 5,
                            (float)Game1.Random.NextDouble() * 10 - 5
                        );

                        var chip = new CasinoChip(
                            Player.Position + offset,
                            16,
                            Color.White,
                            this.Damage,
                            this.ProjectileSpeed,
                            target.Position,
                            BounceLevel + 1,
                            WeaponManager.GetRandomWeaponTexture(WeaponName.CasinoChips)
                        );
                        ActiveProjectiles.Add(chip);

                        _hitEnemies[chip] = new List<Enemy>();

                        WeaponManager.GetWeaponSound(WeaponName.CasinoChips)?.Play();
                    }

                    _shotsFiredInBurst++;
                    _nextShotTimer += ShotIntervalSeconds;
                }

                if (_shotsFiredInBurst >= NumChips)
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

                if (projectile is CasinoChip chip)
                {
                    List<Enemy> alreadyHit = _hitEnemies.ContainsKey(chip) ? _hitEnemies[chip] : new List<Enemy>();

                    for (int i = enemies.Count - 1; i >= 0; i--)
                    {
                        Enemy enemy = enemies[i];
                        if (!enemy.IsAlive) continue;

                        if (alreadyHit.Contains(enemy)) continue;

                        if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                        {
                            enemy.TakeDamage(projectile.Damage);

                            alreadyHit.Add(enemy);
                            _hitEnemies[chip] = alreadyHit;

                            Enemy nextTarget = FindNextTargetForChip(chip, enemies, alreadyHit);
                            if (nextTarget != null && chip.HitsLeft > 0)
                            {
                                chip.UpdateDirection(nextTarget.Position);
                                chip.HitsLeft--;
                            }
                            else
                            {
                                projectile.IsActive = false;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private Enemy FindNextTargetForChip(CasinoChip chip, List<Enemy> enemies, List<Enemy> alreadyHit)
        {
            float minDistance = float.MaxValue;
            Enemy closestEnemy = null;
            Vector2 chipPosition = chip.Position;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                if (alreadyHit.Contains(enemy)) continue;

                float distance = Vector2.Distance(chipPosition, enemy.Position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        private Enemy FindClosestEnemyForChips(List<Enemy> enemies)
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