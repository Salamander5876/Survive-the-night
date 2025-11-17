using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class GoldenSword : Weapon
    {
        public int NumSwords { get; private set; } = 2;
        public float ProjectileSpeed { get; private set; } = 500f;
        public List<GoldenSwordProjectile> ActiveProjectiles { get; private set; } = new List<GoldenSwordProjectile>();
        public int MaxTargets { get; private set; } = 10;

        private float _baseCooldown = 2.0f;
        public float CurrentCooldown => _baseCooldown;

        public int CountLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int TargetsLevel { get; private set; } = 0;

        // Новые флаги для управления перезарядкой
        private bool _waitingForSwordsToReturn = false;
        private bool _canAttack = true;

        // Список врагов, которые уже являются целями активных мечей
        private List<Enemy> _assignedTargets = new List<Enemy>();

        public GoldenSword(Player player) : base(player, WeaponType.Legendary, WeaponName.GoldenSword, 2.0f, 4)
        {
        }

        public override void LevelUp() { }

        public void UpgradeCount()
        {
            if (CountLevel >= 3) return;
            NumSwords += 2;
            CountLevel++;
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 3) return;
            Damage += 4;
            DamageLevel++;
        }

        public void UpgradeTargets()
        {
            if (TargetsLevel >= 3) return;
            MaxTargets += 10;
            TargetsLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Очищаем список назначенных целей
            _assignedTargets.Clear();

            // Обновляем активные мечи и собираем их текущие цели
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var sword = ActiveProjectiles[i];
                if (sword.IsActive)
                {
                    sword.Update(gameTime);

                    // Добавляем текущую цель меча в список занятых целей
                    if (sword.HasAssignedTarget && sword.Target != null && sword.Target.IsAlive)
                    {
                        _assignedTargets.Add(sword.Target);
                    }

                    // Если меч вернулся к игроку, деактивируем его
                    if (sword.HasReturnedToPlayer)
                    {
                        sword.IsActive = false;
                    }
                }
                else
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }

            // Проверяем, все ли мечи вернулись
            if (_waitingForSwordsToReturn && ActiveProjectiles.Count == 0)
            {
                _waitingForSwordsToReturn = false;
                _canAttack = true;
                CooldownTimer = CurrentCooldown; // Запускаем перезарядку только сейчас
            }

            CheckProjectileCollisions(Game1.CurrentEnemies);
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // Атакуем только если можем атаковать и перезарядка прошла
            if (!_canAttack || CooldownTimer > 0f) return;

            // Находим цели для мечей (исключая уже занятых врагов)
            List<Enemy> availableTargets = FindAvailableTargets(enemies);

            if (availableTargets.Count > 0 || NumSwords > 0) // Атакуем даже если врагов нет, но есть мечи
            {
                // Создаем мечи для каждой доступной цели
                int swordsToCreate = Math.Min(NumSwords, availableTargets.Count);

                for (int i = 0; i < swordsToCreate; i++)
                {
                    var target = availableTargets[i];
                    CreateSwordForTarget(target, enemies);
                }

                // Если есть свободные мечи (целей меньше чем мечей), создаем их без целей
                for (int i = swordsToCreate; i < NumSwords; i++)
                {
                    CreateSwordWithoutTarget(enemies);
                }

                WeaponManager.GetWeaponSound(WeaponName.GoldenSword)?.Play();

                // Блокируем следующую атаку до возврата всех мечей
                _canAttack = false;
                _waitingForSwordsToReturn = true;
            }
        }

        private List<Enemy> FindAvailableTargets(List<Enemy> enemies)
        {
            List<Enemy> availableTargets = new List<Enemy>();
            List<Enemy> aliveEnemies = new List<Enemy>();

            // Собираем всех живых врагов в радиусе, которые НЕ являются целями активных мечей
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive &&
                    Vector2.DistanceSquared(Player.Position, enemy.Position) < 1000 * 1000 &&
                    !_assignedTargets.Contains(enemy)) // Исключаем уже занятых врагов
                {
                    aliveEnemies.Add(enemy);
                }
            }

            // Сортируем по расстоянию от игрока
            aliveEnemies.Sort((a, b) =>
                Vector2.DistanceSquared(Player.Position, a.Position).CompareTo(
                Vector2.DistanceSquared(Player.Position, b.Position)));

            // Ограничиваем количество целей максимальным количеством целей меча
            for (int i = 0; i < Math.Min(MaxTargets, aliveEnemies.Count); i++)
            {
                availableTargets.Add(aliveEnemies[i]);
            }

            return availableTargets;
        }

        private void CreateSwordForTarget(Enemy target, List<Enemy> enemies)
        {
            var sword = new GoldenSwordProjectile(
                Player.Position,
                0,
                Color.Gold,
                Damage,
                ProjectileSpeed,
                target,
                enemies,
                Player,
                WeaponManager.GetRandomWeaponTexture(WeaponName.GoldenSword),
                MaxTargets
            );

            // Добавляем цель в список занятых
            _assignedTargets.Add(target);

            ActiveProjectiles.Add(sword);
        }

        private void CreateSwordWithoutTarget(List<Enemy> enemies)
        {
            // Создаем меч без начальной цели
            var sword = new GoldenSwordProjectile(
                Player.Position,
                0,
                Color.Gold,
                Damage,
                ProjectileSpeed,
                null, // Нет начальной цели
                enemies,
                Player,
                WeaponManager.GetRandomWeaponTexture(WeaponName.GoldenSword),
                MaxTargets
            );

            // Немедленно переводим в режим возврата к игроку
            sword.StartReturnToPlayer();

            ActiveProjectiles.Add(sword);
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            foreach (var sword in ActiveProjectiles)
            {
                if (!sword.IsActive) continue;

                // Меч проверяет столкновения только со своей целью
                sword.CheckEnemyHit();
            }
        }
    }
}