using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Projectiles
{
    // Игральная карта - конкретный снаряд
    public class PlayingCard : Projectile
    {
        private Vector2 _velocity;

        // ИСПРАВЛЕННЫЙ КОНСТРУКТОР: Добавлен hitsLeft
        public PlayingCard(Vector2 startPosition, int size, Color color, int damage, float speed, Vector2 targetPosition, int hitsLeft)
             : base(startPosition, size, color, damage, speed, hitsLeft) // Передаем hitsLeft в base
        {
            // Расчет направления
            Vector2 direction = targetPosition - startPosition;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            _velocity = direction * Speed; // Используем унаследованное Speed
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Обновляем позицию на основе рассчитанной скорости
            Position += _velocity * deltaTime;
        }

        // ПРИМЕЧАНИЕ: Методы Draw и GetBounds наследуются от GameObject
    }
}