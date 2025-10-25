using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Items;

namespace Survive_the_night.Entities
{
    public class DynamiteRenderer : GameObject
    {
        private Dynamite _dynamite;
        private Texture2D _texture;

        public DynamiteRenderer(Dynamite dynamite, Texture2D texture)
            : base(dynamite.Position, texture?.Width ?? 16, Color.White)
        {
            _dynamite = dynamite;
            _texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            Position = _dynamite.Position;
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
                    0f,
                    origin,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                base.Draw(spriteBatch, debugTexture, Color.Red);
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