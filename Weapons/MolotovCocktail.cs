using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Survive_the_night.Weapons
{
    public class MolotovCocktail : Weapon
    {
        public int NumBottles { get; private set; } = 1;
        public float BurnDuration { get; private set; } = 5f;
        public float DamageInterval { get; private set; } = 1.2f;

        public List<MolotovProjectile> ActiveBottles { get; private set; } = new List<MolotovProjectile>();
        public List<FireArea> ActiveFires { get; private set; } = new List<FireArea>();

        private static Texture2D _bottleTexture;
        private static Texture2D _fireTexture;

        public int CountLevel { get; private set; } = 0;
        public int DurationLevel { get; private set; } = 0;
        public int RateLevel { get; private set; } = 0;

        public MolotovCocktail(Player player) : base(player, 5.0f, 2)
        {
        }

        public static void SetTextures(Texture2D bottleTexture, Texture2D fireTexture)
        {
            _bottleTexture = bottleTexture;
            _fireTexture = fireTexture;
        }

        // УДАЛИТЬ метод SetSoundsInstance - больше не нужен!

        public override void LevelUp()
        {
            if (Level >= MAX_LEVEL) return;
            Level++;
        }

        public void UpgradeBottleCount()
        {
            if (CountLevel >= 5) return;
            NumBottles += 1;
            CountLevel++;
        }

        public void UpgradeBurnDuration()
        {
            if (DurationLevel >= 5) return;
            BurnDuration += 5f;
            DurationLevel++;
        }

        public void UpgradeDamageRate()
        {
            if (RateLevel >= 5) return;
            DamageInterval -= 0.2f;
            RateLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Обновление летящих бутылок
            for (int i = ActiveBottles.Count - 1; i >= 0; i--)
            {
                var bottle = ActiveBottles[i];
                if (bottle.IsActive)
                {
                    bottle.Update(gameTime);
                }
                else
                {
                    // Создаем огненную область
                    var fireArea = new FireArea(
                        bottle.Position,
                        (int)(_fireTexture.Width * 1.5f),
                        Color.White,
                        Damage,
                        BurnDuration,
                        DamageInterval
                    );

                    ActiveFires.Add(fireArea);
                    ActiveBottles.RemoveAt(i);
                }
            }

            // Обновление активных огненных областей
            bool anyFireActive = false;
            for (int i = ActiveFires.Count - 1; i >= 0; i--)
            {
                var fire = ActiveFires[i];
                if (fire.IsActive)
                {
                    fire.Update(gameTime);
                    anyFireActive = true;
                }
                else
                {
                    ActiveFires.RemoveAt(i);
                }
            }

            // Перезарядка происходит только когда нет активных огней
            if (anyFireActive)
            {
                CooldownTimer = CooldownTime;
            }
        }

        public override void Attack(GameTime gameTime, System.Collections.Generic.List<Enemy> enemies)
        {
            if (ActiveFires.Count == 0 && CooldownTimer <= 0f)
            {
                Debug.WriteLine("🎯 Molotov attacking!");

                for (int i = 0; i < NumBottles; i++)
                {
                    // Бросаем Молотов в радиусе 300-500 вокруг игрока
                    float spawnRadius = 300f + (float)Game1.Random.NextDouble() * 200f;
                    float angle = (float)Game1.Random.NextDouble() * MathHelper.TwoPi;

                    Vector2 offset = new Vector2(
                        (float)System.Math.Cos(angle) * spawnRadius,
                        (float)System.Math.Sin(angle) * spawnRadius
                    );

                    Vector2 randomPositionNearPlayer = Player.Position + offset;

                    // Создаем Молотов
                    MolotovProjectile bottle = new MolotovProjectile(
                        Player.Position,
                        randomPositionNearPlayer,
                        _bottleTexture.Width,
                        Color.White,
                        300f
                    );
                    ActiveBottles.Add(bottle);
                }

                CooldownTimer = CooldownTime;
            }
        }

        public void DrawProjectiles(SpriteBatch spriteBatch)
        {
            foreach (var bottle in ActiveBottles)
            {
                if (bottle.IsActive)
                {
                    bottle.DrawWithTexture(spriteBatch, _bottleTexture);
                }
            }

            foreach (var fire in ActiveFires)
            {
                if (fire.IsActive)
                {
                    fire.DrawWithTexture(spriteBatch, _fireTexture);
                }
            }
        }
    }
}