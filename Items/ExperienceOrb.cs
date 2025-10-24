using Microsoft.Xna.Framework;
using Survive_the_night.Entities;

namespace Survive_the_night.Items
{
    public class ExperienceOrb : Item
    {
        private const float AttractionSpeed = 600f;
        private const float AttractionRadius = 60f;

        public ExperienceOrb(Vector2 position, int value)
        {
            Position = position;
            Value = value;
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
            player.GainExperience(Value);
        }
    }
}