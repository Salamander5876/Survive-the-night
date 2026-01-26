using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using System;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class BanknoteProjectile : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _banknoteTexture;

        // Размеры банкноты (в пикселях)
        // КОММЕНТАРИЙ: Измените эти значения если нужно настроить размер хитбокса
        private const int BANKNOTE_WIDTH = 38;
        private const int BANKNOTE_HEIGHT = 22;

        // Масштаб хитбокса относительно визуального размера (0.0 - 1.0)
        // КОММЕНТАРИЙ: Уменьшите если хитбокс слишком большой, увеличьте если слишком маленький
        private const float HITBOX_SCALE = 0.8f;

        public BanknoteProjectile(Vector2 position, int size, Color color, int damage, float speed, Vector2 direction, int hitsLeft = 1, Texture2D texture = null)
            : base(position, size, color, damage, speed, position + direction, hitsLeft)
        {
            _banknoteTexture = texture ?? _defaultTexture;

            // Устанавливаем направление
            Direction = direction;

            // Устанавливаем фиксированный размер хитбокса
            // КОММЕНТАРИЙ: Если нужно изменить размер хитбокса, поменяйте BANKNOTE_WIDTH и BANKNOTE_HEIGHT выше
            Size = (int)(Math.Max(BANKNOTE_WIDTH, BANKNOTE_HEIGHT) * HITBOX_SCALE);

            // Устанавливаем правильный угол поворота (снаряд смотрит в направлении полета)
            Rotation = (float)Math.Atan2(Direction.Y, Direction.X);
        }

        // Метод для установки текстуры по умолчанию
        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер жизни
            _lifeTimer += deltaTime;
            if (_lifeTimer >= MaxLifeTime)
            {
                IsActive = false;
                OnDeactivate();
                return;
            }

            // Двигаем снаряд
            Position += Direction * Speed * deltaTime;

            // НЕ ВРАЩАЕМ СНАРЯД! Он уже повернут в нужную сторону
            // Убираем: Rotation += 180f * deltaTime;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (_banknoteTexture != null)
            {
                DrawWithTexture(spriteBatch, _banknoteTexture);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }

        public override void DrawWithTexture(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!IsActive || texture == null) return;

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // Рассчитываем масштаб для визуального отображения
            // КОММЕНТАРИЙ: Если нужно изменить визуальный размер, поменяйте BANKNOTE_WIDTH и BANKNOTE_HEIGHT
            float scaleX = (float)BANKNOTE_WIDTH / texture.Width;
            float scaleY = (float)BANKNOTE_HEIGHT / texture.Height;
            float scale = Math.Min(scaleX, scaleY);

            spriteBatch.Draw(
                texture,
                Position,
                null,
                Color,
                Rotation, // Угол в радианах - снаряд смотрит в направлении полета
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        public override Rectangle GetBounds()
        {
            // Хитбокс на основе установленных размеров
            // КОММЕНТАРИЙ: Size уже установлен в конструкторе с учетом HITBOX_SCALE
            return new Rectangle(
                (int)Position.X - Size / 2,
                (int)Position.Y - Size / 2,
                Size,
                Size
            );
        }
    }
}