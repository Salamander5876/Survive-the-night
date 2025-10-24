using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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

        public void EliteKilled()
        {
            _elitesKilled++;

            // Каждые 2 убитых элитных врага повышаем уровень (максимум 8)
            if (_elitesKilled % 2 == 0 && _currentLevel < 8)
            {
                _currentLevel++;
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