using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Items
{
    public class ExperienceOrb : Item
    {
        private const float AttractionSpeed = 600f;
        private const float AttractionRadius = 60f;
        private static Texture2D _texture;

        public ExperienceOrb(Vector2 position, int value) : base(position)
        {
            Value = value;
            System.Diagnostics.Debug.WriteLine($"Опыт создан: {value} на позиции {position}");
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

                System.Diagnostics.Debug.WriteLine($"Опыт движется: {oldPosition} -> {Position}, расстояние до игрока: {distance}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Опыт слишком далеко: расстояние {distance}, радиус притяжения {AttractionRadius}");
            }
        }

        public override void ApplyEffect(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"Применение эффекта опыта: {Value}");
            player.GainExperience(Value);
            System.Diagnostics.Debug.WriteLine($"Опыт передан игроку: {Value}");
        }
    }
}