using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Items
{
    public class Coin : Item
    {
        private const float AttractionSpeed = 350f;
        private const float AttractionRadius = 45f;
        public static Texture2D Texture { get; private set; }

        public Coin(Vector2 position, int value = 1) : base(position)
        {
            Value = value;
            System.Diagnostics.Debug.WriteLine($"Монета создана: {value} на позиции {position}");
        }

        public static void SetTexture(Texture2D texture)
        {
            Texture = texture;
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

                System.Diagnostics.Debug.WriteLine($"Монета движется: {oldPosition} -> {Position}, расстояние до игрока: {distance}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Монета слишком далеко: расстояние {distance}, радиус притяжения {AttractionRadius}");
            }
        }

        public override void ApplyEffect(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"Применение эффекта монеты: {Value}");
            player.AddCoins(Value);
            System.Diagnostics.Debug.WriteLine($"Монеты переданы игроку: {Value}");
        }
    }
}