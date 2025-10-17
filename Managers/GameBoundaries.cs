using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Managers
{
    public class GameBoundaries
    {
        private Viewport _viewport;
        private Camera _camera;

        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Top { get; private set; }
        public float Bottom { get; private set; }

        public float Width { get; private set; }
        public float Height { get; private set; }

        // Расширенные границы для спавна (за пределами экрана)
        public float SpawnLeft { get; private set; }
        public float SpawnRight { get; private set; }
        public float SpawnTop { get; private set; }
        public float SpawnBottom { get; private set; }

        public GameBoundaries(Camera camera, Viewport viewport)
        {
            _camera = camera;
            _viewport = viewport;
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            // Основные границы экрана (видимая область)
            Left = _camera.Position.X;
            Top = _camera.Position.Y;
            Right = Left + _viewport.Width;
            Bottom = Top + _viewport.Height;
            Width = _viewport.Width;
            Height = _viewport.Height;

            // Границы спавна (на 200 пикселей за пределами экрана)
            float spawnMargin = 200f;
            SpawnLeft = Left - spawnMargin;
            SpawnRight = Right + spawnMargin;
            SpawnTop = Top - spawnMargin;
            SpawnBottom = Bottom + spawnMargin;
        }

        public bool IsInsideScreen(Vector2 position)
        {
            return position.X >= Left && position.X <= Right &&
                   position.Y >= Top && position.Y <= Bottom;
        }

        public bool IsInsideSpawnArea(Vector2 position)
        {
            return position.X >= SpawnLeft && position.X <= SpawnRight &&
                   position.Y >= SpawnTop && position.Y <= SpawnBottom;
        }

        public Rectangle GetScreenBounds()
        {
            return new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
        }

        public Rectangle GetSpawnBounds()
        {
            return new Rectangle((int)SpawnLeft, (int)SpawnTop,
                               (int)(SpawnRight - SpawnLeft),
                               (int)(SpawnBottom - SpawnTop));
        }
    }
}