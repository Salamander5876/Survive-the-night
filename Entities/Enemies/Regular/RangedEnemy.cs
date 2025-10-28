// RangedEnemy.cs
using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class RangedEnemy : Enemy
    {
        public RangedEnemy(Vector2 initialPosition, Player playerTarget, int stage)
            : base(initialPosition, playerTarget, 5, 80f, Color.Purple, 5, stage)
        {
        }
    }
}