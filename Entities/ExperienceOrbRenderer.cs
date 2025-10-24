using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Items;

namespace Survive_the_night.Entities
{
    public class ExperienceOrbRenderer : GameObject
    {
        private ExperienceOrb _orb;
        private Texture2D _texture;

        public ExperienceOrbRenderer(ExperienceOrb orb, Texture2D texture)
            : base(orb.Position, texture?.Width ?? 8, Color.Yellow)
        {
            _orb = orb;
            _texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            Position = _orb.Position;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            if (_texture != null)
            {
                // Используем натуральный размер текстуры
                Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    0f,
                    origin,
                    1.0f, // Масштаб 1:1
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                // Резервная отрисовка через код
                base.Draw(spriteBatch, debugTexture, Color.Yellow);
            }
        }

        public override Rectangle GetBounds()
        {
            // Хитбокс соответствует размеру текстуры
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