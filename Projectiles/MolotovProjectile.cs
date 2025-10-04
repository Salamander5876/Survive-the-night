using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Projectiles
{
    public class MolotovProjectile : Projectile
    {
        private float _timeToLive = 5f;
        private float _burnTimer = 0f;
        private float _damageCooldown = 0.5f;
        private float _damageTimer = 0f;
        private float _radius;

        public MolotovProjectile(Vector2 position, float radius, Color color, int damage)
            // ИСПРАВЛЕНО: Устанавливаем Size, который будет использоваться для Rectangle
            : base(position, (int)radius * 2, color, damage, 0f, 0)
        {
            _radius = radius;
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _burnTimer += delta;
            _damageTimer += delta;

            if (_burnTimer >= _timeToLive)
            {
                IsActive = false;
                return;
            }

            // !!! ИСПРАВЛЕНО: Теперь наносим периодический урон !!!
            if (_damageTimer >= _damageCooldown)
            {
                ApplyDamageToEnemies(Game1.CurrentEnemies);
                _damageTimer = 0f;
            }
        }

        // !!! НОВЫЙ МЕТОД: Нанесение урона по области !!!
        private void ApplyDamageToEnemies(List<Enemy> enemies)
        {
            // Проверяем всех живых врагов
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                // Используем проверку дистанции, так как это круглый AOE
                if (Vector2.Distance(Position, enemy.Position) <= _radius)
                {
                    // Враг получает урон
                    enemy.TakeDamage(Damage);
                }
            }
        }

        // !!! ПЕРЕОПРЕДЕЛЕННЫЙ МЕТОД: Возвращаем границы области поражения !!!
        public override Rectangle GetBounds()
        {
            // Границы равны полному диаметру (_radius * 2)
            return new Rectangle(
               (int)(Position.X - _radius),
               (int)(Position.Y - _radius),
               (int)(_radius * 2),
               (int)(_radius * 2)
           );
        }

        // !!! НОВЫЙ МЕТОД: Отрисовка области поражения !!!
        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            // Рассчитываем альфа-канал для визуального затухания
            float alpha = 1f - (_burnTimer / _timeToLive);

            // Используем полупрозрачный оранжево-красный цвет
            Color drawColor = Color.OrangeRed * alpha * 0.5f;

            // Вызываем базовую отрисовку, используя правильные границы (круг будет выглядеть как квадрат)
            spriteBatch.Draw(
                debugTexture,
                GetBounds(),
                drawColor
            );
        }
    }
}