using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Items;

namespace Survive_the_night.Entities
{
    public class HealthOrbRenderer : GameObject
    {
        private static Texture2D _texture;
        private HealthOrb _orb;

        public HealthOrbRenderer(HealthOrb orb)
            : base(orb.Position, _texture?.Width ?? 22, Color.LimeGreen)
        {
            _orb = orb;
        }

        public static void SetTexture(Texture2D texture) { _texture = texture; }

        public override void Update(GameTime gameTime)
        {
            Position = _orb.Position;
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
                    1.0f, // Масштаб 1:1
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                base.Draw(spriteBatch, debugTexture, Color.LimeGreen);
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