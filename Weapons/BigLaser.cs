using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;
using System.Collections.Generic;

namespace Survive_the_night.Weapons
{
    public class BigLaser : Weapon
    {
        public BigLaserProjectile ActiveLaser { get; private set; }
        public float RotationSpeed { get; private set; } = 1.5f;

        // Уровни прокачки
        public int DurationLevel { get; private set; } = 0;    // +10 сек за уровень (вместо интервала атаки)
        public int DamageLevel { get; private set; } = 0;      // +2 урона за уровень
        public int CooldownLevel { get; private set; } = 0;    // -5 сек перезарядки за уровень

        private const float BASE_DURATION = 30f;               // Базовая длительность 30 сек
        private const float BASE_COOLDOWN = 30f;               // Базовая перезарядка 30 сек

        public float CurrentDuration => BASE_DURATION + (DurationLevel * 10f); // +10 сек за уровень
        public float CurrentCooldown => BASE_COOLDOWN - (CooldownLevel * 5f);  // -5 сек за уровень

        private float _cooldownTimer = 0f;
        private bool _isLaserActive = false;

        private List<Enemy> _enemies;

        public BigLaser(Player player) : base(player, WeaponType.Legendary, WeaponName.BigLaser, BASE_COOLDOWN, 2) // Урон понижен до 2
        {
            _enemies = Game1.CurrentEnemies;
            _cooldownTimer = 0f;
        }

        public override void LevelUp()
        {
            // Базовое улучшение не используется, используем отдельные методы
        }

        public void UpgradeDuration()
        {
            if (DurationLevel >= 5) return;
            DurationLevel++;
            // Длительность автоматически обновится через свойство CurrentDuration
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 5) return;
            DamageLevel++;
            Damage += 2;

            if (ActiveLaser != null)
            {
                ActiveLaser.Damage = Damage;
            }
        }

        public void UpgradeCooldown()
        {
            if (CooldownLevel >= 5) return;
            CooldownLevel++;
            // Перезарядка автоматически обновится через свойство CurrentCooldown
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= deltaTime;
            }

            if (ActiveLaser != null)
            {
                ActiveLaser.Update(gameTime);

                if (!ActiveLaser.IsActive)
                {
                    // Останавливаем звук перед удалением лазера
                    ActiveLaser.StopSound();
                    ActiveLaser = null;
                    _isLaserActive = false;
                    _cooldownTimer = CurrentCooldown;
                }
            }

            if (_cooldownTimer <= 0f && !_isLaserActive && ActiveLaser == null)
            {
                ActivateLaser();
            }
        }

        private void ActivateLaser()
        {
            Texture2D laserTexture = WeaponManager.GetRandomWeaponTexture(WeaponName.BigLaser);
            SoundEffect laserSound = WeaponManager.GetWeaponSound(WeaponName.BigLaser);

            ActiveLaser = new BigLaserProjectile(
                Player.Position,
                Player,
                _enemies,
                Damage,
                laserTexture,
                laserSound
            )
            {
                DamageInterval = 0.2f, // Фиксированный интервал 0.1 сек
                RotationSpeed = RotationSpeed
            };

            // Устанавливаем улучшенную длительность
            ActiveLaser.SetLifeTime(CurrentDuration);

            _isLaserActive = true;
        }

        public void UpdateLaserTexture(Texture2D newTexture)
        {
            if (ActiveLaser != null && newTexture != null)
            {
                ActiveLaser.UpdateTexture(newTexture);
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // Атака автоматическая
        }

        public void DrawLaser(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            ActiveLaser?.Draw(spriteBatch, debugTexture);
        }
    }
}