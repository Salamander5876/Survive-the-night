// UndyingEnemy.cs
using Microsoft.Xna.Framework;

namespace Survive_the_night.Entities.Enemies.Regular
{
    public class UndyingEnemy : Enemy
    {
        public UndyingEnemy(Vector2 initialPosition, Player playerTarget, int stage)
            : base(initialPosition, playerTarget, 5, 100f, Color.White, 5, stage)
        {
            HasUndying = true;
        }
    }
}