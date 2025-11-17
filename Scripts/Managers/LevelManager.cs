using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Scripts.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survive_the_night.Scripts.Managers
{
    public class LevelManager
    {
        private int _currentLevel = 1;
        private int _elitesKilled = 0;
        private Dictionary<int, string> _levelFloorTextures;
        private DifficultyManager _difficultyManager;

        public int CurrentLevel => _currentLevel;
        public int ElitesKilled => _elitesKilled;

        public LevelManager(DifficultyManager difficultyManager)
        {
            _difficultyManager = difficultyManager;
            _levelFloorTextures = new Dictionary<int, string>();

            for (int i = 1; i <= 8; i++)
            {
                _levelFloorTextures[i] = $"Sprites/HardCasinoFloor{i}";
            }
        }

        public void EliteKilled()
        {
            _elitesKilled++;

            Debug.WriteLine($"Элитный враг убит! Всего: {_elitesKilled}");

            int requiredElites = _difficultyManager?.ElitesRequiredForStage ?? 2;

            // Проверяем, достигли ли мы нужного количества элитных врагов для перехода
            if (_elitesKilled % requiredElites == 0)
            {
                if (_difficultyManager?.CurrentDifficulty == StartMenu.GameMode.Survival)
                {
                    // В режиме Survival всегда переходим на следующий уровень
                    _currentLevel++;

                    // Если достигли 9 уровня - сбрасываем на 1 и увеличиваем цикл
                    if (_currentLevel > 8)
                    {
                        _currentLevel = 1;
                        _difficultyManager?.AdvanceCycle();
                        Debug.WriteLine($"Режим выживания: переход на цикл {_difficultyManager?.CurrentCycle}");
                    }
                    else
                    {
                        Debug.WriteLine($"Режим выживания: уровень повышен до {_currentLevel}");
                    }
                }
                else if (_currentLevel < 8)
                {
                    // В обычных режимах повышаем уровень только до 8
                    _currentLevel++;
                    Debug.WriteLine($"Уровень повышен до {_currentLevel}! Требовалось элитных: {requiredElites}");
                }
                else
                {
                    Debug.WriteLine($"Достигнут максимальный уровень {_currentLevel}. Элитных убито: {_elitesKilled}");
                }
            }
            else
            {
                Debug.WriteLine($"Текущий уровень: {_currentLevel}, элитных убито: {_elitesKilled}/{requiredElites}");
            }
        }

        public void AdvanceStageInEndless()
        {
            _currentLevel++;
            if (_currentLevel > 8)
            {
                _currentLevel = 1;
                _difficultyManager?.AdvanceCycle();
                Debug.WriteLine($"Режим выживания: цикл {_difficultyManager?.CurrentCycle}");
            }
            else
            {
                Debug.WriteLine($"Режим выживания: уровень повышен до {_currentLevel}");
            }
        }

        private string GetEnemyTypeForStage(int stage)
        {
            switch (stage)
            {
                case 1: return "BasicEnemy";
                case 2: return "TankEnemy";
                case 3: return "FastEnemy";
                case 4: return "StrongEnemy";
                case 5: return "VampireEnemy";
                case 6: return "RangedEnemy";
                case 7: return "UndyingEnemy";
                case 8: return "UndyingEnemy";
                default: return "BasicEnemy";
            }
        }

        public string GetCurrentLevelFloorTexture()
        {
            return _levelFloorTextures[_currentLevel];
        }

        public Texture2D LoadCurrentLevelFloorTexture(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            return content.Load<Texture2D>(GetCurrentLevelFloorTexture());
        }

        public void Reset()
        {
            _currentLevel = 1;
            _elitesKilled = 0;
            Debug.WriteLine($"LevelManager сброшен: уровень={_currentLevel}, элитных убито={_elitesKilled}");
        }

        public bool ShouldSpawnBothEliteTypes()
        {
            return _difficultyManager?.SpawnBothEliteTypes ?? true;
        }
    }
}