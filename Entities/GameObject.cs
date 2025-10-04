using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    // Базовый класс для всех сущностей
    public abstract class GameObject
    {
        public Vector2 Position { get; protected set; }
        public int Size { get; protected set; }
        public Color Color { get; protected set; }

        public GameObject(Vector2 initialPosition, int size, Color color)
        {
            Position = initialPosition;
            Size = size;
            Color = color;
        }

        public abstract void Update(GameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? tint = null)
        {
            Color finalColor = tint ?? Color;

            spriteBatch.Draw(
                debugTexture,
                new Rectangle((int)(Position.X - Size / 2), (int)(Position.Y - Size / 2), Size, Size),
                finalColor
            );
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(Position.X - Size / 2),
                (int)(Position.Y - Size / 2),
                Size,
                Size
            );
        }
    }
}