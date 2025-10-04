using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities
{
    public class HealthOrb : GameObject
    {
        private const int OrbSize = 12;
        private const float AttractionSpeed = 400f; // Скорость притяжения к игроку

        public bool IsActive { get; set; }

        // !!! ИСПРАВЛЕНИЕ ОШИБКИ: Свойство HealAmount !!!
        public float HealAmount { get; private set; }

        public HealthOrb(Vector2 initialPosition, float healPercentage)
            // Health Orb, размер 12, цвет LimeGreen
            : base(initialPosition, OrbSize, Color.LimeGreen)
        {
            HealAmount = healPercentage; // Процент от MaxHealth
            IsActive = true;
        }

        // Требуется для реализации абстрактного метода из GameObject
        public override void Update(GameTime gameTime) { /* Логика не используется */ }

        // Метод Update, используемый в Game1.cs
        // Фрагмент из HealthOrb.cs:
        public void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Притяжение к игроку (если находится в небольшом радиусе)
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();

            // !!! ИСПРАВЛЕНИЕ: Значительно уменьшаем радиус притяжения !!!
            const float NewAttractionRadius = 50f;

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