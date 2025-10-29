// Entities/GameObject.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survive_the_night.Entities
{
    public abstract class GameObject
    {
        public Vector2 Position { get; protected set; }
        public int Size { get; protected set; }
        public Color Color { get; protected set; }

        // Конструктор с 3 параметрами (старый)
        public GameObject(Vector2 position, int size, Color color)
        {
            Position = position;
            Size = size;
            Color = color;
        }

        // Конструктор с 4 параметрами (новый - для ширины и высоты)
        public GameObject(Vector2 position, int width, int height, Color color)
        {
            Position = position;
            Size = Math.Max(width, height); // Используем максимальный размер
            Color = color;
        }

        public abstract void Update(GameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            Rectangle rect = new Rectangle(
                (int)Position.X - Size / 2,
                (int)Position.Y - Size / 2,
                Size,
                Size
            );
            spriteBatch.Draw(debugTexture, rect, color ?? this.Color);
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

        // Новые методы для лучшей гибкости
        public virtual void SetPosition(Vector2 newPosition)
        {
            Position = newPosition;
        }

        public virtual void SetPosition(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public virtual void Move(Vector2 movement)
        {
            Position += movement;
        }

        public virtual bool Intersects(GameObject other)
        {
            return GetBounds().Intersects(other.GetBounds());
        }

        public virtual float DistanceTo(GameObject other)
        {
            return Vector2.Distance(Position, other.Position);
        }

        public virtual float DistanceTo(Vector2 point)
        {
            return Vector2.Distance(Position, point);
        }
    }
}