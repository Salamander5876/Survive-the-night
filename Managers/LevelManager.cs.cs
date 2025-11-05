using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survive_the_night.Managers
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

            // Не повышаем уровень если уже на 8 уровне (кроме бесконечного режима)
            if (_elitesKilled % requiredElites == 0)
            {
                if (_difficultyManager?.CurrentDifficulty == StartMenu.GameMode.Endless)
                {
                    AdvanceStageInEndless();
                }
                else if (_currentLevel < 8)
                {
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
                Debug.WriteLine($"Бесконечный режим: цикл {_difficultyManager?.CurrentCycle}");
            }
            else
            {
                Debug.WriteLine($"Бесконечный режим: уровень повышен до {_currentLevel}");
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
            System.Diagnostics.Debug.WriteLine($"LevelManager сброшен: уровень={_currentLevel}, элитных убито={_elitesKilled}");
        }

        public bool ShouldSpawnBothEliteTypes()
        {
            return _difficultyManager?.SpawnBothEliteTypes ?? true;
        }
    }
}