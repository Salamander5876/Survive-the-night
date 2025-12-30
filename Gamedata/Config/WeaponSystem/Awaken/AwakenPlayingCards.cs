// Gamedata/Config/WeaponSystem/Projectiles/AwakenPlayingCard.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class AwakenPlayingCard : Projectile
    {
        private static Texture2D _defaultAwakenTexture;
        private Texture2D _cardTexture;

        // Список врагов, которым эта карта уже нанесла урон
        private List<Enemy> _hitEnemies = new List<Enemy>();

        public AwakenPlayingCard(Vector2 position, int size, Color color, int damage,
                                float speed, Vector2 direction, int hitsLeft = 1,
                                Texture2D texture = null)
            : base(position, size, color, damage, speed, position + direction * 1000f, hitsLeft)
        {
            _cardTexture = texture ?? _defaultAwakenTexture;

            // Устанавливаем направление
            Direction = direction;

            // Вращаем спрайт в направлении полета + 90 градусов
            float angle = (float)Math.Atan2(direction.Y, direction.X);
            Rotation = angle * (180f / (float)Math.PI) + 90f;

            // Автоматический размер из текстуры * 2
            if (_cardTexture != null)
            {
                Size = Math.Max(_cardTexture.Width, _cardTexture.Height) * 2;
            }

            // Инициализируем список пораженных врагов
            _hitEnemies = new List<Enemy>();
        }

        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultAwakenTexture = texture;
        }

        // Метод для проверки и нанесения урона врагу
        public bool TryHitEnemy(Enemy enemy)
        {
            // Если карта неактивна или уже поразила этого врага
            if (!IsActive || _hitEnemies.Contains(enemy))
                return false;

            // Если есть пересечение
            if (GetBounds().Intersects(enemy.GetBounds()))
            {
                // Наносим урон
                enemy.TakeDamage(Damage);

                // Добавляем врага в список пораженных
                _hitEnemies.Add(enemy);

                // Уменьшаем оставшиеся пробития
                HitsLeft--;

                // Если пробития закончились - деактивируем карту
                if (HitsLeft <= 0)
                {
                    IsActive = false;
                }

                return true;
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер жизни
            _lifeTimer += deltaTime;
            if (_lifeTimer >= MaxLifeTime)
            {
                IsActive = false;
                return;
            }

            // Обновляем позицию
            Position += Direction * Speed * deltaTime;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            if (_cardTexture != null)
            {
                DrawWithTexture(spriteBatch, _cardTexture);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }
    }
}