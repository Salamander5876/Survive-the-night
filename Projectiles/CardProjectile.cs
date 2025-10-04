using Microsoft.Xna.Framework;
using Survive_the_night.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Projectiles // Убедитесь, что namespace корректен
{
    // Конкретный снаряд: самонаводящаяся игральная карта
    public class CardProjectile : Projectile
    {
        private Enemy _target; // Целевой враг

        private const float CardSpeed = 600f; // Скорость снаряда

        // Конструктор: ИСПРАВЛЕНО
        public CardProjectile(Vector2 initialPosition, Enemy target, int damage)
            // Вызываем конструктор базового класса Projectile:
            // (позиция, размер 16, цвет, урон, скорость)
            : base(initialPosition, 16, Color.Gold, damage, CardSpeed)
        {
            _target = target;
        }

        // ИСПРАВЛЕНО: Правильно переопределяем Update из Projectile
        public override void Update(GameTime gameTime, List<Enemy> enemies)
        {
            // Используем унаследованное свойство IsActive: ИСПРАВЛЕНО
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Движение к цели
            if (_target != null && _target.IsAlive)
            {
                // Вектор направления от карты к врагу
                Vector2 direction = _target.Position - Position;

                // Нормализуем, чтобы получить вектор единичной длины
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }

                // Перемещаем карту
                Position += direction * Speed * deltaTime;

                // 2. Проверка столкновения
                if (GetBounds().Intersects(_target.GetBounds()))
                {
                    OnHitEnemy(_target);
                }
            }
            else
            {
                // Если цель умерла или исчезла, снаряд исчезает сам
                IsActive = false;
            }
        }
    }
}