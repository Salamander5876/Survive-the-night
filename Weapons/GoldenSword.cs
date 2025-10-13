using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Managers;
using Survive_the_night.Projectiles;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Weapons
{
    public class GoldenSword : Weapon
    {
        public const string WeaponName = "Золотой меч";

        // Статические поля для текстур и звуков
        private static List<Texture2D> _swordTextures = new List<Texture2D>();
        private static SoundEffect _swordSound;

        // Параметры оружия
        public int NumSwords { get; private set; } = 1;
        public float ProjectileSpeed { get; private set; } = 400f;
        public List<GoldenSwordProjectile> ActiveProjectiles { get; private set; } = new List<GoldenSwordProjectile>();

        // Базовая перезарядка
        private float _baseCooldown = 2.0f;
        public float CurrentCooldown => _baseCooldown;

        // Уровни прокачки
        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int SpeedLevel { get; private set; } = 0; // Заменили ReloadSpeedLevel на SpeedLevel

        // Флаг для блокировки атаки пока мечи активны
        public bool HasActiveSwords => ActiveProjectiles.Count > 0;

        public GoldenSword(Player player) : base(player, 2.0f, 5)
        {
        }

        // Методы для загрузки контента
        public static void AddSwordTexture(Texture2D texture)
        {
            if (texture != null && !_swordTextures.Contains(texture))
            {
                _swordTextures.Add(texture);
            }
        }

        public static void SetSound(SoundEffect sound)
        {
            _swordSound = sound;
        }

        public static Texture2D GetRandomSwordTexture()
        {
            if (_swordTextures.Count == 0)
                return null;

            return _swordTextures[Game1.Random.Next(0, _swordTextures.Count)];
        }

        public override void LevelUp() { }

        // Методы прокачки
        public void UpgradeCount()
        {
            if (CountLevel >= 5) return;
            NumSwords += 1;
            CountLevel++;
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 5) return;
            Damage += 2;
            DamageLevel++;
        }

        public void UpgradeSpeed()
        {
            if (SpeedLevel >= 5) return;
            ProjectileSpeed += 20f; // +20 скорости за уровень
            SpeedLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Обновляем снаряды
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var sword = ActiveProjectiles[i];
                if (sword.IsActive)
                {
                    sword.Update(gameTime);
                }
                else
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }

            // Проверяем столкновения ВНЕ цикла атаки
            CheckProjectileCollisions(Game1.CurrentEnemies);
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // Блокируем атаку, если есть активные мечи ИЛИ идет перезарядка
            if (CooldownTimer > 0f || HasActiveSwords) return;

            // Находим ближайших врагов для каждого меча
            List<Enemy> targets = FindTargetsForSwords(enemies, NumSwords);

            if (targets.Count > 0)
            {
                // Создаем снаряды для каждого найденного врага
                foreach (var target in targets)
                {
                    var sword = new GoldenSwordProjectile(
                        Player.Position,
                        32,
                        Color.Gold,
                        Damage,
                        ProjectileSpeed,
                        target,
                        enemies,
                        Player,
                        GetRandomSwordTexture()
                    );

                    ActiveProjectiles.Add(sword);
                }

                // Воспроизводим звук
                _swordSound?.Play();

                // Запускаем перезарядку
                CooldownTimer = CurrentCooldown;
            }
        }

        private List<Enemy> FindTargetsForSwords(List<Enemy> enemies, int maxTargets)
        {
            List<Enemy> targets = new List<Enemy>();
            List<Enemy> aliveEnemies = new List<Enemy>();

            // Собираем всех живых врагов в радиусе
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive && Vector2.DistanceSquared(Player.Position, enemy.Position) < 1000 * 1000)
                {
                    aliveEnemies.Add(enemy);
                }
            }

            // Сортируем по расстоянию и берем ближайших
            aliveEnemies.Sort((a, b) =>
                Vector2.DistanceSquared(Player.Position, a.Position).CompareTo(
                Vector2.DistanceSquared(Player.Position, b.Position)));

            for (int i = 0; i < Math.Min(maxTargets, aliveEnemies.Count); i++)
            {
                targets.Add(aliveEnemies[i]);
            }

            return targets;
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            foreach (var sword in ActiveProjectiles)
            {
                if (!sword.IsActive) continue;

                foreach (var enemy in enemies)
                {
                    if (sword.CheckEnemyHit(enemy))
                    {
                        enemy.TakeDamage(sword.Damage);
                    }
                }
            }
        }
    }
}