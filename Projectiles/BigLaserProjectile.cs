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
        public float DamageInterval { get; set; } = 0.2f;
        private float _damageTimer = 0f;

        private float _targetRotation;
        public float RotationSpeed { get; set; } = 1.5f; // Скорость поворота 1.5

        // Для отслеживания уже пораженных врагов в текущем цикле урона
        private HashSet<Enemy> _hitEnemiesThisCycle = new HashSet<Enemy>();

        // Текущая цель для прицеливания
        private Enemy _currentTarget;

        // Звук лазера
        private SoundEffect _laserSound;
        private SoundEffectInstance _laserSoundInstance;

        // Фиксированные размеры хитбокса (толщина луча 33 пикселя)
        private const float HITBOX_WIDTH = 33f;
        private const float HITBOX_LENGTH = 800f;

        public BigLaserProjectile(Vector2 position, Player player, List<Enemy> enemies, int damage, Texture2D texture = null, SoundEffect laserSound = null)
            : base(position, 20, Color.White, damage, 0f, Vector2.Zero, int.MaxValue)
        {
            _player = player;
            _enemies = enemies;
            _laserTexture = texture ?? _defaultTexture;
            _laserSound = laserSound;

            // Автоматически определяем размеры из текстуры для отрисовки
            if (_laserTexture != null)
            {
                Width = _laserTexture.Height;   // 90 пикселей
                Length = _laserTexture.Width;   // 800 пикселей
            }
            else
            {
                Width = 90f;
                Length = 800f;
            }

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
                if (distance > HITBOX_LENGTH) continue;

                // Вычисляем "ценность" цели:
                // 1. Близость к игроку (чем ближе, тем лучше)
                float proximityScore = 1.0f - (distance / HITBOX_LENGTH);

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

                // Урон наносится только если враг пересекается с хитбоксом лазера
                if (IsEnemyInSpriteCollision(enemy))
                {
                    enemy.TakeDamage(Damage);
                    _hitEnemiesThisCycle.Add(enemy);
                }
            }
        }

        private bool IsEnemyInSpriteCollision(Enemy enemy)
        {
            // Получаем вершины повернутого хитбокса лазера
            Vector2[] laserVertices = GetRotatedHitboxVertices();

            // Получаем вершины врага (предполагаем, что это прямоугольник)
            Rectangle enemyBounds = enemy.GetBounds();
            Vector2[] enemyVertices = new Vector2[]
            {
                new Vector2(enemyBounds.Left, enemyBounds.Top),
                new Vector2(enemyBounds.Right, enemyBounds.Top),
                new Vector2(enemyBounds.Right, enemyBounds.Bottom),
                new Vector2(enemyBounds.Left, enemyBounds.Bottom)
            };

            // Проверяем пересечение с помощью Separating Axis Theorem (SAT)
            return PolygonsIntersect(laserVertices, enemyVertices);
        }

        // Получаем вершины повернутого хитбокса
        private Vector2[] GetRotatedHitboxVertices()
        {
            Vector2 endPoint = Position + new Vector2(
                (float)System.Math.Cos(Rotation) * HITBOX_LENGTH,
                (float)System.Math.Sin(Rotation) * HITBOX_LENGTH
            );

            Vector2 laserDirection = Vector2.Normalize(endPoint - Position);
            Vector2 perpendicular = new Vector2(-laserDirection.Y, laserDirection.X);

            return new Vector2[]
            {
                Position - perpendicular * HITBOX_WIDTH / 2,  // Верхний левый
                Position + perpendicular * HITBOX_WIDTH / 2,  // Верхний правый
                endPoint + perpendicular * HITBOX_WIDTH / 2,   // Нижний правый
                endPoint - perpendicular * HITBOX_WIDTH / 2    // Нижний левый
            };
        }

        // Проверка пересечения двух выпуклых полигонов с помощью SAT
        private bool PolygonsIntersect(Vector2[] polyA, Vector2[] polyB)
        {
            // Проверяем все оси полигона A
            for (int i = 0; i < polyA.Length; i++)
            {
                Vector2 edge = polyA[(i + 1) % polyA.Length] - polyA[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                if (!ProjectionsOverlap(polyA, polyB, axis))
                    return false;
            }

            // Проверяем все оси полигона B
            for (int i = 0; i < polyB.Length; i++)
            {
                Vector2 edge = polyB[(i + 1) % polyB.Length] - polyB[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                if (!ProjectionsOverlap(polyA, polyB, axis))
                    return false;
            }

            return true;
        }

        // Проверка перекрытия проекций на ось
        private bool ProjectionsOverlap(Vector2[] polyA, Vector2[] polyB, Vector2 axis)
        {
            float minA = float.MaxValue;
            float maxA = float.MinValue;
            float minB = float.MaxValue;
            float maxB = float.MinValue;

            // Проецируем полигон A
            foreach (Vector2 vertex in polyA)
            {
                float projection = Vector2.Dot(vertex, axis);
                minA = MathHelper.Min(minA, projection);
                maxA = MathHelper.Max(maxA, projection);
            }

            // Проецируем полигон B
            foreach (Vector2 vertex in polyB)
            {
                float projection = Vector2.Dot(vertex, axis);
                minB = MathHelper.Min(minB, projection);
                maxB = MathHelper.Max(maxB, projection);
            }

            return maxA >= minB && maxB >= minA;
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

            // Хитбоксы УБРАНЫ - оставляем только красивый лазер!
        }

        public override Rectangle GetBounds()
        {
            // Возвращаем пустой прямоугольник, так как коллизии работают через SAT
            return Rectangle.Empty;
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