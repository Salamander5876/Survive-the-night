using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Survive_the_night.Projectiles
{
    public class RouletteParticle : Projectile
    {
        private static List<Texture2D> _particleTextures = new List<Texture2D>();
        private static int _textureIndex = 0;

        private Texture2D _currentParticleTexture;
        private float _particleLifeTimer = 0f;

        public RouletteParticle(Vector2 position, int size, Color color, int damage, float lifetime)
            : base(position, size, color, damage, 0, position, 1)
        {
            SetLifeTime(lifetime);

            // �������� ��������� �������� �� ������������������
            if (_particleTextures.Count > 0)
            {
                _currentParticleTexture = _particleTextures[_textureIndex];
                _textureIndex = (_textureIndex + 1) % _particleTextures.Count;

                // ������������� ���������� ������ �� ��������
                if (size == 0 && _currentParticleTexture != null)
                {
                    Size = _currentParticleTexture.Width; // ���������� ������ ��������
                }
            }

            // ���� �������� �� ���������, ������������� ����������� ������
            if (Size == 0)
            {
                Size = 16; // ����������� ������
            }
        }

        public static void AddParticleTexture(Texture2D texture)
        {
            if (texture != null && !_particleTextures.Contains(texture))
            {
                _particleTextures.Add(texture);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ��������� ������ �����
            _particleLifeTimer += deltaTime;
            if (_particleLifeTimer >= MaxLifeTime)
            {
                IsActive = false;
                return;
            }

            // Fade-out ������
            float alpha = 1f - (_particleLifeTimer / MaxLifeTime);
            Color = new Color(Color.R, Color.G, Color.B, alpha);
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            if (_currentParticleTexture != null)
            {
                DrawWithTexture(spriteBatch, _currentParticleTexture);
            }
            else
            {
                // ���������� ���������
                Rectangle rect = new Rectangle(
                    (int)Position.X - Size / 2,
                    (int)Position.Y - Size / 2,
                    Size,
                    Size
                );
                spriteBatch.Draw(debugTexture, rect, Color);
            }
        }

        public override Rectangle GetBounds()
        {
            // �������� ������� ��� ������� ����������� ������������
            int hitboxSize = Size + 4; // +4 ������� ��� ������� �����������
            return new Rectangle(
                (int)Position.X - hitboxSize / 2,
                (int)Position.Y - hitboxSize / 2,
                hitboxSize,
                hitboxSize
            );
        }
    }
}