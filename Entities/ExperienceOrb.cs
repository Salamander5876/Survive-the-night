using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities; // Для наследования от GameObject

namespace Survive_the_night.Entities
{
    // Класс, представляющий сферу опыта (XP Orb)
    public class ExperienceOrb : GameObject
    {
        // Количество опыта, которое дает эта сфера
        public int Value { get; private set; }

        // Расстояние, на котором сфера начинает притягиваться к игроку
        private const float AttractionRange = 100f;

        // Скорость, с которой сфера летит к игроку (пикселей в секунду)
        private const float AttractionSpeed = 400f;

        // Флаг для определения, активна ли сфера
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Конструктор сферы опыта
        /// </summary>
        /// <param name="initialPosition">Начальная позиция (обычно позиция мертвого врага).</param>
        /// <param name="value">Количество опыта.</param>
        public ExperienceOrb(Vector2 initialPosition, int value)
            // Размер 8, цвет желтый/золотой
            : base(initialPosition, 8, Color.Gold)
        {
            Value = value;
        }

        // !!! ИСПРАВЛЕНИЕ ОШИБКИ: 
        // Добавляем обязательную реализацию абстрактного метода.
        // Оставляем его пустым, так как основная логика обновления 
        // с игроком происходит в перегруженном методе ниже.
        public override void Update(GameTime gameTime)
        {
            // Этот метод должен быть реализован, но нам он здесь не нужен.
        }

        /// <summary>
        /// Обновляет логику сферы опыта (полет к игроку).
        /// </summary>
        public void Update(GameTime gameTime, Player player) // <-- Этот метод используем в Game1
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Проверяем расстояние до игрока
            float distanceToPlayer = Vector2.Distance(Position, player.Position);

            // 2. Если игрок достаточно близко, начинаем притяжение
            if (distanceToPlayer <= AttractionRange)
            {
                // Вычисляем вектор направления к игроку
                Vector2 direction = player.Position - Position;

                // Нормализуем, чтобы получить только направление (длина = 1)
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }

                // Двигаем сферу к игроку
                Position += direction * AttractionSpeed * deltaTime;
            }

            // 3. Проверка столкновения с игроком
            if (GetBounds().Intersects(player.GetBounds()))
            {
                // Сфера собрана, ее нужно удалить в Game1
                IsActive = false;
                // В Game1 мы добавим логику, чтобы игрок получил опыт!
            }
        }
    }
}