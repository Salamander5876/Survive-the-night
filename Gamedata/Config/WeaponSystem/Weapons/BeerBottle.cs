using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class BeerBottle : Weapon
    {
        public int NumBottles { get; private set; } = 1;
        public float PuddleDuration { get; private set; } = 5f;
        public float DamageInterval { get; private set; } = 2f;

        public List<BeerBottleProjectile> ActiveBottles { get; private set; } = new List<BeerBottleProjectile>();
        public List<BeerPuddle> ActivePuddles { get; private set; } = new List<BeerPuddle>();

        private static Texture2D _bottleTexture;
        private static Texture2D _puddleTexture;

        public int CountLevel { get; private set; } = 0;
        public int DurationLevel { get; private set; } = 0;
        public int IntervalLevel { get; private set; } = 0;

        public BeerBottle(Player player) : base(player, WeaponType.Regular, WeaponName.BeerBottle, 3.0f, 4)
        {
        }

        public static void SetTexturesAndSounds(Texture2D bottleTexture, Texture2D puddleTexture, SoundEffect throwSound, SoundEffect puddleSound)
        {
            _bottleTexture = bottleTexture;
            _puddleTexture = puddleTexture;
            // Звуки теперь управляются через SoundManager, поэтому не сохраняем их здесь
        }

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

        public void UpgradePuddleDuration()
        {
            if (DurationLevel >= 5) return;
            PuddleDuration += 5f;
            DurationLevel++;
        }

        public void UpgradeDamageInterval()
        {
            if (IntervalLevel >= 5) return;
            DamageInterval -= 0.3f;
            if (DamageInterval < 0.5f) DamageInterval = 0.5f;
            IntervalLevel++;
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
                    var beerPuddle = new BeerPuddle(
                        bottle.Position,
                        _puddleTexture.Width,
                        Color.White,
                        Damage,
                        PuddleDuration,
                        DamageInterval
                    );

                    ActivePuddles.Add(beerPuddle);
                    ActiveBottles.RemoveAt(i);
                }
            }

            bool anyPuddleActive = false;
            for (int i = ActivePuddles.Count - 1; i >= 0; i--)
            {
                var puddle = ActivePuddles[i];
                if (puddle.IsActive)
                {
                    puddle.Update(gameTime);
                    anyPuddleActive = true;
                }
                else
                {
                    ActivePuddles.RemoveAt(i);
                }
            }

            if (anyPuddleActive)
            {
                CooldownTimer = CooldownTime;
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (ActivePuddles.Count == 0 && CooldownTimer <= 0f)
            {
                Debug.WriteLine("Beer Bottle attacking!");

                // Воспроизводим звук броска через WeaponManager
                WeaponManager.PlayWeaponSound(WeaponName.BeerBottle);

                for (int i = 0; i < NumBottles; i++)
                {
                    float spawnRadius = 200f + (float)Game1.Random.NextDouble() * 150f;
                    float angle = (float)Game1.Random.NextDouble() * MathHelper.TwoPi;

                    Vector2 offset = new Vector2(
                        (float)System.Math.Cos(angle) * spawnRadius,
                        (float)System.Math.Sin(angle) * spawnRadius
                    );

                    Vector2 randomPositionNearPlayer = Player.Position + offset;

                    BeerBottleProjectile bottle = new BeerBottleProjectile(
                        Player.Position,
                        randomPositionNearPlayer,
                        _bottleTexture.Width,
                        Color.White,
                        250f
                    );
                    ActiveBottles.Add(bottle);
                }

                CooldownTimer = CooldownTime;
            }
        }

        public void DrawProjectiles(SpriteBatch spriteBatch)
        {
            // Сначала отрисовываем лужи (ПОД всеми)
            foreach (var puddle in ActivePuddles)
            {
                if (puddle.IsActive && _puddleTexture != null)
                {
                    puddle.DrawWithTexture(spriteBatch, _puddleTexture);
                }
            }

            // Затем отрисовываем летящие бутылки (НАД лужами, но под другими объектами)
            foreach (var bottle in ActiveBottles)
            {
                if (bottle.IsActive && _bottleTexture != null)
                {
                    bottle.DrawWithTexture(spriteBatch, _bottleTexture);
                }
            }
        }
    }
}