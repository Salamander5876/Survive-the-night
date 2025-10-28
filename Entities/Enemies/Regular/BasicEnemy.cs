// BasicEnemy.cs
using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class BasicEnemy : Enemy
    {
        public BasicEnemy(Vector2 initialPosition, Player playerTarget, int stage)
            : base(initialPosition, playerTarget, 5, 100f, Color.Red, 5, stage)
        {
        }
    }
}