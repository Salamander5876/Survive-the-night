using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Interfaces;
using Survive_the_night.Managers;

namespace Survive_the_night.Entities.Enemies.Elite
{
    public class EliteEnemy : Enemy
    {
        public enum EliteType
        {
            Type1,
            Type2
        }

        public EliteType Type { get; private set; }

        public const int ExperienceOrbCount = 10;
        public const int ChestDropCount = 1;

        public EliteEnemy(Vector2 initialPosition, Player playerTarget, int stage, EliteType type, DifficultyManager difficultyManager)
            : base(initialPosition, playerTarget, GetBaseHealth(type), 80f, Color.Blue, 15, stage, difficultyManager)
        {
            Type = type;
            Damage = CalculateDamageForStage(15, stage);
            speed = CalculateSpeedForStage(80f, stage);
        }

        public EliteEnemy(Vector2 initialPosition, Player playerTarget)
            : this(initialPosition, playerTarget, 1, EliteType.Type1, null)
        {
        }

        protected override int CalculateHealthForStage(int baseHealth, int stage)
        {
            if (_difficultyManager != null && _difficultyManager.CurrentDifficulty == StartMenu.GameMode.Survival)
            {
                // Для режима выживания используем прогрессивную систему
                return CalculateSurvivalHealth(stage);
            }
            else
            {
                // Стандартная логика для обычных режимов
                int stageHealth = baseHealth + (stage - 1) * 100;
                if (_difficultyManager != null)
                {
                    stageHealth = (int)(stageHealth * _difficultyManager.EnemyHealthMultiplier);
                }
                return stageHealth;
            }
        }

        // Новая логика для режима выживания
        protected override int CalculateSurvivalHealth(int stage)
        {
            if (_difficultyManager == null) return Type == EliteType.Type1 ? 50 : 100;

            // Получаем базовое HP из DifficultyManager
            int survivalBaseHealth = _difficultyManager.GetSurvivalBaseHealth(true, Type);

            // Добавляем бонус за этапы (как в обычной системе)
            int stageBonus = (stage - 1) * 100;

            // Применяем множитель текущего цикла
            float cycleMultiplier = GetCurrentCycleMultiplier();

            int finalHealth = (int)((survivalBaseHealth + stageBonus) * cycleMultiplier);

            System.Diagnostics.Debug.WriteLine($"EliteEnemy {Type} создан: этап={stage}, цикл={_difficultyManager.CurrentCycle}, базовое HP={survivalBaseHealth}, бонус этапа={stageBonus}, множитель={cycleMultiplier}, итого={finalHealth}");

            return finalHealth;
        }

        // Получение множителя текущего цикла
        private float GetCurrentCycleMultiplier()
        {
            if (_difficultyManager == null) return 1.0f;

            switch (_difficultyManager.CurrentCycle)
            {
                case 1: return 1.0f;
                case 2: return 1.5f;
                case 3: return 2.0f;
                case 4: return 2.0f;
                default: return 2.0f;
            }
        }

        private static int GetBaseHealth(EliteType type)
        {
            return type == EliteType.Type1 ? 50 : 100;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            Color drawColor = color ?? Color;
            int size = 48;

            spriteBatch.Draw(debugTexture,
                new Rectangle((int)(Position.X - size / 2), (int)(Position.Y - size / 2), size, size),
                drawColor);
        }
    }
}