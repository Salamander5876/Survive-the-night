using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Projectiles
{
    // Игральная карта - конкретный снаряд
    public class PlayingCard : Projectile
    {
        private Vector2 _velocity;

        // ИСПРАВЛЕНО: Убрано const float Speed = 500f, чтобы не скрывать Projectile.Speed

        // ИСПРАВЛЕННЫЙ КОНСТРУКТОР: Соответствует Projectile.Projectile
        public PlayingCard(Vector2 startPosition, int size, Color color, int damage, float speed, Vector2 targetPosition)
             : base(startPosition, size, color, damage, speed)
        {
            // Расчет направления
            Vector2 direction = targetPosition - startPosition;
            direction.Normalize();

            _velocity = direction * Speed; // Используем унаследованное Speed
        }

        // ИСПРАВЛЕНО: Убираем этот метод, так как он реализован в Projectile.Update(GameTime)
        // Но так как у нас есть логика полета, мы используем единственный метод Update
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Обновляем позицию на основе рассчитанной скорости
            Position += _velocity * deltaTime;
        }

        // ПРИМЕЧАНИЕ: Методы Draw и GetBounds наследуются от GameObject
    }
}