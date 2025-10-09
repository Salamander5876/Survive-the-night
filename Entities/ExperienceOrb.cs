using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities
{
    public class ExperienceOrb : GameObject
    {
        private const int OrbSize = 8;
        private const float AttractionSpeed = 600f;

        public bool IsActive { get; set; }
        public int Value { get; private set; }

        public ExperienceOrb(Vector2 initialPosition, int value)
            : base(initialPosition, OrbSize, Color.Yellow)
        {
            Value = value;
            IsActive = true;
        }

        public override void Update(GameTime gameTime)
        {
            // Базовая реализация - ничего не делает
            // Основная логика в методе Update с игроком
        }

        // Метод Update, используемый в Game1.cs
        public void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Притяжение к игроку
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();

            const float NewAttractionRadius = 60f;

            if (distance < NewAttractionRadius)
            {
                direction.Normalize();
                Position += direction * AttractionSpeed * deltaTime;
            }

            // Проверка сбора
            if (GetBounds().Intersects(player.GetBounds()))
            {
                IsActive = false;
            }
        }
    }
}