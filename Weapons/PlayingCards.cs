// Weapons/PlayingCards.cs

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;

namespace Survive_the_night.Weapons
{
    public class PlayingCards : Weapon
    {
        public int NumCards { get; private set; } = 1;
        public bool IsPiercing { get; private set; } = false;
        // !!! НОВОЕ СВОЙСТВО: Дальность атаки !!!
        public float Range { get; private set; } = 0.20f; // Начальная дальность в 300 единиц
        public List<Projectile> ActiveProjectiles { get; private set; } = new List<Projectile>();

        public PlayingCards(Player player) : base(player, 0.5f, 1)
        {
        }

        public override void LevelUp()
        {
            if (Level >= MAX_LEVEL) return;

            Level++;

            Damage += 100;
            NumCards += 1;

            if (Level >= 7)
            {
                IsPiercing = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var card = ActiveProjectiles[i];
                if (card.IsActive)
                {
                    card.Update(gameTime);
                }
                else
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (CooldownTimer <= 0f)
            {
                for (int i = 0; i < NumCards; i++)
                {
                    Enemy target = FindClosestEnemy(enemies);

                    if (target != null)
                    {
                        Vector2 offset = new Vector2(
                            (float)Game1.Random.NextDouble() * 10 - 5,
                            (float)Game1.Random.NextDouble() * 10 - 5
                        );

                        // PlayingCard должен принимать параметр для пробивания
                        PlayingCard card = new PlayingCard(
                            Player.Position + offset,
                            16,
                            Color.Red,
                            this.Damage,
                            500f,
                            target.Position,
                            this.IsPiercing ? int.MaxValue : 1 // 1-цель или бесконечно
                        );
                        ActiveProjectiles.Add(card);
                    }
                }

                CooldownTimer = CooldownTime;
            }

            CheckProjectileCollisions(enemies);
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                if (!enemy.IsAlive) continue;

                for (int j = ActiveProjectiles.Count - 1; j >= 0; j--)
                {
                    // Предполагаем, что PlayingCard является Projectile
                    Projectile projectile = ActiveProjectiles[j];

                    if (!projectile.IsActive) continue;

                    if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(projectile.Damage);

                        // Логика пробивания (projectile.HitsLeft - 1)
                        if (IsPiercing)
                        {
                            // Если пробивает, projectile.HitsLeft остается > 0
                        }
                        else
                        {
                            projectile.IsActive = false; // Иначе удаляем
                        }

                        if (!projectile.IsActive) break; // Если карта удалена, переходим к следующему врагу
                    }
                }
            }
        }
        // !!! НОВЫЙ МЕТОД: Поиск ближайшего врага с ограничением дальности !!!
        protected Enemy FindClosestEnemyInRange(List<Enemy> enemies, float maxRange)
        {
            float maxRangeSquared = maxRange * maxRange;
            float minDistanceSquared = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(Player.Position, enemy.Position);

                // Если враг ближе, чем максимальная дальность И ближе, чем предыдущий найденный
                if (distanceSquared < maxRangeSquared && distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }
    }
}