using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class EventHorizonStarProjectile : Projectile
    {
        private static Texture2D _defaultTexture;

        // Параметры орбитального движения
        private float _baseOrbitSpeed = 300f; // Начальная скорость кружения
        private float _currentOrbitAngle;
        private float _ringRadius;
        private Vector2 _ringCenter;

        // Переменные для смены направления
        private float _directionChangeTimer = 0f;
        private const float DIRECTION_CHANGE_INTERVAL = 0.3f;
        private int _currentDirection = 1; // 1 = влево, -1 = вправо

        // Скорость вращения спрайта
        private const float SPRITE_ROTATION_SPEED = 720f;

        // Время жизни снаряда (в секундах)
        private float _projectileLifetime = 5f;

        // === МЕХАНИКА САМОНАВЕДЕНИЯ ===
        public bool IsHoming { get; private set; } = false; // Режим самонаведения
        private Enemy _targetEnemy; // Целевой враг
        private const float DETECTION_RADIUS = 100f; // Радиус обнаружения врагов
        private const float HOMING_SPEED = 400f; // Скорость самонаведения

        // === НАСТРОЙКИ ОТЛАДКИ ===
        public static bool ShowDamageHitbox { get; set; } = false; // Показывать хитбокс урона
        public static bool ShowDetectionRadius { get; set; } = false; // Показывать радиус обнаружения
        public static Color DamageHitboxColor { get; set; } = Color.Red * 0.5f; // Цвет хитбокса урона
        public static Color DetectionRadiusColor { get; set; } = Color.Blue * 0.3f; // Цвет радиуса обнаружения

        public EventHorizonStarProjectile(Vector2 position, int size, Color color, int damage, float expansionSpeed,
                                        Vector2 target, float lifetime, float initialAngle)
            : base(position, size, color, damage, expansionSpeed, target, 1)
        {
            _ringCenter = position;
            _ringRadius = 0f;
            _currentOrbitAngle = initialAngle;

            // Сохраняем время жизни и устанавливаем через базовый метод
            _projectileLifetime = lifetime;
            SetLifeTime(lifetime);

            if (_defaultTexture != null && size == 0)
            {
                Size = Math.Max(_defaultTexture.Width, _defaultTexture.Height);
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

            // Используем базовую логику времени жизни
            base.Update(gameTime);
            if (!IsActive) return;

            // === ПРОВЕРКА САМОНАВЕДЕНИЯ ===
            if (!IsHoming)
            {
                CheckForHomingTargets();
            }

            if (IsHoming)
            {
                UpdateHomingMovement(deltaTime);
            }
            else
            {
                UpdateOrbitalMovement(deltaTime);
            }

            // Вращаем спрайт в соответствии с направлением движения
            Rotation += _currentDirection * SPRITE_ROTATION_SPEED * deltaTime;
        }

        // === МЕТОД: Проверка врагов для самонаведения ===
        private void CheckForHomingTargets()
        {
            if (IsHoming) return; // Уже в режиме самонаведения

            List<Enemy> enemies = Game1.CurrentEnemies;
            if (enemies == null) return;

            // Ищем ближайшего врага в радиусе обнаружения
            Enemy closestEnemy = null;
            float closestDistanceSquared = DETECTION_RADIUS * DETECTION_RADIUS;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(Position, enemy.Position);
                if (distanceSquared <= closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }

            // Если нашли врага - переходим в режим самонаведения
            if (closestEnemy != null)
            {
                IsHoming = true;
                _targetEnemy = closestEnemy;
                Speed = HOMING_SPEED; // Меняем скорость на скорость самонаведения
            }
        }

        // === МЕТОД: Движение с самонаведением ===
        private void UpdateHomingMovement(float deltaTime)
        {
            // Если цель мертва или неактивна - уничтожаем звезду
            if (_targetEnemy == null || !_targetEnemy.IsAlive)
            {
                IsActive = false;
                return;
            }

            // Двигаемся к цели
            Vector2 direction = Vector2.Normalize(_targetEnemy.Position - Position);
            Position += direction * Speed * deltaTime;

            // Обновляем направление для плавного поворота
            Direction = direction;
        }

        // === МЕТОД: Орбитальное движение ===
        private void UpdateOrbitalMovement(float deltaTime)
        {
            // Обновляем таймер смены направления
            _directionChangeTimer += deltaTime;
            if (_directionChangeTimer >= DIRECTION_CHANGE_INTERVAL)
            {
                _currentDirection *= -1;
                _directionChangeTimer = 0f;
            }

            // Расширяем кольцо
            _ringRadius += Speed * deltaTime;

            // === ФОРМУЛА РАСЧЕТА СКОРОСТИ ВРАЩЕНИЯ ===
            float timeProgress = _lifeTimer / _projectileLifetime; // от 0 до 1
            float currentOrbitSpeed = _baseOrbitSpeed * (1f - (float)Math.Sqrt(timeProgress));

            // Обновляем угол орбиты с учетом замедления
            _currentOrbitAngle += _currentDirection * currentOrbitSpeed * deltaTime;

            // Нормализуем угол
            if (_currentOrbitAngle >= 360f) _currentOrbitAngle -= 360f;
            if (_currentOrbitAngle < 0f) _currentOrbitAngle += 360f;

            // Вычисляем новую позицию на кольце
            float orbitRadians = MathHelper.ToRadians(_currentOrbitAngle);
            Vector2 orbitOffset = new Vector2(
                (float)Math.Cos(orbitRadians) * _ringRadius,
                (float)Math.Sin(orbitRadians) * _ringRadius
            );

            Position = _ringCenter + orbitOffset;
        }

        // === МЕТОД: Получить радиус обнаружения ===
        public Circle GetDetectionCircle()
        {
            return new Circle(Position, DETECTION_RADIUS);
        }

        // === МЕТОД: Получить хитбокс урона ===
        public Circle GetDamageHitbox()
        {
            // Хитбокс урона немного больше спрайта для лучшего обнаружения
            float hitboxRadius = Size * _textureScale / 2f + 3f;
            return new Circle(Position, hitboxRadius);
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            // === ОТРИСОВКА РАДИУСА ОБНАРУЖЕНИЯ ===
            if (ShowDetectionRadius && !IsHoming)
            {
                DrawDetectionRadius(spriteBatch, debugTexture);
            }

            // === ОТРИСОВКА ХИТБОКСА УРОНА ===
            if (ShowDamageHitbox)
            {
                DrawDamageHitbox(spriteBatch, debugTexture);
            }

            // Отрисовываем текстуру звезды
            if (_defaultTexture != null)
            {
                DrawWithTexture(spriteBatch, _defaultTexture);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }

        // === МЕТОД: Отрисовка радиуса обнаружения ===
        private void DrawDetectionRadius(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            Circle detectionCircle = GetDetectionCircle();
            DrawCircle(spriteBatch, debugTexture, detectionCircle, DetectionRadiusColor, 2);
        }

        // === МЕТОД: Отрисовка хитбокса урона ===
        private void DrawDamageHitbox(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            Circle damageHitbox = GetDamageHitbox();
            DrawCircle(spriteBatch, debugTexture, damageHitbox, DamageHitboxColor, 2);
        }

        // === МЕТОД: Отрисовка круга ===
        private void DrawCircle(SpriteBatch spriteBatch, Texture2D texture, Circle circle, Color color, int thickness)
        {
            const int segments = 32;
            float angleStep = MathHelper.TwoPi / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;

                Vector2 point1 = circle.Center + new Vector2(
                    (float)Math.Cos(angle1) * circle.Radius,
                    (float)Math.Sin(angle1) * circle.Radius
                );

                Vector2 point2 = circle.Center + new Vector2(
                    (float)Math.Cos(angle2) * circle.Radius,
                    (float)Math.Sin(angle2) * circle.Radius
                );

                DrawLine(spriteBatch, texture, point1, point2, color, thickness);
            }
        }

        // === Вспомогательный метод для отрисовки линий ===
        private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(texture,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
        }

        public override Rectangle GetBounds()
        {
            // Используем круговой хитбокс для более точного обнаружения
            Circle damageHitbox = GetDamageHitbox();
            return new Rectangle(
                (int)(damageHitbox.Center.X - damageHitbox.Radius),
                (int)(damageHitbox.Center.Y - damageHitbox.Radius),
                (int)(damageHitbox.Radius * 2),
                (int)(damageHitbox.Radius * 2)
            );
        }

        public void ApplyTexture(Texture2D texture)
        {
            _currentTexture = texture;
            if (texture != null)
            {
                Size = Math.Max(texture.Width, texture.Height);
                _textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
                float baseSize = 32f;
                _textureScale = baseSize / Math.Max(texture.Width, texture.Height);
            }
        }
    }

    // === СТРУКТУРА: Круг для обнаружения ===
    public struct Circle
    {
        public Vector2 Center;
        public float Radius;

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Contains(Vector2 point)
        {
            return Vector2.DistanceSquared(Center, point) <= Radius * Radius;
        }

        public bool Intersects(Circle other)
        {
            return Vector2.DistanceSquared(Center, other.Center) <= (Radius + other.Radius) * (Radius + other.Radius);
        }

        public bool Intersects(Rectangle rectangle)
        {
            // Находим ближайшую точку прямоугольника к кругу
            float closestX = MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right);
            float closestY = MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom);

            // Проверяем расстояние
            float distanceX = Center.X - closestX;
            float distanceY = Center.Y - closestY;
            float distanceSquared = distanceX * distanceX + distanceY * distanceY;

            return distanceSquared <= Radius * Radius;
        }
    }
}