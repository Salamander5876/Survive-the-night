using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    public class Enemy : GameObject
    {
        private Player _target;
        private float speed;

        public int Health { get; protected set; }
        // !!! ДОБАВЛЕНО: Свойство Damage (Урон) !!!
        public int Damage { get; protected set; }

        public bool IsAlive => Health > 0;

        // !!! ДОБАВЛЕНО: Переменные для отталкивания !!!
        private Vector2 _knockbackVelocity = Vector2.Zero;
        private float _knockbackDecay = 0.9f; // Замедление отталкивания

        // !!! НОВЫЙ МАСТЕР-КОНСТРУКТОР !!!
        public Enemy(Vector2 initialPosition, Player playerTarget, int health, float speed, Color color, int damage)
            : base(initialPosition, 24, color)
        {
            _target = playerTarget;
            Health = health;
            this.speed = speed;
            this.Damage = damage; // Установка урона
        }

        // !!! КОНСТРУКТОР ДЛЯ ОБЫЧНЫХ КРАСНЫХ ВРАГОВ (ДЕФОЛТ) !!!
        public Enemy(Vector2 initialPosition, Player playerTarget)
            // Красный враг: Здоровье 3, Скорость 100f, Цвет Красный, Урон 5
            : this(initialPosition, playerTarget, 3, 100f, Color.Red, 5) // <-- Урон по умолчанию 5
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsAlive || _target == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // !!! ОБРАБОТКА ОТТАЛКИВАНИЯ !!!
            if (_knockbackVelocity != Vector2.Zero)
            {
                Position += _knockbackVelocity * deltaTime;
                _knockbackVelocity *= _knockbackDecay; // Постепенное замедление

                // Если отталкивание стало очень маленьким, обнуляем его
                if (_knockbackVelocity.LengthSquared() < 1f)
                {
                    _knockbackVelocity = Vector2.Zero;
                }
            }
            else
            {
                // Обычное движение к игроку (только если нет отталкивания)
                Vector2 direction = _target.Position - Position;
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }
                Position += direction * speed * deltaTime;
            }
        }

        public void TakeDamage(int damageAmount)
        {
            Health -= damageAmount;
            if (Health < 0)
            {
                Health = 0;
            }
        }

        // !!! НОВЫЙ МЕТОД: ПРИМЕНЕНИЕ ОТТАЛКИВАНИЯ !!!
        public void ApplyKnockback(Vector2 force)
        {
            _knockbackVelocity = force;
        }
    }
}