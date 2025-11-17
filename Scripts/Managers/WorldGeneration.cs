using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Survive_the_night.Scripts.Managers
{
    public class WorldGeneration
    {
        private Texture2D _floorTexture;
        private int _tileSize;
        private Viewport _viewport;
        private Camera _camera;

        public WorldGeneration(Texture2D floorTexture, Camera camera, Viewport viewport)
        {
            _floorTexture = floorTexture;
            _camera = camera;
            _viewport = viewport;
            _tileSize = floorTexture.Width;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Получаем позицию игрока (центр камеры)
            Vector2 playerPosition = _camera.Position;

            // Рассчитываем область отрисовки относительно игрока
            // Используем большой радиус для бесконечного пола
            int tilesInViewX = _viewport.Width / _tileSize + 10; // +10 для запаса
            int tilesInViewY = _viewport.Height / _tileSize + 10;

            // Центр отрисовки - позиция игрока
            int centerTileX = (int)(playerPosition.X / _tileSize);
            int centerTileY = (int)(playerPosition.Y / _tileSize);

            // Отрисовываем тайлы в большом радиусе вокруг игрока
            for (int x = centerTileX - tilesInViewX; x <= centerTileX + tilesInViewX; x++)
            {
                for (int y = centerTileY - tilesInViewY; y <= centerTileY + tilesInViewY; y++)
                {
                    Vector2 tilePosition = new Vector2(
                        x * _tileSize,
                        y * _tileSize
                    );

                    spriteBatch.Draw(
                        _floorTexture,
                        tilePosition,
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }

        public void UpdateViewport(Viewport viewport)
        {
            _viewport = viewport;
        }

        public void ChangeFloorTexture(Texture2D newTexture)
        {
            _floorTexture = newTexture;
            _tileSize = newTexture.Width;
        }
    }
}