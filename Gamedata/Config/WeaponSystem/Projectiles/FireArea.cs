using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Scripts.Managers;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
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

        // Анимационные поля
        private List<Texture2D> _fireTextures;
        private int _currentFrame = 0;
        private float _frameTimer = 0f;
        private float _frameTime = 0.15f; // Скорость анимации - можно настроить

        public FireArea(Vector2 position, int size, Color color, int damage, float duration, float damageInterval, List<Texture2D> fireTextures)
            : base(position, size, color, damage, 0f, position, 1)
        {
            _timeToLive = duration;
            _damageCooldown = damageInterval;
            _fireTextures = fireTextures ?? new List<Texture2D>();

            // Устанавливаем первую текстуру, если есть
            if (_fireTextures.Count > 0)
            {
                SetTexture(_fireTextures[0]);
            }

            // Создаем зацикленный звук ГОРЕНИЯ через отдельный метод
            _fireSoundInstance = CreateFireBurnSound();
        }

        private SoundEffectInstance CreateFireBurnSound()
        {
            // Используем прямой доступ к SoundManager для звука горения
            string fireBurnKey = "fire_burn";
            if (SoundManager.Instance.ContainsSound(fireBurnKey))
            {
                var instance = SoundManager.Instance.PlaySoundInstance(fireBurnKey, 0.4f, true);
                // Добавляем в группу звуков огня для управления паузой
                SoundManager.Instance.AddToSoundGroup("fire_sounds", instance);
                return instance;
            }
            return null;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _burnTimer += delta;
            _damageTimer += delta;
            _frameTimer += delta;

            // Обновляем анимацию
            if (_fireTextures.Count > 1 && _frameTimer >= _frameTime)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _fireTextures.Count;
                SetTexture(_fireTextures[_currentFrame]);
            }

            // Плавное уменьшение громкости в конце жизни
            if (_burnTimer >= _timeToLive - _fadeOutDuration)
            {
                float fadeProgress = (_burnTimer - (_timeToLive - _fadeOutDuration)) / _fadeOutDuration;
                float volume = MathHelper.Clamp(0.4f * (1f - fadeProgress), 0f, 0.4f);

                if (_fireSoundInstance != null && !_fireSoundInstance.IsDisposed)
                {
                    _fireSoundInstance.Volume = volume;
                }
            }

            // Полное исчезновение
            if (_burnTimer >= _timeToLive)
            {
                IsActive = false;
                StopSound();
                return;
            }

            if (_damageTimer >= _damageCooldown)
            {
                ApplyDamageToEnemies(Game1.CurrentEnemies);
                _damageTimer = 0f;
            }
        }

        public void PauseSound()
        {
            if (_fireSoundInstance != null && _fireSoundInstance.State == SoundState.Playing)
            {
                _fireSoundInstance.Pause();
            }
        }

        public void ResumeSound()
        {
            if (_fireSoundInstance != null && _fireSoundInstance.State == SoundState.Paused)
            {
                _fireSoundInstance.Resume();
            }
        }

        public void StopSound()
        {
            if (_fireSoundInstance != null)
            {
                try
                {
                    if (!_fireSoundInstance.IsDisposed)
                    {
                        _fireSoundInstance.Stop();
                        _fireSoundInstance.Dispose();
                    }
                }
                catch (System.Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine($"Ошибка остановки звука огня: {ex.Message}");
                }
                finally
                {
                    _fireSoundInstance = null;
                    SoundManager.Instance.RemoveFromSoundGroup("fire_sounds", _fireSoundInstance);
                }
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
            // Используем текущую текстуру из анимации
            if (!IsActive || _currentTexture == null) return;

            Vector2 origin = new Vector2(_currentTexture.Width / 2, _currentTexture.Height / 2);

            // Мерцание огня
            float pulse = (float)((System.Math.Sin(_burnTimer * 10f) + 1f) * 0.2f + 0.6f);

            // Плавное исчезновение (альфа-канал)
            float alpha = 1f;
            if (_burnTimer >= _timeToLive - _fadeOutDuration)
            {
                float fadeProgress = (_burnTimer - (_timeToLive - _fadeOutDuration)) / _fadeOutDuration;
                alpha = MathHelper.Clamp(1f - fadeProgress, 0f, 1f);
            }

            Color drawColor = Color * pulse * alpha;

            float layerDepth = 0.1f;

            spriteBatch.Draw(
                _currentTexture,
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

        protected override void OnDeactivate()
        {
            StopSound();
            base.OnDeactivate();
        }

        // Метод для установки скорости анимации
        public void SetAnimationSpeed(float frameTime)
        {
            _frameTime = frameTime;
        }
    }
}