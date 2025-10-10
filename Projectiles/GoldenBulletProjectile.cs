// Projectiles/GoldenBulletProjectile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survive_the_night.Projectiles
{
    public class GoldenBulletProjectile : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _bulletTexture;

        public GoldenBulletProjectile(Vector2 position, int size, Color color, int damage, float speed, Vector2 target, Texture2D texture = null)
            : base(position, size, color, damage, speed, target, 1) // hitsLeft = 1 (без пробити€)
        {
            _bulletTexture = texture ?? _defaultTexture;

            // ƒобавл€ем начальный поворот, чтобы пул€ летела верхней стороной вперед
            Rotation = CalculateRotationToTarget(target);
        }

        // ћетод дл€ установки текстуры по умолчанию
        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ќбновл€ем поворот, чтобы пул€ всегда была направлена в сторону движени€
            Rotation = CalculateRotationToDirection(Direction);
        }

        // –асчет поворота к цели
        private float CalculateRotationToTarget(Vector2 target)
        {
            Vector2 direction = Vector2.Normalize(target - Position);
            return CalculateRotationToDirection(direction);
        }

        // –асчет поворота по направлению
        private float CalculateRotationToDirection(Vector2 direction)
        {
            // јтангенс2 возвращает угол в радианах, конвертируем в градусы
            // » добавл€ем 90 градусов, чтобы верх текстуры смотрел вперед
            float angle = MathHelper.ToDegrees((float)Math.Atan2(direction.Y, direction.X)) + 90f;
            return angle;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (_bulletTexture != null)
            {
                // »спользуем метод из базового класса дл€ отрисовки с текстурой
                DrawWithTexture(spriteBatch, _bulletTexture);
            }
            else
            {
                // «апасной вариант - отрисовка пр€моугольника
                base.Draw(spriteBatch, debugTexture);
            }
        }
    }
}