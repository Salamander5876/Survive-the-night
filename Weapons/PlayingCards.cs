using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;

namespace Survive_the_night.Weapons
{
    public class PlayingCards : Weapon
    {
        // ИСПРАВЛЕНО: Используем 'new' для скрытия Weapon.Damage, 
        // чтобы LevelUpMenu мог к нему обратиться. 
        public new int Damage { get; private set; }

        private float _attackTimer = 0f;

        public List<Projectile> ActiveProjectiles { get; private set; } = new List<Projectile>();

        // ИСПРАВЛЕННЫЙ КОНСТРУКТОР: Передаем начальные значения в Weapon
        // ИСХОДНЫЕ ЗНАЧЕНИЯ: 0.5s кулдаун, 1 урон.
        public PlayingCards(Player player) : base(player, 0.5f, 1)
        {
            // Теперь Damage инициализируется в базовом классе Weapon, но мы хотим, 
            // чтобы LevelUpMenu использовал нашу публичную версию, 
            // поэтому вручную устанавливаем нашу публичную версию.
            this.Damage = base.Damage;
        }

        public void UpgradeDamage(int amount)
        {
            this.Damage += amount;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime); // Обновляем таймер в базовом классе

            // Обновление всех активных снарядов
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var card = ActiveProjectiles[i];
                if (card.IsActive)
                {
                    card.Update(gameTime); // Обновление Projectile.Update(GameTime)
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
                Enemy target = FindClosestEnemy(enemies);

                if (target != null)
                {
                    // ПРЕДПОЛОЖЕНИЕ: PlayingCard имеет Size=16 и Color=Color.Red
                    PlayingCard card = new PlayingCard(Player.Position, 16, Color.Red, this.Damage, 500f, target.Position);
                    ActiveProjectiles.Add(card);

                    CooldownTimer = CooldownTime; // Сброс таймера из базового класса
                }
            }

            CheckProjectileCollisions(enemies);
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                // ПРЕДПОЛОЖЕНИЕ: У класса Enemy есть свойство IsAlive
                if (!enemy.IsAlive) continue;

                for (int j = ActiveProjectiles.Count - 1; j >= 0; j--)
                {
                    Projectile projectile = ActiveProjectiles[j];
                    if (!projectile.IsActive) continue;

                    if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        // Столкновение!
                        // Используем доступное свойство Damage из Projectile
                        enemy.TakeDamage(projectile.Damage);
                        projectile.IsActive = false;

                        break;
                    }
                }
            }
        }
    }
}