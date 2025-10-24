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
    }

    public class LevelUpMenu
    {
        private Player _player;
        private List<Weapon> _weapons;

        // Переменные состояния для надежного ввода с клавиатуры и мыши
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        // Используем статический Random из Game1
        private System.Random _random => Game1.Random;

        public List<UpgradeOption> CurrentOptions { get; private set; } = new List<UpgradeOption>();
        public bool IsVisible => _player.IsLevelUpPending;

        private Texture2D _debugTexture; // ВОССТАНОВИТЬ
        private SpriteFont _font; // ВОССТАНОВИТЬ
        private GraphicsDevice _graphicsDevice; // ВОССТАНОВИТЬ

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

            // 1. Улучшения оружия (оставляем только это)
            foreach (var weapon in _weapons)
            {
                if (weapon is GoldenSword gs)
                {
                    if (gs.CountLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenSword)}: Количество мечей +1 (Ур. {gs.CountLevel}/5)",
                            Description = $"Текущее количество: {gs.NumSwords}",
                            ApplyUpgrade = () => gs.UpgradeCount()
                        });
                    }
                    if (gs.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenSword)}: Урон +2 (Ур. {gs.DamageLevel}/5)",
                            Description = $"Текущий урон: {gs.Damage}",
                            ApplyUpgrade = () => gs.UpgradeDamage()
                        });
                    }
                    if (gs.SpeedLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenSword)}: Скорость полета +20 (Ур. {gs.SpeedLevel}/5)",
                            Description = $"Текущая скорость: {gs.ProjectileSpeed:0}",
                            ApplyUpgrade = () => gs.UpgradeSpeed()
                        });
                    }
                }
                else

                if (weapon is PlayingCards pc)
                {
                    if (pc.CountLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.PlayingCards)}: Количество карт +1 (Ур. {pc.CountLevel}/10)",
                            Description = $"Текущее количество: {pc.NumCards}",
                            ApplyUpgrade = () => pc.UpgradeCount()
                        });
                    }
                    if (pc.DamageLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.PlayingCards)}: Урон карты +1 (Ур. {pc.DamageLevel}/10)",
                            Description = $"Текущий урон: {pc.Damage}",
                            ApplyUpgrade = () => pc.UpgradeDamage()
                        });
                    }
                    if (pc.ReloadSpeedLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.PlayingCards)}: Скорость перезарядки (Ур. {pc.ReloadSpeedLevel}/10)",
                            Description = $"Текущая перезарядка: {pc.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => pc.UpgradeReloadSpeed()
                        });
                    }
                }
                else

                if (weapon is GoldenBullet gb)
                {
                    if (gb.CountLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenBullet)}: Количество пуль +1 (Ур. {gb.CountLevel}/10)",
                            Description = $"Текущее количество: {gb.NumBullets}",
                            ApplyUpgrade = () => gb.UpgradeCount()
                        });
                    }
                    if (gb.DamageLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenBullet)}: Урон +1 (Ур. {gb.DamageLevel}/10)",
                            Description = $"Текущий урон: {gb.Damage}",
                            ApplyUpgrade = () => gb.UpgradeDamage()
                        });
                    }
                    if (gb.SpeedLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.GoldenBullet)}: Скорость полета +50 (Ур. {gb.SpeedLevel}/10)",
                            Description = $"Текущая скорость: {gb.ProjectileSpeed:0}",
                            ApplyUpgrade = () => gb.UpgradeSpeed()
                        });
                    }
                }
                else

                // В цикле улучшений оружия добавьте:
                if (weapon is CasinoChips cc)
                {
                    if (cc.DamageLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.CasinoChips)}: Урон +1 (Ур. {cc.DamageLevel}/10)",
                            Description = $"Текущий урон: {cc.Damage}",
                            ApplyUpgrade = () => cc.UpgradeDamage()
                        });
                    }
                    if (cc.ReloadSpeedLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.CasinoChips)}: Скорость перезарядки -0.2с (Ур. {cc.ReloadSpeedLevel}/10)",
                            Description = $"Текущая перезарядка: {cc.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => cc.UpgradeReloadSpeed()
                        });
                    }
                    if (cc.BounceLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.CasinoChips)}: Отскоки +1 (Ур. {cc.BounceLevel}/10)",
                            Description = $"Текущее количество отскоков: {cc.BounceLevel + 1}",
                            ApplyUpgrade = () => cc.UpgradeBounceCount()
                        });
                    }
                }
                else

                if (weapon is MolotovCocktail mc)
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = $"{WeaponManager.GetDisplayName(WeaponName.MolotovCocktail)}: Количество бутылок +1 (Ур. {mc.CountLevel}/5)",
                        Description = $"Текущее количество: {mc.NumBottles}",
                        ApplyUpgrade = () => mc.UpgradeBottleCount()
                    });

                    pool.Add(new UpgradeOption
                    {
                        Title = $"{WeaponManager.GetDisplayName(WeaponName.MolotovCocktail)}: Время горения +5 сек (Ур. {mc.DurationLevel}/5)",
                        Description = $"Текущее время: {mc.BurnDuration:0} сек",
                        ApplyUpgrade = () => mc.UpgradeBurnDuration()
                    });

                    pool.Add(new UpgradeOption
                    {
                        Title = $"{WeaponManager.GetDisplayName(WeaponName.MolotovCocktail)}: Частота урона (Ур. {mc.RateLevel}/5)",
                        Description = $"Текущий интервал: {mc.DamageInterval:0.0} сек",
                        ApplyUpgrade = () => mc.UpgradeDamageRate()
                    });
                }
                else

                if (weapon is BigLaser bl)
                {
                    if (bl.DurationLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.BigLaser)}: Длительность +10с (Ур. {bl.DurationLevel}/5)",
                            Description = $"Текущая длительность: {bl.CurrentDuration:0}с",
                            ApplyUpgrade = () => bl.UpgradeDuration()
                        });
                    }
                    if (bl.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.BigLaser)}: Урон +2 (Ур. {bl.DamageLevel}/5)",
                            Description = $"Текущий урон: {bl.Damage}",
                            ApplyUpgrade = () => bl.UpgradeDamage()
                        });
                    }
                    if (bl.CooldownLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.BigLaser)}: Перезарядка -5с (Ур. {bl.CooldownLevel}/5)",
                            Description = $"Текущая перезарядка: {bl.CurrentCooldown:0}с",
                            ApplyUpgrade = () => bl.UpgradeCooldown()
                        });
                    }
                }
                else

                // В цикле улучшений оружия добавьте:
                if (weapon is StickyBomb sb)
                {
                    if (sb.DamageLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.StickyBomb)}: Урон +1 (Ур. {sb.DamageLevel}/10)",
                            Description = $"Текущий урон: {sb.Damage}",
                            ApplyUpgrade = () => sb.UpgradeDamage()
                        });
                    }
                    if (sb.CountLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.StickyBomb)}: Количество +1 (Ур. {sb.CountLevel}/10)",
                            Description = $"Текущее количество: {sb.NumBombs}",
                            ApplyUpgrade = () => sb.UpgradeCount()
                        });
                    }
                    if (sb.ExplosionTimeLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.StickyBomb)}: Время взрыва -5сек (Ур. {sb.ExplosionTimeLevel}/10)",
                            Description = $"Текущее время: {sb.ExplosionTime:0} сек",
                            ApplyUpgrade = () => sb.UpgradeExplosionTime()
                        });
                    }
                }
                else

                if (weapon is DiceWeapon dw)
                {
                    if (dw.DamageBonusLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.Dice)}: Дополнительный урон +1 (Ур. {dw.DamageBonusLevel}/10)",
                            Description = $"Текущий бонус урона: +{dw.DamageBonusLevel}",
                            ApplyUpgrade = () => dw.UpgradeDamage()
                        });
                    }
                    if (dw.PierceBonusLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.Dice)}: Дополнительное пробитие +1 (Ур. {dw.PierceBonusLevel}/10)",
                            Description = $"Текущий бонус пробития: +{dw.PierceBonusLevel}",
                            ApplyUpgrade = () => dw.UpgradePierce()
                        });
                    }
                    if (dw.CooldownLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{WeaponManager.GetDisplayName(WeaponName.Dice)}: Перезарядка -0.5с (Ур. {dw.CooldownLevel}/10)",
                            Description = $"Текущая перезарядка: {dw.CurrentCooldown:0.0}с",
                            ApplyUpgrade = () => dw.UpgradeCooldown()
                        });
                    }
                }
                else
                {
                    if (weapon.Level < Weapon.MAX_LEVEL)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"Улучшить {weapon.GetType().Name} (Ур.{weapon.Level + 1})",
                            Description = GetWeaponUpgradeDescription(weapon),
                            ApplyUpgrade = () => weapon.LevelUp()
                        });
                    }
                }
            }

            // УБИРАЕМ улучшения игрока (максимальное ХП, скорость, бонус лечения)
            // Теперь это только в магазине бонусов

            // 2. Выбор 3 случайных уникальных опций (только улучшения оружия)
            int count = Math.Min(3, pool.Count);

            if (pool.Count <= count)
            {
                CurrentOptions.AddRange(pool);
            }
            else
            {
                HashSet<int> indices = new HashSet<int>();
                while (indices.Count < count)
                {
                    indices.Add(_random.Next(0, pool.Count));
                }

                foreach (int index in indices)
                {
                    CurrentOptions.Add(pool[index]);
                }
            }
        }

        private string GetWeaponUpgradeDescription(Weapon weapon)
        {
            if (weapon is PlayingCards pc)
            {
                return "Игровые карты: выбирайте отдельные улучшения (кол-во/урон/перезарядка)";
            }
            if (weapon is MolotovCocktail mc)
            {
                string bottles = (mc.Level % 2 == 1) ? "" : " (+1 Бутылка)";
                return $"+1 Урон, +15 Площадь.{bottles}";
            }
            return "Улучшение характеристик";
        }


        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            KeyboardState currentKs = Keyboard.GetState();
            MouseState currentMs = Mouse.GetState();

            bool choiceMade = false;

            // Определяем размеры и позиции для расчета кликов
            Vector2 startPosition = new Vector2(50, 50);
            const int boxHeight = 150;
            const int boxSpacing = 20;
            int boxWidth = _graphicsDevice.Viewport.Width - 100;

            // --- 1. Логика выбора с помощью клавиш D1-D3 (только при первом нажатии) ---
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

            // --- 2. Логика выбора с помощью мыши (при клике) ---
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
                        break; // Выбираем только одну опцию за клик
                    }
                }
            }

            // Обновляем состояния для следующего кадра
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