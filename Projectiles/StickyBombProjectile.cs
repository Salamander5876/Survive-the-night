using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Survive_the_night.Entities;

namespace Survive_the_night.Projectiles
{
    public class StickyBombProjectile : Projectile
    {
        public Enemy StuckEnemy { get; private set; }
        private float _explosionTimer;
        private float _explosionDuration = 0.2f;
        private bool _isExploding = false;
        public bool HasExploded { get; private set; } = false;

        private static Texture2D _bombTexture;
        private static Texture2D _explosionTexture;
        private SoundEffect _explosionSound;

        private List<Enemy> _damagedEnemies = new List<Enemy>();
        private bool _hasPlayedExplosionSound = false;
        private bool _isStuck = false;

        // �������������� �������
        private int _bombSize;
        private int _explosionSize;

        public StickyBombProjectile(Vector2 position, int size, Color color, int damage, float speed, Enemy targetEnemy, float explosionTime, SoundEffect explosionSound)
            : base(position, size, color, damage, speed, targetEnemy.Position, 1)
        {
            StuckEnemy = targetEnemy;
            _explosionTimer = explosionTime;
            _explosionSound = explosionSound;
            MaxLifeTime = 120f;

            // ������������� ���������� ������� �� �������
            _bombSize = _bombTexture?.Width ?? size;
            _explosionSize = _explosionTexture?.Width ?? 80; // �� ��������� 80 ���� �������� ���

            // ������������� ��������� ������ ��� ������ �����
            Size = _bombSize;
        }

        public static void SetTextures(Texture2D bombTexture, Texture2D explosionTexture)
        {
            _bombTexture = bombTexture;
            _explosionTexture = explosionTexture;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ���������, ��� �� ����
            if (StuckEnemy != null && !StuckEnemy.IsAlive && !_isExploding)
            {
                IsActive = false;
                return;
            }

            if (!_isExploding)
            {
                if (StuckEnemy != null && StuckEnemy.IsAlive)
                {
                    if (!_isStuck && Vector2.Distance(Position, StuckEnemy.Position) > 5f)
                    {
                        // ����� � ����� � �������������� � ��� �����������
                        Vector2 direction = StuckEnemy.Position - Position;
                        Direction = Vector2.Normalize(direction);

                        // ��������� ���� �������� � ����� (� ��������)
                        Rotation = MathHelper.ToDegrees((float)System.Math.Atan2(direction.Y, direction.X));

                        Position += Direction * Speed * deltaTime;
                    }
                    else
                    {
                        // �������� � ����� - ��������� ������� ������� (�� ����������!)
                        _isStuck = true;
                        Position = StuckEnemy.Position;
                        // Rotation ����������� �����, ����� ��� ��� ������� � �����

                        // ��������� ������ ������ ������ ����� ��������
                        _explosionTimer -= deltaTime;
                        if (_explosionTimer <= 0f)
                        {
                            StartExplosion();
                        }
                    }
                }
            }
            else
            {
                // ������� ������
                _explosionDuration -= deltaTime;
                if (_explosionDuration <= 0f)
                {
                    IsActive = false;
                }
            }

            UpdateLifeTime(gameTime);
        }

        private void StartExplosion()
        {
            _isExploding = true;
            HasExploded = true;

            if (!_hasPlayedExplosionSound && _explosionSound != null)
            {
                _explosionSound.Play();
                _hasPlayedExplosionSound = true;
            }

            // ������� ���� ���� ������ � ������� ������
            if (Game1.CurrentEnemies != null)
            {
                foreach (var enemy in Game1.CurrentEnemies)
                {
                    if (enemy.IsAlive && !_damagedEnemies.Contains(enemy))
                    {
                        float distance = Vector2.Distance(Position, enemy.Position);
                        // ���������� �������� ������ ������ ��� �������
                        if (distance < _explosionSize / 2f)
                        {
                            enemy.TakeDamage(Damage);
                            _damagedEnemies.Add(enemy);
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            if (!_isExploding)
            {
                // ��������� �����
                if (_bombTexture != null)
                {
                    DrawWithTexture(spriteBatch, _bombTexture);
                }
                else
                {
                    base.Draw(spriteBatch, debugTexture);
                }
            }
            else
            {
                // ��������� ������ - ���������� ������ ������
                if (_explosionTexture != null)
                {
                    // �������� ������ ������ ��� ��������� ������
                    int originalSize = Size;
                    Size = _explosionSize;
                    DrawWithTexture(spriteBatch, _explosionTexture);
                    Size = originalSize;
                }
                else
                {
                    Color explosionColor = Color.Red;
                    Rectangle rect = new Rectangle(
                        (int)Position.X - _explosionSize / 2,
                        (int)Position.Y - _explosionSize / 2,
                        _explosionSize,
                        _explosionSize
                    );
                    spriteBatch.Draw(debugTexture, rect, explosionColor);
                }
            }
        }

        public override void DrawWithTexture(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!IsActive || texture == null) return;

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // �������������� ��������������� ��� ������ ��������
            float scale = (float)Size / texture.Width;

            spriteBatch.Draw(
                texture,
                Position,
                null,
                Color,
                MathHelper.ToRadians(Rotation),
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        public override Rectangle GetBounds()
        {
            if (_isExploding)
            {
                // ��� ������ ���������� ������ ������
                return new Rectangle(
                    (int)Position.X - _explosionSize / 2,
                    (int)Position.Y - _explosionSize / 2,
                    _explosionSize,
                    _explosionSize
                );
            }
            else
            {
                // ��� ����� ���������� ������ �����
                return new Rectangle(
                    (int)Position.X - _bombSize / 2,
                    (int)Position.Y - _bombSize / 2,
                    _bombSize,
                    _bombSize
                );
            }
        }

        protected override void OnDeactivate()
        {
            StuckEnemy = null;
            _damagedEnemies.Clear();
            _hasPlayedExplosionSound = false;
        }
    }
}