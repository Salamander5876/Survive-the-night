using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Scripts.Managers;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class VampireEnemy : Enemy
    {
        public VampireEnemy(Vector2 initialPosition, Player playerTarget, int stage, DifficultyManager difficultyManager)
            : base(initialPosition, playerTarget, 6, 90f, Color.DarkRed, 12, stage, difficultyManager)
        {
            Damage = CalculateDamageForStage(12, stage);
            speed = CalculateSpeedForStage(90f, stage);
            Vampirism = 0.2f;
        }

        public VampireEnemy(Vector2 initialPosition, Player playerTarget)
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