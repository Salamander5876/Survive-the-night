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
            // Синий Элитный Враг: 
            // 1. Здоровье: 30 
            // 2. Скорость: 80f 
            // 3. Цвет: Color.Blue
            // 4. !!! ИСПРАВЛЕНИЕ: Урон: 15 (установлен явно для элитки) !!!
            : base(initialPosition, playerTarget, 30, 80f, Color.Blue, 15)
        {
        }
    }
}