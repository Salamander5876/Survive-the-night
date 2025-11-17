using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Scripts.Managers;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class FastEnemy : Enemy
    {
        public FastEnemy(Vector2 initialPosition, Player playerTarget, int stage, DifficultyManager difficultyManager)
            : base(initialPosition, playerTarget, 3, 180f, Color.Yellow, 6, stage, difficultyManager)
        {
            Damage = CalculateDamageForStage(6, stage);
            speed = CalculateSpeedForStage(180f, stage);
        }

        public FastEnemy(Vector2 initialPosition, Player playerTarget)
            : this(initialPosition, playerTarget, 1, null)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            Color drawColor = color ?? Color;
            int size = Size - 4;
            spriteBatch.Draw(debugTexture,
                new Rectangle((int)(Position.X - size / 2), (int)(Position.Y - size / 2), size, size),
                drawColor);
        }
    }
}