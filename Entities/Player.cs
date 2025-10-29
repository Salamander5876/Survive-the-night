using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Survive_the_night.Entities
{
    public class Player : GameObject
    {
        // === СВОЙСТВА ЗДОРОВЬЯ ===
        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; set; } // ИЗМЕНИЛОСЬ: set стал публичным
        public bool IsAlive { get; set; } = true;

        // Прокачка: отдельные уровни для здоровья и бонуса хила сердец
        public int HealthLevel { get; private set; } = 0;
        public int HeartHealBonusLevel { get; private set; } = 0;
        public float HeartHealBonusPercent => HeartHealBonusLevel * 0.01f;

        // Таймер неуязвимости
        private float _invulnerabilityTimer = 0f;
        private const float InvulnerabilityDuration = 0.5f;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        // === СИСТЕМА ОПЫТА И УРОВНЕЙ ===
        public int Level { get; set; } = 1; // ИЗМЕНИЛОСЬ: добавлен set
        public int CurrentExperience { get; set; } = 0; // ИЗМЕНИЛОСЬ: добавлен set
        public int ExperienceToNextLevel { get; private set; } = 10;
        public bool IsLevelUpPending { get; set; } = false;

        // === СКОРОСТЬ ===
        public float BaseSpeed { get; set; } = 250f;
        public float MovementSpeed => BaseSpeed;

        // === ВАЛЮТА ===
        public int Coins { get; set; } = 0; // ИЗМЕНИЛОСЬ: добавлен set

        public Player(Vector2 initialPosition)
            : base(initialPosition, 24, Color.Blue)
        {
            CurrentHealth = MaxHealth;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            IsAlive = true;

            // Обновляем таймер неуязвимости
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= deltaTime;
            }

            // Если игрок мертв или ждет выбора улучшения, он не должен двигаться
            if (!IsAlive || IsLevelUpPending)
            {
                return;
            }

            // Обработка ввода и движения
            KeyboardState kState = Keyboard.GetState();
            Vector2 direction = Vector2.Zero;

            float speed = MovementSpeed;

            if (kState.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (kState.IsKeyDown(Keys.S)) direction.Y += 1;
            if (kState.IsKeyDown(Keys.A)) direction.X -= 1;
            if (kState.IsKeyDown(Keys.D)) direction.X += 1;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            // Используем метод Move из GameObject
            Move(direction * speed * deltaTime);
        }


        public void TakeDamage(int damage)
        {
            if (!IsInvulnerable)
            {
                CurrentHealth -= damage;
                _invulnerabilityTimer = InvulnerabilityDuration;

                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;
                    IsAlive = false;
                    System.Diagnostics.Debug.WriteLine("💀 ИГРОК УМЕР: Здоровье игрока исчерпано.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚡ Игрок получил урон: {damage}. Осталось здоровья: {CurrentHealth}");
                }
            }
        }

        public void IncreaseMaxHealth(int amount)
        {
            MaxHealth += amount;
            CurrentHealth += amount; // Также увеличиваем текущее здоровье
            System.Diagnostics.Debug.WriteLine($"Увеличено макс. здоровье: +{amount}. Теперь: {MaxHealth}");
        }

        public void Heal(float amount)
        {
            CurrentHealth = (int)MathHelper.Min(CurrentHealth + amount, MaxHealth);
            System.Diagnostics.Debug.WriteLine($"Лечение: +{amount}. Текущее HP: {CurrentHealth}/{MaxHealth}");
        }

        public void GainExperience(int amount)
        {
            if (IsLevelUpPending)
            {
                System.Diagnostics.Debug.WriteLine($"⏸️ Опыт не начисляется - ожидание выбора улучшения");
                return;
            }

            int oldExp = CurrentExperience;
            CurrentExperience += amount;
            System.Diagnostics.Debug.WriteLine($"🎯 Игрок получил {amount} опыта. Было: {oldExp}, Стало: {CurrentExperience}/{ExperienceToNextLevel}");

            if (CurrentExperience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            Level++;
            CurrentExperience -= ExperienceToNextLevel;
            ExperienceToNextLevel += 10;
            IsLevelUpPending = true;
            System.Diagnostics.Debug.WriteLine($"УРОВЕНЬ ПОВЫШЕН! Новый уровень: {Level}");

            if (CurrentExperience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        public void CompleteLevelUp()
        {
            IsLevelUpPending = false;
        }

        // === МЕТОДЫ ДЛЯ БОНУСОВ И ПРЕДМЕТОВ ===

        public void UpgradeMaxHealth()
        {
            if (HealthLevel >= 10) return;
            MaxHealth += 10;
            CurrentHealth += 10;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
            HealthLevel++;
            System.Diagnostics.Debug.WriteLine($"Улучшение: Макс. здоровье +10 (Ур. {HealthLevel}/10)");
        }

        public void UpgradeHeartHealBonus()
        {
            if (HeartHealBonusLevel >= 10) return;
            HeartHealBonusLevel++;
            System.Diagnostics.Debug.WriteLine($"Улучшение: Бонус лечения от сердец +1% (Ур. {HeartHealBonusLevel}/10)");
        }

        public float GetGoldenHeartHealAmount(float basePercent)
        {
            // Теперь золотое сердце всегда лечит фиксированный процент
            return basePercent * MaxHealth;
        }

        // === МЕТОДЫ ДЛЯ ВАЛЮТЫ ===

        public void AddCoins(int amount)
        {
            Coins += amount;
            System.Diagnostics.Debug.WriteLine($"💰 Игрок получил {amount} монет. Всего: {Coins}");
        }

        public bool SpendCoins(int amount)
        {
            if (Coins >= amount)
            {
                Coins -= amount;
                System.Diagnostics.Debug.WriteLine($"Потрачено монет: -{amount}. Осталось: {Coins}");
                return true;
            }
            return false;
        }

        // === УНИВЕРСАЛЬНЫЙ МЕТОД ДЛЯ ПРИМЕНЕНИЯ УЛУЧШЕНИЙ ===

        public void ApplyUpgrade(int upgradeId, float value)
        {
            switch (upgradeId)
            {
                case 1: // Здоровье
                    MaxHealth += (int)value;
                    CurrentHealth += (int)value;
                    if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Здоровье +{value}");
                    break;
                case 2: // Скорость
                    BaseSpeed += value;
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Скорость +{value}");
                    break;
                case 3: // Бонус лечения от сердец
                    HeartHealBonusLevel += (int)value;
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Бонус лечения сердец +{value}%");
                    break;
                    // УБИРАЕМ case 4 для золотых сердец
            }
        }

        public void ResetExperienceRequirements()
        {
            // Сбрасываем опыт к начальным значениям
            CurrentExperience = 0;
            ExperienceToNextLevel = 10;
            Level = 1;
            IsLevelUpPending = false;

            // Сбрасываем уровни улучшений здоровья
            HealthLevel = 0;
            HeartHealBonusLevel = 0;

            // Восстанавливаем базовые характеристики
            MaxHealth = 100;
            CurrentHealth = MaxHealth;
            BaseSpeed = 250f;

            System.Diagnostics.Debug.WriteLine($"Опыт игрока сброшен: уровень={Level}, опыт={CurrentExperience}/{ExperienceToNextLevel}");
        }

    }
}