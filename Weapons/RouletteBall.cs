using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Survive_the_night.Weapons
{
    public class RouletteBall : Weapon
    {
        public List<RouletteParticle> ActiveParticles { get; private set; } = new List<RouletteParticle>();
        public List<RouletteBallProjectile> ActiveBalls { get; private set; } = new List<RouletteBallProjectile>();

        // ��������������
        public float ProjectileSpeed { get; private set; } = 500f;
        public float ParticleLifetime { get; private set; } = 0.5f;
        public int MaxBounces { get; private set; } = 10;
        public int ParticleDamage { get; private set; } = 2; // ��������� � 1 �� 2
        public int BallDamage { get; private set; } = 2; // ��������� � 1 �� 2

        // ������ ��������
        public int SpeedLevel { get; private set; } = 0;
        public int LifetimeLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;

        private float _cooldownTimer = 0f;
        private float _baseCooldown = 4f;

        // ������ ��� �������� ��������
        private float _particleTimer = 0f;
        private const float PARTICLE_INTERVAL = 0.1f;

        public RouletteBall(Player player) : base(player, WeaponType.Regular, WeaponName.RouletteBall, 4f, 2) // ���� �������� � 1 �� 2
        {
            Damage = 2; // ������������� ������� ����
        }

        public override void LevelUp()
        {
        }

        public void UpgradeSpeed()
        {
            if (SpeedLevel >= 10) return;
            ProjectileSpeed += 50f;
            SpeedLevel++;
            Debug.WriteLine($"�������� �������� �������: {ProjectileSpeed} (��. {SpeedLevel})");
        }

        public void UpgradeLifetime()
        {
            if (LifetimeLevel >= 10) return;
            ParticleLifetime += 0.5f;
            LifetimeLevel++;
            Debug.WriteLine($"�������� ����� �������� �������: {ParticleLifetime}� (��. {LifetimeLevel})");
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 10) return;
            ParticleDamage += 2; // ��������� � +1 �� +2 �� �������
            BallDamage += 2; // ��������� � +1 �� +2 �� �������
            DamageLevel++;
            Debug.WriteLine($"������� ���� �������: �����={BallDamage}, ��������={ParticleDamage} (��. {DamageLevel})");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _cooldownTimer -= deltaTime;
            _particleTimer += deltaTime;

            // ��������� ��� ������
            for (int i = ActiveBalls.Count - 1; i >= 0; i--)
            {
                var ball = ActiveBalls[i];
                if (ball.IsActive)
                {
                    ball.Update(gameTime);

                    // ������� �������� ����� � ���������� 0.1 �������
                    if (_particleTimer >= PARTICLE_INTERVAL)
                    {
                        CreateParticle(ball.Position);
                    }

                    // ��������� ������������ ������ � �������
                    CheckBallCollisions(ball, Game1.CurrentEnemies);
                }
                else
                {
                    // ������� ���������� ������
                    ActiveBalls.RemoveAt(i);
                    Debug.WriteLine($"����� �������� �������. �������� �������: {ActiveBalls.Count}");
                }
            }

            // ���������� ������ �������� ����� ��������
            if (_particleTimer >= PARTICLE_INTERVAL)
            {
                _particleTimer = 0f;
            }

            // ��������� ��������
            for (int i = ActiveParticles.Count - 1; i >= 0; i--)
            {
                var particle = ActiveParticles[i];
                particle.Update(gameTime);

                if (!particle.IsActive)
                {
                    ActiveParticles.RemoveAt(i);
                }
            }

            // ��������� ������������ �������� � �������
            CheckParticleCollisions(Game1.CurrentEnemies);
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // ����� ������ ������ ����� ��� ��������
            if (_cooldownTimer > 0f || ActiveBalls.Count > 0) return;

            // ������� ���������� ����� � ������
            Enemy closestEnemy = FindClosestEnemyToPlayer(enemies);
            if (closestEnemy == null)
            {
                // ���� ������ ���, ���������� ��������� �����������
                Vector2 randomDirection = new Vector2(
                    (float)(Game1.Random.NextDouble() * 2 - 1),
                    (float)(Game1.Random.NextDouble() * 2 - 1)
                );
                randomDirection.Normalize();

                CreateBall(Player.Position, randomDirection);
            }
            else
            {
                // ����� � ���������� ����� (� ������)
                Vector2 direction = Vector2.Normalize(closestEnemy.Position - Player.Position);
                CreateBall(Player.Position, direction);
            }

            _cooldownTimer = _baseCooldown;
        }

        // ���� ����� ���������� � ������
        private Enemy FindClosestEnemyToPlayer(List<Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0) return null;

            Enemy closestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distance = Vector2.DistanceSquared(Player.Position, enemy.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        private void CreateBall(Vector2 position, Vector2 direction)
        {
            var newBall = new RouletteBallProjectile(
                position,
                0,
                Color.White,
                ProjectileSpeed,
                direction,
                MaxBounces,
                BallDamage // ������ ���� 2 (�������)
            );

            // ������������� ������ �� ������
            newBall.SetWeapon(this);

            ActiveBalls.Add(newBall);
            Debug.WriteLine($"������ ����� �����. ����: {BallDamage}, ��������: {ProjectileSpeed}, ��������: {MaxBounces}");
        }

        // ����� ��� ������ ������ ����� ����� ������� - ���� ���������� � ������
        public Vector2? FindNextTarget(Vector2 currentPosition, List<Enemy> enemies)
        {
            Enemy closestEnemy = FindClosestEnemyToPlayer(enemies);
            return closestEnemy?.Position;
        }

        // ����� �����: ��������������� ����� ������� (����� ���������� �� RouletteBallProjectile)
        public void PlayBounceSound()
        {
            WeaponManager.GetWeaponSound(WeaponName.RouletteBall)?.Play();
        }

        // �������� ������������ ������ � �������
        private void CheckBallCollisions(RouletteBallProjectile ball, List<Enemy> enemies)
        {
            if (!ball.IsActive) return;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (!enemy.IsAlive) continue;

                if (ball.GetBounds().Intersects(enemy.GetBounds()))
                {
                    // ����� ������� ����, �� �� ������������
                    enemy.TakeDamage(ball.Damage);

                    // ������ ���� - ���� ������ ��� �������� �� ����
                    Debug.WriteLine($"����� ����� ���� {ball.Damage} �����");
                    // �� break - ����� ����� �������� ���������� ������ �� ���� ����
                }
            }
        }

        private void CreateParticle(Vector2 position)
        {
            var particle = new RouletteParticle(
                position,
                0,
                Color.White,
                ParticleDamage, // ������ ���� 2 (�������)
                ParticleLifetime
            );
            ActiveParticles.Add(particle);
        }

        private void CheckParticleCollisions(List<Enemy> enemies)
        {
            for (int i = ActiveParticles.Count - 1; i >= 0; i--)
            {
                var particle = ActiveParticles[i];
                if (!particle.IsActive) continue;

                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var enemy = enemies[j];
                    if (!enemy.IsAlive) continue;

                    if (particle.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(particle.Damage);
                        particle.IsActive = false; // ���������� ��������

                        // ������ ���� - ���� ������ ��� �������� �� ����
                        Debug.WriteLine($"�������� ������� ���� {particle.Damage} �����");
                        break;
                    }
                }
            }
        }
    }
}