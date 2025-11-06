using Survive_the_night.Entities.Enemies.Elite;
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

        // Новые поля для хранения прогрессии HP
        public int SurvivalBaseHealthRegular { get; private set; } = 5; // Базовое HP обычных врагов
        public int SurvivalBaseHealthEliteType1 { get; private set; } = 50; // Базовое HP элитных Type1
        public int SurvivalBaseHealthEliteType2 { get; private set; } = 100; // Базовое HP элитных Type2

        public enum GameMode
        {
            Easy,
            Hard,
            Insane,
            Survival,
            Custom,
            Endless
        }

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

                case StartMenu.GameMode.Survival:
                    EnemyHealthMultiplier = 1.0f;
                    EnemyDamageMultiplier = 1.0f;
                    EnemySpeedMultiplier = 1.0f;
                    SpawnRateMultiplier = 1.0f;
                    ElitesRequiredForStage = 2;
                    SpawnBothEliteTypes = true;
                    break;

                case StartMenu.GameMode.Custom:
                    // Настройки по умолчанию для кастомного режима
                    EnemyHealthMultiplier = 1.0f;
                    EnemyDamageMultiplier = 1.0f;
                    EnemySpeedMultiplier = 1.0f;
                    SpawnRateMultiplier = 1.0f;
                    ElitesRequiredForStage = 2;
                    SpawnBothEliteTypes = true;
                    break;

                case StartMenu.GameMode.Endless:
                    // В разработке - используем настройки Easy
                    EnemyHealthMultiplier = 1.0f;
                    EnemyDamageMultiplier = 1.0f;
                    EnemySpeedMultiplier = 1.0f;
                    SpawnRateMultiplier = 1.0f;
                    ElitesRequiredForStage = 1;
                    SpawnBothEliteTypes = false;
                    break;
            }
        }

        public void AdvanceCycle()
        {
            if (CurrentDifficulty == StartMenu.GameMode.Survival)
            {
                int oldCycle = CurrentCycle;
                CurrentCycle++;

                // Обновляем базовое HP для нового цикла
                UpdateSurvivalBaseHealth(CurrentCycle);

                if (CurrentCycle == 2)
                {
                    EnemyDamageMultiplier = 1.5f;
                    SpawnRateMultiplier = 2.0f;
                }
                else if (CurrentCycle >= 3)
                {
                    EnemyDamageMultiplier = 2.0f;
                    SpawnRateMultiplier = 4.0f;
                }

                System.Diagnostics.Debug.WriteLine($"Survival: переход на цикл {CurrentCycle}, множители - Урон: {EnemyDamageMultiplier}, Спаун: {SpawnRateMultiplier}");
            }
        }

        public bool CheckVictoryCondition(int currentStage, int totalElitesKilled)
        {
            switch (CurrentDifficulty)
            {
                case StartMenu.GameMode.Survival:
                    // Победа на 8 этапе в 4 цикле
                    bool victory = currentStage == 8 && CurrentCycle == 4 && totalElitesKilled >= ElitesRequiredForStage;
                    if (victory)
                    {
                        System.Diagnostics.Debug.WriteLine($"УСЛОВИЕ ПОБЕДЫ: уровень={currentStage}, цикл={CurrentCycle}, элитные={totalElitesKilled}/{ElitesRequiredForStage}");
                    }
                    return victory;

                case StartMenu.GameMode.Easy:
                    return currentStage == 8 && totalElitesKilled >= ElitesRequiredForStage;

                case StartMenu.GameMode.Hard:
                case StartMenu.GameMode.Insane:
                    return currentStage == 8 && totalElitesKilled >= ElitesRequiredForStage;

                case StartMenu.GameMode.Custom:
                case StartMenu.GameMode.Endless:
                    return currentStage == 8 && totalElitesKilled >= ElitesRequiredForStage;

                default:
                    return currentStage == 8 && totalElitesKilled >= ElitesRequiredForStage;
            }
        }

        public void Reset()
        {
            CurrentCycle = 1;
            SurvivalBaseHealthRegular = 5;
            SurvivalBaseHealthEliteType1 = 50;
            SurvivalBaseHealthEliteType2 = 100;
            UpdateMultipliers();
            System.Diagnostics.Debug.WriteLine($"DifficultyManager сброшен: цикл={CurrentCycle}, базовое HP сброшено");
        }

        // Метод для получения базового HP с учетом прогрессии
        public int GetSurvivalBaseHealth(bool isElite = false, EliteEnemy.EliteType? eliteType = null)
        {
            if (!isElite)
            {
                return SurvivalBaseHealthRegular;
            }
            else if (eliteType.HasValue)
            {
                return eliteType.Value == EliteEnemy.EliteType.Type1
                    ? SurvivalBaseHealthEliteType1
                    : SurvivalBaseHealthEliteType2;
            }
            return 5; // fallback
        }

        // Метод для обновления базового HP при смене цикла
        public void UpdateSurvivalBaseHealth(int cycle)
        {
            if (CurrentDifficulty == StartMenu.GameMode.Survival)
            {
                // Сохраняем финальное HP предыдущего цикла как базовое для нового цикла
                // Для обычных врагов: базовое HP = финальное HP 8 этапа предыдущего цикла
                SurvivalBaseHealthRegular = CalculateFinalHealthRegular(cycle - 1);

                // Для элитных врагов
                SurvivalBaseHealthEliteType1 = CalculateFinalHealthElite(cycle - 1, EliteEnemy.EliteType.Type1);
                SurvivalBaseHealthEliteType2 = CalculateFinalHealthElite(cycle - 1, EliteEnemy.EliteType.Type2);

                System.Diagnostics.Debug.WriteLine($"Survival: обновлено базовое HP - Обычные: {SurvivalBaseHealthRegular}, Элитные Type1: {SurvivalBaseHealthEliteType1}, Type2: {SurvivalBaseHealthEliteType2}");
            }
        }

        // Расчет финального HP для обычных врагов
        private int CalculateFinalHealthRegular(int cycle)
        {
            if (cycle == 0) return 5; // Начальное значение

            float multiplier = GetCycleMultiplier(cycle);
            int baseHealth = CalculateFinalHealthRegular(cycle - 1);
            return (int)((baseHealth + 35) * multiplier); // 35 = бонус за 7 этапов (5*7)
        }

        // Расчет финального HP для элитных врагов
        private int CalculateFinalHealthElite(int cycle, EliteEnemy.EliteType eliteType)
        {
            if (cycle == 0) return eliteType == EliteEnemy.EliteType.Type1 ? 50 : 100;

            float multiplier = GetCycleMultiplier(cycle);
            int baseHealth = CalculateFinalHealthElite(cycle - 1, eliteType);
            return (int)((baseHealth + 700) * multiplier); // 700 = бонус за 7 этапов (100*7)
        }

        // Получение множителя цикла
        private float GetCycleMultiplier(int cycle)
        {
            switch (cycle)
            {
                case 1: return 1.5f; // Цикл 2
                case 2: return 2.0f; // Цикл 3
                case 3: return 2.0f; // Цикл 4
                default: return 2.0f; // Для циклов выше 4
            }
        }
    }
}