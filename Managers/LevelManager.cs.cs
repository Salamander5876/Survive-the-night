using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survive_the_night.Managers
{
    public class LevelManager
    {
        private int _currentLevel = 1;
        private int _elitesKilled = 0;
        private Dictionary<int, string> _levelFloorTextures;

        public int CurrentLevel => _currentLevel;
        public int ElitesKilled => _elitesKilled;

        public LevelManager()
        {
            _levelFloorTextures = new Dictionary<int, string>();

            // Инициализируем текстуры пола для каждого уровня
            for (int i = 1; i <= 8; i++)
            {
                _levelFloorTextures[i] = $"Sprites/HardCasinoFloor{i}";
            }
        }

        // В LevelManager.cs добавьте отладочный вывод
        public void EliteKilled()
        {
            _elitesKilled++;

            Debug.WriteLine($"Элитный враг убит! Всего: {_elitesKilled}");

            // Каждые 2 убитых элитных врага повышаем уровень (максимум 8)
            if (_elitesKilled % 2 == 0 && _currentLevel < 8)
            {
                _currentLevel++;
                Debug.WriteLine($"Уровень повышен до {_currentLevel}!");
            }
            else
            {
                Debug.WriteLine($"Текущий уровень: {_currentLevel}, элитных убито: {_elitesKilled}");
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
        }
    }
}