using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities;
using Survive_the_night.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Managers
{
    public class UpgradeOption
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public System.Action ApplyUpgrade { get; set; }
        public bool IsSkipOption { get; set; } = false;
    }

    public class LevelUpMenu
    {
        private Player _player;
        private List<Weapon> _weapons;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        private System.Random _random => Game1.Random;

        public List<UpgradeOption> CurrentOptions { get; private set; } = new List<UpgradeOption>();
        public bool IsVisible => _player.IsLevelUpPending;

        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public LevelUpMenu(Player player, List<Weapon> allWeapons, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _player = player;
            _weapons = allWeapons;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
        }

        public void GenerateOptions()
        {
            if (!_player.IsLevelUpPending) return;

            CurrentOptions.Clear();
            List<UpgradeOption> pool = new List<UpgradeOption>();

            foreach (var weapon in _weapons)
            {
                if (weapon is PlayingCards pc)
                {
                    if (pc.CountLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.PlayingCards)}: Количество карт +1 (Ур. {pc.CountLevel + 1}/5)",
                            Description = $"Текущее количество: {pc.NumCards}",
                            ApplyUpgrade = () => pc.UpgradeCount()
                        });
                    }
                    if (pc.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.PlayingCards)}: Урон карты +2 (Ур. {pc.DamageLevel + 1}/5)",
                            Description = $"Текущий урон: {pc.Damage}",
                            ApplyUpgrade = () => pc.UpgradeDamage()
                        });
                    }
                    if (pc.ReloadSpeedLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.PlayingCards)}: Скорость перезарядки -0.2с (Ур. {pc.ReloadSpeedLevel + 1}/5)",
                            Description = $"Текущая перезарядка: {pc.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => pc.UpgradeReloadSpeed()
                        });
                    }
                }
                else if (weapon is GoldenBullet gb)
                {
                    if (gb.CountLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenBullet)}: Количество пуль +1 (Ур. {gb.CountLevel + 1}/5)",
                            Description = $"Текущее количество: {gb.NumBullets}",
                            ApplyUpgrade = () => gb.UpgradeCount()
                        });
                    }
                    if (gb.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenBullet)}: Урон +2 (Ур. {gb.DamageLevel + 1}/5)",
                            Description = $"Текущий урон: {gb.Damage}",
                            ApplyUpgrade = () => gb.UpgradeDamage()
                        });
                    }
                    if (gb.KnockbackLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenBullet)}: Отталкивание +5px (Ур. {gb.KnockbackLevel + 1}/5)",
                            Description = $"Текущее отталкивание: {gb.GetKnockbackForce()}px",
                            ApplyUpgrade = () => gb.UpgradeKnockback()
                        });
                    }
                }
                else if (weapon is CasinoChips cc)
                {
                    if (cc.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.CasinoChips)}: Урон +2 (Ур. {cc.DamageLevel + 1}/5)",
                            Description = $"Текущий урон: {cc.Damage}",
                            ApplyUpgrade = () => cc.UpgradeDamage()
                        });
                    }
                    if (cc.ReloadSpeedLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.CasinoChips)}: Скорость перезарядки -0.3с (Ур. {cc.ReloadSpeedLevel + 1}/5)",
                            Description = $"Текущая перезарядка: {cc.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => cc.UpgradeReloadSpeed()
                        });
                    }
                    if (cc.BounceLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.CasinoChips)}: Отскоки +1 (Ур. {cc.BounceLevel + 1}/5)",
                            Description = $"Текущее количество отскоков: {cc.BounceLevel + 1}",
                            ApplyUpgrade = () => cc.UpgradeBounceCount()
                        });
                    }
                }
                else if (weapon is StickyBomb sb)
                {
                    if (sb.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.StickyBomb)}: Урон +4 (Ур. {sb.DamageLevel + 1}/5)",
                            Description = $"Текущий урон: {sb.Damage}",
                            ApplyUpgrade = () => sb.UpgradeDamage()
                        });
                    }
                    if (sb.CountLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.StickyBomb)}: Количество +1 (Ур. {sb.CountLevel + 1}/5)",
                            Description = $"Текущее количество: {sb.NumBombs}",
                            ApplyUpgrade = () => sb.UpgradeCount()
                        });
                    }
                    if (sb.ExplosionTimeLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.StickyBomb)}: Время взрыва -1,5сек (Ур. {sb.ExplosionTimeLevel + 1}/5)",
                            Description = $"Текущее время: {sb.ExplosionTime:0} сек",
                            ApplyUpgrade = () => sb.UpgradeExplosionTime()
                        });
                    }
                }
                else if (weapon is DiceWeapon dw)
                {
                    if (dw.DamageBonusLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.Dice)}: Дополнительный урон +1 (Ур. {dw.DamageBonusLevel + 1}/5)",
                            Description = $"Текущий бонус урона: +{dw.DamageBonusLevel}",
                            ApplyUpgrade = () => dw.UpgradeDamage()
                        });
                    }
                    if (dw.PierceBonusLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.Dice)}: Дополнительное пробитие +1 (Ур. {dw.PierceBonusLevel + 1}/5)",
                            Description = $"Текущий бонус пробития: +{dw.PierceBonusLevel}",
                            ApplyUpgrade = () => dw.UpgradePierce()
                        });
                    }
                    if (dw.CooldownLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.Dice)}: Перезарядка -1с (Ур. {dw.CooldownLevel + 1}/5)",
                            Description = $"Текущая перезарядка: {dw.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => dw.UpgradeCooldown()
                        });
                    }
                }
                else if (weapon is RouletteBall rb)
                {
                    if (rb.SpeedLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.RouletteBall)}: Скорость +100 (Ур. {rb.SpeedLevel + 1}/5)",
                            Description = $"Текущая скорость: {rb.ProjectileSpeed:0}",
                            ApplyUpgrade = () => rb.UpgradeSpeed()
                        });
                    }
                    if (rb.LifetimeLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.RouletteBall)}: Время частичек +1с (Ур. {rb.LifetimeLevel + 1}/5)",
                            Description = $"Текущее время: {rb.ParticleLifetime:0.0}с",
                            ApplyUpgrade = () => rb.UpgradeLifetime()
                        });
                    }
                    if (rb.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.RouletteBall)}: Урон частичек +1 (Ур. {rb.DamageLevel + 1}/5)",
                            Description = $"Текущий урон: {rb.ParticleDamage}",
                            ApplyUpgrade = () => rb.UpgradeDamage()
                        });
                    }
                }

                // ЛЕГЕНДАРНЫЕ ОРУЖИЯ
                else if (weapon is GoldenSword gs)
                {
                    if (gs.CountLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenSword)}: Количество мечей +2 (Ур. {gs.CountLevel + 1}/3)",
                            Description = $"Текущее количество: {gs.NumSwords}",
                            ApplyUpgrade = () => gs.UpgradeCount()
                        });
                    }
                    if (gs.DamageLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenSword)}: Урон +4 (Ур. {gs.DamageLevel + 1}/3)",
                            Description = $"Текущий урон: {gs.Damage}",
                            ApplyUpgrade = () => gs.UpgradeDamage()
                        });
                    }
                    if (gs.TargetsLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenSword)}: Целей +10 (Ур. {gs.TargetsLevel + 1}/3)",
                            Description = $"Текущее количество целей: {gs.MaxTargets}",
                            ApplyUpgrade = () => gs.UpgradeTargets()
                        });
                    }
                }
                else if (weapon is MolotovCocktail mc)
                {
                    if (mc.CountLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.MolotovCocktail)}: Количество бутылок +2 (Ур. {mc.CountLevel + 1}/3)",
                            Description = $"Текущее количество: {mc.NumBottles}",
                            ApplyUpgrade = () => mc.UpgradeBottleCount()
                        });
                    }
                    if (mc.DurationLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.MolotovCocktail)}: Время горения +10с (Ур. {mc.DurationLevel + 1}/3)",
                            Description = $"Текущее время: {mc.BurnDuration:0} сек",
                            ApplyUpgrade = () => mc.UpgradeBurnDuration()
                        });
                    }
                    if (mc.DamageLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.MolotovCocktail)}: Урон огня +1 (Ур. {mc.DamageLevel + 1}/3)",
                            Description = $"Текущий урон: {mc.Damage}",
                            ApplyUpgrade = () => mc.UpgradeDamage()
                        });
                    }
                }
                else if (weapon is BigLaser bl)
                {
                    if (bl.DurationLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.BigLaser)}: Длительность +10с (Ур. {bl.DurationLevel + 1}/3)",
                            Description = $"Текущая длительность: {bl.CurrentDuration:0}с",
                            ApplyUpgrade = () => bl.UpgradeDuration()
                        });
                    }
                    if (bl.DamageLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.BigLaser)}: Урон +1 (Ур. {bl.DamageLevel + 1}/3)",
                            Description = $"Текущий урон: {bl.Damage}",
                            ApplyUpgrade = () => bl.UpgradeDamage()
                        });
                    }
                    if (bl.CooldownLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.BigLaser)}: Перезарядка -5с (Ур. {bl.CooldownLevel + 1}/3)",
                            Description = $"Текущая перезарядка: {bl.CurrentCooldown:0}с",
                            ApplyUpgrade = () => bl.UpgradeCooldown()
                        });
                    }
                }
                else if (weapon is GoldenTyphoon gt)
                {
                    if (gt.CountLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenTyphoon)}: Количество снарядов +2 (Ур. {gt.CountLevel + 1}/3)",
                            Description = $"Текущее количество: {gt.NumProjectiles}",
                            ApplyUpgrade = () => gt.UpgradeCount()
                        });
                    }
                    if (gt.DamageLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenTyphoon)}: Урон +4 (Ур. {gt.DamageLevel + 1}/3)",
                            Description = $"Текущий урон: {gt.Damage}",
                            ApplyUpgrade = () => gt.UpgradeDamage()
                        });
                    }
                    if (gt.CooldownLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenTyphoon)}: Перезарядка -0.2с (Ур. {gt.CooldownLevel + 1}/3)",
                            Description = $"Текущая перезарядка: {gt.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => gt.UpgradeCooldown()
                        });
                    }
                }
                else if (weapon is EventHorizon eh)
                {
                    if (eh.CountLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.EventHorizon)}: Количество звезд +2 (Ур. {eh.CountLevel + 1}/3)",
                            Description = $"Текущее количество: {eh.NumStars}",
                            ApplyUpgrade = () => eh.UpgradeCount()
                        });
                    }
                    if (eh.DamageLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.EventHorizon)}: Урон +5 (Ур. {eh.DamageLevel + 1}/3)",
                            Description = $"Текущий урон: {eh.Damage}",
                            ApplyUpgrade = () => eh.UpgradeDamage()
                        });
                    }
                    if (eh.CooldownLevel < 3)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.EventHorizon)}: Перезарядка -0.3с (Ур. {eh.CooldownLevel + 1}/3)",
                            Description = $"Текущая перезарядка: {eh.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => eh.UpgradeCooldown()
                        });
                    }
                }
            }

            // Всегда показываем 3 опции
            int targetCount = 3;

            if (pool.Count >= targetCount)
            {
                HashSet<int> indices = new HashSet<int>();
                while (indices.Count < targetCount)
                {
                    indices.Add(_random.Next(0, pool.Count));
                }

                foreach (int index in indices)
                {
                    CurrentOptions.Add(pool[index]);
                }
            }
            else
            {
                CurrentOptions.AddRange(pool);

                int skipOptionsNeeded = targetCount - pool.Count;
                for (int i = 0; i < skipOptionsNeeded; i++)
                {
                    CurrentOptions.Add(new UpgradeOption
                    {
                        Title = "Пропустить улучшение",
                        Description = "Все оружия максимально прокачаны или недостаточно доступных улучшений",
                        ApplyUpgrade = () => { /* Ничего не делаем - просто пропускаем */ },
                        IsSkipOption = true
                    });
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            KeyboardState currentKs = Keyboard.GetState();
            MouseState currentMs = Mouse.GetState();

            bool choiceMade = false;

            Vector2 startPosition = new Vector2(50, 50);
            const int boxHeight = 150;
            const int boxSpacing = 20;
            int boxWidth = _graphicsDevice.Viewport.Width - 100;

            if (currentKs.IsKeyDown(Keys.D1) && !_previousKeyboardState.IsKeyDown(Keys.D1) && CurrentOptions.Count > 0)
            {
                ApplyChoice(0);
                choiceMade = true;
            }
            else if (currentKs.IsKeyDown(Keys.D2) && !_previousKeyboardState.IsKeyDown(Keys.D2) && CurrentOptions.Count > 1)
            {
                ApplyChoice(1);
                choiceMade = true;
            }
            else if (currentKs.IsKeyDown(Keys.D3) && !_previousKeyboardState.IsKeyDown(Keys.D3) && CurrentOptions.Count > 2)
            {
                ApplyChoice(2);
                choiceMade = true;
            }

            if (!choiceMade && currentMs.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                Point mousePosition = currentMs.Position;

                for (int i = 0; i < CurrentOptions.Count; i++)
                {
                    Rectangle box = new Rectangle(
                        (int)startPosition.X,
                        (int)startPosition.Y + i * boxHeight + i * boxSpacing,
                        boxWidth,
                        boxHeight
                    );

                    if (box.Contains(mousePosition))
                    {
                        ApplyChoice(i);
                        choiceMade = true;
                        break;
                    }
                }
            }

            _previousKeyboardState = currentKs;
            _previousMouseState = currentMs;
        }

        public void ApplyChoice(int index)
        {
            CurrentOptions[index].ApplyUpgrade.Invoke();
            _player.IsLevelUpPending = false;
        }
    }
}