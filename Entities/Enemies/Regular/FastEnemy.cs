// FastEnemy.cs
using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class FastEnemy : Enemy
    {
        public FastEnemy(Vector2 initialPosition, Player playerTarget, int stage)
            : base(initialPosition, playerTarget, 5, 150f, Color.Yellow, 5, stage)
        {
        }
    }
}