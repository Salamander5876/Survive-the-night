using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            float scale = (float)Size / texture.Width;

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
    }
}