using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class BanknoteWeapon : Weapon
    {
        public int NumBanknotes { get; private set; } = 1; // Изменено с 5 на 1
        public float ProjectileSpeed { get; private set; } = 500f;
        public List<BanknoteProjectile> ActiveProjectiles { get; private set; } = new List<BanknoteProjectile>();

        private float _baseCooldown = 2.5f;
        public float CurrentCooldown => _baseCooldown - ReloadSpeedLevel * 0.4f;

        // Уровни прокачки
        public int DamageLevel { get; private set; } = 0;
        public int ReloadSpeedLevel { get; private set; } = 0;
        public int CountLevel { get; private set; } = 0; // Добавлено: уровень количества

        private const float MAX_FAN_ANGLE = 50f; // Максимальный угол веера в градусах
        private float _cooldownTimer = 0f;

        public BanknoteWeapon(Player player) : base(player, WeaponType.Regular, WeaponName.Banknote, 2.5f, 1)
        {
            _cooldownTimer = 0f;
        }

        public override void LevelUp() { }

        // Методы улучшения
        public void UpgradeDamage()
        {
            if (DamageLevel >= 5) return;
            Damage += 1;
            DamageLevel++;
        }

        public void UpgradeReloadSpeed()
        {
            if (ReloadSpeedLevel >= 5) return;
            ReloadSpeedLevel++;
        }

        // Изменено: вместо скорости теперь количество
        public void UpgradeCount()
        {
            if (CountLevel >= 5) return;
            CountLevel++;
            NumBanknotes = 1 + CountLevel; // 1 уровень = 2 снаряда, 5 уровня = 6 снарядов
        }

        // Вспомогательный метод для вычисления угла веера в зависимости от уровня
        private float GetFanAngle()
        {
            if (CountLevel == 0) return 0f; // 1 снаряд - без веера

            // Линейное увеличение угла от 10 до 50 градусов
            return 10f + (CountLevel - 1) * 10f;
        }

        // Вспомогательный метод для вычисления углов для каждого снаряда
        private List<float> GetProjectileAngles(float baseAngle)
        {
            var angles = new List<float>();

            if (NumBanknotes == 1)
            {
                // 1 снаряд - без веера
                angles.Add(baseAngle);
                return angles;
            }

            float fanAngle = GetFanAngle();
            float angleStep = fanAngle / (NumBanknotes - 1);

            // Начальный угол (самый левый в веере)
            float startAngle = baseAngle - fanAngle / 2;

            for (int i = 0; i < NumBanknotes; i++)
            {
                angles.Add(startAngle + i * angleStep);
            }

            return angles;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Обновляем таймер перезарядки
            _cooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cooldownTimer < 0f) _cooldownTimer = 0f;

            // Обновляем активные снаряды
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var banknote = ActiveProjectiles[i];
                if (banknote.IsActive)
                {
                    banknote.Update(gameTime);
                }
                else
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }

            // Проверяем столкновения в Update на каждом кадре
            CheckCollisions();
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (_cooldownTimer > 0f) return;

            // Ищем ближайшего врага для направления атаки
            Enemy target = FindClosestEnemy(enemies);
            if (target == null) return;

            // Проигрываем звук атаки ОДИН РАЗ
            WeaponManager.PlayWeaponSound(WeaponName.Banknote);

            // Вектор направления к цели
            Vector2 directionToTarget = Vector2.Normalize(target.Position - Player.Position);

            // Базовый угол (направление на цель) - это будет направление ЦЕНТРА веера
            float baseAngle = MathHelper.ToDegrees((float)Math.Atan2(directionToTarget.Y, directionToTarget.X));

            // Получаем углы для всех снарядов
            List<float> projectileAngles = GetProjectileAngles(baseAngle);

            // Создаем банкноты с разными углами
            for (int i = 0; i < NumBanknotes; i++)
            {
                float currentAngle = projectileAngles[i];

                // Преобразуем угол обратно в вектор направления
                float angleRadians = MathHelper.ToRadians(currentAngle);
                Vector2 direction = new Vector2(
                    (float)Math.Cos(angleRadians),
                    (float)Math.Sin(angleRadians)
                );

                // Небольшой offset ВДОЛЬ направления полета для визуального разделения
                Vector2 spawnOffset = direction * (i * 3f);

                // Создаем банкноту
                var banknote = new BanknoteProjectile(
                    Player.Position + spawnOffset,
                    0, // размер будет установлен в конструкторе BanknoteProjectile
                    Color.White,
                    Damage,
                    ProjectileSpeed,
                    direction,
                    1, // HitsLeft = 1 (уничтожается при первом попадании)
                    WeaponManager.GetRandomWeaponTexture(WeaponName.Banknote)
                );

                // Устанавливаем время жизни 3 секунды
                banknote.SetLifeTime(3f);

                ActiveProjectiles.Add(banknote);
            }

            // Сбрасываем таймер перезарядки
            _cooldownTimer = CurrentCooldown;
        }

        // Простая проверка столкновений
        private void CheckCollisions()
        {
            // Получаем список врагов из Game1
            var enemies = Game1.CurrentEnemies;
            if (enemies == null) return;

            // Проверяем каждую активную банкноту
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var banknote = ActiveProjectiles[i];
                if (!banknote.IsActive) continue;

                var banknoteBounds = banknote.GetBounds();

                // Проверяем со всеми врагами
                for (int j = 0; j < enemies.Count; j++)
                {
                    var enemy = enemies[j];
                    if (!enemy.IsAlive) continue;

                    if (banknoteBounds.Intersects(enemy.GetBounds()))
                    {
                        // Наносим урон врагу
                        enemy.TakeDamage(banknote.Damage);

                        // Уничтожаем банкноту (один урон - одна банкнота уничтожается)
                        banknote.IsActive = false;

                        // Прерываем проверку для этой банкноты
                        break;
                    }
                }
            }
        }
    }
}