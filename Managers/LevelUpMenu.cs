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

            // 1. Улучшение существующего оружия (если уровень < MAX_LEVEL)
            foreach (var weapon in _weapons.Where(w => w.Level < Weapon.MAX_LEVEL))
            {
                pool.Add(new UpgradeOption
                {
                    Title = $"Улучшить {weapon.GetType().Name} (Ур.{weapon.Level + 1})",
                    Description = GetWeaponUpgradeDescription(weapon),
                    ApplyUpgrade = () => weapon.LevelUp()
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
                Title = "Увеличить Здоровье",
                Description = $"+{_player.MaxHealth * 0.1f:0} Максимального Здоровья",
                ApplyUpgrade = () =>
                {
                    int healthIncrease = (int)(_player.MaxHealth * 1.1f) - _player.MaxHealth;
                    _player.MaxHealth += healthIncrease;
                    _player.Heal(healthIncrease);
                }
            });
            pool.Add(new UpgradeOption
            {
                Title = "Увеличить Скорость",
                Description = "+10% Скорости Передвижения",
                ApplyUpgrade = () => _player.BaseSpeed *= 1.1f
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
                string piercing = pc.Level == 6 ? " (Получает пробивание!)" : "";
                return $"+1 Карта, +1 Урон.{piercing}";
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