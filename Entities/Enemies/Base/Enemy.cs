// Enemy.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    public class Enemy : GameObject
    {
        protected Player _target;
        protected float speed;

        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int Damage { get; protected set; }
        public int CurrentStage { get; protected set; }

        public bool IsAlive => Health > 0;

        // ����� �������� ��� ������� ������
        public float DamageResistance { get; protected set; } = 0f;
        public float Vampirism { get; protected set; } = 0f;
        public bool HasUndying { get; protected set; } = false;

        // ��� ������������
        private Vector2 _knockbackVelocity = Vector2.Zero;
        private float _knockbackDecay = 0.9f;

        // �������� �����������
        public Enemy(Vector2 initialPosition, Player playerTarget, int baseHealth, float speed, Color color, int baseDamage, int stage)
            : base(initialPosition, 24, color)
        {
            _target = playerTarget;
            this.speed = speed;
            CurrentStage = stage;

            // ������ ������������� �� �����
            MaxHealth = CalculateHealthForStage(baseHealth, stage);
            Health = MaxHealth;
            Damage = baseDamage;
        }

        // ����������� ��� ������� ������ (�������������)
        public Enemy(Vector2 initialPosition, Player playerTarget)
            : this(initialPosition, playerTarget, 3, 100f, Color.Red, 5, 1)
        {
        }

        protected virtual int CalculateHealthForStage(int baseHealth, int stage)
        {
            // ��� ������� ������: 5 HP �� 1 �����, +5 �� ������ ����
            // ���� 1: 5, ���� 2: 10, ���� 3: 15 � �.�.
            return baseHealth + (stage - 1) * 5;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsAlive || _target == null) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // ��������� ������������
            if (_knockbackVelocity != Vector2.Zero)
            {
                Position += _knockbackVelocity * deltaTime;
                _knockbackVelocity *= _knockbackDecay;

                if (_knockbackVelocity.LengthSquared() < 1f)
                {
                    _knockbackVelocity = Vector2.Zero;
                }
            }
            else
            {
                // �������� � ������
                Vector2 direction = _target.Position - Position;
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }
                Position += direction * speed * deltaTime;
            }
        }

        public virtual void TakeDamage(int damageAmount)
        {
            int actualDamage = (int)(damageAmount * (1f - DamageResistance));
            Health -= actualDamage;

            if (Health <= 0)
            {
                if (HasUndying && new System.Random().NextDouble() < 0.5f)
                {
                    Health = MaxHealth / 2;
                }
                else
                {
                    Health = 0;
                }
            }
        }

        public virtual void ApplyVampirism(int damageDealt)
        {
            if (Vampirism > 0f)
            {
                int healAmount = (int)(damageDealt * Vampirism);
                Health = MathHelper.Clamp(Health + healAmount, 0, MaxHealth);
            }
        }

        public void ApplyKnockback(Vector2 force)
        {
            _knockbackVelocity = force;
        }
    }
}