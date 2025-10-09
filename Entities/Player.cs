using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Survive_the_night.Entities
{
    public class Player : GameObject
    {
        // === СВОЙСТВА ЗДОРОВЬЯ ===
        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        // Прокачка: отдельные уровни для здоровья и бонуса хила сердец
        public int HealthLevel { get; private set; } = 0; // 0..10
        public int HeartHealBonusLevel { get; private set; } = 0; // 0..10
        public float HeartHealBonusPercent => HeartHealBonusLevel * 0.01f; // +1% за уровень
        public int GoldenHeartBonusLevel { get; private set; } = 0; // 0..10
        public float GoldenHeartBonusPercent => GoldenHeartBonusLevel * 0.02f; // +2% за уровень

        // Таймер, пока игрок неуязвим после получения урона
        private float _invulnerabilityTimer = 0f;
        private const float InvulnerabilityDuration = 0.5f;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        // === СИСТЕМА ОПЫТА И УРОВНЕЙ ===
        public int Level { get; private set; } = 1;
        public int CurrentExperience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 10;
        public bool IsLevelUpPending { get; set; } = false;

        // === СКОРОСТЬ ===
        public float BaseSpeed { get; set; } = 250f;
        public float MovementSpeed => BaseSpeed;

        public Player(Vector2 initialPosition)
            : base(initialPosition, 24, Color.Blue) // Вызов конструктора GameObject
        {
            CurrentHealth = MaxHealth;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

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

            Position += direction * speed * deltaTime;
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
                    System.Diagnostics.Debug.WriteLine("ИГРА ОКОНЧЕНА: Здоровье игрока исчерпано.");
                }
            }
        }

        public void Heal(float amount)
        {
            CurrentHealth = (int)MathHelper.Min(CurrentHealth + amount, MaxHealth);
            System.Diagnostics.Debug.WriteLine($"Лечение: +{amount}. Текущее HP: {CurrentHealth}/{MaxHealth}");
        }

        public void GainExperience(int amount)
        {
            if (IsLevelUpPending) return;

            CurrentExperience += amount;

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

        // Метод ApplyUpgrade, который LevelUpMenu будет использовать
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
                    BaseSpeed += value; // Используем BaseSpeed
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Скорость +{value}");
                    break;
            }
        }

        // Явные улучшения для меню
        public void UpgradeMaxHealth()
        {
            if (HealthLevel >= 10) return;
            MaxHealth += 10;
            CurrentHealth += 10;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
            HealthLevel++;
        }

        public void UpgradeHeartHealBonus()
        {
            if (HeartHealBonusLevel >= 10) return;
            HeartHealBonusLevel++;
            GoldenHeartBonusLevel++;
        }

        public float GetGoldenHeartHealAmount(float basePercent)
        {
            return (basePercent + GoldenHeartBonusPercent) * MaxHealth;
        }
    }
}