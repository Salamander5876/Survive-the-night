using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Weapons;
using System;
using System.Diagnostics;

namespace Survive_the_night.Projectiles
{
    public class RouletteBallProjectile : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _ballTexture;

        public int BouncesLeft { get; private set; }
        public float DistanceTraveled { get; set; } = 0f;

        // Для отслеживания отладочной информации
        private int _totalBounces = 0;

        // Подвижные границы (будут обновляться извне)
        public Rectangle ScreenBounds { get; set; }

        // Ссылка на оружие для поиска целей
        private RouletteBall _weapon;

        public RouletteBallProjectile(Vector2 position, int size, Color color, float speed, Vector2 direction, int maxBounces, int damage)
            : base(position, size, color, damage, speed, position + direction, maxBounces)
        {
            BouncesLeft = maxBounces;
            Direction = direction;
            _ballTexture = _defaultTexture;
            Damage = damage;

            if (_ballTexture != null && size == 0)
            {
                Size = Math.Max(_ballTexture.Width, _ballTexture.Height);
            }

            SetLifeTime(300f);
            ScreenBounds = new Rectangle(0, 0, 1280, 720);

            Debug.WriteLine($"Создан шарик рулетки. Урон: {Damage}, Скорость: {Speed}, Отскоков: {BouncesLeft}");
        }

        public void SetWeapon(RouletteBall weapon)
        {
            _weapon = weapon;
        }

        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 oldPosition = Position;
            Position += Direction * Speed * deltaTime;
            DistanceTraveled += Vector2.Distance(oldPosition, Position);

            UpdateLifeTime(gameTime);
            CheckBoundaryCollision();
        }

        protected override void UpdateLifeTime(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lifeTimer += deltaTime;

            if (_lifeTimer >= MaxLifeTime)
            {
                IsActive = false;
                Debug.WriteLine($"Шарик исчез по времени: {_lifeTimer:0}с, всего отскоков: {_totalBounces}");
                return;
            }
        }

        private void CheckBoundaryCollision()
        {
            bool bounced = false;

            if (Position.X - Size / 2 < ScreenBounds.Left)
            {
                HandleBoundaryBounce();
                Position = new Vector2(ScreenBounds.Left + Size / 2 + 1, Position.Y);
                bounced = true;
            }
            else if (Position.X + Size / 2 > ScreenBounds.Right)
            {
                HandleBoundaryBounce();
                Position = new Vector2(ScreenBounds.Right - Size / 2 - 1, Position.Y);
                bounced = true;
            }

            if (Position.Y - Size / 2 < ScreenBounds.Top)
            {
                HandleBoundaryBounce();
                Position = new Vector2(Position.X, ScreenBounds.Top + Size / 2 + 1);
                bounced = true;
            }
            else if (Position.Y + Size / 2 > ScreenBounds.Bottom)
            {
                HandleBoundaryBounce();
                Position = new Vector2(Position.X, ScreenBounds.Bottom - Size / 2 - 1);
                bounced = true;
            }
        }

        private void HandleBoundaryBounce()
        {
            BouncesLeft--;
            _totalBounces++;

            Debug.WriteLine($"Отскок #{_totalBounces} от границы. Осталось отскоков: {BouncesLeft}");

            // ВОСПРОИЗВОДИМ ЗВУК ПРИ ОТСКОКЕ ОТ СТЕНЫ
            if (_weapon != null)
            {
                _weapon.PlayBounceSound();
            }

            if (BouncesLeft <= 0)
            {
                IsActive = false;
                Debug.WriteLine($"Шарик завершил все {_totalBounces} отскоков");
                return;
            }

            // ПОСЛЕ ОТСКОКА ОТ СТЕНЫ - ЛЕТИМ К НОВОМУ ВРАГУ
            if (_weapon != null)
            {
                var nextTarget = _weapon.FindNextTarget(Position, Game1.CurrentEnemies);
                if (nextTarget.HasValue)
                {
                    Direction = Vector2.Normalize(nextTarget.Value - Position);
                    Debug.WriteLine($"Новое направление к врагу: {Direction}");
                }
                else
                {
                    Direction = new Vector2(
                        (float)(Game1.Random.NextDouble() * 2 - 1),
                        (float)(Game1.Random.NextDouble() * 2 - 1)
                    );
                    Direction.Normalize();
                    Debug.WriteLine($"Новое случайное направление: {Direction}");
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            if (_ballTexture != null)
            {
                DrawWithTexture(spriteBatch, _ballTexture);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }
    }
}