// Projectiles/GoldenBulletProjectile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Survive_the_night.Projectiles
{
    public class GoldenBulletProjectile : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _bulletTexture;

        public GoldenBulletProjectile(Vector2 position, int size, Color color, int damage, float speed, Vector2 target, Texture2D texture = null)
            : base(position, size, color, damage, speed, target, 1) // hitsLeft = 1 (��� ��������)
        {
            _bulletTexture = texture ?? _defaultTexture;

            // ��������� ��������� �������, ����� ���� ������ ������� �������� ������
            Rotation = CalculateRotationToTarget(target);
        }

        // ����� ��� ��������� �������� �� ���������
        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ��������� �������, ����� ���� ������ ���� ���������� � ������� ��������
            Rotation = CalculateRotationToDirection(Direction);
        }

        // ������ �������� � ����
        private float CalculateRotationToTarget(Vector2 target)
        {
            Vector2 direction = Vector2.Normalize(target - Position);
            return CalculateRotationToDirection(direction);
        }

        // ������ �������� �� �����������
        private float CalculateRotationToDirection(Vector2 direction)
        {
            // ��������2 ���������� ���� � ��������, ������������ � �������
            // � ��������� 90 ��������, ����� ���� �������� ������� ������
            float angle = MathHelper.ToDegrees((float)Math.Atan2(direction.Y, direction.X)) + 90f;
            return angle;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (_bulletTexture != null)
            {
                // ���������� ����� �� �������� ������ ��� ��������� � ���������
                DrawWithTexture(spriteBatch, _bulletTexture);
            }
            else
            {
                // �������� ������� - ��������� ��������������
                base.Draw(spriteBatch, debugTexture);
            }
        }
    }
}