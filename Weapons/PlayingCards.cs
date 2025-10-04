using Microsoft.Xna.Framework;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles; // Важно: для доступа к CardProjectile
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Weapons // Убедитесь, что namespace корректен
{
    // ИСПРАВЛЕНО: Класс должен наследоваться от Weapon
    public class PlayingCards : Weapon
    {
        private const float BaseCooldown = 1.0f;

        // ИСПРАВЛЕНО: ActiveProjectiles теперь public get
        // Это устраняет ошибку доступности, позволяя Game1.cs увидеть его
        public List<CardProjectile> ActiveProjectiles { get; } = new List<CardProjectile>();

        // ИСПРАВЛЕНО: Конструктор, принимающий Player, вызывает базовый конструктор
        public PlayingCards(Player player)
            : base(player, BaseCooldown, 1)
        {
        }

        // ... (Update и Attack остаются прежними)
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Обновляем и удаляем неактивные снаряды
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = ActiveProjectiles[i];
                // Обновляем, передавая пустой список врагов, т.к. CardProjectile уже знает свою цель
                projectile.Update(gameTime, new List<Enemy>());

                if (!projectile.IsActive)
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (CooldownTimer <= 0f)
            {
                // 1. Находим ближайшего живого врага
                var target = enemies
                    .Where(e => e.IsAlive)
                    .OrderBy(e => Vector2.Distance(Player.Position, e.Position))
                    .FirstOrDefault();

                // 2. Если цель найдена, спауним карту
                if (target != null)
                {
                    var newCard = new CardProjectile(Player.Position, target, Damage);
                    ActiveProjectiles.Add(newCard);

                    // 3. Сбрасываем таймер перезарядки
                    CooldownTimer = CooldownTime;
                }
            }
        }
    }
}