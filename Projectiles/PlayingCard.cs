using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Projectiles
{
    // Игральная карта - конкретный снаряд
    public class PlayingCard : Projectile
    {
        private Vector2 _velocity;
        private float _rotation;
        private static Texture2D _texture; // общий спрайт пули
        private const int BulletWidth = 10;
        private const int BulletHeight = 16;

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
            // Вращение: "верх" текстуры должен смотреть по направлению движения
            _rotation = (float)System.Math.Atan2(_velocity.Y, _velocity.X) + MathHelper.PiOver2;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Обновляем позицию на основе рассчитанной скорости
            Position += _velocity * deltaTime;
        }

        // Загрузка текстуры из Game1 один раз (лениво)
        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }

        // Рисуем пулю как спрайт, если текстура доступна; иначе квадрат
        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            if (_texture != null)
            {
                // Масштабируем текстуру под желаемые размеры 10x16
                Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
                float scaleX = BulletWidth / (float)_texture.Width;
                float scaleY = BulletHeight / (float)_texture.Height;
                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White, // не тонируем, чтобы сохранить исходный цвет спрайта
                    _rotation,
                    origin,
                    new Vector2(scaleX, scaleY),
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                base.Draw(spriteBatch, debugTexture, color);
            }
        }

        // Прямоугольный хитбокс 10x16, ось-выравнивание (без поворота) — просто и стабильно
        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(Position.X - BulletWidth / 2f),
                (int)(Position.Y - BulletHeight / 2f),
                BulletWidth,
                BulletHeight
            );
        }
    }
}