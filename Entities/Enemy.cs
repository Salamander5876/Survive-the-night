using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    // Класс врага, наследуется от базового GameObject
    public class Enemy : GameObject
    {
        private Player _target; // Ссылка на игрока для самонаведения

        private float speed = 100f; // Скорость врага (Теперь будет использоваться!)
        public int Health { get; private set; } = 3;

        // Свойство для проверки, жив ли враг
        public bool IsAlive => Health > 0;

        // Конструктор: теперь принимает Player playerTarget
        public Enemy(Vector2 initialPosition, Player playerTarget)
            : base(initialPosition, 24, Color.Red)
        {
            _target = playerTarget; // Сохраняем цель
        }

        // РЕАЛИЗАЦИЯ ДВИЖЕНИЯ: Теперь speed используется, и предупреждение исчезнет.
        public override void Update(GameTime gameTime)
        {
            // Проверяем, жив ли враг и есть ли цель
            if (!IsAlive || _target == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Вектор направления к игроку
            Vector2 direction = _target.Position - Position;

            // 2. Нормализуем направление
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            // 3. Применяем движение к позиции
            // ЗДЕСЬ ИСПОЛЬЗУЕТСЯ speed!
            Position += direction * speed * deltaTime;
        }

        // Метод для нанесения урона врагу
        public void TakeDamage(int damageAmount)
        {
            Health -= damageAmount;
            if (Health < 0)
            {
                Health = 0;
            }
        }
    }
}