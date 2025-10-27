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
        private bool _soundPlayed = false;
        private float _fadeOutDuration = 1.5f;

        public FireArea(Vector2 position, int size, Color color, int damage, float duration, float damageInterval)
            : base(position, size, color, damage, 0f, position, 1) // Урон передается из MolotovCocktail
        {
            _timeToLive = duration;
            _damageCooldown = damageInterval;

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
            if (!_soundPlayed && _fireSoundInstance != null)
            {
                _fireSoundInstance.Play();
                _soundPlayed = true;
            }

            // Плавное уменьшение громкости в конце жизни
            if (_burnTimer >= _timeToLive - _fadeOutDuration)
            {
                float fadeProgress = (_burnTimer - (_timeToLive - _fadeOutDuration)) / _fadeOutDuration;
                float volume = MathHelper.Clamp(0.4f * (1f - fadeProgress), 0f, 0.4f); // Ограничиваем громкость

                if (_fireSoundInstance != null)
                {
                    _fireSoundInstance.Volume = volume;
                }
            }

            // Полное исчезновение
            if (_burnTimer >= _timeToLive)
            {
                IsActive = false;
                // Плавно останавливаем звук
                if (_fireSoundInstance != null)
                {
                    _fireSoundInstance.Stop();
                }
                return;
            }

            if (_damageTimer >= _damageCooldown)
            {
                ApplyDamageToEnemies(Game1.CurrentEnemies);
                _damageTimer = 0f;
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

            // Мерцание огня
            float pulse = (float)((System.Math.Sin(_burnTimer * 10f) + 1f) * 0.2f + 0.6f);

            // Плавное исчезновение (альфа-канал)
            float alpha = 1f;
            if (_burnTimer >= _timeToLive - _fadeOutDuration)
            {
                float fadeProgress = (_burnTimer - (_timeToLive - _fadeOutDuration)) / _fadeOutDuration;
                alpha = MathHelper.Clamp(1f - fadeProgress, 0f, 1f); // Ограничиваем альфа-канал
            }

            Color drawColor = Color * pulse * alpha;

            // Используем безопасное значение layerDepth (0.0 - 1.0)
            float layerDepth = 0.1f; // НИЗКИЙ LAYER DEPTH - отрисовывается ПОД другими объектами

            spriteBatch.Draw(
                texture,
                Position,
                null,
                drawColor,
                MathHelper.ToRadians(Rotation),
                origin,
                1.0f,
                SpriteEffects.None,
                layerDepth
            );
        }
    }
}