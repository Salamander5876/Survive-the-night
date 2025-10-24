using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities;

namespace Survive_the_night.Managers
{
    public class BonusShopMenu
    {
        public bool IsVisible { get; set; }

        private Player _player;
        private Random _random;
        private KeyboardState _previousKeyboardState;

        // Система бонусов
        private Dictionary<string, BonusData> _playerBonuses = new Dictionary<string, BonusData>();
        private BonusData _currentBonus;

        // Система навыков (пока заглушка)
        private SkillData _currentSkill;

        // Прогрессирующие цены
        public int CurrentBonusPrice { get; private set; } = 25;
        public int CurrentSkillPrice { get; private set; } = 50;

        // Счетчики покупок для прогрессии цен
        private int _bonusPurchaseCount = 0;
        private int _skillPurchaseCount = 0;

        public BonusShopMenu(Player player)
        {
            _player = player;
            _random = Game1.Random;
            InitializeBonuses();
        }

        private void InitializeBonuses()
        {
            // Инициализируем базовые бонусы с новыми значениями
            _playerBonuses["MaxHealth"] = new BonusData
            {
                Name = "Макс. здоровье",
                Description = "Увеличивает максимальное здоровье на 50",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 50f  // +50 HP за уровень
            };

            _playerBonuses["HeartHeal"] = new BonusData
            {
                Name = "Лечение сердец",
                Description = "Увеличивает эффективность обычных сердец на 5%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.05f  // +5% за уровень
            };

            _playerBonuses["MovementSpeed"] = new BonusData
            {
                Name = "Скорость",
                Description = "Увеличивает скорость передвижения на 20%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.2f  // +20% за уровень
            };
        }

        public void Show()
        {
            IsVisible = true;
            _currentBonus = null;
            _currentSkill = null;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void RollBonus()
        {
            if (_player.Coins < CurrentBonusPrice) return;

            _player.SpendCoins(CurrentBonusPrice);

            // Увеличиваем счетчик покупок и цену
            _bonusPurchaseCount++;
            CurrentBonusPrice += 10; // +10 монет за каждую покупку

            // Выбираем случайный бонус
            var availableBonuses = new List<BonusData>();
            foreach (var bonus in _playerBonuses.Values)
            {
                if (bonus.CurrentLevel < bonus.MaxLevel)
                {
                    availableBonuses.Add(bonus);
                }
            }

            if (availableBonuses.Count > 0)
            {
                int index = _random.Next(0, availableBonuses.Count);
                _currentBonus = availableBonuses[index];

                // Применяем улучшение
                ApplyBonus(_currentBonus);
            }
            else
            {
                // Все бонусы на максимальном уровне
                _currentBonus = new BonusData
                {
                    Name = "Все бонусы максимальны!",
                    Description = "Вы прокачали все доступные бонусы",
                    CurrentLevel = 3,
                    MaxLevel = 3
                };
            }
        }

        public void RollSkill()
        {
            if (_player.Coins < CurrentSkillPrice) return;

            _player.SpendCoins(CurrentSkillPrice);

            // Увеличиваем счетчик покупок и цену
            _skillPurchaseCount++;
            CurrentSkillPrice += 20; // +20 монет за каждую покупку

            // Заглушка для навыков
            _currentSkill = new SkillData
            {
                Name = "Навык в разработке",
                Description = "Система навыков будет добавлена в будущем",
                CurrentLevel = 0,
                MaxLevel = 3
            };

            // Пока навыки не применяются
        }

        private void ApplyBonus(BonusData bonus)
        {
            bonus.CurrentLevel++;

            switch (bonus.Name)
            {
                case "Макс. здоровье":
                    _player.IncreaseMaxHealth((int)bonus.BaseValue);  // +50 HP
                    break;
                case "Лечение сердец":
                    // +5% за уровень - нужно вызвать метод 5 раз для +5%
                    for (int i = 0; i < 5; i++)
                    {
                        _player.UpgradeHeartHealBonus();
                    }
                    break;
                case "Скорость":
                    _player.BaseSpeed += bonus.BaseValue * 250f; // +20% от базовой скорости (50 единиц)
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            KeyboardState currentKs = Keyboard.GetState();

            // Выход по ESC или B
            if ((currentKs.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape)) ||
                (currentKs.IsKeyDown(Keys.B) && !_previousKeyboardState.IsKeyDown(Keys.B)))
            {
                Hide();
            }

            _previousKeyboardState = currentKs;
        }

        // Свойства для доступа из интерфейса
        public BonusData CurrentBonus => _currentBonus;
        public SkillData CurrentSkill => _currentSkill;
        public Dictionary<string, BonusData> PlayerBonuses => _playerBonuses;
        public int PlayerCoins => _player.Coins;
    }

    public class BonusData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public float BaseValue { get; set; }

        public string LevelText => $"{CurrentLevel}/{MaxLevel}";
        public bool IsMaxLevel => CurrentLevel >= MaxLevel;
    }

    public class SkillData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }

        public string LevelText => $"{CurrentLevel}/{MaxLevel}";
        public bool IsMaxLevel => CurrentLevel >= MaxLevel;
    }
}