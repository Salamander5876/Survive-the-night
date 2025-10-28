// StrongEnemy.cs
using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class StrongEnemy : Enemy
    {
        public StrongEnemy(Vector2 initialPosition, Player playerTarget, int stage)
            : base(initialPosition, playerTarget, 5, 100f, Color.Orange, 10, stage)
        {
        }
    }
}