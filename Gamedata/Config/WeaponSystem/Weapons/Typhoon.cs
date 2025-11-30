using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Survive_the_night.Entities;
using System;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using Survive_the_night.Gamedata.Config.WeaponSystem;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class Typhoon : Weapon
    {
        // Основные параметры оружия
        public int NumProjectiles { get; private set; } = 1;
        public float ProjectileSpeed { get; private set; } = 800f;
        public float ProjectileLifetime { get; private set; } = 3f;

        // Система перезарядки как у игральных карт
        private float _baseCooldown = 2.0f; // Начальная перезарядка 2 сек
        public float CurrentCooldown => _baseCooldown - CooldownLevel * 0.2f;

        // Уровни прокачки (обычное оружие - максимум 5)
        public int DamageLevel { get; private set; } = 0;
        public int CountLevel { get; private set; } = 0;
        public int CooldownLevel { get; private set; } = 0;

        // Активные снаряды
        public List<TyphoonProjectile> ActiveProjectiles { get; private set; } = new List<TyphoonProjectile>();

        // Система выстрелов как у игральных карт
        private const float SHOT_INTERVAL = 0.3f; // Задержка между снарядами 0.3 сек
        private float _burstCooldown => CurrentCooldown; // Перезарядка всей группы
        private bool _isBurstActive = false;
        private int _shotsFiredInBurst = 0;
        private float _nextShotTimer = 0f;
        private float _burstCooldownTimer = 0f;

        public Typhoon(Player player) : base(player, WeaponType.Regular, WeaponName.Typhoon, 2f, 2)
        {
        }

        // Ветки прокачки (обычное оружие - максимум 5 уровней)
        public void UpgradeDamage()
        {
            if (DamageLevel >= 5) return;
            DamageLevel++;
            Damage += 1;
        }

        public void UpgradeCount()
        {
            if (CountLevel >= 5) return;
            CountLevel++;
            NumProjectiles++;
        }

        public void UpgradeCooldown()
        {
            if (CooldownLevel >= 5) return;
            CooldownLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

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

            // Система перезарядки как у игральных карт
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

                while (_nextShotTimer <= 0f && _shotsFiredInBurst < NumProjectiles)
                {
                    SpawnSingleProjectile();
                    _shotsFiredInBurst++;
                    _nextShotTimer += SHOT_INTERVAL;
                }

                if (_shotsFiredInBurst >= NumProjectiles)
                {
                    _isBurstActive = false;
                    _burstCooldownTimer = _burstCooldown;
                }
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // Атака теперь управляется через систему в Update, как у игральных карт
            // Этот метод может остаться пустым или использоваться для инициализации
        }

        private void SpawnSingleProjectile()
        {
            // Находим ближайшего врага для направления
            Enemy closestEnemy = FindClosestEnemy(Game1.CurrentEnemies);
            if (closestEnemy == null) return;

            Vector2 targetPosition = closestEnemy.Position;

            var projectile = new TyphoonProjectile(
                Player.Position,
                0, // Размер будет установлен автоматически из текстуры
                Color.White,
                Damage,
                ProjectileSpeed,
                targetPosition,
                ProjectileLifetime
            );

            ActiveProjectiles.Add(projectile);

            // Проигрываем звук для каждого снаряда
            WeaponManager.PlayWeaponSound(WeaponName.Typhoon);
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
                        ((TyphoonProjectile)projectile).RegisterHit(enemy);
                    }
                }
            }
        }

        public override void LevelUp()
        {
            // Пустая реализация - прокачка через отдельные методы
        }
    }
}