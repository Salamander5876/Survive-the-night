// WealthArtifactMoneyProjectile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Scripts.Managers;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class WealthArtifactMoneyProjectile : Projectile
    {
        private static Texture2D[] _moneyTextures = new Texture2D[5];
        private float _rotationSpeed = 720f; // Высокая скорость вращения влево
        private Texture2D _currentMoneyTexture;

        // Настройка отрисовки хитбокса
        public static bool ShowHitboxDebug = false;

        public WealthArtifactMoneyProjectile(Vector2 position, int damage, Vector2 direction)
            : base(position, 26, Color.White, damage, 300f, position + direction, 1)
        {
            // Хитбокс 26x26 - КОММЕНТАРИЙ: Здесь задается размер хитбокса
            Size = 26;

            Speed = 300f;
            Direction = Vector2.Normalize(direction);

            // Устанавливаем случайную текстуру монеты
            if (_moneyTextures != null && _moneyTextures.Length > 0)
            {
                int randomIndex = Game1.Random.Next(0, _moneyTextures.Length);
                if (_moneyTextures[randomIndex] != null)
                {
                    _currentMoneyTexture = _moneyTextures[randomIndex];
                    SetTexture(_currentMoneyTexture);
                }
            }
        }

        public static void SetTextures(Texture2D money1, Texture2D money2, Texture2D money3,
                                     Texture2D money4, Texture2D money5)
        {
            _moneyTextures[0] = money1;
            _moneyTextures[1] = money2;
            _moneyTextures[2] = money3;
            _moneyTextures[3] = money4;
            _moneyTextures[4] = money5;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Вращение влево (отрицательное)
            Rotation -= _rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            if (_currentMoneyTexture != null)
            {
                DrawWithTexture(spriteBatch, _currentMoneyTexture);
            }
            else
            {
                // Если текстура не загружена, рисуем белый квадрат
                base.Draw(spriteBatch, debugTexture);
            }

            // Отрисовка хитбокса для отладки
            if (ShowHitboxDebug)
            {
                Rectangle hitbox = GetBounds();
                spriteBatch.Draw(debugTexture, hitbox, Color.Red * 0.5f);
            }
        }

        public override Rectangle GetBounds()
        {
            // Хитбокс 26x26
            return new Rectangle(
                (int)Position.X - 13,  // 26/2 = 13
                (int)Position.Y - 13,
                26,
                26
            );
        }
    }
}