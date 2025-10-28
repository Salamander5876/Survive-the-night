// VampireEnemy.cs
using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class VampireEnemy : Enemy
    {
        public VampireEnemy(Vector2 initialPosition, Player playerTarget, int stage)
            : base(initialPosition, playerTarget, 5, 100f, new Color(139, 0, 0), 5, stage)
        {
            Vampirism = 0.25f;
        }
    }
}