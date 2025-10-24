using Microsoft.Xna.Framework;
using Survive_the_night.Entities;

namespace Survive_the_night.Items
{
    public abstract class Item
    {
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; } = true;
        public int Value { get; protected set; }

        // Конструктор без параметров или с минимальными параметрами
        protected Item()
        {
        }

        public abstract void Update(GameTime gameTime, Player player);
        public abstract void ApplyEffect(Player player);

        public virtual bool CheckCollision(Player player)
        {
            return Vector2.Distance(Position, player.Position) < 30f;
        }
    }
}