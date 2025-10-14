using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Survive_the_night.Projectiles
{
    public class MolotovProjectile : Projectile
    {
        private Vector2 _targetPosition;
        private float _maxFlightTime = 2f;
        private float _flightTime = 0f;
        private bool _soundPlayed = false;

        public MolotovProjectile(Vector2 startPosition, Vector2 targetPosition, int size, Color color, float speed = 300f)
            : base(startPosition, size, color, 0, speed, targetPosition, 1)
        {
            _targetPosition = targetPosition;
            Direction = Vector2.Normalize(targetPosition - startPosition);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _flightTime += deltaTime;

            // Проигрываем звук броска при старте (только один раз) - КАК В CARDS
            if (!_soundPlayed)
            {
                Game1.SFXThrowMolotov?.Play();
                _soundPlayed = true;
                Debug.WriteLine("🔊 Molotov throw sound played!");
            }

            // Двигаемся к цели
            Position += Direction * Speed * deltaTime;

            // Вращение бутылки для эффекта полета
            Rotation += 180f * deltaTime;

            // Проверяем достижение цели или истечение времени полета
            float distanceToTarget = Vector2.Distance(Position, _targetPosition);
            if (_flightTime >= _maxFlightTime || distanceToTarget < 10f)
            {
                IsActive = false;
            }
        }
    }
}