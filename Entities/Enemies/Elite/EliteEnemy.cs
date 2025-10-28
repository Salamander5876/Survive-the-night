// EliteEnemy.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities.Enemies.Elite
{
    public class EliteEnemy : Enemy
    {
        public enum EliteType
        {
            Type1,
            Type2
        }

        public EliteType Type { get; private set; }

        // ������� �� ��������
        public const int ExperienceOrbCount = 10;
        public const int ChestDropCount = 1;

        // ����������� ��� ����� �������
        public EliteEnemy(Vector2 initialPosition, Player playerTarget, int stage, EliteType type)
            : base(initialPosition, playerTarget, GetBaseHealth(type), 80f, Color.Blue, 15, stage)
        {
            Type = type;
        }

        // ������ ����������� ��� �������������
        public EliteEnemy(Vector2 initialPosition, Player playerTarget)
            : this(initialPosition, playerTarget, 1, EliteType.Type1)
        {
        }

        protected override int CalculateHealthForStage(int baseHealth, int stage)
        {
            return baseHealth + (stage - 1) * 100;
        }

        private static int GetBaseHealth(EliteType type)
        {
            return type == EliteType.Type1 ? 50 : 100;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            // ����������� ������ � 2 ���� ��� ������� ������
            Color drawColor = color ?? Color;
            int size = 48; // � 2 ���� ������ �������

            spriteBatch.Draw(debugTexture,
                new Rectangle((int)(Position.X - size / 2), (int)(Position.Y - size / 2), size, size),
                drawColor);
        }
    }
}