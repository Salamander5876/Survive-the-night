using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    // Сущность для восстановления здоровья
    public class HealthOrb
    {
        public Vector2 Position { get; private set; }
        public bool IsActive { get; set; } = true;
        public float HealingPercentage { get; }

        private const float Radius = 10f;
        private const float AttractionDistance = 150f;
        private const float PickupDistance = 30f;
        private const float MovementSpeed = 400f;

        public Color Color { get; } = Color.LimeGreen;

        public HealthOrb(Vector2 position, float healingPercentage)
        {
            Position = position;
            HealingPercentage = healingPercentage;
        }

        public void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 directionToPlayer = player.Position - Position;
            float distance = directionToPlayer.Length();

            if (distance < PickupDistance)
            {
                // Игрок подобрал хилку
                player.Heal(player.MaxHealth * HealingPercentage);
                IsActive = false;
                return;
            }

            if (distance < AttractionDistance)
            {
                // Притяжение к игроку
                directionToPlayer.Normalize();
                Position += directionToPlayer * MovementSpeed * deltaTime;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color color)
        {
            if (!IsActive) return;

            // Рисуем хилку (используем квадрат/прямоугольник, как и для других сущностей)
            spriteBatch.Draw(
                debugTexture,
                new Rectangle((int)Position.X - (int)Radius, (int)Position.Y - (int)Radius, (int)Radius * 2, (int)Radius * 2),
                color
            );
        }
    }
}