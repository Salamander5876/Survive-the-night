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
        public float ExplosionTime { get; private set; } = 10f;
        public List<StickyBombProjectile> ActiveBombs { get; private set; } = new List<StickyBombProjectile>();
        public float ProjectileSpeed { get; private set; } = 400f;

        private float _throwCooldown = 0.2f;
        private float _throwTimer = 0f;
        private bool _isThrowing = false;
        private int _bombsThrownInCycle = 0;
        private bool _waitingForAllBombsToExplode = false;

        public int DamageLevel { get; private set; } = 0;
        public int CountLevel { get; private set; } = 0;
        public int ExplosionTimeLevel { get; private set; } = 0;

        private List<Enemy> _enemiesWithBombs = new List<Enemy>();

        private static SoundEffect _throwSound;
        private static SoundEffect _explosionSound;

        public StickyBomb(Player player) : base(player, WeaponType.Regular, WeaponName.StickyBomb, 0f, 10)
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
            if (DamageLevel >= 5) return;
            Damage += 4;
            DamageLevel++;
        }

        public void UpgradeCount()
        {
            if (CountLevel >= 5) return;
            NumBombs += 1;
            CountLevel++;
        }

        public void UpgradeExplosionTime()
        {
            if (ExplosionTimeLevel >= 5) return;
            ExplosionTime -= 1.5f;
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

            // Обновляем активные бомбы и удаляем неактивные
            for (int i = ActiveBombs.Count - 1; i >= 0; i--)
            {
                var bomb = ActiveBombs[i];
                if (bomb.IsActive)
                {
                    bomb.Update(gameTime);
                }
                else
                {
                    // Удаляем врага из списка, если бомба прилипла к нему
                    if (bomb.StuckEnemy != null && _enemiesWithBombs.Contains(bomb.StuckEnemy))
                    {
                        _enemiesWithBombs.Remove(bomb.StuckEnemy);
                    }
                    ActiveBombs.RemoveAt(i);
                }
            }

            // Проверяем, взорвались ли все бомбы из текущей группы
            if (_waitingForAllBombsToExplode && ActiveBombs.Count == 0)
            {
                _waitingForAllBombsToExplode = false;
                _isThrowing = true;
                _bombsThrownInCycle = 0;
            }

            // Если не бросаем и не ждем взрыва всех бомб, и нет активных бомб, и таймер перезарядки прошел
            if (!_isThrowing && !_waitingForAllBombsToExplode && ActiveBombs.Count == 0 && _throwTimer <= 0f)
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

                // Если бросили все бомбы из группы, переходим в режим ожидания
                if (_bombsThrownInCycle >= NumBombs)
                {
                    _isThrowing = false;
                    _waitingForAllBombsToExplode = true;
                }
            }
        }

        private void CreateBombForTarget(Enemy target)
        {
            var bomb = new StickyBombProjectile(
                Player.Position,
                20,
                Color.White,
                this.Damage,
                this.ProjectileSpeed,
                target,
                this.ExplosionTime,
                _explosionSound
            );

            ActiveBombs.Add(bomb);
            _enemiesWithBombs.Add(target);

            _throwSound?.Play();
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