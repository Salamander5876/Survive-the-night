using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class BreakerBladeProjectile : Projectile
    {
        private static Texture2D _defaultTexture;

        // Параметры анимации меча
        private float _currentAngle;
        private readonly float _startAngle;
        private readonly float _endAngle;
        private readonly float _rotationSpeed;
        private readonly bool _isClockwise;

        // Владелец (игрок)
        private readonly Player _player;

        // Для отзеркаливания
        private SpriteEffects _spriteEffect;

        // Отрисовка хитбокса - всегда включена
        private static readonly bool SHOW_HITBOX = false;
        private Color _hitboxColor = Color.Red * 0.5f;

        // Автоматические параметры хитбокса на основе текстуры
        private float _hitboxWidth;
        private float _hitboxLength;

        // Для обработки углов больше 360°
        private float _totalRotation = 0f;
        private readonly float _targetRotation;

        // Список врагов, которые уже получили урон от этого взмаха
        private HashSet<Enemy> _hitEnemies = new HashSet<Enemy>();

        public BreakerBladeProjectile(Vector2 position, Player player, float startAngle, float endAngle,
                                    float rotationSpeed, bool isClockwise, int damage, Color color, SpriteEffects spriteEffect)
            : base(position, 0, color, damage, 0, Vector2.Zero, 1)
        {
            _player = player;
            _startAngle = startAngle;
            _endAngle = endAngle;
            _currentAngle = startAngle;
            _rotationSpeed = rotationSpeed;
            _isClockwise = isClockwise;
            _spriteEffect = spriteEffect;

            // Вычисляем общее вращение, которое нужно пройти
            if (isClockwise)
            {
                if (endAngle < startAngle)
                {
                    _targetRotation = (360f - startAngle) + endAngle;
                }
                else
                {
                    _targetRotation = endAngle - startAngle;
                }
            }
            else
            {
                if (endAngle > startAngle)
                {
                    _targetRotation = startAngle + (360f - endAngle);
                }
                else
                {
                    _targetRotation = startAngle - endAngle;
                }
            }

            // Автоматически определяем размер хитбокса из текстуры
            if (_defaultTexture != null)
            {
                Size = Math.Max(_defaultTexture.Width, _defaultTexture.Height);
                // Хитбокс равен размеру текстуры
                _hitboxLength = _defaultTexture.Width;
                _hitboxWidth = _defaultTexture.Height * 0.3f; // Ширина 30% от высоты текстуры
            }
            else
            {
                // Значения по умолчанию если текстура не загружена
                _hitboxLength = 120f;
                _hitboxWidth = 40f;
            }

            // Активируем снаряд сразу
            IsActive = true;

            // Отладочная информация
            if (SHOW_HITBOX)
            {
                System.Diagnostics.Debug.WriteLine($"[BLADE] Created: {startAngle}° -> {endAngle}°, Clockwise: {isClockwise}, Hitbox: {_hitboxWidth}x{_hitboxLength}");
            }
        }

        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем позицию чтобы следовать за игроком
            Position = _player.Position;

            float rotationThisFrame = _rotationSpeed * deltaTime;

            // Вращаем меч в указанном направлении
            if (_isClockwise)
            {
                _currentAngle += rotationThisFrame;
                _totalRotation += rotationThisFrame;

                // Нормализуем угол от 0 до 360
                if (_currentAngle >= 360f) _currentAngle -= 360f;

                // Проверяем завершение по пройденному расстоянию
                if (_totalRotation >= _targetRotation)
                {
                    _currentAngle = _endAngle;
                    IsActive = false;
                }
            }
            else
            {
                _currentAngle -= rotationThisFrame;
                _totalRotation += rotationThisFrame;

                // Нормализуем угол от 0 до 360
                if (_currentAngle < 0f) _currentAngle += 360f;

                // Проверяем завершение по пройденному расстоянию
                if (_totalRotation >= _targetRotation)
                {
                    _currentAngle = _endAngle;
                    IsActive = false;
                }
            }

            Rotation = MathHelper.ToRadians(_currentAngle);
        }

        // Метод для проверки коллизий (возвращает список врагов, которые еще не получали урон)
        public List<Enemy> GetNewEnemyHits(List<Enemy> enemies)
        {
            var newHits = new List<Enemy>();

            if (!IsActive) return newHits;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                if (_hitEnemies.Contains(enemy)) continue; // Пропускаем врагов, которые уже получили урон

                // Используем SAT коллизии
                if (IsEnemyInHitbox(enemy))
                {
                    newHits.Add(enemy);
                    _hitEnemies.Add(enemy); // Добавляем в список уже пораженных врагов
                }
            }

            return newHits;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            // Используем текстуру по умолчанию если доступна
            if (_defaultTexture != null)
            {
                // Origin всегда слева по центру (у рукоятки)
                Vector2 origin = new Vector2(0, _defaultTexture.Height / 2);
                float scale = 1.0f;

                spriteBatch.Draw(
                    _defaultTexture,
                    Position,
                    null,
                    Color.White,
                    Rotation,
                    origin,
                    scale,
                    _spriteEffect,
                    0f
                );
            }
            else
            {
                // Отладочная отрисовка если текстура не загружена
                base.Draw(spriteBatch, debugTexture);
            }

            // Отрисовываем хитбокс если включено
            if (SHOW_HITBOX && debugTexture != null)
            {
                DrawHitbox(spriteBatch, debugTexture);
            }
        }

        // Метод для отрисовки хитбокса
        private void DrawHitbox(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            // Получаем вершины повернутого хитбокса меча
            Vector2[] hitboxVertices = GetRotatedHitboxVertices();

            // Рисуем полигон хитбокса
            for (int i = 0; i < hitboxVertices.Length; i++)
            {
                Vector2 start = hitboxVertices[i];
                Vector2 end = hitboxVertices[(i + 1) % hitboxVertices.Length];

                DrawLine(spriteBatch, debugTexture, start, end, _hitboxColor, 2);
            }

            // Дополнительно рисуем заполнение
            DrawPolygonFill(spriteBatch, debugTexture, hitboxVertices, _hitboxColor * 0.3f);
        }

        // Вспомогательные методы для отрисовки линий и полигонов
        private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(texture,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
                null,
                color,
                angle,
                new Vector2(0, 0.5f),
                SpriteEffects.None,
                0);
        }

        private void DrawPolygonFill(SpriteBatch spriteBatch, Texture2D texture, Vector2[] vertices, Color color)
        {
            // Простая заливка - рисуем треугольники
            if (vertices.Length >= 3)
            {
                for (int i = 1; i < vertices.Length - 1; i++)
                {
                    DrawTriangle(spriteBatch, texture, vertices[0], vertices[i], vertices[i + 1], color);
                }
            }
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Texture2D texture, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            // Упрощенная отрисовка треугольника - для демонстрации
            DrawLine(spriteBatch, texture, a, b, color, 1);
            DrawLine(spriteBatch, texture, b, c, color, 1);
            DrawLine(spriteBatch, texture, c, a, color, 1);
        }

        // Получаем вершины повернутого хитбокса
        private Vector2[] GetRotatedHitboxVertices()
        {
            Vector2 endPoint = Position + new Vector2(
                (float)Math.Cos(Rotation) * _hitboxLength,
                (float)Math.Sin(Rotation) * _hitboxLength
            );

            Vector2 bladeDirection = Vector2.Normalize(endPoint - Position);
            Vector2 perpendicular = new Vector2(-bladeDirection.Y, bladeDirection.X);

            return new Vector2[]
            {
                Position - perpendicular * _hitboxWidth / 2,  // Верхний левый
                Position + perpendicular * _hitboxWidth / 2,  // Верхний правый
                endPoint + perpendicular * _hitboxWidth / 2,   // Нижний правый
                endPoint - perpendicular * _hitboxWidth / 2    // Нижний левый
            };
        }

        public override Rectangle GetBounds()
        {
            // Используем SAT коллизии
            return Rectangle.Empty;
        }

        // Метод для проверки коллизий
        public bool IsEnemyInHitbox(Enemy enemy)
        {
            if (!IsActive) return false;

            // Получаем вершины повернутого хитбокса меча
            Vector2[] bladeVertices = GetRotatedHitboxVertices();

            // Получаем вершины врага
            Rectangle enemyBounds = enemy.GetBounds();
            Vector2[] enemyVertices = new Vector2[]
            {
                new Vector2(enemyBounds.Left, enemyBounds.Top),
                new Vector2(enemyBounds.Right, enemyBounds.Top),
                new Vector2(enemyBounds.Right, enemyBounds.Bottom),
                new Vector2(enemyBounds.Left, enemyBounds.Bottom)
            };

            // Проверяем пересечение с помощью Separating Axis Theorem (SAT)
            return PolygonsIntersect(bladeVertices, enemyVertices);
        }

        // SAT проверка пересечения полигонов
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

        public void ApplyTexture(Texture2D texture)
        {
            _currentTexture = texture;
        }
    }
}