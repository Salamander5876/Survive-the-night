using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using System.Diagnostics;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class BeerBottleProjectile : Projectile
    {
        private Vector2 _targetPosition;
        private float _maxFlightTime = 1.5f;
        private float _flightTime = 0f;
        private bool _soundPlayed = false;
        private SoundEffect _throwSound;

        public BeerBottleProjectile(Vector2 startPosition, Vector2 targetPosition, int size, Color color, float speed, SoundEffect throwSound)
            : base(startPosition, size, color, 0, speed, targetPosition, 1)
        {
            _targetPosition = targetPosition;
            Direction = Vector2.Normalize(targetPosition - startPosition);
            _throwSound = throwSound;

            // Устанавливаем реальный размер на основе текстуры
            Size = size;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _flightTime += deltaTime;

            if (!_soundPlayed && _throwSound != null)
            {
                _throwSound.Play();
                _soundPlayed = true;
                Debug.WriteLine("Beer bottle throw sound played!");
            }

            Position += Direction * Speed * deltaTime;
            Rotation += 180f * deltaTime;

            float distanceToTarget = Vector2.Distance(Position, _targetPosition);
            if (_flightTime >= _maxFlightTime || distanceToTarget < 10f)
            {
                IsActive = false;
            }
        }

        public override void DrawWithTexture(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!IsActive || texture == null) return;

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // Используем реальный размер текстуры
            float scale = 1.0f; // Без масштабирования - используем оригинальный размер

            spriteBatch.Draw(
                texture,
                Position,
                null,
                Color,
                MathHelper.ToRadians(Rotation),
                origin,
                scale,
                SpriteEffects.None,
                0.3f // Layer depth между лужей и другими объектами
            );
        }
    }
}