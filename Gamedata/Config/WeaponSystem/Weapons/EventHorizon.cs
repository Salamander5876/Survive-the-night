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
    public class EventHorizon : Weapon
    {
        // Основные параметры оружия
        public int NumStars { get; private set; } = 2;
        public float ExpansionSpeed { get; private set; } = 300f; // Скорость расширения кольца
        public float ProjectileLifetime { get; private set; } = 5f; // Время жизни звезд (5 секунд)

        // Уровни прокачки
        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int CooldownLevel { get; private set; } = 0;

        // Перезарядка
        public float CurrentCooldown => 1f - CooldownLevel * 0.2f;

        // Группы звезд и их углы
        private List<ProjectileGroup> _projectileGroups = new List<ProjectileGroup>();
        public List<EventHorizonStarProjectile> ActiveProjectiles { get; private set; } = new List<EventHorizonStarProjectile>();

        public EventHorizon(Player player) : base(player, WeaponType.Legendary, WeaponName.EventHorizon, 1f, 5)
        {
            InitializeProjectileGroups();
        }

        private void InitializeProjectileGroups()
        {
            // Базовые группы с автоматическим расчетом углов
            UpdateProjectileAngles();
        }

        private void UpdateProjectileAngles()
        {
            _projectileGroups.Clear();

            if (NumStars <= 0) return;

            // Рассчитываем углы равномерно по кругу
            float angleStep = 360f / NumStars;
            float[] angles = new float[NumStars];

            for (int i = 0; i < NumStars; i++)
            {
                angles[i] = i * angleStep;
            }

            _projectileGroups.Add(new ProjectileGroup
            {
                Angles = angles,
                IsUnlocked = true
            });
        }

        // === МЕТОДЫ ПРОКАЧКИ ===

        public void UpgradeCount()
        {
            if (CountLevel >= 3) return;
            CountLevel++;

            // Увеличиваем количество звезд
            NumStars += 2;
            UpdateProjectileAngles();
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 3) return;
            DamageLevel++;
            Damage += 5;
        }

        public void UpgradeCooldown()
        {
            if (CooldownLevel >= 3) return;
            CooldownLevel++;
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
                    // Создаем фиктивную цель (не используется для направления движения)
                    Vector2 fakeTarget = Player.Position + new Vector2(1, 0);

                    var projectile = new EventHorizonStarProjectile(
                        Player.Position, // Центр кольца в позиции игрока
                        0,
                        Color.White,
                        Damage,
                        ExpansionSpeed, // Скорость расширения кольца
                        fakeTarget,
                        ProjectileLifetime, // Время жизни звезд
                        angle // Начальный угол для этой звезды
                    );

                    ActiveProjectiles.Add(projectile);
                }
            }

            // Проигрываем звук
            WeaponManager.GetWeaponSound(WeaponName.EventHorizon)?.Play();

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

                    // Универсальная проверка для ВСЕХ звезд
                    if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        // Наносим урон и уничтожаем звезду
                        enemy.TakeDamage(projectile.Damage);
                        projectile.IsActive = false;
                        break;
                    }
                }
            }
        }

        public override void LevelUp()
        {
            // Пустая реализация - прокачка через отдельные методы
        }

        // Структура для групп снарядов
        private struct ProjectileGroup
        {
            public float[] Angles;
            public bool IsUnlocked;
        }
    }
}