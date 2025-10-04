using Microsoft.Xna.Framework;
using Survive_the_night.Entities;
using System.Collections.Generic;

namespace Survive_the_night.Weapons
{
    // Базовый класс для всего оружия в игре
    public abstract class Weapon
    {
        protected Player Player { get; private set; }

        public float CooldownTime { get; protected set; }

        public int Damage { get; protected set; }

        protected float CooldownTimer { get; set; } = 0f;

        // ИСПРАВЛЕННЫЙ КОНСТРУКТОР: Добавлены аргументы damage и cooldownTime
        public Weapon(Player player, float cooldownTime, int damage)
        {
            Player = player;
            CooldownTime = cooldownTime;
            Damage = damage;
        }

        public virtual void Update(GameTime gameTime)
        {
            CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (CooldownTimer < 0f)
            {
                CooldownTimer = 0f;
            }
        }

        public abstract void Attack(GameTime gameTime, List<Enemy> enemies);

        // Вспомогательный метод для поиска ближайшего врага (часто нужен оружию)
        protected Enemy FindClosestEnemy(List<Enemy> enemies)
        {
            float minDistanceSquared = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in enemies)
            {
                // ПРЕДПОЛОЖЕНИЕ: У класса Enemy есть свойство IsAlive
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(Player.Position, enemy.Position);

                // Радиус поиска (например, 700x700 пикселей)
                if (distanceSquared < minDistanceSquared && distanceSquared < 490000)
                {
                    minDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }
    }
}