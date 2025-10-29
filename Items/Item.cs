using Microsoft.Xna.Framework;
using Survive_the_night.Entities;

namespace Survive_the_night.Items
{
    public abstract class Item
    {
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; } = true;
        public int Value { get; protected set; }

        // Конструктор с позицией
        protected Item(Vector2 position)
        {
            Position = position;
        }

        // Конструктор по умолчанию для совместимости
        protected Item()
        {
            Position = Vector2.Zero;
        }

        public abstract void Update(GameTime gameTime, Player player);
        public abstract void ApplyEffect(Player player);

        public virtual bool CheckCollision(Player player)
        {
            // Используем тот же размер, что и у игрока (24)
            float collisionDistance = 24f;
            bool isColliding = Vector2.Distance(Position, player.Position) < collisionDistance;

            if (isColliding)
            {
                System.Diagnostics.Debug.WriteLine($"Коллизия предмета {GetType().Name} с игроком. Расстояние: {Vector2.Distance(Position, player.Position)}");
            }

            return isColliding;
        }
    }
}