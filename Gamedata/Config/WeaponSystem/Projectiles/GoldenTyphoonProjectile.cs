using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System.Collections.Generic;
using System;
using Survive_the_night.Gamedata.Config.WeaponSystem;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class GoldenTyphoonProjectile : Projectile
    {
        private static Texture2D _defaultTexture;

        // Переменная отвечающая за скорость поворота спрайта - ПРОТИВ часовой стрелки
        private const float ROTATION_SPEED = -1080f;

        // Система отслеживания попаданий с задержкой
        private Dictionary<Enemy, float> _hitCooldowns = new Dictionary<Enemy, float>();
        private const float HIT_COOLDOWN = 0.2f;

        // Простой таймер жизни
        private float _lifeTimer = 0f;
        private float _maxLifeTime;

        public GoldenTyphoonProjectile(Vector2 position, int size, Color color, int damage, float speed, Vector2 target, float lifetime)
            : base(position, size, color, damage, speed, target, int.MaxValue)
        {
            _maxLifeTime = lifetime;

            // Автоматически определяем размер из текстуры
            if (_defaultTexture != null && size == 0)
            {
                Size = Math.Max(_defaultTexture.Width, _defaultTexture.Height);
            }
        }

        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Логика времени жизни
            _lifeTimer += deltaTime;
            if (_lifeTimer >= _maxLifeTime)
            {
                IsActive = false;
                return;
            }

            // Двигаем снаряд
            Position += Direction * Speed * deltaTime;

            // Вращаем ПРОТИВ часовой стрелки
            Rotation += ROTATION_SPEED * deltaTime;

            // Обновляем кд попаданий
            UpdateHitCooldowns(deltaTime);
        }

        private void UpdateHitCooldowns(float deltaTime)
        {
            List<Enemy> toRemove = new List<Enemy>();

            foreach (var pair in _hitCooldowns)
            {
                float newCooldown = pair.Value - deltaTime;
                if (newCooldown <= 0f)
                {
                    toRemove.Add(pair.Key);
                }
                else
                {
                    _hitCooldowns[pair.Key] = newCooldown;
                }
            }

            foreach (var enemy in toRemove)
            {
                _hitCooldowns.Remove(enemy);
            }
        }

        public void RegisterHit(Enemy enemy)
        {
            // Проверяем, есть ли уже информация об этом враге
            if (_hitCooldowns.TryGetValue(enemy, out float cooldown))
            {
                // Если кд еще не прошло - пропускаем
                if (cooldown > 0f) return;

                // Наносим урон и устанавливаем кд
                enemy.TakeDamage(Damage);
                _hitCooldowns[enemy] = HIT_COOLDOWN;
            }
            else
            {
                // Первое попадание - наносим урон мгновенно
                enemy.TakeDamage(Damage);
                _hitCooldowns[enemy] = HIT_COOLDOWN;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            // Отрисовываем текстуру снаряда
            if (_defaultTexture != null)
            {
                DrawWithTexture(spriteBatch, _defaultTexture);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }

        public override Rectangle GetBounds()
        {
            if (_defaultTexture != null)
            {
                int width = (int)(_defaultTexture.Width * _textureScale);
                int height = (int)(_defaultTexture.Height * _textureScale);

                return new Rectangle(
                    (int)Position.X - width / 2,
                    (int)Position.Y - height / 2,
                    width,
                    height
                );
            }
            else
            {
                return base.GetBounds();
            }
        }

        public void ApplyTexture(Texture2D texture)
        {
            _currentTexture = texture;
            if (texture != null)
            {
                Size = Math.Max(texture.Width, texture.Height);
                _textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
                float baseSize = 32f;
                _textureScale = baseSize / Math.Max(texture.Width, texture.Height);
            }
        }
    }
}