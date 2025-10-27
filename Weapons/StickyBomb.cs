using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;

namespace Survive_the_night.Weapons
{
    public class StickyBomb : Weapon
    {
        public int NumBombs { get; private set; } = 1;
        public float ExplosionTime { get; private set; } = 15f; // ”ћ≈Ќ№Ў≈Ќќ с 60 до 15 секунд
        public List<StickyBombProjectile> ActiveBombs { get; private set; } = new List<StickyBombProjectile>();
        public float ProjectileSpeed { get; private set; } = 200f;

        private float _throwCooldown = 0.2f;
        private float _throwTimer = 0f;
        private bool _isThrowing = false;
        private int _bombsThrownInCycle = 0;

        public int DamageLevel { get; private set; } = 0;
        public int CountLevel { get; private set; } = 0;
        public int ExplosionTimeLevel { get; private set; } = 0;

        private List<Enemy> _enemiesWithBombs = new List<Enemy>();

        // 
        private static SoundEffect _throwSound;
        private static SoundEffect _explosionSound;

        public StickyBomb(Player player) : base(player, WeaponType.Regular, WeaponName.StickyBomb, 0f, 5)
        {
        }

        public static void SetSounds(SoundEffect throwSound, SoundEffect explosionSound)
        {
            _throwSound = throwSound;
            _explosionSound = explosionSound;
        }

        public override void LevelUp() { }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 10) return;
            Damage += 3; // ”¬≈Ћ»„≈Ќќ с +1 до +3 за уровень
            DamageLevel++;
        }

        public void UpgradeCount()
        {
            if (CountLevel >= 10) return;
            NumBombs += 1;
            CountLevel++;
        }

        public void UpgradeExplosionTime()
        {
            if (ExplosionTimeLevel >= 10) return;
            ExplosionTime -= 1f; // ”ћ≈Ќ№Ў≈Ќќ с -5с до -1с за уровень
            ExplosionTimeLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_throwTimer > 0f)
            {
                _throwTimer -= deltaTime;
            }

            bool needNewBomb = false;
            for (int i = ActiveBombs.Count - 1; i >= 0; i--)
            {
                var bomb = ActiveBombs[i];
                if (bomb.IsActive)
                {
                    bomb.Update(gameTime);
                }
                else
                {
                    if (!bomb.HasExploded && bomb.StuckEnemy != null && !bomb.StuckEnemy.IsAlive)
                    {
                        needNewBomb = true;
                    }

                    if (bomb.StuckEnemy != null && _enemiesWithBombs.Contains(bomb.StuckEnemy))
                    {
                        _enemiesWithBombs.Remove(bomb.StuckEnemy);
                    }
                    ActiveBombs.RemoveAt(i);
                }
            }

            if (needNewBomb && _throwTimer <= 0f)
            {
                CreateNewBombForNewTarget();
            }

            if (!_isThrowing && ActiveBombs.Count == 0 && _throwTimer <= 0f)
            {
                _isThrowing = true;
                _bombsThrownInCycle = 0;
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (!_isThrowing) return;

            Enemy target = FindEnemyWithoutBomb(enemies);
            if (target != null && _throwTimer <= 0f && _bombsThrownInCycle < NumBombs)
            {
                CreateBombForTarget(target);
                _bombsThrownInCycle++;
                _throwTimer = _throwCooldown;

                if (_bombsThrownInCycle >= NumBombs)
                {
                    _isThrowing = false;
                }
            }
        }

        private void CreateBombForTarget(Enemy target)
        {
            // –азмер передаетс€, но будет автоматически переопределен в конструкторе бомбы
            var bomb = new StickyBombProjectile(
                Player.Position,
                20, // Ётот размер будет переопределен автоматически
                Color.White,
                this.Damage, // ”рон теперь 5 (базовый) и +3 за уровень
                this.ProjectileSpeed,
                target,
                this.ExplosionTime, // “еперь 15 секунд вместо 60
                _explosionSound
            );

            ActiveBombs.Add(bomb);
            _enemiesWithBombs.Add(target);

            _throwSound?.Play();
        }

        private void CreateNewBombForNewTarget()
        {
            Enemy newTarget = FindEnemyWithoutBomb(Game1.CurrentEnemies);
            if (newTarget != null)
            {
                CreateBombForTarget(newTarget);
            }
        }

        private Enemy FindEnemyWithoutBomb(List<Enemy> enemies)
        {
            if (enemies == null) return null;

            Enemy closestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive || _enemiesWithBombs.Contains(enemy)) continue;

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