using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Survive_the_night.Entities
{
    public class Player : GameObject
    {
        // === СВОЙСТВА ЗДОРОВЬЯ ===
        // !!! ИСПРАВЛЕНО: Добавлен public set для LevelUpMenu !!!
        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        // Таймер, пока игрок неуязвим после получения урона
        private float _invulnerabilityTimer = 0f;
        private const float InvulnerabilityDuration = 1.0f;
        // !!! ИСПРАВЛЕНО: Свойство IsInvulnerable теперь определено !!!
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        // === СИСТЕМА ОПЫТА И УРОВНЕЙ ===
        public int Level { get; private set; } = 1;
        public int CurrentExperience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 10;
        // !!! ИСПРАВЛЕНО: Добавлен public set для LevelUpMenu !!!
        public bool IsLevelUpPending { get; set; } = false;

        // === СКОРОСТЬ ===
        // !!! ИСПРАВЛЕНО: BaseSpeed теперь можно устанавливать из LevelUpMenu !!!
        public float BaseSpeed { get; set; } = 250f;
        public float MovementSpeed => BaseSpeed;

        // !!! ИСПРАВЛЕНО: Конструктор теперь вызывает базовый конструктор GameObject !!!
        public Player(Vector2 initialPosition)
            : base(initialPosition, 24, Color.Blue) // Вызов конструктора GameObject
        {
            CurrentHealth = MaxHealth;
        }

        // !!! ИСПРАВЛЕНО: Реализация абстрактного метода Update из GameObject !!!
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

        // !!! ИСПРАВЛЕНО: Метод TakeDamage теперь определен !!!
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

        // !!! ИСПРАВЛЕНО: Метод Heal теперь определен !!!
        public void Heal(float amount)
        {
            CurrentHealth = (int)MathHelper.Min(CurrentHealth + amount, MaxHealth);
            System.Diagnostics.Debug.WriteLine($"Лечение: +{amount}. Текущее HP: {CurrentHealth}/{MaxHealth}");
        }

        // !!! ИСПРАВЛЕНО: Метод GainExperience теперь определен !!!
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

        // ВАЖНО: LevelUpMenu должен использовать прямое присвоение _player.IsLevelUpPending = false
        // или вызов _player.CompleteLevelUp().
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
    }
}