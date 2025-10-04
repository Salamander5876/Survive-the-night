using Microsoft.Xna.Framework;
using Math = System.Math;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using System.Collections.Generic;
using System.Diagnostics; // Добавлено для отладки

namespace Survive_the_night.Weapons
{
    public class MolotovCocktail : Weapon
    {
        public int NumBottles { get; private set; } = 1;
        public float Radius { get; private set; } = 50f;

        public List<MolotovProjectile> ActiveAreas { get; private set; } = new List<MolotovProjectile>();

        // ИСХОДНЫЕ ЗНАЧЕНИЯ: 2.0s кулдаун, 2 урон.
        public MolotovCocktail(Player player) : base(player, 2.0f, 2)
        {
        }

        public override void LevelUp()
        {
            if (Level >= MAX_LEVEL) return;

            Level++;

            Damage += 1;
            Radius += 15f;

            // Количество бутылок +1 каждые 2 уровня
            if (Level % 2 == 0)
            {
                NumBottles += 1;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Обновление всех активных областей поражения
            for (int i = ActiveAreas.Count - 1; i >= 0; i--)
            {
                var area = ActiveAreas[i];
                if (area.IsActive)
                {
                    area.Update(gameTime);
                }
                else
                {
                    ActiveAreas.RemoveAt(i);
                }
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (CooldownTimer <= 0f)
            {
                for (int i = 0; i < NumBottles; i++)
                {
                    // !!! ИСПРАВЛЕНИЕ: Бросаем Молотов в радиусе 500 вокруг игрока !!!
                    float spawnRadius = 500f;
                    float angle = (float)Game1.Random.NextDouble() * MathHelper.TwoPi;
                    float distance = (float)Game1.Random.NextDouble() * spawnRadius;

                    Vector2 offset = new Vector2(
                        (float)Math.Cos(angle) * distance,
                        (float)Math.Sin(angle) * distance
                    );

                    Vector2 randomPositionNearPlayer = Player.Position + offset;

                    // Создаем Молотов
                    MolotovProjectile area = new MolotovProjectile(randomPositionNearPlayer, Radius, Color.Orange, this.Damage);
                    ActiveAreas.Add(area);

                    // Диагностика:
                    Debug.WriteLine($"Молотов создан: Позиция ({area.Position.X:0}, {area.Position.Y:0}), Радиус: {Radius:0}");
                }

                CooldownTimer = CooldownTime;
            }
        }
    }
}