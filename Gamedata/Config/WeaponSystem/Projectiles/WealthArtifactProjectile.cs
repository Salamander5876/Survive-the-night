// WealthArtifactProjectile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Scripts.Managers;
using System;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class WealthArtifactProjectile : Projectile
    {
        private static Texture2D _artifactTexture;
        private static SoundEffect _artifactSpawnSound; // Звук вылета артефакта

        public Player Player { get; private set; }
        public float OrbitRadius { get; private set; } = 300f;
        public float OrbitAngle { get; set; }
        public bool IsReturning { get; private set; } = false;

        private float _normalRotationSpeed = 90f; // Медленное вращение вправо
        private float _shootingRotationSpeed = 360f; // Быстрое вращение при стрельбе
        private float _currentRotationSpeed;
        private float _moveProgress = 0f; // От 0 до 1
        private bool _isInPosition = false;
        private Vector2 _startPos;
        private bool _hasPlayedSpawnSound = false; // Флаг чтобы звук проигрался только один раз

        public WealthArtifactProjectile(Vector2 startPosition, Player player, float orbitAngle)
            : base(startPosition, 32, Color.White, 0, 0f, Vector2.Zero, int.MaxValue)
        {
            Player = player;
            OrbitAngle = orbitAngle;
            _startPos = startPosition;
            Position = startPosition;

            _currentRotationSpeed = _normalRotationSpeed;

            // Устанавливаем текстуру, если она загружена
            if (_artifactTexture != null)
            {
                SetTexture(_artifactTexture);
            }
            
            // Сразу проигрываем звук вылета артефакта
            PlaySpawnSound();
        }

        public static void SetTexture(Texture2D texture)
        {
            _artifactTexture = texture;
        }

        public static void SetSpawnSound(SoundEffect sound)
        {
            _artifactSpawnSound = sound;
        }

        private Vector2 CalculateTargetPosition()
        {
            return Player.Position + new Vector2(
                (float)Math.Cos(OrbitAngle) * OrbitRadius,
                (float)Math.Sin(OrbitAngle) * OrbitRadius
            );
        }

        public void StartShooting()
        {
            _currentRotationSpeed = _shootingRotationSpeed;
        }

        public void StopShooting()
        {
            _currentRotationSpeed = _normalRotationSpeed;
        }

        public void StartReturn()
        {
            IsReturning = true;
            _isInPosition = false;
            _moveProgress = 0f;
            _startPos = Position;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive || Player == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isInPosition)
            {
                if (IsReturning)
                {
                    // Плавно возвращаемся к игроку
                    _moveProgress += deltaTime * 2f; // Быстрее возвращаемся
                    _moveProgress = MathHelper.Clamp(_moveProgress, 0f, 1f);

                    Position = Vector2.Lerp(_startPos, Player.Position, _moveProgress);

                    // Если достигли игрока
                    if (_moveProgress >= 1f)
                    {
                        IsActive = false;
                    }
                }
                else
                {
                    // Плавно вылетаем к целевой позиции
                    _moveProgress += deltaTime * 2f; // Скорость вылета
                    _moveProgress = MathHelper.Clamp(_moveProgress, 0f, 1f);

                    // Рассчитываем целевую позицию (движется с игроком)
                    Vector2 targetPos = CalculateTargetPosition();

                    // Обновляем стартовую позицию, чтобы она тоже двигалась с игроком
                    _startPos = Player.Position;

                    // Интерполируем между стартовой и целевой позицией
                    Position = Vector2.Lerp(_startPos, targetPos, _moveProgress);

                    if (_moveProgress >= 1f)
                    {
                        _isInPosition = true;
                    }
                }
            }
            else
            {
                // Двигаемся вместе с игроком - всегда на фиксированном расстоянии под нужным углом
                Position = CalculateTargetPosition();
            }

            // Вращение вправо (положительное)
            Rotation += _currentRotationSpeed * deltaTime;
        }

        private void PlaySpawnSound()
        {
            if (!_hasPlayedSpawnSound && _artifactSpawnSound != null)
            {
                // Используем SoundManager для воспроизведения звука
                SoundManager.Instance.PlaySound("golden_artifact", 0.5f);
                _hasPlayedSpawnSound = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            if (_artifactTexture != null)
            {
                // Используем исправленный метод отрисовки с правильным размером
                Vector2 origin = new Vector2(_artifactTexture.Width / 2, _artifactTexture.Height / 2);
                
                // Автоматический масштаб на основе размера текстуры
                // Хотим чтобы артефакт был примерно 64x64 пикселей
                float targetSize = 64f;
                float textureSize = Math.Max(_artifactTexture.Width, _artifactTexture.Height);
                float scale = targetSize / textureSize;
                
                spriteBatch.Draw(
                    _artifactTexture,
                    Position,
                    null,
                    Color.White,
                    MathHelper.ToRadians(Rotation),
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                // Если текстура не загружена, рисуем белый квадрат
                base.Draw(spriteBatch, debugTexture);
            }
        }

        public override Rectangle GetBounds()
        {
            // У артефакта НЕТ хитбокса
            return Rectangle.Empty;
        }
    }
}