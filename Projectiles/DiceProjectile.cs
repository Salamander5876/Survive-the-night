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

            // ������������� ��������� ����
            float initialAngle = (float)(diceValue * MathHelper.TwoPi / 6f);
            OrbitAngle = initialAngle;

            // ����� ������������� ������� �� ������
            UpdatePosition();

            // ��������� �������� = 0 (����� �� ��������� ��� ���������)
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

        // ����� ��� ���������� ������� �� ������
        private void UpdatePosition()
        {
            if (Player == null) return;

            Position = Player.Position + new Vector2(
                (float)System.Math.Cos(OrbitAngle) * OrbitRadius,
                (float)System.Math.Sin(OrbitAngle) * OrbitRadius
            );
        }

        // ����������� ���������� �� ������� (�� ����� ������)
        public void UpdateAggressiveFollow()
        {
            if (!IsActive || Player == null) return;

            UpdateLifeTime(new GameTime());

            // ������ ����������� � ������� ������� ������
            UpdatePosition();

            // �����: ��� �������� �� ����� ������������ ����������
            // Rotation �� ���������� - ����� �������� � ��������� ����������
        }

        // ���������� �������� �� ������ (����� ������ ���� ������)
        public void UpdateNormalOrbit(float deltaTime)
        {
            if (!IsActive || Player == null) return;

            UpdateLifeTime(new GameTime());

            // ��������� ���� ������
            float direction = OrbitClockwise ? 1f : -1f;
            OrbitAngle += MathHelper.ToRadians(_orbitSpeed * direction * deltaTime);

            // ��������� �������
            UpdatePosition();

            // �����: �������� ���������� ������ ����� ����� �������� ������ ������
            Rotation += 360f * deltaTime;
        }

        // ������������ � ����� ��������
        public void StartOrbiting()
        {
            _isOrbiting = true;
        }

        public override void Update(GameTime gameTime)
        {
            // ���� ����� ������ �� ������������ - ���������� ��������� ������
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