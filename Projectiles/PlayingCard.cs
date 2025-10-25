// Projectiles/PlayingCard.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survive_the_night.Projectiles
{
    public class PlayingCard : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _cardTexture;

        public PlayingCard(Vector2 position, int size, Color color, int damage, float speed, Vector2 target, int hitsLeft = 1, Texture2D texture = null) : base(position, size, color, damage, speed, target, hitsLeft)
        {
            _cardTexture = texture ?? _defaultTexture;

            // Автоматически определяем размер из текстуры
            if (_cardTexture != null && size == 0)
            {
                Size = Math.Max(_cardTexture.Width, _cardTexture.Height);
            }
        }

        // Метод для установки текстуры по умолчанию
        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        // Метод для установки конкретной текстуры для этой карты
        public void SetTexture(Texture2D texture)
        {
            _cardTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Увеличиваем скорость вращения для карт (быстрее чем базовое)
            Rotation += 360f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (_cardTexture != null)
            {
                // Используем метод из базового класса для отрисовки с текстурой
                DrawWithTexture(spriteBatch, _cardTexture);
            }
            else
            {
                // Запасной вариант - отрисовка прямоугольника
                base.Draw(spriteBatch, debugTexture);
            }
        }
    }
}