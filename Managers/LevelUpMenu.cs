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
        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        // Переменные состояния для надежного ввода с клавиатуры и мыши
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        // Используем статический Random из Game1
        private System.Random _random => Game1.Random;

        public List<UpgradeOption> CurrentOptions { get; private set; } = new List<UpgradeOption>();
        public bool IsVisible => _player.IsLevelUpPending;

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

            // 1. Улучшения оружия
            foreach (var weapon in _weapons)
            {
                if (weapon is GoldenSword gs)
                {
                    if (gs.CountLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{GoldenSword.WeaponName}: Количество мечей +1 (Ур. {gs.CountLevel}/5)",
                            Description = $"Текущее количество: {gs.NumSwords}",
                            ApplyUpgrade = () => gs.UpgradeCount()
                        });
                    }
                    if (gs.DamageLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{GoldenSword.WeaponName}: Урон +2 (Ур. {gs.DamageLevel}/5)",
                            Description = $"Текущий урон: {gs.Damage}",
                            ApplyUpgrade = () => gs.UpgradeDamage()
                        });
                    }
                    if (gs.SpeedLevel < 5)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{GoldenSword.WeaponName}: Скорость полета +20 (Ур. {gs.SpeedLevel}/5)",
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
                            Title = $"{PlayingCards.WeaponName}: Количество карт +1 (Ур. {pc.CountLevel}/10)",
                            Description = $"Текущее количество: {pc.NumCards}",
                            ApplyUpgrade = () => pc.UpgradeCount()
                        });
                    }
                    if (pc.DamageLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{PlayingCards.WeaponName}: Урон карты +1 (Ур. {pc.DamageLevel}/10)",
                            Description = $"Текущий урон: {pc.Damage}",
                            ApplyUpgrade = () => pc.UpgradeDamage()
                        });
                    }
                    if (pc.ReloadSpeedLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{PlayingCards.WeaponName}: Скорость перезарядки (Ур. {pc.ReloadSpeedLevel}/10)",
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
                            Title = $"{GoldenBullet.WeaponName}: Количество пуль +1 (Ур. {gb.CountLevel}/10)",
                            Description = $"Текущее количество: {gb.NumBullets}",
                            ApplyUpgrade = () => gb.UpgradeCount()
                        });
                    }
                    if (gb.DamageLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{GoldenBullet.WeaponName}: Урон +1 (Ур. {gb.DamageLevel}/10)",
                            Description = $"Текущий урон: {gb.Damage}",
                            ApplyUpgrade = () => gb.UpgradeDamage()
                        });
                    }
                    if (gb.SpeedLevel < 10)
                    {
                        pool.Add(new UpgradeOption
                        {
                            Title = $"{GoldenBullet.WeaponName}: Скорость полета +50 (Ур. {gb.SpeedLevel}/10)",
                            Description = $"Текущая скорость: {gb.ProjectileSpeed:0}",
                            ApplyUpgrade = () => gb.UpgradeSpeed()
                        });
                    }
                }
                else

                if (weapon is MolotovCocktail mc)
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = $"Молотов: Количество бутылок +1 (Ур. {mc.CountLevel}/5)",
                        Description = $"Текущее количество: {mc.NumBottles}",
                        ApplyUpgrade = () => mc.UpgradeBottleCount()
                    });

                    pool.Add(new UpgradeOption
                    {
                        Title = $"Молотов: Время горения +5 сек (Ур. {mc.DurationLevel}/5)",
                        Description = $"Текущее время: {mc.BurnDuration:0} сек",
                        ApplyUpgrade = () => mc.UpgradeBurnDuration()
                    });

                    pool.Add(new UpgradeOption
                    {
                        Title = $"Молотов: Частота урона (Ур. {mc.RateLevel}/5)",
                        Description = $"Текущий интервал: {mc.DamageInterval:0.0} сек",
                        ApplyUpgrade = () => mc.UpgradeDamageRate()
                    });
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

            // Опция получения Золотого меча
            bool hasGoldenSword = _weapons.Any(w => w is GoldenSword);
            if (!hasGoldenSword)
            {
                pool.Add(new UpgradeOption
                {
                    Title = "Взять Золотой меч",
                    Description = "Добавляет новое оружие: золотые мечи, летящие по траектории бумеранга с автонаводкой.",
                    ApplyUpgrade = () => _weapons.Add(new GoldenSword(_player))
                });
            }

            // Опция получения Золотой пули
            bool hasGoldenBullet = _weapons.Any(w => w is GoldenBullet);
            if (!hasGoldenBullet)
            {
                pool.Add(new UpgradeOption
                {
                    Title = "Взять Золотую пулю",
                    Description = "Добавляет новое оружие: точные золотые пули без пробития.",
                    ApplyUpgrade = () => _weapons.Add(new GoldenBullet(_player))
                });
            }

            // Опция получения Молотова
            bool hasMolotov = _weapons.Any(w => w is MolotovCocktail);
            if (!hasMolotov)
            {
                pool.Add(new UpgradeOption
                {
                    Title = "Взять Коктейль Молотова",
                    Description = "Добавляет новое метательное оружие: бросает бутылки, создающие огненные зоны.",
                    ApplyUpgrade = () => _weapons.Add(new MolotovCocktail(_player))
                });
            }




            // 2. Улучшения игрока
            pool.Add(new UpgradeOption
            {
                Title = $"Макс. здоровье +10 (Ур. {_player.HealthLevel}/10)",
                Description = $"Макс. здоровье: {_player.MaxHealth}",
                ApplyUpgrade = () => _player.UpgradeMaxHealth()
            });
            pool.Add(new UpgradeOption
            {
                Title = $"Бонус получение хп от сердца +1% и золотого сердца +2%(Ур. {_player.HeartHealBonusLevel}/10)",
                Description = $"Бонус к обычному сердцу: +{_player.HeartHealBonusPercent * 100:0}%\nБонус к золотому сердцу: +{_player.GoldenHeartBonusPercent * 100:0}%",
                ApplyUpgrade = () => _player.UpgradeHeartHealBonus()
            });

            // 3. Выбор 3 случайных уникальных опций
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

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsVisible) return;

            Vector2 startPosition = new Vector2(50, 50);
            int boxHeight = 150;
            int boxWidth = _graphicsDevice.Viewport.Width - 100;
            const int boxSpacing = 20;

            spriteBatch.DrawString(font, "ВЫБЕРИТЕ УЛУЧШЕНИЕ", startPosition - new Vector2(0, 40), Color.Yellow);

            // Получаем текущую позицию мыши для выделения
            Point mousePosition = Mouse.GetState().Position;

            for (int i = 0; i < CurrentOptions.Count; i++)
            {
                UpgradeOption option = CurrentOptions[i];
                Rectangle box = new Rectangle((int)startPosition.X, (int)startPosition.Y + i * boxHeight + i * boxSpacing, boxWidth, boxHeight);

                // Фон
                Color boxColor = Color.DarkBlue;
                if (box.Contains(mousePosition))
                {
                    boxColor = Color.DarkSlateGray; // Выделяем при наведении
                }

                spriteBatch.Draw(_debugTexture, box, boxColor);

                // Текст
                Vector2 textPos = new Vector2(box.X + 20, box.Y + 10);
                spriteBatch.DrawString(font, $"[{i + 1}] {option.Title}", textPos, Color.White);

                textPos.Y += 40;
                spriteBatch.DrawString(font, option.Description, textPos, Color.LightGray);
            }
        }
    }
}