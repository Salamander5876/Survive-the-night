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

        // ������� �������
        private Dictionary<string, BonusData> _playerBonuses = new Dictionary<string, BonusData>();
        private BonusData _currentBonus;

        // ������� ������� (���� ��������)
        private SkillData _currentSkill;

        // ��������������� ����
        public int CurrentBonusPrice { get; private set; } = 50; // ��������: ���� 25, ����� 50
        public int CurrentSkillPrice { get; private set; } = 100; // ��������: ���� 50, ����� 100

        // �������� ������� ��� ���������� ���
        private int _bonusPurchaseCount = 0;
        private int _skillPurchaseCount = 0;

        // ���� ��� �������������� ������������ �������
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
            // �������������� ������� ������ � ������ ����������
            _playerBonuses["MaxHealth"] = new BonusData
            {
                Name = "����. ��������",
                Description = "����������� ������������ �������� �� 50",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 50f
            };

            _playerBonuses["HeartHeal"] = new BonusData
            {
                Name = "������� ������",
                Description = "����������� ������������� ������� ������ �� 5%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.05f
            };

            _playerBonuses["MovementSpeed"] = new BonusData
            {
                Name = "��������",
                Description = "����������� �������� ������������ �� 20%",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 0.2f
            };

            // �����: �������������� ����
            _playerBonuses["ExperienceBonus"] = new BonusData
            {
                Name = "�������� �����",
                Description = "�������� ����� ���� �������������� ���� +1",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 1f
            };

            // �����: �������������� ������
            _playerBonuses["CoinBonus"] = new BonusData
            {
                Name = "���. ������",
                Description = "������ ���� �������������� ������ +1",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 1f  // +1 �������������� ������
            };

            // �����: ���� ��������
            _playerBonuses["DynamiteDamage"] = new BonusData
            {
                Name = "���� ��������",
                Description = "����������� ���� �������� �� 5",
                CurrentLevel = 0,
                MaxLevel = 3,
                BaseValue = 5f
            };

            // �����: �������� �������
            _playerBonuses["MagnetSpeed"] = new BonusData
            {
                Name = "�������� �������",
                Description = "����������� �������� ���������� ������� �� 200",
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
            // ��������� ����������� �������
            if (!_canPurchaseBonus) return;
            if (_player.Coins < CurrentBonusPrice) return;

            _player.SpendCoins(CurrentBonusPrice);

            // ����������� ������� �������
            _bonusPurchaseCount++;

            // ����� ������: 50% ���� ��������� ������
            if (_random.NextDouble() < 0.5) // 50% ����
            {
                // ��������� ���� �� ��������� ������ ��� ���������
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
                        Name = "��� ������ �����������!",
                        Description = "�� ��������� ��� ��������� ������",
                        CurrentLevel = 3,
                        MaxLevel = 3
                    };
                    // ����������� ���� �� +10 ���� ������ �� ������
                    CurrentBonusPrice += 10;
                }
                else
                {
                    // �������� ��������� ����� �� ���������
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

                        // ����������� ���� �� +25 ���� ����� �����
                        CurrentBonusPrice += 25;
                    }
                }
            }
            else
            {
                // 50% ���� ��� ������ �� �������
                _currentBonus = new BonusData
                {
                    Name = "�����!",
                    Description = "��� �� �������, ���������� ��� ���",
                    CurrentLevel = 0,
                    MaxLevel = 0
                };

                // ����������� ���� �� +10 ���� ������ �� ������
                CurrentBonusPrice += 10;
            }

            // ��������� ��������� ������� �� ���������� �����
            _canPurchaseBonus = false;
        }

        public void RollSkill()
        {
            // ��������� ����������� �������
            if (!_canPurchaseSkill) return;
            if (_player.Coins < CurrentSkillPrice) return;

            _player.SpendCoins(CurrentSkillPrice);

            // ����������� ������� ������� � ����
            _skillPurchaseCount++;
            CurrentSkillPrice += 50; // ��������: ���� +20, ����� +50

            // ��������� ��������� ������� �� ���������� �����
            _canPurchaseSkill = false;

            // �������� ��� �������
            _currentSkill = new SkillData
            {
                Name = "����� � ����������",
                Description = "������� ������� ����� ��������� � �������",
                CurrentLevel = 0,
                MaxLevel = 3
            };
        }

        // ����� ��� ������������� ������ (���������� �� ���������� ��� ���������� ������ ����)
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
                case "����. ��������":
                    _player.IncreaseMaxHealth((int)bonus.BaseValue);
                    break;
                case "������� ������":
                    for (int i = 0; i < 5; i++)
                    {
                        _player.UpgradeHeartHealBonus();
                    }
                    break;
                case "��������":
                    _player.BaseSpeed += bonus.BaseValue * 250f;
                    break;
                case "�������� �����":
                    _itemManager.ApplyExperienceBonus((int)bonus.BaseValue);
                    break;
                case "���. ������":
                    _itemManager.ApplyCoinBonus((int)bonus.BaseValue);
                    break;
                case "���� ��������":
                    Dynamite.ApplyDamageBonus((int)bonus.BaseValue);
                    break;
                case "�������� �������":
                    Magnet.ApplySpeedBonus((int)bonus.BaseValue);
                    break;
            }
        }

        // ����� ��� �������� ����������� �������
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