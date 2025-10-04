// Weapons/Weapon.cs

using Microsoft.Xna.Framework;
using Survive_the_night.Entities;
using System.Collections.Generic;

namespace Survive_the_night.Weapons
{
    public abstract class Weapon
    {
        protected Player Player { get; private set; }

        public float CooldownTime { get; protected set; }

        public int Damage { get; protected set; }

        public int Level { get; protected set; } = 1;
        public const int MAX_LEVEL = 10;

        protected float CooldownTimer { get; set; } = 0f;

        public Weapon(Player player, float cooldownTime, int damage)
        {
            Player = player;
            CooldownTime = cooldownTime;
            Damage = damage;
        }

        public abstract void LevelUp(); // Метод для прокачки

        public virtual void Update(GameTime gameTime)
        {
            CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (CooldownTimer < 0f)
            {
                CooldownTimer = 0f;
            }
        }

        public abstract void Attack(GameTime gameTime, List<Enemy> enemies);

        protected Enemy FindClosestEnemy(List<Enemy> enemies)
        {
            float minDistanceSquared = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(Player.Position, enemy.Position);

                // Ограничиваем радиус поиска, чтобы не сканировать всю карту
                if (distanceSquared < minDistanceSquared && distanceSquared < 490000) // 700x700
                {
                    minDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }
    }
}