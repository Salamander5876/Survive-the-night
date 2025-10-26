using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Items;

namespace Survive_the_night.Entities
{
    public class MagnetRenderer : GameObject
    {
        private Magnet _magnet;
        private Texture2D _texture;

        public MagnetRenderer(Magnet magnet, Texture2D texture)
            : base(magnet.Position, texture?.Width ?? 16, Color.White)
        {
            _magnet = magnet;
            _texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            Position = _magnet.Position;
            // УБРАЛИ вращение - магнит не крутится пока не подобран
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            if (_texture != null)
            {
                Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    0f, // УБРАЛИ вращение
                    origin,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                base.Draw(spriteBatch, debugTexture, Color.Blue);
            }
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