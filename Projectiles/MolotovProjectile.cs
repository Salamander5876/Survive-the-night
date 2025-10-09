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
            : base(position, (int)radius * 2, color, damage, 0f, position, 1) // Исправлен конструктор
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

            // Наносим периодический урон
            if (_damageTimer >= _damageCooldown)
            {
                ApplyDamageToEnemies(Game1.CurrentEnemies);
                _damageTimer = 0f;
            }
        }

        // Нанесение урона по области
        private void ApplyDamageToEnemies(List<Enemy> enemies)
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                if (Vector2.Distance(Position, enemy.Position) <= _radius)
                {
                    enemy.TakeDamage(Damage);
                }
            }
        }

        // Границы области поражения
        public override Rectangle GetBounds()
        {
            return new Rectangle(
               (int)(Position.X - _radius),
               (int)(Position.Y - _radius),
               (int)(_radius * 2),
               (int)(_radius * 2)
           );
        }

        // ИСПРАВЛЕННЫЙ МЕТОД: Убрали необязательный параметр color
        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            // Рассчитываем альфа-канал для визуального затухания
            float alpha = 1f - (_burnTimer / _timeToLive);

            // Используем полупрозрачный оранжево-красный цвет
            Color drawColor = Color.OrangeRed * alpha * 0.5f;

            // Отрисовываем область
            spriteBatch.Draw(
                debugTexture,
                GetBounds(),
                drawColor
            );
        }
    }
}