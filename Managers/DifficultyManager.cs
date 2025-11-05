using Survive_the_night.Interfaces;

namespace Survive_the_night.Managers
{
    public class DifficultyManager
    {
        public StartMenu.GameMode CurrentDifficulty { get; private set; }
        public int CurrentCycle { get; private set; } = 1;

        public float EnemyHealthMultiplier { get; private set; }
        public float EnemyDamageMultiplier { get; private set; }
        public float EnemySpeedMultiplier { get; private set; }
        public float SpawnRateMultiplier { get; private set; }
        public int ElitesRequiredForStage { get; private set; }
        public bool SpawnBothEliteTypes { get; private set; }

        public void SetDifficulty(StartMenu.GameMode difficulty)
        {
            CurrentDifficulty = difficulty;
            CurrentCycle = 1;
            UpdateMultipliers();
        }

        private void UpdateMultipliers()
        {
            switch (CurrentDifficulty)
            {
                case StartMenu.GameMode.Easy:
                    EnemyHealthMultiplier = 1.0f;
                    EnemyDamageMultiplier = 1.0f;
                    EnemySpeedMultiplier = 1.0f;
                    SpawnRateMultiplier = 1.0f;
                    ElitesRequiredForStage = 1;
                    SpawnBothEliteTypes = false;
                    break;

                case StartMenu.GameMode.Hard:
                    EnemyHealthMultiplier = 1.5f;
                    EnemyDamageMultiplier = 1.5f;
                    EnemySpeedMultiplier = 1.0f;
                    SpawnRateMultiplier = 2.0f;
                    ElitesRequiredForStage = 2;
                    SpawnBothEliteTypes = true;
                    break;

                case StartMenu.GameMode.Insane:
                    EnemyHealthMultiplier = 2.0f;
                    EnemyDamageMultiplier = 2.0f;
                    EnemySpeedMultiplier = 1.0f;
                    SpawnRateMultiplier = 4.0f;
                    ElitesRequiredForStage = 2;
                    SpawnBothEliteTypes = true;
                    break;

                case StartMenu.GameMode.Endless:
                    EnemyHealthMultiplier = 1.0f;
                    EnemyDamageMultiplier = 1.0f;
                    EnemySpeedMultiplier = 1.0f;
                    SpawnRateMultiplier = 1.0f;
                    ElitesRequiredForStage = 2;
                    SpawnBothEliteTypes = true;
                    break;
            }
        }

        public void AdvanceCycle()
        {
            if (CurrentDifficulty == StartMenu.GameMode.Endless)
            {
                CurrentCycle++;

                if (CurrentCycle == 2)
                {
                    EnemyHealthMultiplier = 1.5f;
                    EnemyDamageMultiplier = 1.5f;
                    SpawnRateMultiplier = 2.0f;
                }
                else if (CurrentCycle == 3)
                {
                    EnemyHealthMultiplier = 2.0f;
                    EnemyDamageMultiplier = 2.0f;
                    SpawnRateMultiplier = 4.0f;
                }
            }
        }

        public bool CheckVictoryCondition(int currentStage, int totalElitesKilled)
        {
            if (CurrentDifficulty == StartMenu.GameMode.Endless)
            {
                // Для бесконечного режима: победа на 8 уровне в 3 цикле
                return currentStage == 8 && CurrentCycle == 3 && totalElitesKilled >= ElitesRequiredForStage;
            }

            // Для обычных режимов: победа на 8 уровне при достижении нужного количества элитных врагов
            return currentStage == 8 && totalElitesKilled >= ElitesRequiredForStage;
        }

        public void Reset()
        {
            CurrentCycle = 1;
            UpdateMultipliers();
        }
    }
}