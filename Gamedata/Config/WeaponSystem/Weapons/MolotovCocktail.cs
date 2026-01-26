using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class MolotovCocktail : Weapon
    {
        public int NumBottles { get; private set; } = 2;
        public float BurnDuration { get; private set; } = 10f;
        public float DamageInterval { get; private set; } = 0.2f; // ФИКСИРОВАННЫЙ интервал

        public List<MolotovProjectile> ActiveBottles { get; private set; } = new List<MolotovProjectile>();
        public List<FireArea> ActiveFires { get; private set; } = new List<FireArea>();

        private static Texture2D _bottleTexture;
        private static List<Texture2D> _fireTextures = new List<Texture2D>(); // Изменено на список текстур

        public int CountLevel { get; private set; } = 0;
        public int DurationLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;

        public MolotovCocktail(Player player) : base(player, WeaponType.Legendary, WeaponName.MolotovCocktail, 5.0f, 1)
        {
        }

        // Обновленный метод для установки текстур анимации
        public static void SetTextures(Texture2D bottleTexture, params Texture2D[] fireTextures)
        {
            _bottleTexture = bottleTexture;
            _fireTextures.Clear();
            _fireTextures.AddRange(fireTextures);
        }

        // Метод для настройки скорости анимации всех активных огней
        public void SetFireAnimationSpeed(float frameTime)
        {
            foreach (var fire in ActiveFires)
            {
                fire.SetAnimationSpeed(frameTime);
            }
        }

        public override void LevelUp()
        {
            if (Level >= MAX_LEVEL) return;
            Level++;
        }

        public void UpgradeBottleCount()
        {
            if (CountLevel >= 3) return;
            NumBottles += 2;
            CountLevel++;
        }

        public void UpgradeBurnDuration()
        {
            if (DurationLevel >= 3) return;
            BurnDuration += 10f;
            DurationLevel++;
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 3) return;
            Damage += 1;
            DamageLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = ActiveBottles.Count - 1; i >= 0; i--)
            {
                var bottle = ActiveBottles[i];
                if (bottle.IsActive)
                {
                    bottle.Update(gameTime);
                }
                else
                {
                    var fireArea = new FireArea(
                        bottle.Position,
                        (int)((_fireTextures.Count > 0 ? _fireTextures[0].Width : 64) * 1.5f),
                        Color.White,
                        Damage,
                        BurnDuration,
                        DamageInterval,
                        _fireTextures
                    );

                    // Настраиваем скорость анимации для нового огня
                    fireArea.SetAnimationSpeed(0.15f); // Стандартная скорость

                    ActiveFires.Add(fireArea);
                    ActiveBottles.RemoveAt(i);
                }
            }

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

            if (anyFireActive)
            {
                CooldownTimer = CooldownTime;
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (ActiveFires.Count == 0 && CooldownTimer <= 0f)
            {
                //Debug.WriteLine("Molotov attacking!");

                // Проигрываем звук броска через WeaponManager
                WeaponManager.PlayWeaponSound(WeaponName.MolotovCocktail);

                for (int i = 0; i < NumBottles; i++)
                {
                    float spawnRadius = 300f + (float)Game1.Random.NextDouble() * 200f;
                    float angle = (float)Game1.Random.NextDouble() * MathHelper.TwoPi;

                    Vector2 offset = new Vector2(
                        (float)System.Math.Cos(angle) * spawnRadius,
                        (float)System.Math.Sin(angle) * spawnRadius
                    );

                    Vector2 randomPositionNearPlayer = Player.Position + offset;

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
                    // Теперь DrawWithTexture будет использовать текущую текстуру анимации
                    fire.DrawWithTexture(spriteBatch, null);
                }
            }
        }

        public void PauseAllSounds()
        {
            foreach (var fire in ActiveFires)
            {
                fire.PauseSound();
            }
        }

        public void ResumeAllSounds()
        {
            foreach (var fire in ActiveFires)
            {
                fire.ResumeSound();
            }
        }

        public void StopAllSounds()
        {
            foreach (var fire in ActiveFires)
            {
                fire.StopSound();
            }
            //System.Diagnostics.Debug.WriteLine("Остановлены все звуки MolotovCocktail");
        }
    }
}