using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class GoldenSwordProjectile : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _swordTexture;

        private Vector2 _startPosition;
        private Player _player;
        private Vector2 _controlPoint1;
        private Vector2 _controlPoint2;
        private float _curveProgress = 0f;
        private float _curveSpeed;
        private Enemy _target;
        private List<Enemy> _potentialTargets;
        private bool _returning = false;
        private Vector2 _currentTargetPosition;

        // Новые свойства для улучшенной логики
        private int _maxTargets;
        private int _targetsHit = 0;
        private List<Enemy> _hitEnemies = new List<Enemy>();

        // Новые флаги для управления состоянием меча
        public bool IsReturningToPlayer => _returning;
        public bool HasReturnedToPlayer => _returning && _curveProgress >= 1.0f;
        public bool HasAssignedTarget => _target != null;
        public Enemy Target => _target; // Публичное свойство для доступа к цели

        // Флаг для отладки хитбокса
        private bool _showHitbox = false;

        // РАЗМЕРЫ ХИТБОКСА - можно менять здесь
        private const int HITBOX_WIDTH = 94;   // Ширина хитбокса в пикселях
        private const int HITBOX_HEIGHT = 14;  // Высота хитбокса в пикселях

        // Для вращающегося хитбокса
        private Vector2 _hitboxOrigin;

        public GoldenSwordProjectile(Vector2 position, int size, Color color, int damage, float speed, Enemy target, List<Enemy> potentialTargets, Player player, Texture2D texture = null, int maxTargets = 10)
            : base(position, size, color, damage, speed, Vector2.Zero, int.MaxValue)
        {
            _swordTexture = texture ?? _defaultTexture;
            _maxTargets = maxTargets;

            // Размер спрайта автоматически определяется из текстуры
            if (_swordTexture != null && size == 0)
            {
                Size = Math.Max(_swordTexture.Width, _swordTexture.Height);
            }

            _startPosition = position;
            _player = player;
            _target = target;
            _potentialTargets = potentialTargets;

            // Если цель не задана, сразу переходим в режим возврата
            if (_target == null)
            {
                _returning = true;
                _currentTargetPosition = _player.Position;
            }
            else
            {
                _currentTargetPosition = target.Position;
            }

            _curveSpeed = speed * 1.5f / 400f;

            SetLifeTime(30f);

            // Инициализируем центр хитбокса
            _hitboxOrigin = new Vector2(HITBOX_WIDTH / 2f, HITBOX_HEIGHT / 2f);

            if (!_returning)
            {
                CalculateCurvePoints();
            }
            else
            {
                CalculateReturnCurvePoints();
            }
        }

        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public void SetTexture(Texture2D texture)
        {
            _swordTexture = texture;
        }

        // Новый метод: принудительно начать возврат к игроку
        public void StartReturnToPlayer()
        {
            if (!_returning)
            {
                _returning = true;
                _curveProgress = 0f;
                _startPosition = Position;
                CalculateReturnCurvePoints();
            }
        }

        private void CalculateCurvePoints()
        {
            Vector2 toTarget = _currentTargetPosition - _startPosition;
            float distance = toTarget.Length();

            Vector2 perpendicular = new Vector2(-toTarget.Y, toTarget.X);
            perpendicular = Vector2.Normalize(perpendicular) * (distance * 0.6f);

            _controlPoint1 = _startPosition + toTarget * 0.3f + perpendicular;
            _controlPoint2 = _startPosition + toTarget * 0.7f - perpendicular;
        }

        private void CalculateReturnCurvePoints()
        {
            Vector2 currentPosition = Position;
            Vector2 toPlayer = _player.Position - currentPosition;
            float distance = toPlayer.Length();

            Vector2 perpendicular = new Vector2(-toPlayer.Y, toPlayer.X);
            perpendicular = Vector2.Normalize(perpendicular) * (distance * 0.4f);

            _controlPoint1 = currentPosition + toPlayer * 0.3f + perpendicular;
            _controlPoint2 = currentPosition + toPlayer * 0.7f - perpendicular;
        }

        private Enemy FindNewTarget()
        {
            if (_potentialTargets == null) return null;

            Enemy closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in _potentialTargets)
            {
                if (!enemy.IsAlive || _hitEnemies.Contains(enemy)) continue;

                float distance = Vector2.DistanceSquared(Position, enemy.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            base.Update(gameTime);
            if (!IsActive) return;

            // ПРОВЕРКА ЛИМИТА ЦЕЛЕЙ
            if (_targetsHit >= _maxTargets)
            {
                StartReturnToPlayer();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Если у меча есть цель и она жива, обновляем позицию цели
            if (!_returning && _target != null && _target.IsAlive)
            {
                _currentTargetPosition = _target.Position;
            }

            // Логика поиска новой цели (только для мечей с назначенной целью)
            if (!_returning && _target != null && (!_target.IsAlive || _hitEnemies.Contains(_target)))
            {
                Enemy newTarget = FindNewTarget();
                if (newTarget != null)
                {
                    _target = newTarget;
                    _currentTargetPosition = _target.Position;
                    _startPosition = Position;
                    _curveProgress = 0f;
                    CalculateCurvePoints();
                }
                else
                {
                    StartReturnToPlayer();
                }
            }

            _curveProgress += _curveSpeed * deltaTime;

            if (!_returning)
            {
                if (_curveProgress < 1.0f)
                {
                    Position = CalculateCubicBezierPoint(_startPosition, _controlPoint1, _controlPoint2,
                                                        _currentTargetPosition, _curveProgress);
                }
                else
                {
                    StartReturnToPlayer();
                }
            }
            else
            {
                if (_curveProgress < 1.0f)
                {
                    Position = CalculateCubicBezierPoint(_startPosition, _controlPoint1, _controlPoint2,
                                                        _player.Position, _curveProgress);
                }
                else
                {
                    // Меч вернулся к игроку
                    Position = _player.Position;
                }
            }

            Rotation += 450f * deltaTime;
        }

        private Vector2 CalculateCubicBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float ttt = tt * t;
            float uu = u * u;
            float uuu = uu * u;

            return uuu * p0 +
                   3 * uu * t * p1 +
                   3 * u * tt * p2 +
                   ttt * p3;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            // Отрисовываем хитбокс для отладки
            if (_showHitbox && debugTexture != null)
            {
                DrawRotatedHitbox(spriteBatch, debugTexture);
            }

            if (_swordTexture != null)
            {
                DrawWithTexture(spriteBatch, _swordTexture);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }

        // Метод для отрисовки вращающегося хитбокса
        private void DrawRotatedHitbox(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            // Создаем прямоугольник хитбокса
            Rectangle hitboxRect = new Rectangle(0, 0, HITBOX_WIDTH, HITBOX_HEIGHT);

            // Отрисовываем с поворотом
            spriteBatch.Draw(
                debugTexture,
                Position,
                hitboxRect,
                Color.Red * 0.5f, // Полупрозрачный красный
                MathHelper.ToRadians(Rotation), // Поворачиваем хитбокс так же как меч
                _hitboxOrigin, // Центр хитбокса
                1.0f,
                SpriteEffects.None,
                0f
            );
        }

        public void CheckEnemyHit()
        {
            if (!IsActive || _targetsHit >= _maxTargets)
                return;

            // Если у меча есть назначенная цель, проверяем столкновение только с ней
            if (_target != null && _target.IsAlive && !_hitEnemies.Contains(_target))
            {
                if (CheckRotatedHitboxCollision(_target))
                {
                    _target.TakeDamage(Damage);
                    _hitEnemies.Add(_target);
                    _targetsHit++;
                }
            }
        }

        // Метод для проверки столкновений с вращающимся хитбоксом
        private bool CheckRotatedHitboxCollision(Enemy enemy)
        {
            // Получаем границы врага
            Rectangle enemyBounds = enemy.GetBounds();

            // Создаем матрицу трансформации для хитбокса
            Matrix transform = Matrix.CreateTranslation(-_hitboxOrigin.X, -_hitboxOrigin.Y, 0f) *
                              Matrix.CreateRotationZ(MathHelper.ToRadians(Rotation)) *
                              Matrix.CreateTranslation(Position.X, Position.Y, 0f);

            // Углы хитбокса
            Vector2[] hitboxCorners = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(HITBOX_WIDTH, 0),
                new Vector2(HITBOX_WIDTH, HITBOX_HEIGHT),
                new Vector2(0, HITBOX_HEIGHT)
            };

            // Преобразуем углы хитбокса с учетом поворота
            for (int i = 0; i < 4; i++)
            {
                hitboxCorners[i] = Vector2.Transform(hitboxCorners[i], transform);
            }

            // Проверяем пересечение вращающегося хитбокса с прямоугольником врага
            return CheckPolygonRectangleIntersection(hitboxCorners, enemyBounds);
        }

        // Метод для проверки пересечения полигона (хитбокса) с прямоугольником (врагом)
        private bool CheckPolygonRectangleIntersection(Vector2[] polygon, Rectangle rectangle)
        {
            // Проверяем, находится ли любая точка полигона внутри прямоугольника
            foreach (Vector2 point in polygon)
            {
                if (rectangle.Contains((int)point.X, (int)point.Y))
                    return true;
            }

            // Проверяем, находится ли любая точка прямоугольника внутри полигона
            Vector2[] rectCorners = new Vector2[4]
            {
                new Vector2(rectangle.Left, rectangle.Top),
                new Vector2(rectangle.Right, rectangle.Top),
                new Vector2(rectangle.Right, rectangle.Bottom),
                new Vector2(rectangle.Left, rectangle.Bottom)
            };

            foreach (Vector2 point in rectCorners)
            {
                if (IsPointInPolygon(point, polygon))
                    return true;
            }

            return false;
        }

        // Метод для проверки, находится ли точка внутри полигона
        private bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y ||
                    polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) *
                        (polygon[j].X - polygon[i].X) < point.X)
                    {
                        inside = !inside;
                    }
                }
                j = i;
            }

            return inside;
        }

        // ОРИГИНАЛЬНЫЙ GetBounds() - возвращает границы спрайта (не меняем)
        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X - Size,
                (int)Position.Y - Size,
                Size * 2,
                Size * 2
            );
        }

        public void ResetHitEnemies()
        {
            _hitEnemies.Clear();
        }

        // Метод для включения/выключения отображения хитбокса
        public void ToggleHitboxVisibility(bool show)
        {
            _showHitbox = show;
        }
    }
}