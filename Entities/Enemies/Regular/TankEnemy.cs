// TankEnemy.cs
using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class TankEnemy : Enemy
    {
        public TankEnemy(Vector2 initialPosition, Player playerTarget, int stage)
            : base(initialPosition, playerTarget, 5, 80f, Color.Green, 5, stage)
        {
            DamageResistance = 0.5f;
        }
    }
}