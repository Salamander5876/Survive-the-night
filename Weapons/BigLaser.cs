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
        public float RotationSpeed { get; private set; } = 1.0f;

        // Уровни прокачки
        public int DamageIntervalLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;
        public int CooldownLevel { get; private set; } = 0;

        private const float LASER_DURATION = 180f;
        private const float BASE_COOLDOWN = 60f;
        public float CurrentCooldown => BASE_COOLDOWN - (CooldownLevel * 10f);

        private float _cooldownTimer = 0f;
        private bool _isLaserActive = false;

        private List<Enemy> _enemies;

        public BigLaser(Player player) : base(player, WeaponType.Legendary, WeaponName.BigLaser, BASE_COOLDOWN, 5)
        {
            _enemies = Game1.CurrentEnemies;
            _cooldownTimer = 0f;
        }

        public override void LevelUp()
        {
            // Базовое улучшение не используется, используем отдельные методы
        }

        public void UpgradeDamageInterval()
        {
            if (DamageIntervalLevel >= 5) return;
            DamageIntervalLevel++;

            if (ActiveLaser != null)
            {
                ActiveLaser.DamageInterval = 0.11f - (DamageIntervalLevel * 0.02f);
            }
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
                laserSound  // Передаем звук в проектиль
            )
            {
                DamageInterval = 0.11f - (DamageIntervalLevel * 0.02f),
                RotationSpeed = RotationSpeed
            };

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