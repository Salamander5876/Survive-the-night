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
        public float RotationSpeed { get; set; } = 1.5f; // �������� �������� 1.5

        // ��� ������������ ��� ���������� ������ � ������� ����� �����
        private HashSet<Enemy> _hitEnemiesThisCycle = new HashSet<Enemy>();

        // ������� ���� ��� ������������
        private Enemy _currentTarget;

        // ���� ������
        private SoundEffect _laserSound;
        private SoundEffectInstance _laserSoundInstance;

        // ������������� ������� �������� (������� ���� 33 �������)
        private const float HITBOX_WIDTH = 33f;
        private const float HITBOX_LENGTH = 800f;

        public BigLaserProjectile(Vector2 position, Player player, List<Enemy> enemies, int damage, Texture2D texture = null, SoundEffect laserSound = null)
            : base(position, 20, Color.White, damage, 0f, Vector2.Zero, int.MaxValue)
        {
            _player = player;
            _enemies = enemies;
            _laserTexture = texture ?? _defaultTexture;
            _laserSound = laserSound;

            // ������������� ���������� ������� �� �������� ��� ���������
            if (_laserTexture != null)
            {
                Width = _laserTexture.Height;   // 90 ��������
                Length = _laserTexture.Width;   // 800 ��������
            }
            else
            {
                Width = 90f;
                Length = 800f;
            }

            Rotation = 0f;
            _targetRotation = 0f;
            _currentTarget = FindBestTarget();

            // ������� ��������� ����� ��� ������������ ���������������
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

            // ������� �� �������
            Position = _player.Position;

            // ��������� ���� ������ 0.5 ������� ��� �����������
            if (gameTime.TotalGameTime.TotalSeconds % 0.5 < deltaTime)
            {
                _currentTarget = FindBestTarget();
            }

            // ��������� �� ����
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                Vector2 direction = _currentTarget.Position - Position;
                _targetRotation = (float)System.Math.Atan2(direction.Y, direction.X);
            }
            else
            {
                // ���� ���� ���, ���� �����
                _currentTarget = FindBestTarget();
            }

            // ������� �������
            float rotationDifference = _targetRotation - Rotation;

            // ����������� �������
            while (rotationDifference > MathHelper.Pi)
                rotationDifference -= MathHelper.TwoPi;
            while (rotationDifference < -MathHelper.Pi)
                rotationDifference += MathHelper.TwoPi;

            Rotation += rotationDifference * RotationSpeed * deltaTime;

            // ��������� ������ �����
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

                // ���������, ��� ���� � �������� ������������
                if (distance > HITBOX_LENGTH) continue;

                // ��������� "��������" ����:
                // 1. �������� � ������ (��� �����, ��� �����)
                float proximityScore = 1.0f - (distance / HITBOX_LENGTH);

                // 2. ��������� �����, ������� ������ ����� (������������� �� ��� �����)
                float typeScore = GetEnemyTypeScore(enemy);

                // 3. ����� �� ��, ��� ���� ��� � ���� �����
                float inDamageZoneBonus = IsEnemyInSpriteCollision(enemy) ? 0.4f : 0f;

                // �������� ����
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
            // EliteEnemy - ����� ������ ����
            if (enemy is EliteEnemy)
                return 1.0f;

            // ������� ����� - ������� ��������
            return 0.5f;
        }

        private void ApplyDamageToEnemiesInBeam()
        {
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsAlive) continue;

                if (_hitEnemiesThisCycle.Contains(enemy)) continue;

                // ���� ��������� ������ ���� ���� ������������ � ��������� ������
                if (IsEnemyInSpriteCollision(enemy))
                {
                    enemy.TakeDamage(Damage);
                    _hitEnemiesThisCycle.Add(enemy);
                }
            }
        }

        private bool IsEnemyInSpriteCollision(Enemy enemy)
        {
            // �������� ������� ����������� �������� ������
            Vector2[] laserVertices = GetRotatedHitboxVertices();

            // �������� ������� ����� (������������, ��� ��� �������������)
            Rectangle enemyBounds = enemy.GetBounds();
            Vector2[] enemyVertices = new Vector2[]
            {
                new Vector2(enemyBounds.Left, enemyBounds.Top),
                new Vector2(enemyBounds.Right, enemyBounds.Top),
                new Vector2(enemyBounds.Right, enemyBounds.Bottom),
                new Vector2(enemyBounds.Left, enemyBounds.Bottom)
            };

            // ��������� ����������� � ������� Separating Axis Theorem (SAT)
            return PolygonsIntersect(laserVertices, enemyVertices);
        }

        // �������� ������� ����������� ��������
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
                Position - perpendicular * HITBOX_WIDTH / 2,  // ������� �����
                Position + perpendicular * HITBOX_WIDTH / 2,  // ������� ������
                endPoint + perpendicular * HITBOX_WIDTH / 2,   // ������ ������
                endPoint - perpendicular * HITBOX_WIDTH / 2    // ������ �����
            };
        }

        // �������� ����������� ���� �������� ��������� � ������� SAT
        private bool PolygonsIntersect(Vector2[] polyA, Vector2[] polyB)
        {
            // ��������� ��� ��� �������� A
            for (int i = 0; i < polyA.Length; i++)
            {
                Vector2 edge = polyA[(i + 1) % polyA.Length] - polyA[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis.Normalize();

                if (!ProjectionsOverlap(polyA, polyB, axis))
                    return false;
            }

            // ��������� ��� ��� �������� B
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

        // �������� ���������� �������� �� ���
        private bool ProjectionsOverlap(Vector2[] polyA, Vector2[] polyB, Vector2 axis)
        {
            float minA = float.MaxValue;
            float maxA = float.MinValue;
            float minB = float.MaxValue;
            float maxB = float.MinValue;

            // ���������� ������� A
            foreach (Vector2 vertex in polyA)
            {
                float projection = Vector2.Dot(vertex, axis);
                minA = MathHelper.Min(minA, projection);
                maxA = MathHelper.Max(maxA, projection);
            }

            // ���������� ������� B
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

            // ��������� ������ � ���������� �����������
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

            // �������� ������ - ��������� ������ �������� �����!
        }

        public override Rectangle GetBounds()
        {
            // ���������� ������ �������������, ��� ��� �������� �������� ����� SAT
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

        // ������������� ���� ��� ����������� ������
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