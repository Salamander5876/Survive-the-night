using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survive_the_night.Projectiles
{
    public abstract class Projectile
    {
        public Vector2 Position { get; protected set; }
        public Vector2 Direction { get; set; }
        public int Size { get; protected set; }
        public Color Color { get; protected set; }
        public int Damage { get; set; } // ИСПРАВЛЕНО: убрал protected
        public float Speed { get; protected set; }
        public bool IsActive { get; set; }
        public int HitsLeft { get; set; }
        public float Rotation { get; protected set; }


        // Новые свойства для работы с текстурами
        protected Texture2D _currentTexture;
        protected Vector2 _textureOrigin;
        protected float _textureScale = 1f;

        protected float _lifeTimer = 0f; // ИЗМЕНИТЕ с private на protected
        protected float MaxLifeTime = 3f;

        protected Projectile(Vector2 position, int size, Color color, int damage, float speed, Vector2 target, int hitsLeft = 1)
        {
            Position = position;
            Size = size;
            Color = color;
            Damage = damage;
            Speed = speed;
            IsActive = true;
            HitsLeft = hitsLeft;
            Rotation = 0f;

            Direction = Vector2.Normalize(target - position);
        }

        public void SetLifeTime(float seconds)
        {
            _lifeTimer = 0f;
            MaxLifeTime = seconds;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер жизни
            _lifeTimer += deltaTime;
            if (_lifeTimer >= MaxLifeTime)
            {
                IsActive = false;
                OnDeactivate(); // Вызываем метод при деактивации
                return;
            }

            Position += Direction * Speed * deltaTime;
            Rotation += 180f * deltaTime;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            Rectangle rect = new Rectangle(
                (int)Position.X - Size / 2,
                (int)Position.Y - Size / 2,
                Size,
                Size
            );

            spriteBatch.Draw(debugTexture, rect, Color);
        }

        public virtual void DrawWithTexture(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!IsActive || texture == null) return;

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // Используем реальный размер текстуры для масштабирования
            float scaleX = (float)Size / texture.Width;
            float scaleY = (float)Size / texture.Height;

            // Для снарядов используем равномерное масштабирование
            float scale = Math.Min(scaleX, scaleY);

            spriteBatch.Draw(
                texture,
                Position,
                null,
                Color,
                MathHelper.ToRadians(Rotation),
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        public virtual Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X - Size / 2,
                (int)Position.Y - Size / 2,
                Size,
                Size
            );
        }

        // В класс Projectile добавь:
        protected virtual void OnDeactivate()
        {
            // Базовый метод ничего не делает, но может быть переопределен в наследниках
        }

        // В класс Projectile добавьте этот метод:
        protected virtual void UpdateLifeTime(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер жизни
            _lifeTimer += deltaTime;
            if (_lifeTimer >= MaxLifeTime)
            {
                IsActive = false;
                OnDeactivate();
                return;
            }
        }

        // Метод для установки текстуры и автоматического расчета размера
        protected void SetTexture(Texture2D texture)
        {
            _currentTexture = texture;
            if (texture != null)
            {
                // Автоматически устанавливаем размер на основе текстуры
                Size = Math.Max(texture.Width, texture.Height);
                _textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

                // Масштабируем до разумного размера (можно настроить)
                float baseSize = 32f; // Базовый размер для большинства снарядов
                _textureScale = baseSize / Math.Max(texture.Width, texture.Height);
            }
        }
    }
}