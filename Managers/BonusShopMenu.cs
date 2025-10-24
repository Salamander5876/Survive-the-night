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

        // ������� �������
        private Dictionary<string, BonusData> _playerBonuses = new Dictionary<string, BonusData>();
        private BonusData _currentBonus;

        // ������� ������� (���� ��������)
        private SkillData _currentSkill;

        // ��������������� ����
        public int CurrentBonusPrice { get; private set; } = 25;
        public int CurrentSkillPrice { get; private set; } = 50;

        // �������� ������� ��� ���������� ���
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
            // �������������� ������� ������ � ������ ����������
            _playerBonuses["MaxHealth"] = new BonusData
            {
                Name = "����. ��������",
                Description = "����������� ������������ �������� �� 50",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 50f  // +50 HP �� �������
            };

            _playerBonuses["HeartHeal"] = new BonusData
            {
                Name = "������� ������",
                Description = "����������� ������������� ������� ������ �� 5%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.05f  // +5% �� �������
            };

            _playerBonuses["MovementSpeed"] = new BonusData
            {
                Name = "��������",
                Description = "����������� �������� ������������ �� 20%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.2f  // +20% �� �������
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

            // ����������� ������� ������� � ����
            _bonusPurchaseCount++;
            CurrentBonusPrice += 10; // +10 ����� �� ������ �������

            // �������� ��������� �����
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

                // ��������� ���������
                ApplyBonus(_currentBonus);
            }
            else
            {
                // ��� ������ �� ������������ ������
                _currentBonus = new BonusData
                {
                    Name = "��� ������ �����������!",
                    Description = "�� ��������� ��� ��������� ������",
                    CurrentLevel = 3,
                    MaxLevel = 3
                };
            }
        }

        public void RollSkill()
        {
            if (_player.Coins < CurrentSkillPrice) return;

            _player.SpendCoins(CurrentSkillPrice);

            // ����������� ������� ������� � ����
            _skillPurchaseCount++;
            CurrentSkillPrice += 20; // +20 ����� �� ������ �������

            // �������� ��� �������
            _currentSkill = new SkillData
            {
                Name = "����� � ����������",
                Description = "������� ������� ����� ��������� � �������",
                CurrentLevel = 0,
                MaxLevel = 3
            };

            // ���� ������ �� �����������
        }

        private void ApplyBonus(BonusData bonus)
        {
            bonus.CurrentLevel++;

            switch (bonus.Name)
            {
                case "����. ��������":
                    _player.IncreaseMaxHealth((int)bonus.BaseValue);  // +50 HP
                    break;
                case "������� ������":
                    // +5% �� ������� - ����� ������� ����� 5 ��� ��� +5%
                    for (int i = 0; i < 5; i++)
                    {
                        _player.UpgradeHeartHealBonus();
                    }
                    break;
                case "��������":
                    _player.BaseSpeed += bonus.BaseValue * 250f; // +20% �� ������� �������� (50 ������)
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            KeyboardState currentKs = Keyboard.GetState();

            // ����� �� ESC ��� B
            if ((currentKs.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape)) ||
                (currentKs.IsKeyDown(Keys.B) && !_previousKeyboardState.IsKeyDown(Keys.B)))
            {
                Hide();
            }

            _previousKeyboardState = currentKs;
        }

        // �������� ��� ������� �� ����������
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