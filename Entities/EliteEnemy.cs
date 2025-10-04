using Microsoft.Xna.Framework;
using System;

namespace Survive_the_night.Entities
{
    public class EliteEnemy : Enemy
    {
        // Награда за убийство
        public const int ExperienceOrbCount = 10;
        public const int ChestDropCount = 1;

        public EliteEnemy(Vector2 initialPosition, Player playerTarget)
            // Синий Элитный Враг: Здоровье 30 (в 10 раз больше), Скорость 80f (немного медленнее), Цвет Синий
            : base(initialPosition, playerTarget, 30, 80f, Color.Blue)
        {
        }
    }
}