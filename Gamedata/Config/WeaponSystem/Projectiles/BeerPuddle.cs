using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class BeerPuddle : Projectile
    {
        private float _timeToLive;
        private float _lifeTimer = 0f;
        private float _damageCooldown;
        private Dictionary<Enemy, float> _enemyDamageTimers = new Dictionary<Enemy, float>();
        private SoundEffectInstance _puddleSoundInstance;
        private bool _soundPlayed = false;
        private float _fadeOutDuration = 1.0f;

        public BeerPuddle(Vector2 position, int size, Color color, int damage, float duration, float damageInterval, SoundEffect puddleSound)
            : base(position, size, color, damage, 0f, position, 1)
        {
            _timeToLive = duration;
            _damageCooldown = damageInterval;
            Size = size;

            if (puddleSound != null)
            {
                _puddleSoundInstance = puddleSound.CreateInstance();
                _puddleSoundInstance.Volume = 0.3f;
                _puddleSoundInstance.IsLooped = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lifeTimer += delta;

            if (!_soundPlayed && _puddleSoundInstance != null)
            {
                _puddleSoundInstance.Play();
                _soundPlayed = true;
            }

            if (_lifeTimer >= _timeToLive - _fadeOutDuration)
            {
                float fadeProgress = (_lifeTimer - (_timeToLive - _fadeOutDuration)) / _fadeOutDuration;
                float volume = MathHelper.Clamp(0.3f * (1f - fadeProgress), 0f, 0.3f);

                if (_puddleSoundInstance != null)
                {
                    _puddleSoundInstance.Volume = volume;
                }
            }

            if (_lifeTimer >= _timeToLive)
            {
                IsActive = false;
                if (_puddleSoundInstance != null)
                {
                    _puddleSoundInstance.Stop();
                }
                _enemyDamageTimers.Clear();
                return;
            }

            ApplyDamageToEnemies(Game1.CurrentEnemies, delta);
        }

        private void ApplyDamageToEnemies(List<Enemy> enemies, float deltaTime)
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                if (Vector2.Distance(Position, enemy.Position) <= Size / 2)
                {
                    if (!_enemyDamageTimers.ContainsKey(enemy))
                    {
                        _enemyDamageTimers[enemy] = 0f;
                    }

                    _enemyDamageTimers[enemy] += deltaTime;

                    if (_enemyDamageTimers[enemy] >= _damageCooldown)
                    {
                        enemy.TakeDamage(Damage);
                        _enemyDamageTimers[enemy] = 0f;
                    }
                }
                else
                {
                    if (_enemyDamageTimers.ContainsKey(enemy))
                    {
                        _enemyDamageTimers.Remove(enemy);
                    }
                }
            }

            var deadEnemies = new List<Enemy>();
            foreach (var kvp in _enemyDamageTimers)
            {
                if (!kvp.Key.IsAlive)
                {
                    deadEnemies.Add(kvp.Key);
                }
            }

            foreach (var deadEnemy in deadEnemies)
            {
                _enemyDamageTimers.Remove(deadEnemy);
            }
        }

        public override void DrawWithTexture(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!IsActive || texture == null) return;

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            float pulse = (float)((System.Math.Sin(_lifeTimer * 5f) + 1f) * 0.1f + 0.9f);

            float alpha = 1f;
            if (_lifeTimer >= _timeToLive - _fadeOutDuration)
            {
                float fadeProgress = (_lifeTimer - (_timeToLive - _fadeOutDuration)) / _fadeOutDuration;
                alpha = MathHelper.Clamp(1f - fadeProgress, 0f, 1f);
            }

            Color drawColor = Color * pulse * alpha;

            // ОЧЕНЬ НИЗКИЙ layer depth - отрисовывается ПОД ВСЕМИ объектами
            float layerDepth = 0.01f;

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

        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X - Size / 2,
                (int)Position.Y - Size / 2,
                Size,
                Size
            );
        }
    }
}