using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Managers
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        public Vector2 Position { get; private set; } // ДОБАВЛЕНО: свойство Position
        private Player _target;
        private Viewport _viewport;

        public Camera(Player target, Viewport viewport)
        {
            _target = target;
            _viewport = viewport;
        }

        public void Follow()
        {
            // Ограничиваем камеру границами мира
            var position = Matrix.CreateTranslation(
                -_target.Position.X - (_target.Size / 2),
                -_target.Position.Y - (_target.Size / 2),
                0);

            var offset = Matrix.CreateTranslation(
                _viewport.Width / 2,
                _viewport.Height / 2,
                0);

            Transform = position * offset;

            // ДОБАВЛЕНО: вычисляем позицию камеры
            Position = new Vector2(
                _target.Position.X + (_target.Size / 2) - _viewport.Width / 2,
                _target.Position.Y + (_target.Size / 2) - _viewport.Height / 2
            );
        }
    }
}