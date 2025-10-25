using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Projectiles
{
    public class GoldenSwordProjectile : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _swordTexture;

        // Параметры траектории бумеранга
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

        // УБРАНО: дублирование полей _lifeTimer и MaxLifeTime (они уже в базовом классе)

        // Для предотвращения многократного попадания по одному врагу
        private List<Enemy> _hitEnemies = new List<Enemy>();

        public GoldenSwordProjectile(Vector2 position, int size, Color color, int damage, float speed, Enemy target, List<Enemy> potentialTargets, Player player, Texture2D texture = null) : base(position, size, color, damage, speed, Vector2.Zero, int.MaxValue)
        {
            _swordTexture = texture ?? _defaultTexture;

            // Автоматически определяем размер из текстуры
            if (_swordTexture != null && size == 0)
            {
                Size = Math.Max(_swordTexture.Width, _swordTexture.Height);
            }

            _startPosition = position;
            _player = player;
            _target = target;
            _potentialTargets = potentialTargets;
            _currentTargetPosition = target?.Position ?? position + new Vector2(300, 0);
            _curveSpeed = (speed * 1.5f) / 400f;

            // УСТАНОВКА времени жизни через базовый класс
            SetLifeTime(30f); // 30 секунд вместо 2 минут (было слишком долго)

            CalculateCurvePoints();
        }

        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public void SetTexture(Texture2D texture)
        {
            _swordTexture = texture;
        }

        private void CalculateCurvePoints()
        {
            Vector2 toTarget = _currentTargetPosition - _startPosition;
            float distance = toTarget.Length();

            // Создаем плавную овальную траекторию
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

            // Создаем плавную траекторию возврата к игроку
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

            // УБРАНО: дублирование логики таймера жизни (она в базовом классе)
            // Просто вызываем базовый метод
            base.Update(gameTime);
            if (!IsActive) return; // Проверяем после вызова base.Update

            // Обновляем позицию цели, если она жива
            if (_target != null && _target.IsAlive)
            {
                _currentTargetPosition = _target.Position;
            }

            // Проверяем, жива ли текущая цель (только если не возвращаемся)
            if (!_returning && _target != null && (!_target.IsAlive || _hitEnemies.Contains(_target)))
            {
                // Ищем новую цель
                Enemy newTarget = FindNewTarget();
                if (newTarget != null)
                {
                    // Нашли новую цель - продолжаем лететь к ней
                    _target = newTarget;
                    _currentTargetPosition = _target.Position;
                    _startPosition = Position; // Начинаем новую траекторию с текущей позиции
                    _curveProgress = 0f;
                    CalculateCurvePoints();
                }
                else
                {
                    // Не нашли новую цель - начинаем возвращаться к игроку
                    _returning = true;
                    _curveProgress = 0f;
                    CalculateReturnCurvePoints();
                }
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Двигаемся по кривой Безье
            _curveProgress += _curveSpeed * deltaTime;

            if (!_returning)
            {
                // Движение от текущей позиции к цели
                if (_curveProgress < 1.0f)
                {
                    Position = CalculateCubicBezierPoint(_startPosition, _controlPoint1, _controlPoint2,
                                                        _currentTargetPosition, _curveProgress);
                }
                else
                {
                    // Достигли цели - начинаем возвращаться к игроку
                    _returning = true;
                    _curveProgress = 0f;
                    CalculateReturnCurvePoints();
                }
            }
            else
            {
                // Движение обратно к игроку
                if (_curveProgress < 1.0f)
                {
                    Position = CalculateCubicBezierPoint(_currentTargetPosition, _controlPoint1, _controlPoint2,
                                                        _player.Position, _curveProgress);
                }
                else
                {
                    // Вернулись к игроку - деактивируем
                    IsActive = false;
                }
            }

            // Вращение снаряда
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

            if (_swordTexture != null)
            {
                DrawWithTexture(spriteBatch, _swordTexture);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }

        // Метод для проверки столкновения с врагом
        public bool CheckEnemyHit(Enemy enemy)
        {
            if (!IsActive || _hitEnemies.Contains(enemy) || !enemy.IsAlive)
                return false;

            // Увеличиваем хитбокс для лучшего обнаружения
            Rectangle projectileBounds = GetBounds();
            Rectangle enemyBounds = enemy.GetBounds();

            // Расширяем хитбокс снаряда
            projectileBounds.Inflate(8, 8);

            if (projectileBounds.Intersects(enemyBounds))
            {
                _hitEnemies.Add(enemy);
                return true;
            }

            return false;
        }

        public override Rectangle GetBounds()
        {
            // Увеличиваем хитбокс для лучшего обнаружения столкновений
            return new Rectangle(
                (int)Position.X - Size,
                (int)Position.Y - Size,
                Size * 2,
                Size * 2
            );
        }

        // Сброс списка пораженных врагов
        public void ResetHitEnemies()
        {
            _hitEnemies.Clear();
        }
    }
}