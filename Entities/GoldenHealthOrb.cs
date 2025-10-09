using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    public class GoldenHealthOrb : BaseHealthOrb
    {
        private static Texture2D _texture;
        public GoldenHealthOrb(Vector2 initialPosition, float healPercentage = 0.5f)
            : base(initialPosition, OrbHeight, Color.Gold, healPercentage) { }
        public static void SetTexture(Texture2D texture) { _texture = texture; }
        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            if (_texture != null)
            {
                Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
                float scaleX = OrbWidth / (float)_texture.Width;
                float scaleY = OrbHeight / (float)_texture.Height;
                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    0f,
                    origin,
                    new Vector2(scaleX, scaleY),
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                base.Draw(spriteBatch, debugTexture, color ?? this.Color);
            }
        }
        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(Position.X - OrbWidth / 2f),
                (int)(Position.Y - OrbHeight / 2f),
                OrbWidth,
                OrbHeight
            );
        }
    }
}
