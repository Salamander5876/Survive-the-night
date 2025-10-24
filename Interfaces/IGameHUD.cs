using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Interfaces
{
    public interface IGameHUD
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
        void DrawStageAnnouncement(SpriteBatch spriteBatch);
        void ShowStageAnnouncement(int stage);
    }
}