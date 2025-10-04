using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    // Базовый класс для всех сущностей в игре (игрок, враги, снаряды)
    public abstract class GameObject
    {
        // Позиция объекта в игровом мире. protected set позволяет наследникам менять позицию.
        public Vector2 Position { get; protected set; }

        // Размер объекта. Теперь public get, чтобы к нему могли обращаться SpawnManager и т.д.
        public int Size { get; protected set; }

        // Цвет для отладочной отрисовки (теперь Color)
        public Color Color { get; protected set; } // ИЗМЕНЕНО: Сделано public get

        // Конструктор
        public GameObject(Vector2 initialPosition, int size, Color color)
        {
            Position = initialPosition;
            Size = size;
            Color = color; // ИЗМЕНЕНО: Использовано новое свойство Color
        }

        // Абстрактный метод: вся логика обновления должна быть реализована в наследниках с помощью 'override'.
        public abstract void Update(GameTime gameTime);

        // МЕТОД ИЗМЕНЕН: Теперь принимает необязательный аргумент 'tint' для изменения оттенка.
        public virtual void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? tint = null)
        {
            // Используем оттенок, если он передан, иначе используем основной цвет объекта.
            // Это позволяет нам менять прозрачность или цвет из Game1.cs.
            Color finalColor = tint ?? Color;

            spriteBatch.Draw(
                debugTexture,
                // Прямоугольник для отрисовки (позиция от центра объекта)
                new Rectangle((int)(Position.X - Size / 2), (int)(Position.Y - Size / 2), Size, Size),
                finalColor
            );
        }

        // Метод для получения области столкновения (Bounding Box)
        public Rectangle GetBounds()
        {
            // Возвращаем прямоугольник с учетом смещения для центрирования
            return new Rectangle(
                (int)(Position.X - Size / 2),
                (int)(Position.Y - Size / 2),
                Size,
                Size
            );
        }
    }
}