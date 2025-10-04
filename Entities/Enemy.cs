using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    public class Enemy : GameObject
    {
        private Player _target;
        private float speed;

        // Health теперь должен быть protected set, чтобы EliteEnemy мог его изменить.
        public int Health { get; protected set; }

        public bool IsAlive => Health > 0;

        // !!! НОВЫЙ МАСТЕР-КОНСТРУКТОР !!!
        public Enemy(Vector2 initialPosition, Player playerTarget, int health, float speed, Color color)
            : base(initialPosition, 24, color)
        {
            _target = playerTarget;
            Health = health;
            this.speed = speed;
        }

        // !!! КОНСТРУКТОР ДЛЯ ОБЫЧНЫХ КРАСНЫХ ВРАГОВ (ДЕФОЛТ) !!!
        public Enemy(Vector2 initialPosition, Player playerTarget)
            // Красный враг: Здоровье 3, Скорость 100f, Цвет Красный
            : this(initialPosition, playerTarget, 3, 100f, Color.Red)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsAlive || _target == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = _target.Position - Position;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            Position += direction * speed * deltaTime;
        }

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