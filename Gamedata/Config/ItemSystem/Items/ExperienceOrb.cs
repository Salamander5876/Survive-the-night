using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.Items;

namespace Survive_the_night.Gamedata.Config.ItemSystem.Items
{
    public class ExperienceOrb : Item
    {
        private const float AttractionSpeed = 600f;
        private const float AttractionRadius = 60f;
        private static Texture2D _texture;

        public ExperienceOrb(Vector2 position, int value) : base(position)
        {
            Value = value;
        }

        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();

            // ДЕБАГ: выводим информацию о движении
            if (distance < AttractionRadius)
            {
                direction.Normalize();
                Vector2 oldPosition = Position;
                Position += direction * AttractionSpeed * deltaTime;
            }
            else
            {
            }
        }

        public override void ApplyEffect(Player player)
        {
            player.GainExperience(Value);
            ItemSoundManager.PlayExperienceSound();
        }
    }
}