// Projectiles/CasinoChip.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Projectiles
{
    public class CasinoChip : Projectile
    {
        private static Texture2D _defaultTexture;
        private Texture2D _chipTexture;

        public CasinoChip(Vector2 position, int size, Color color, int damage, float speed, Vector2 target, int hitsLeft = 1, Texture2D texture = null)
            : base(position, size, color, damage, speed, target, hitsLeft)
        {
            _chipTexture = texture ?? _defaultTexture;

            // ����������� ����� ����� �� 60 ������
            SetLifeTime(60f);
        }

        // ����� ��� ��������� �������� �� ���������
        public static void SetDefaultTexture(Texture2D texture)
        {
            _defaultTexture = texture;
        }

        // ����� ��� ��������� ���������� �������� ��� ���� �����
        public void SetTexture(Texture2D texture)
        {
            _chipTexture = texture;
        }

        // ����� ��� ���������� �����������
        public void UpdateDirection(Vector2 newTarget)
        {
            Direction = Vector2.Normalize(newTarget - Position);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // �������� �����
            Rotation += 180f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (_chipTexture != null)
            {
                // ���������� ����� �� �������� ������ ��� ��������� � ���������
                DrawWithTexture(spriteBatch, _chipTexture);
            }
            else
            {
                // �������� ������� - ��������� ��������������
                base.Draw(spriteBatch, debugTexture);
            }
        }
    }
}