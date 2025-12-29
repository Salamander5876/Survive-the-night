using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Weapons;
using Survive_the_night.Localizations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Gamedata.Managers
{
    public class RouletteManager
    {
        private Player _player;
        private List<Weapon> _weapons;

        // Переменные состояния для надежного ввода с клавиатуры и мыши
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        // Используем статический Random из Game1
        private Random _random => Game1.Random;

        public List<UpgradeOption> CurrentOptions { get; private set; } = new List<UpgradeOption>();
        public bool IsVisible { get; private set; }

        // ДОБАВЛЕНО: Свойство IsActive для отслеживания активности рулетки
        public bool IsActive => IsVisible;

        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public RouletteManager(Player player, List<Weapon> allWeapons, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _player = player;
            _weapons = allWeapons;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
        }

        public void StartRoulette()
        {
            GenerateRouletteOptions();
            IsVisible = true;
            // Явно указываем полное пространство имен для GameState
            Game1.CurrentState = GameState.Roulette;
        }

        public void GenerateRouletteOptions()
        {
            CurrentOptions.Clear();
            List<UpgradeOption> pool = new List<UpgradeOption>();

            // Проверяем, есть ли еще обычные оружия для получения
            // Используем списки из WeaponManager для проверки
            bool hasAllRegularWeapons = WeaponManager.RegularWeapons.All(weaponName =>
                _weapons.Any(w => GetWeaponType(w.Name) == WeaponType.Regular && w.Name == weaponName));

            // Если все обычные оружия получены, показываем только легендарные
            if (hasAllRegularWeapons)
            {
                // Легендарные оружия (гарантированно)
                AddLegendaryWeaponsToPool(pool, true); // true = гарантированное добавление
            }
            else
            {
                // Смешанный пул: обычные оружия + 10% шанс на легендарные
                AddRegularWeaponsToPool(pool);
                AddLegendaryWeaponsToPool(pool, false); // false = 10% шанс
            }

            // --- ИСПРАВЛЕНИЕ: Гарантируем, что всегда будет 3 варианта ---

            // Если доступных опций меньше 3, добавляем легендарные оружия (если они еще не добавлены)
            if (pool.Count < 3)
            {
                // Добавляем все недостающие легендарные оружия
                AddMissingLegendaryWeapons(pool);
            }

            // Если все равно меньше 3 опций, добавляем кнопку-пустышку
            while (pool.Count < 3)
            {
                pool.Add(new UpgradeOption
                {
                    Title = "Пропустить выбор",
                    Description = "Продолжить без получения нового оружия. Удача улыбнется в следующий раз!",
                    ApplyUpgrade = () => { /* Ничего не делаем - пустышка */ }
                });
            }

            // Выбираем 3 случайных уникальных опции
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

        // Вспомогательный метод для определения типа оружия
        private WeaponType GetWeaponType(WeaponName weaponName)
        {
            return WeaponManager.LegendaryWeapons.Contains(weaponName) ?
                WeaponType.Legendary : WeaponType.Regular;
        }

        // Метод для добавления обычных оружий в пул
        private void AddRegularWeaponsToPool(List<UpgradeOption> pool)
        {
            foreach (var weaponName in WeaponManager.RegularWeapons)
            {
                if (!HasWeapon(weaponName))
                {
                    pool.Add(CreateUpgradeOption(weaponName));
                }
            }
        }

        // Метод для добавления легендарных оружий в пул
        private void AddLegendaryWeaponsToPool(List<UpgradeOption> pool, bool guaranteed)
        {
            foreach (var weaponName in WeaponManager.LegendaryWeapons)
            {
                if (!HasWeapon(weaponName))
                {
                    // Если гарантированное добавление или 10% шанс
                    if (guaranteed || _random.NextDouble() < 0.1)
                    {
                        pool.Add(CreateUpgradeOption(weaponName));
                    }
                }
            }
        }

        // Метод для добавления недостающих легендарных оружий
        private void AddMissingLegendaryWeapons(List<UpgradeOption> pool)
        {
            foreach (var weaponName in WeaponManager.LegendaryWeapons)
            {
                if (!HasWeapon(weaponName) && !pool.Any(o => GetWeaponNameFromTitle(o.Title) == weaponName))
                {
                    pool.Add(CreateUpgradeOption(weaponName));
                    if (pool.Count >= 3) break;
                }
            }
        }

        // Проверяет, есть ли оружие у игрока
        private bool HasWeapon(WeaponName weaponName)
        {
            return _weapons.Any(w => w.Name == weaponName);
        }

        // Создает опцию улучшения для оружия
        private UpgradeOption CreateUpgradeOption(WeaponName weaponName)
        {
            // Получаем соответствующее оружие для создания
            Weapon weapon = WeaponManager.CreateWeapon(weaponName, _player);

            return new UpgradeOption
            {
                Title = LocalizationManager.GetWeaponRouletteTitle(weaponName),
                Description = LocalizationManager.GetWeaponRouletteDescription(weaponName),
                ApplyUpgrade = () => _weapons.Add(weapon)
            };
        }

        // Вспомогательный метод для получения WeaponName из заголовка
        private WeaponName GetWeaponNameFromTitle(string title)
        {
            // Проходим по всем оружиям и ищем совпадение
            foreach (WeaponName weaponName in Enum.GetValues(typeof(WeaponName)))
            {
                string weaponTitle = LocalizationManager.GetWeaponRouletteTitle(weaponName);
                if (title == weaponTitle)
                {
                    return weaponName;
                }
            }

            // Если не нашли, возвращаем первое оружие (заглушка)
            return WeaponName.PlayingCards;
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
                        break;
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
            IsVisible = false;
            // Явно указываем полное пространство имен для GameState
            Game1.CurrentState = GameState.Playing;
            CurrentOptions.Clear();
        }
    }
}