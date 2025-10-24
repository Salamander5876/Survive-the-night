using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Items;

namespace Survive_the_night.Entities
{
    public class ExperienceOrbRenderer : GameObject
    {
        private ExperienceOrb _orb;

        public ExperienceOrbRenderer(ExperienceOrb orb)
            : base(orb.Position, 8, Color.Yellow)
        {
            _orb = orb;
        }

        public override void Update(GameTime gameTime)
        {
            Position = _orb.Position;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            // Отрисовка через код (без текстуры)
            base.Draw(spriteBatch, debugTexture, Color.Yellow);
        }
    }
}