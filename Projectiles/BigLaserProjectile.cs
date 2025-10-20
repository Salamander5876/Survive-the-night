using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Weapons
{
    public class BigLaserProjectile : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _laserTexture;
        private Player _player;
        private List<Enemy> _enemies;

        public new float Rotation { get; set; }
        public float Length { get; private set; }
        public float Width { get; private set; }
        public float DamageInterval { get; set; } = 0.11f;
        private float _damageTimer = 0f;

        private float _targetRotation;
        public float RotationSpeed { get; set; } = 1.0f;

        // Для отслеживания уже пораженных врагов в текущем цикле урона
        private HashSet<Enemy> _hitEnemiesThisCycle = new HashSet<Enemy>();

        // Текущая цель для прицеливания
        private Enemy _currentTarget;

        // Звук лазера
        private SoundEffect _laserSound;
        private SoundEffectInstance _laserSoundInstance;

        public BigLaserProjectile(Vector2 position, Player player, List<Enemy> enemies, int damage, Texture2D texture = null, SoundEffect laserSound = null)
            : base(position, 20, Color.White, damage, 0f, Vector2.Zero, int.MaxValue)
        {
            _player = player;
            _enemies = enemies;
            _laserTexture = texture ?? _defaultTexture;
            _laserSound = laserSound;

            // Автоматически определяем размеры из текстуры
            if (_laserTexture != null)
            {
                Width = _laserTexture.Height;
                Length = _laserTexture.Width;
            }
            else
            {
                Width = 40f;
                Length = 800f;
            }

            SetLifeTime(180f);

            Rotation = 0f;
            _targetRotation = 0f;
            _currentTarget = FindBestTarget();

            // Создаем экземпляр звука для непрерывного воспроизведения
            if (_laserSound != null)
            {
                _laserSoundInstance = _laserSound.CreateInstance();
                _laserSoundInstance.IsLooped = true;
                _laserSoundInstance.Play();
            }
        }

        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Следуем за игроком
            Position = _player.Position;

            // Обновляем цель каждые 0.5 секунды для оптимизации
            if (gameTime.TotalGameTime.TotalSeconds % 0.5 < deltaTime)
            {
                _currentTarget = FindBestTarget();
            }

            // Наводимся на цель
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                Vector2 direction = _currentTarget.Position - Position;
                _targetRotation = (float)System.Math.Atan2(direction.Y, direction.X);
            }
            else
            {
                // Если цели нет, ищем новую
                _currentTarget = FindBestTarget();
            }

            // Плавный поворот
            float rotationDifference = _targetRotation - Rotation;

            // Нормализуем разницу
            while (rotationDifference > MathHelper.Pi)
                rotationDifference -= MathHelper.TwoPi;
            while (rotationDifference < -MathHelper.Pi)
                rotationDifference += MathHelper.TwoPi;

            Rotation += rotationDifference * RotationSpeed * deltaTime;

            // Обновляем таймер урона
            _damageTimer -= deltaTime;
            if (_damageTimer <= 0f)
            {
                ApplyDamageToEnemiesInBeam();
                _damageTimer = DamageInterval;
                _hitEnemiesThisCycle.Clear();
            }
        }

        private Enemy FindBestTarget()
        {
            Enemy bestTarget = null;
            float bestScore = float.MinValue;

            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive) continue;

                float distance = Vector2.Distance(Position, enemy.Position);

                // Проверяем, что враг в пределах досягаемости
                if (distance > Length) continue;

                // Вычисляем "ценность" цели:
                // 1. Близость к игроку (чем ближе, тем лучше)
                float proximityScore = 1.0f - (distance / Length);

                // 2. Приоритет целей, которые дольше живут (ориентируемся на тип врага)
                float typeScore = GetEnemyTypeScore(enemy);

                // 3. Бонус за то, что цель уже в зоне урона
                float inDamageZoneBonus = IsEnemyInSpriteCollision(enemy) ? 0.4f : 0f;

                // Итоговый счет
                float totalScore = proximityScore * 0.3f + typeScore * 0.3f + inDamageZoneBonus * 0.4f;

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }

        private float GetEnemyTypeScore(Enemy enemy)
        {
            // EliteEnemy - самые ценные цели
            if (enemy is EliteEnemy)
                return 1.0f;

            // Обычные враги - средняя ценность
            return 0.5f;
        }

        private void ApplyDamageToEnemiesInBeam()
        {
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive) continue;

                if (_hitEnemiesThisCycle.Contains(enemy)) continue;

                // Урон наносится только если враг пересекается со спрайтом лазера
                if (IsEnemyInSpriteCollision(enemy))
                {
                    enemy.TakeDamage(Damage);
                    _hitEnemiesThisCycle.Add(enemy);
                }
            }
        }

        private bool IsEnemyInSpriteCollision(Enemy enemy)
        {
            // Получаем границы спрайта лазера
            Rectangle laserSpriteBounds = GetLaserSpriteBounds();

            // Получаем границы врага
            Rectangle enemyBounds = enemy.GetBounds();

            // Проверяем пересечение спрайта лазера с врагом
            return laserSpriteBounds.Intersects(enemyBounds);
        }

        private Rectangle GetLaserSpriteBounds()
        {
            // Вычисляем конечную точку лазера
            Vector2 endPoint = Position + new Vector2(
                (float)System.Math.Cos(Rotation) * Length,
                (float)System.Math.Sin(Rotation) * Length
            );

            // Создаем прямоугольник, представляющий спрайт лазера
            Vector2 laserDirection = Vector2.Normalize(endPoint - Position);
            Vector2 perpendicular = new Vector2(-laserDirection.Y, laserDirection.X);

            // Четыре угла прямоугольника спрайта лазера
            Vector2 topLeft = Position - perpendicular * Width / 2;
            Vector2 topRight = Position + perpendicular * Width / 2;
            Vector2 bottomLeft = endPoint - perpendicular * Width / 2;
            Vector2 bottomRight = endPoint + perpendicular * Width / 2;

            // Находим границы прямоугольника
            float minX = MathHelper.Min(MathHelper.Min(topLeft.X, topRight.X), MathHelper.Min(bottomLeft.X, bottomRight.X));
            float minY = MathHelper.Min(MathHelper.Min(topLeft.Y, topRight.Y), MathHelper.Min(bottomLeft.Y, bottomRight.Y));
            float maxX = MathHelper.Max(MathHelper.Max(topLeft.X, topRight.X), MathHelper.Max(bottomLeft.X, bottomRight.X));
            float maxY = MathHelper.Max(MathHelper.Max(topLeft.Y, topRight.Y), MathHelper.Max(bottomLeft.Y, bottomRight.Y));

            return new Rectangle(
                (int)minX,
                (int)minY,
                (int)(maxX - minX),
                (int)(maxY - minY)
            );
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive || _laserTexture == null) return;

            // Отрисовка лазера с правильной ориентацией
            Vector2 origin = new Vector2(0, _laserTexture.Height / 2);
            float scale = 1.0f;

            spriteBatch.Draw(
                _laserTexture,
                Position,
                null,
                Color.White * 0.8f,
                Rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        public override Rectangle GetBounds()
        {
            return GetLaserSpriteBounds();
        }

        public void UpdateTexture(Texture2D newTexture)
        {
            if (newTexture != null)
            {
                _laserTexture = newTexture;
                Width = _laserTexture.Height;
                Length = _laserTexture.Width;
            }
        }

        // Останавливаем звук при деактивации лазера
        public void StopSound()
        {
            _laserSoundInstance?.Stop();
            _laserSoundInstance?.Dispose();
        }

        protected override void OnDeactivate()
        {
            StopSound();
            base.OnDeactivate();
        }
    }
}