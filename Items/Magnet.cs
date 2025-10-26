using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Items
{
    public class Magnet : Item
    {
        private const float AttractionSpeed = 200f;
        private const float AttractionRadius = 60f;
        private static Texture2D _texture;

        // Статические свойства для бонусов
        public static float BaseAttractionSpeed { get; private set; } = 200f;
        public static float SpeedBonus { get; private set; } = 0f;
        public static float TotalAttractionSpeed => BaseAttractionSpeed + SpeedBonus;

        public Magnet(Vector2 position)
        {
            Position = position;
            Value = 0;
        }

        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }

        // Метод для применения бонуса из магазина
        public static void ApplySpeedBonus(float bonusAmount)
        {
            SpeedBonus += bonusAmount;
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
            // Магнит активируется через ItemManager
            IsActive = false;
        }

        public override bool CheckCollision(Player player)
        {
            return Vector2.Distance(Position, player.Position) < 25f;
        }
    }
}