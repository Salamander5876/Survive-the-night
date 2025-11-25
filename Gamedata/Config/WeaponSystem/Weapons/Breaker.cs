using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using Survive_the_night.Gamedata.Config.WeaponSystem;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class Breaker : Weapon
    {
        // Основные параметры оружия
        public float SwingSpeed { get; private set; } = 500f;
        public float CurrentCooldown => 0.8f - CooldownLevel * 0.2f;

        // Состояние атаки
        private AttackState _currentState = AttackState.Cooldown;
        private float _stateTimer = 0f;
        private bool _currentCycleRightFirst = true;

        // Активные снаряды
        public List<BreakerBladeProjectile> ActiveBlades { get; private set; } = new List<BreakerBladeProjectile>();

        // Уровни прокачки
        public int SpeedLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int CooldownLevel { get; private set; } = 0;

        // Отладочная информация
        private static readonly bool SHOW_DEBUG_INFO = true;

        public Breaker(Player player) : base(player, WeaponType.Legendary, WeaponName.Breaker, 0.8f, 5)
        {
        }

        public static void SetSwingSound(SoundEffect sound)
        {
            // Этот метод оставлен для обратной совместимости
            // Звуки теперь управляются через SoundManager
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем состояние атаки
            UpdateAttackState(deltaTime);

            // Обновляем активные лезвия и проверяем коллизии
            for (int i = ActiveBlades.Count - 1; i >= 0; i--)
            {
                var blade = ActiveBlades[i];

                if (blade.IsActive)
                {
                    blade.Update(gameTime);
                    CheckBladeCollisions(blade);
                }
                else
                {
                    ActiveBlades.RemoveAt(i);
                }
            }
        }

        private void UpdateAttackState(float deltaTime)
        {
            _stateTimer += deltaTime;

            switch (_currentState)
            {
                case AttackState.Cooldown:
                    if (_stateTimer >= CurrentCooldown)
                    {
                        StartNewAttackCycle();
                    }
                    break;

                case AttackState.FirstSwing:
                    // Время для полукруга (180 градусов)
                    float swingTime = 180f / SwingSpeed;
                    if (_stateTimer >= swingTime)
                    {
                        StartSecondSwing();
                    }
                    break;

                case AttackState.SecondSwing:
                    swingTime = 180f / SwingSpeed;
                    if (_stateTimer >= swingTime)
                    {
                        StartCooldown();
                    }
                    break;
            }
        }

        private void StartNewAttackCycle()
        {
            // Чередуем циклы как нужно:
            // Цикл 1: вправо потом влево
            // Цикл 2: влево потом вправо
            _currentCycleRightFirst = !_currentCycleRightFirst;

            if (_currentCycleRightFirst)
            {
                // Цикл 1: сперва вправо потом влево
                // Первый взмах вправо (по часовой стрелке от -90° до 90°)
                CreateBlade(-90f, 90f, true, SpriteEffects.None);
                System.Diagnostics.Debug.WriteLine($"[BREAKER] Starting Cycle 1: Right first (-90° -> 90°)");
            }
            else
            {
                // Цикл 2: сперва влево потом вправо  
                // Первый взмах влево (против часовой стрелки от -90° до 90°)
                CreateBlade(-90f, 90f, false, SpriteEffects.FlipVertically);
                System.Diagnostics.Debug.WriteLine($"[BREAKER] Starting Cycle 2: Left first (-90° -> 90°)");
            }

            _currentState = AttackState.FirstSwing;
            _stateTimer = 0f;

            // Проигрываем звук атаки
            PlaySwingSound();
        }

        private void StartSecondSwing()
        {
            if (_currentCycleRightFirst)
            {
                // Цикл 1: второй взмах влево (против часовой стрелки от -90° до 90°)
                CreateBlade(-90f, 90f, false, SpriteEffects.FlipVertically);
                System.Diagnostics.Debug.WriteLine($"[BREAKER] Cycle 1: Second swing Left (-90° -> 90°)");
            }
            else
            {
                // Цикл 2: второй взмах вправо (по часовой стрелке от -90° до 90°)
                CreateBlade(-90f, 90f, true, SpriteEffects.None);
                System.Diagnostics.Debug.WriteLine($"[BREAKER] Cycle 2: Second swing Right (-90° -> 90°)");
            }

            _currentState = AttackState.SecondSwing;
            _stateTimer = 0f;

            // Проигрываем звук атаки
            PlaySwingSound();
        }

        private void StartCooldown()
        {
            _currentState = AttackState.Cooldown;
            _stateTimer = 0f;

            if (SHOW_DEBUG_INFO)
            {
                System.Diagnostics.Debug.WriteLine($"[BREAKER] Cycle complete. Cooldown started");
            }
        }

        private void CreateBlade(float startAngle, float endAngle, bool isClockwise, SpriteEffects spriteEffect)
        {
            var blade = new BreakerBladeProjectile(
                Player.Position,
                Player,
                startAngle,
                endAngle,
                SwingSpeed,
                isClockwise,
                CalculateDamage(),
                Color.White,
                spriteEffect
            );

            ActiveBlades.Add(blade);

            if (SHOW_DEBUG_INFO)
            {
                string direction = isClockwise ? "Clockwise" : "Counter-Clockwise";
                System.Diagnostics.Debug.WriteLine($"[BREAKER] Created blade: {startAngle}° -> {endAngle}°, {direction}, SpriteEffect={spriteEffect}");
            }
        }

        private void CheckBladeCollisions(BreakerBladeProjectile blade)
        {
            if (Game1.CurrentEnemies == null) return;

            // Получаем только новых врагов, которые еще не получали урон от этого взмаха
            var newHits = blade.GetNewEnemyHits(Game1.CurrentEnemies);

            int hits = 0;
            foreach (var enemy in newHits)
            {
                if (!enemy.IsAlive) continue;

                int finalDamage = CalculateDamage();

                // Бонус урон к обычным врагам с полным HP (не работает на элитных)
                if (!IsEliteEnemy(enemy) && IsEnemyAtFullHealth(enemy))
                {
                    finalDamage = (int)(finalDamage * 2.5f); // +150% урона
                }

                enemy.TakeDamage(finalDamage);
                hits++;
            }

            if (SHOW_DEBUG_INFO && hits > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[BREAKER] Blade hit {hits} new enemies");
            }
        }

        // Проверяем, является ли враг элитным
        private bool IsEliteEnemy(Enemy enemy)
        {
            return enemy.GetType().Namespace?.Contains("Elite") == true ||
                   enemy.GetType().Name.Contains("Elite");
        }

        // Проверяем, есть ли у врага полное HP
        private bool IsEnemyAtFullHealth(Enemy enemy)
        {
            return enemy.Health >= enemy.MaxHealth;
        }

        private int CalculateDamage()
        {
            return Damage;
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // Атака управляется через Update, этот метод не используется
        }

        public override void LevelUp()
        {
            // Пустая реализация - прокачка через отдельные методы
        }

        // Методы прокачки
        public void UpgradeSpeed()
        {
            if (SpeedLevel >= 3) return;
            SpeedLevel++;
            SwingSpeed += 150f;
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

        private void PlaySwingSound()
        {
            WeaponManager.PlayWeaponSound(WeaponName.Breaker);
        }

        private enum AttackState
        {
            Cooldown,
            FirstSwing,
            SecondSwing
        }
    }
}