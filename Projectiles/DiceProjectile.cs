using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;

namespace Survive_the_night.Projectiles
{
    public class DiceProjectile : Projectile
    {
        public int DiceValue { get; private set; }
        public Player Player { get; private set; }
        public float OrbitRadius { get; private set; } = 100f;
        public float OrbitAngle { get; private set; }
        public bool OrbitClockwise { get; private set; }

        private static SoundEffect _hitSound;
        private static Texture2D[] _diceTextures = new Texture2D[6];
        private float _orbitSpeed = 180f;

        private bool _isOrbiting = false;

        public DiceProjectile(int diceValue, Player player, bool orbitClockwise,
                            int damage, int hitsLeft = 2)
            : base(Vector2.Zero, 32, Color.White, damage, 0f, Vector2.Zero, hitsLeft)
        {
            DiceValue = diceValue;
            Player = player;
            OrbitClockwise = orbitClockwise;

            SetLifeTime(90f);

            // Устанавливаем начальный угол
            float initialAngle = (float)(diceValue * MathHelper.TwoPi / 6f);
            OrbitAngle = initialAngle;

            // Сразу устанавливаем позицию на орбите
            UpdatePosition();

            // НАЧАЛЬНОЕ ВРАЩЕНИЕ = 0 (кости не вращаются при появлении)
            Rotation = 0f;
        }

        public static void SetHitSound(SoundEffect sound)
        {
            _hitSound = sound;
        }

        public static void SetTextures(Texture2D dice1, Texture2D dice2, Texture2D dice3,
                                     Texture2D dice4, Texture2D dice5, Texture2D dice6)
        {
            _diceTextures[0] = dice1;
            _diceTextures[1] = dice2;
            _diceTextures[2] = dice3;
            _diceTextures[3] = dice4;
            _diceTextures[4] = dice5;
            _diceTextures[5] = dice6;
        }

        // Метод для обновления позиции на орбите
        private void UpdatePosition()
        {
            if (Player == null) return;

            Position = Player.Position + new Vector2(
                (float)System.Math.Cos(OrbitAngle) * OrbitRadius,
                (float)System.Math.Sin(OrbitAngle) * OrbitRadius
            );
        }

        // Агрессивное следование за игроком (во время спавна)
        public void UpdateAggressiveFollow()
        {
            if (!IsActive || Player == null) return;

            UpdateLifeTime(new GameTime());

            // Жестко привязываем к текущей позиции игрока
            UpdatePosition();

            // ВАЖНО: НЕТ ВРАЩЕНИЯ во время агрессивного следования
            // Rotation не изменяется - кости остаются в начальной ориентации
        }

        // Нормальное вращение на орбите (после спавна всех костей)
        public void UpdateNormalOrbit(float deltaTime)
        {
            if (!IsActive || Player == null) return;

            UpdateLifeTime(new GameTime());

            // Обновляем угол орбиты
            float direction = OrbitClockwise ? 1f : -1f;
            OrbitAngle += MathHelper.ToRadians(_orbitSpeed * direction * deltaTime);

            // Обновляем позицию
            UpdatePosition();

            // ВАЖНО: ВРАЩЕНИЕ начинается только когда кости крутятся вокруг игрока
            Rotation += 360f * deltaTime;
        }

        // Переключение в режим вращения
        public void StartOrbiting()
        {
            _isOrbiting = true;
        }

        public override void Update(GameTime gameTime)
        {
            // Этот метод теперь не используется - используем отдельные методы
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            if (_diceTextures != null && DiceValue >= 1 && DiceValue <= 6 && _diceTextures[DiceValue - 1] != null)
            {
                DrawWithTexture(spriteBatch, _diceTextures[DiceValue - 1]);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }

        public void OnHitEnemy()
        {
            _hitSound?.Play();
        }
    }
}