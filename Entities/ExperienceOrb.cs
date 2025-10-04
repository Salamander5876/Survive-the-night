using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities
{
    public class ExperienceOrb : GameObject
    {
        private const int OrbSize = 8;
        private const float AttractionSpeed = 600f; // Скорость притяжения выше, чем у хилок

        public bool IsActive { get; set; }
        public int Value { get; private set; } // Количество опыта

        public ExperienceOrb(Vector2 initialPosition, int value)
            // Experience Orb, размер 8, цвет Yellow
            : base(initialPosition, OrbSize, Color.Yellow)
        {
            Value = value;
            IsActive = true;
        }

        // Требуется для реализации абстрактного метода из GameObject
        public override void Update(GameTime gameTime) { /* Логика не используется */ }

        // Метод Update, используемый в Game1.cs
        // Фрагмент из ExperienceOrb.cs:
        public void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Притяжение к игроку (если находится в небольшом радиусе)
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();

            // !!! ИСПРАВЛЕНИЕ: Значительно уменьшаем радиус притяжения !!!
            const float NewAttractionRadius = 60f;

            if (distance < NewAttractionRadius)
            {
                direction.Normalize();
                Position += direction * AttractionSpeed * deltaTime;
            }

            // 2. Проверка сбора (срабатывает при столкновении)
            if (GetBounds().Intersects(player.GetBounds()))
            {
                IsActive = false;
            }
        }
    }
}