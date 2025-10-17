using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Survive_the_night.Managers
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
            _tileSize = floorTexture.Width; // ���������� �������� ������ ��������
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // �������� ������� ������ ����� �������� Position
            Vector2 cameraPosition = _camera.Position;

            // ������������ ������� �������
            float visibleLeft = cameraPosition.X;
            float visibleTop = cameraPosition.Y;
            float visibleRight = cameraPosition.X + _viewport.Width;
            float visibleBottom = cameraPosition.Y + _viewport.Height;

            // ���������� ��������� � �������� ����� ��� ���������
            int startTileX = (int)(visibleLeft / _tileSize) - 1;
            int startTileY = (int)(visibleTop / _tileSize) - 1;
            int endTileX = (int)(visibleRight / _tileSize) + 1;
            int endTileY = (int)(visibleBottom / _tileSize) + 1;

            // ������������ ������ ������� �����
            for (int x = startTileX; x <= endTileX; x++)
            {
                for (int y = startTileY; y <= endTileY; y++)
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
                        1f, // ���������� �������� ������� ��������
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }

        // ����� ��� ���������� viewport (���� ��������� ������ ����)
        public void UpdateViewport(Viewport viewport)
        {
            _viewport = viewport;
        }

        // ����� ��� ��������� ��������
        public void ChangeFloorTexture(Texture2D newTexture)
        {
            _floorTexture = newTexture;
            _tileSize = newTexture.Width; // ��������� ������ �����
        }
    }
}