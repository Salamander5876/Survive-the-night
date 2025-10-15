// Projectiles/CasinoChip.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Projectiles
{
    public class CasinoChip : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _chipTexture;

        public CasinoChip(Vector2 position, int size, Color color, int damage, float speed, Vector2 target, int hitsLeft = 1, Texture2D texture = null)
            : base(position, size, color, damage, speed, target, hitsLeft)
        {
            _chipTexture = texture ?? _defaultTexture;

            // Увеличиваем время жизни до 60 секунд
            SetLifeTime(60f);
        }

        // Метод для установки текстуры по умолчанию
        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        // Метод для установки конкретной текстуры для этой фишки
        public void SetTexture(Texture2D texture)
        {
            _chipTexture = texture;
        }

        // Метод для обновления направления
        public void UpdateDirection(Vector2 newTarget)
        {
            Direction = Vector2.Normalize(newTarget - Position);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Вращение фишки
            Rotation += 180f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (_chipTexture != null)
            {
                // Используем метод из базового класса для отрисовки с текстурой
                DrawWithTexture(spriteBatch, _chipTexture);
            }
            else
            {
                // Запасной вариант - отрисовка прямоугольника
                base.Draw(spriteBatch, debugTexture);
            }
        }
    }
}