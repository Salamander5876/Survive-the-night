using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Survive_the_night.Entities;
using System;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using Survive_the_night.Gamedata.Config.WeaponSystem;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class GoldenTyphoon : Weapon
    {
        // Основные параметры оружия
        public int NumProjectiles { get; private set; } = 2;
        public float ProjectileSpeed { get; private set; } = 800f;
        public float ProjectileLifetime { get; private set; } = 3f;
        public float CurrentCooldown => 1f - CooldownLevel * 0.2f;

        // Уровни прокачки
        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int CooldownLevel { get; private set; } = 0;

        // Группы снарядов и их углы
        private List<ProjectileGroup> _projectileGroups = new List<ProjectileGroup>();
        public List<GoldenTyphoonProjectile> ActiveProjectiles { get; private set; } = new List<GoldenTyphoonProjectile>();

        public GoldenTyphoon(Player player) : base(player, WeaponType.Legendary, WeaponName.GoldenTyphoon, 1f, 5)
        {
            InitializeProjectileGroups();
        }

        private void InitializeProjectileGroups()
        {
            // Группа 1: базовые снаряды (90° и 270°)
            _projectileGroups.Add(new ProjectileGroup
            {
                Angles = new float[] { 90f, 270f },
                IsUnlocked = true
            });

            // Группа 2: первый уровень (0° и 180°)
            _projectileGroups.Add(new ProjectileGroup
            {
                Angles = new float[] { 0f, 180f },
                IsUnlocked = false
            });

            // Группа 3: второй уровень (45° и 225°)
            _projectileGroups.Add(new ProjectileGroup
            {
                Angles = new float[] { 45f, 225f },
                IsUnlocked = false
            });

            // Группа 4: третий уровень (135° и 315°)
            _projectileGroups.Add(new ProjectileGroup
            {
                Angles = new float[] { 135f, 315f },
                IsUnlocked = false
            });
        }

        // Ветки прокачки
        public void UpgradeCount()
        {
            if (CountLevel >= 3) return;
            CountLevel++;

            // Разблокируем соответствующую группу снарядов
            if (CountLevel >= 1 && _projectileGroups.Count > 1)
            {
                var group = _projectileGroups[1];
                group.IsUnlocked = true;
                _projectileGroups[1] = group;
            }
            if (CountLevel >= 2 && _projectileGroups.Count > 2)
            {
                var group = _projectileGroups[2];
                group.IsUnlocked = true;
                _projectileGroups[2] = group;
            }
            if (CountLevel >= 3 && _projectileGroups.Count > 3)
            {
                var group = _projectileGroups[3];
                group.IsUnlocked = true;
                _projectileGroups[3] = group;
            }

            UpdateTotalProjectileCount();
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 3) return;
            DamageLevel++;
            Damage += 4;
        }

        public void UpgradeCooldown()
        {
            if (CooldownLevel >= 3) return;
            CooldownLevel++;
        }

        private void UpdateTotalProjectileCount()
        {
            NumProjectiles = 0;
            foreach (var group in _projectileGroups)
            {
                if (group.IsUnlocked)
                    NumProjectiles += group.Angles.Length;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Обновляем снаряды независимо от перезарядки
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = ActiveProjectiles[i];
                if (projectile.IsActive)
                {
                    projectile.Update(gameTime);
                }
                else
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }

            // Проверяем коллизии ВНЕ метода Attack
            CheckProjectileCollisions(Game1.CurrentEnemies);
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (CooldownTimer > 0f) return;

            // Создаем снаряды для всех разблокированных групп
            foreach (var group in _projectileGroups)
            {
                if (!group.IsUnlocked) continue;

                foreach (var angle in group.Angles)
                {
                    Vector2 direction = AngleToDirection(angle);
                    Vector2 targetPosition = Player.Position + direction * 1000f;

                    var projectile = new GoldenTyphoonProjectile(
                        Player.Position,
                        0,
                        Color.Gold,
                        Damage,
                        ProjectileSpeed,
                        targetPosition,
                        ProjectileLifetime
                    );

                    ActiveProjectiles.Add(projectile);
                }
            }

            // Проигрываем звук
            WeaponManager.PlayWeaponSound(WeaponName.GoldenTyphoon);

            // Устанавливаем перезарядку с учетом прокачки
            CooldownTimer = CurrentCooldown;
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            if (enemies == null) return;

            foreach (var projectile in ActiveProjectiles)
            {
                if (!projectile.IsActive) continue;

                foreach (var enemy in enemies)
                {
                    if (!enemy.IsAlive) continue;

                    if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        projectile.RegisterHit(enemy);
                    }
                }
            }
        }

        private Vector2 AngleToDirection(float angle)
        {
            float radians = MathHelper.ToRadians(angle);
            return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        public override void LevelUp()
        {
            // Пустая реализация
        }

        // Структура для групп снарядов
        private struct ProjectileGroup
        {
            public float[] Angles;
            public bool IsUnlocked;
        }
    }
}