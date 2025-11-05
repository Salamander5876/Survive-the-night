using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Managers;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class StrongEnemy : Enemy
    {
        public StrongEnemy(Vector2 initialPosition, Player playerTarget, int stage, DifficultyManager difficultyManager)
            : base(initialPosition, playerTarget, 8, 80f, Color.Orange, 15, stage, difficultyManager)
        {
            Damage = CalculateDamageForStage(15, stage);
            speed = CalculateSpeedForStage(80f, stage);
        }

        public StrongEnemy(Vector2 initialPosition, Player playerTarget)
            : this(initialPosition, playerTarget, 1, null)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            Color drawColor = color ?? Color;
            spriteBatch.Draw(debugTexture,
                new Rectangle((int)(Position.X - Size / 2), (int)(Position.Y - Size / 2), Size, Size),
                drawColor);
        }
    }
}