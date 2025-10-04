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

        // === НОВЫЕ ПОЛЯ: СИСТЕМА ОПЫТА И УРОВНЕЙ ===
        public int Level { get; private set; } = 1;
        public int CurrentExperience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 10;
        public bool IsLevelUpPending { get; private set; } = false;

        // === НОВОЕ ПОЛЕ: СКОРОСТЬ ===
        public float BaseSpeed { get; private set; } = 250f;

        // Свойство, которое вернет текущую скорость (пока равна BaseSpeed)
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

            // !!! ИСПОЛЬЗУЕМ НОВУЮ СКОРОСТЬ MovementSpeed !!!
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

        // ... (Метод TakeDamage остается прежним) ...
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

        // ... (Метод GainExperience остается прежним) ...
        public void GainExperience(int amount)
        {
            if (IsLevelUpPending) return;

            CurrentExperience += amount;

            if (CurrentExperience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        // ... (Метод LevelUp остается прежним) ...
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

        // ... (Метод CompleteLevelUp остается прежним) ...
        public void CompleteLevelUp()
        {
            IsLevelUpPending = false;
        }

        // === НОВЫЙ МЕТОД: ПРИМЕНЕНИЕ УЛУЧШЕНИЯ ===
        /// <summary>
        /// Применяет улучшения к игроку, исходя из ID.
        /// </summary>
        /// <param name="upgradeId">ID улучшения из LevelUpMenu.</param>
        /// <param name="value">Значение, на которое увеличивается характеристика.</param>
        public void ApplyUpgrade(int upgradeId, float value)
        {
            switch (upgradeId)
            {
                case 1: // Здоровье (+10)
                    MaxHealth += (int)value;
                    // Также восстанавливаем здоровье
                    CurrentHealth += (int)value;
                    if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Здоровье +{value}");
                    break;
                case 2: // Скорость (+50f)
                    BaseSpeed += value;
                    System.Diagnostics.Debug.WriteLine($"Улучшение: Скорость +{value}");
                    break;
                    // case 3 - Урон, обрабатывается в классе оружия (но мы его пока не обновили)
            }
        }
    }
}
