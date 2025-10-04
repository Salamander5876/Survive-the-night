using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    public abstract class GameObject
    {
        public Vector2 Position { get; protected set; }
        public int Size { get; protected set; }
        public Color Color { get; protected set; } // Свойство Color содержит цвет, установленный в конструкторе

        // Конструктор
        public GameObject(Vector2 initialPosition, int size, Color color)
        {
            Position = initialPosition;
            Size = size;
            Color = color;
        }

        public abstract void Update(GameTime gameTime);

        public virtual Rectangle GetBounds()
        {
            // Возвращает прямоугольник для коллизий
            return new Rectangle(
                (int)(Position.X - Size / 2),
                (int)(Position.Y - Size / 2),
                Size,
                Size
            );
        }

        // !!! ИСПРАВЛЕННЫЙ МЕТОД DRAW !!!
        public virtual void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            // Теперь, если параметр 'color' равен null, используется свойство объекта 'this.Color', 
            // которое для EliteEnemy равно Color.Blue.
            spriteBatch.Draw(
                debugTexture,
                GetBounds(),
                color ?? this.Color // <--- ИСПРАВЛЕНО: используем this.Color, а не Color.Red
            );
        }
    }
}
