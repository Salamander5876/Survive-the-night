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
        public float ExplosionTime { get; private set; } = 60f; // Время до взрыва
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

        // Звуки
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
            Damage += 1;
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
            ExplosionTime -= 5f;
            ExplosionTimeLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер броска
            if (_throwTimer > 0f)
            {
                _throwTimer -= deltaTime;
            }

            // Обновляем активные бомбы и проверяем нужно ли создать новые
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
                    // Если бомба деактивирована без взрыва (враг умер), отмечаем что нужна новая
                    if (!bomb.HasExploded && bomb.StuckEnemy != null && !bomb.StuckEnemy.IsAlive)
                    {
                        needNewBomb = true;
                    }

                    // Удаляем врага из списка если бомба взорвалась или деактивирована
                    if (bomb.StuckEnemy != null && _enemiesWithBombs.Contains(bomb.StuckEnemy))
                    {
                        _enemiesWithBombs.Remove(bomb.StuckEnemy);
                    }
                    ActiveBombs.RemoveAt(i);
                }
            }

            // Создаем новую бомбу если нужно
            if (needNewBomb && _throwTimer <= 0f)
            {
                CreateNewBombForNewTarget();
            }

            // Проверяем возможность начать новый цикл бросков
            if (!_isThrowing && ActiveBombs.Count == 0 && _throwTimer <= 0f)
            {
                _isThrowing = true;
                _bombsThrownInCycle = 0;
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (!_isThrowing) return;

            // Ищем врага без бомбы
            Enemy target = FindEnemyWithoutBomb(enemies);
            if (target != null && _throwTimer <= 0f && _bombsThrownInCycle < NumBombs)
            {
                CreateBombForTarget(target);
                _bombsThrownInCycle++;
                _throwTimer = _throwCooldown;

                // Если достигли максимального количества бросков в цикле
                if (_bombsThrownInCycle >= NumBombs)
                {
                    _isThrowing = false;
                }
            }
        }

        private void CreateBombForTarget(Enemy target)
        {
            // Создаем бомбу
            var bomb = new StickyBombProjectile(
                Player.Position,
                20,
                Color.White,
                this.Damage,
                this.ProjectileSpeed,
                target,
                this.ExplosionTime,
                _explosionSound // Передаем звук взрыва в снаряд
            );

            ActiveBombs.Add(bomb);
            _enemiesWithBombs.Add(target);

            // Проигрываем звук броска
            _throwSound?.Play();
        }

        private void CreateNewBombForNewTarget()
        {
            // Ищем нового врага без бомбы
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