using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Scripts.Managers;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class TankEnemy : Enemy
    {
        public TankEnemy(Vector2 initialPosition, Player playerTarget, int stage, DifficultyManager difficultyManager)
            : base(initialPosition, playerTarget, 15, 60f, Color.Gray, 8, stage, difficultyManager)
        {
            Damage = CalculateDamageForStage(8, stage);
            speed = CalculateSpeedForStage(60f, stage);
            DamageResistance = 0.3f;
        }

        public TankEnemy(Vector2 initialPosition, Player playerTarget)
            : this(initialPosition, playerTarget, 1, null)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            Color drawColor = color ?? Color;
            int size = Size + 8;
            spriteBatch.Draw(debugTexture,
                new Rectangle((int)(Position.X - size / 2), (int)(Position.Y - size / 2), size, size),
                drawColor);
        }
    }
}