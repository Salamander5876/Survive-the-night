using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities;
using Survive_the_night.Items;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Managers
{
    public class BonusShopMenu
    {
        public bool IsVisible { get; set; }

        private Player _player;
        private ItemManager _itemManager;
        private Random _random;
        private KeyboardState _previousKeyboardState;

        // Система бонусов
        private Dictionary<string, BonusData> _playerBonuses = new Dictionary<string, BonusData>();
        private BonusData _currentBonus;

        // Система навыков (пока заглушка)
        private SkillData _currentSkill;

        // Прогрессирующие цены
        public int CurrentBonusPrice { get; private set; } = 50; // ИЗМЕНЕНО: было 25, стало 50
        public int CurrentSkillPrice { get; private set; } = 100; // ИЗМЕНЕНО: было 50, стало 100

        // Счетчики покупок для прогрессии цен
        private int _bonusPurchaseCount = 0;
        private int _skillPurchaseCount = 0;

        // Флаг для предотвращения многократных покупок
        private bool _canPurchaseBonus = true;
        private bool _canPurchaseSkill = true;

        public BonusShopMenu(Player player, ItemManager itemManager)
        {
            _player = player;
            _itemManager = itemManager;
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
                BaseValue = 50f
            };

            _playerBonuses["HeartHeal"] = new BonusData
            {
                Name = "Лечение сердец",
                Description = "Увеличивает эффективность обычных сердец на 5%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.05f
            };

            _playerBonuses["MovementSpeed"] = new BonusData
            {
                Name = "Скорость",
                Description = "Увеличивает скорость передвижения на 20%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.2f
            };

            // Бонус: Дополнительный опыт
            _playerBonuses["ExperienceBonus"] = new BonusData
            {
                Name = "Эссенция опыта",
                Description = "Эссенция опыта дает дополнительный опыт +1",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 1f
            };

            // Бонус: Дополнительные монеты
            _playerBonuses["CoinBonus"] = new BonusData
            {
                Name = "Доп. монеты",
                Description = "Монеты дают дополнительную монету +1",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 1f  // +1 дополнительная монета
            };

            // Бонус: Урон динамита
            _playerBonuses["DynamiteDamage"] = new BonusData
            {
                Name = "Урон динамита",
                Description = "Увеличивает урон динамита на 5",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 5f
            };

            // Бонус: Скорость магнита
            _playerBonuses["MagnetSpeed"] = new BonusData
            {
                Name = "Скорость магнита",
                Description = "Увеличивает скорость притяжения магнита на 200",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 200f
            };
        }

        public void Show()
        {
            IsVisible = true;
            _currentBonus = null;
            _currentSkill = null;
            _canPurchaseBonus = true;
            _canPurchaseSkill = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void RollBonus()
        {
            // Проверяем возможность покупки
            if (!_canPurchaseBonus) return;
            if (_player.Coins < CurrentBonusPrice) return;

            _player.SpendCoins(CurrentBonusPrice);

            // Увеличиваем счетчик покупок
            _bonusPurchaseCount++;

            // НОВАЯ ЛОГИКА: 50% шанс выпадения бонуса
            if (_random.NextDouble() < 0.5) // 50% шанс
            {
                // Проверяем есть ли доступные бонусы для улучшения
                bool hasAvailableBonuses = false;
                foreach (var bonus in _playerBonuses.Values)
                {
                    if (bonus.CurrentLevel < bonus.MaxLevel)
                    {
                        hasAvailableBonuses = true;
                        break;
                    }
                }

                if (!hasAvailableBonuses)
                {
                    _currentBonus = new BonusData
                    {
                        Name = "Все бонусы максимальны!",
                        Description = "Вы прокачали все доступные бонусы",
                        CurrentLevel = 3,
                        MaxLevel = 3
                    };
                    // Увеличиваем цену на +10 если ничего не выпало
                    CurrentBonusPrice += 10;
                }
                else
                {
                    // Выбираем случайный бонус из доступных
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

                        // Увеличиваем цену на +25 если бонус выпал
                        CurrentBonusPrice += 25;
                    }
                }
            }
            else
            {
                // 50% шанс что ничего не выпадет
                _currentBonus = new BonusData
                {
                    Name = "ПУСТО!",
                    Description = "Вам не повезло, попробуйте еще раз",
                    CurrentLevel = 0,
                    MaxLevel = 0
                };

                // Увеличиваем цену на +10 если ничего не выпало
                CurrentBonusPrice += 10;
            }

            // Блокируем повторную покупку до следующего клика
            _canPurchaseBonus = false;
        }

        public void RollSkill()
        {
            // Проверяем возможность покупки
            if (!_canPurchaseSkill) return;
            if (_player.Coins < CurrentSkillPrice) return;

            _player.SpendCoins(CurrentSkillPrice);

            // Увеличиваем счетчик покупок и цену
            _skillPurchaseCount++;
            CurrentSkillPrice += 50; // ИЗМЕНЕНО: было +20, стало +50

            // Блокируем повторную покупку до следующего клика
            _canPurchaseSkill = false;

            // Заглушка для навыков
            _currentSkill = new SkillData
            {
                Name = "Навык в разработке",
                Description = "Система навыков будет добавлена в будущем",
                CurrentLevel = 0,
                MaxLevel = 3
            };
        }

        // Метод для разблокировки кнопок (вызывается из интерфейса при отпускании кнопки мыши)
        public void ResetPurchaseFlags()
        {
            _canPurchaseBonus = true;
            _canPurchaseSkill = true;
        }

        private void ApplyBonus(BonusData bonus)
        {
            bonus.CurrentLevel++;

            switch (bonus.Name)
            {
                case "Макс. здоровье":
                    _player.IncreaseMaxHealth((int)bonus.BaseValue);
                    break;
                case "Лечение сердец":
                    for (int i = 0; i < 5; i++)
                    {
                        _player.UpgradeHeartHealBonus();
                    }
                    break;
                case "Скорость":
                    _player.BaseSpeed += bonus.BaseValue * 250f;
                    break;
                case "Эссенция опыта":
                    _itemManager.ApplyExperienceBonus((int)bonus.BaseValue);
                    break;
                case "Доп. монеты":
                    _itemManager.ApplyCoinBonus((int)bonus.BaseValue);
                    break;
                case "Урон динамита":
                    Dynamite.ApplyDamageBonus((int)bonus.BaseValue);
                    break;
                case "Скорость магнита":
                    Magnet.ApplySpeedBonus((int)bonus.BaseValue);
                    break;
            }
        }

        // Метод для проверки доступности бонусов
        public bool AreAllBonusesMaxLevel()
        {
            foreach (var bonus in _playerBonuses.Values)
            {
                if (bonus.CurrentLevel < bonus.MaxLevel)
                    return false;
            }
            return true;
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
        public bool CanPurchaseBonus => _canPurchaseBonus;
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