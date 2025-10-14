using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System.Collections.Generic;

namespace Survive_the_night.Projectiles
{
    public class FireArea : Projectile
    {
        private float _timeToLive;
        private float _burnTimer = 0f;
        private float _damageCooldown;
        private float _damageTimer = 0f;
        private SoundEffectInstance _fireSoundInstance;
        private bool _isSoundStopped = false;

        public FireArea(Vector2 position, int size, Color color, int damage, float duration, float damageInterval)
            : base(position, size, color, damage, 0f, position, 1)
        {
            _timeToLive = duration;
            _damageCooldown = damageInterval;

            // Создаем экземпляр звука для этой огненной области
            if (Game1.SFXFireBurn != null)
            {
                _fireSoundInstance = Game1.SFXFireBurn.CreateInstance();
                _fireSoundInstance.Volume = 0.4f;
                _fireSoundInstance.IsLooped = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _burnTimer += delta;
            _damageTimer += delta;

            // Запускаем звук при первом обновлении
            if (_fireSoundInstance != null && _fireSoundInstance.State != SoundState.Playing && !_isSoundStopped)
            {
                _fireSoundInstance.Play();
            }

            // Проверяем истечение времени жизни
            if (_burnTimer >= _timeToLive)
            {
                IsActive = false;
                // Останавливаем звук при исчезновении области
                StopSound();
                return;
            }

            // Наносим периодический урон
            if (_damageTimer >= _damageCooldown)
            {
                ApplyDamageToEnemies(Game1.CurrentEnemies);
                _damageTimer = 0f;
            }
        }

        private void StopSound()
        {
            if (_fireSoundInstance != null && _fireSoundInstance.State == SoundState.Playing)
            {
                _fireSoundInstance.Stop();
                _isSoundStopped = true;
            }
        }

        private void ApplyDamageToEnemies(List<Enemy> enemies)
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                if (Vector2.Distance(Position, enemy.Position) <= Size / 2)
                {
                    enemy.TakeDamage(Damage);
                }
            }
        }

        public override void DrawWithTexture(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!IsActive || texture == null) return;

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // Мерцание огня для эффекта
            float pulse = (float)((System.Math.Sin(_burnTimer * 10f) + 1f) * 0.2f + 0.6f);
            Color drawColor = Color * pulse;

            spriteBatch.Draw(
                texture,
                Position,
                null,
                drawColor,
                MathHelper.ToRadians(Rotation),
                origin,
                1.0f,
                SpriteEffects.None,
                0f
            );
        }
    }
}