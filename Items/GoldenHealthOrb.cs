using Microsoft.Xna.Framework;
using Survive_the_night.Entities;

namespace Survive_the_night.Items
{
    public class GoldenHealthOrb : Item
    {
        private const float AttractionSpeed = 400f;
        private const float AttractionRadius = 50f;
        public float HealPercentage { get; private set; }

        public GoldenHealthOrb(Vector2 initialPosition, float healPercentage = 1.0f)  // Изменено с 0.5f на 1.0f (100%)
    : base(initialPosition, healPercentage)
        {
            Value = 2;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();

            if (distance < AttractionRadius)
            {
                direction.Normalize();
                Position += direction * AttractionSpeed * deltaTime;
            }
        }

        public override void ApplyEffect(Player player)
        {
            float healAmount = player.GetGoldenHeartHealAmount(HealPercentage);
            player.Heal(healAmount);
        }
    }
}