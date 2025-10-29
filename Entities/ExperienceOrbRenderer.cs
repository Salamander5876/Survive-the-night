using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Items;

namespace Survive_the_night.Entities
{
    public class ExperienceOrbRenderer : GameObject
    {
        private ExperienceOrb _orb;
        private Texture2D _texture;
        private float _rotation = 0f;

        public ExperienceOrbRenderer(ExperienceOrb orb, Texture2D texture)
            : base(orb.Position, texture?.Width ?? 8, Color.Yellow)
        {
            _orb = orb;
            _texture = texture;

            // Отладочная информация
            System.Diagnostics.Debug.WriteLine($"ExperienceOrbRenderer создан: texture={texture != null}, size={texture?.Width}x{texture?.Height}");
        }

        public override void Update(GameTime gameTime)
        {
            Position = _orb.Position; // Это мировые координаты

            // ДЕБАГ: выводим информацию о позиции рендерера
            System.Diagnostics.Debug.WriteLine($"Рендерер опыта обновлен: мировые координаты = {Position}, активен = {_orb.IsActive}");
        }

        // ПЕРЕОПРЕДЕЛЯЕМ метод Draw с вращением
        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            if (_texture != null && _orb.IsActive)
            {
                Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    _rotation, // Добавляем вращение
                    origin,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );

                System.Diagnostics.Debug.WriteLine($"Отрисовка опыта: позиция={Position}, текстура={_texture.Width}x{_texture.Height}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА отрисовки опыта: текстура={_texture != null}, активен={_orb.IsActive}");
                // Резервная отрисовка через код (без вращения)
                base.Draw(spriteBatch, debugTexture, Color.Yellow);
            }
        }

        // Метод для установки вращения из ItemManager
        public void SetRotation(float rotation)
        {
            _rotation = rotation;
        }

        public override Rectangle GetBounds()
        {
            if (_texture != null)
            {
                return new Rectangle(
                    (int)(Position.X - _texture.Width / 2f),
                    (int)(Position.Y - _texture.Height / 2f),
                    _texture.Width,
                    _texture.Height
                );
            }
            else
            {
                return base.GetBounds();
            }
        }
    }
}