using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;

namespace Survive_the_night.Managers
{
    // Класс, отвечающий за следование камеры за игроком
    public class Camera
    {
        public Matrix Transform { get; private set; }
        private Player _target; // Объект, за которым мы следим
        private Viewport _viewport;

        public Camera(Player target, Viewport viewport)
        {
            _target = target;
            _viewport = viewport;
        }

        public void Follow()
        {
            // 1. Находим центр экрана
            Vector2 screenCenter = new Vector2(_viewport.Width / 2, _viewport.Height / 2);

            // 2. Рассчитываем, насколько нужно сместить мир, чтобы игрок оказался в центре.
            // Смещение мира = Центр экрана - Позиция игрока.
            Vector3 translation = new Vector3(screenCenter - _target.Position, 0);

            // 3. Создаем матрицу:
            // - Сначала смещаем мир (translation)
            // - Затем поворачиваем и масштабируем (сейчас не используем)
            // - Затем центрируем
            Transform = Matrix.CreateTranslation(translation) *
                        Matrix.CreateRotationZ(0) *
                        Matrix.CreateScale(1, 1, 1) *
                        Matrix.CreateTranslation(new Vector3(0, 0, 0));
        }
    }
}