using System;

namespace Survive_the_night.Scripts.Managers
{
    // Статический класс для методов расширения стандартного класса Random
    public static class RandomExtensions
    {
        // Метод, позволяющий получить случайное число типа float в заданном диапазоне
        public static float NextFloat(this Random random, float minimum, float maximum)
        {
            return (float)random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}