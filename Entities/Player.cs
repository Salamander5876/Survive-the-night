using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Survive_the_night.Entities
{
    // Класс игрока, наследуется от базового GameObject
    public class Player : GameObject
    {
        // === СВОЙСТВА ЗДОРОВЬЯ ===
        public int MaxHealth { get; private set; } = 100;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        // Таймер, пока игрок неуязвим после получения урона
        private float _invulnerabilityTimer = 0f;
        private const float InvulnerabilityDuration = 1.0f; // 1 секунда неуязвимости
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        // === СИСТЕМА ОПЫТА И УРОВНЕЙ ===
        public int Level { get; private set; } = 1;
        public int CurrentExperience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 10;
        public bool IsLevelUpPending { get; private set; } = false;

        // === СКОРОСТЬ ===
        public float BaseSpeed { get; private set; } = 250f;
        public float MovementSpeed => BaseSpeed;

        // Конструктор
        public Player(Vector2 initialPosition)
            : base(initialPosition, 24, Color.Blue)
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

        /// <summary>
        /// НОВЫЙ МЕТОД: Восстанавливает здоровье игроку, не превышая MaxHealth.
        /// </summary>
        public void Heal(float amount)
        {
            // Используем MathHelper.Min, чтобы не превысить максимальное здоровье
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

        /// <summary>
        /// Применяет улучшения к игроку, исходя из ID.
        /// </summary>
        public void ApplyUpgrade(int upgradeId, float value)
        {
            switch (upgradeId)
            {
                case 1: // Здоровье
                    MaxHealth += (int)value;
                    // Также восстанавливаем здоровье
                    CurrentHealth += (int)value;
                    if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Здоровье +{value}");
                    break;
                case 2: // Скорость
                    BaseSpeed += value;
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Скорость +{value}");
                    break;
                    // case 3 - Урон, обрабатывается в классе оружия
            }
        }
    }
}