using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using Survive_the_night.Scripts.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class RouletteBall : Weapon
    {
        public List<RouletteParticle> ActiveParticles { get; private set; } = new List<RouletteParticle>();
        public List<RouletteBallProjectile> ActiveBalls { get; private set; } = new List<RouletteBallProjectile>();

        // Характеристики
        public float ProjectileSpeed { get; private set; } = 500f;
        public float ParticleLifetime { get; private set; } = 0.5f;
        public int MaxBounces { get; private set; } = 10;
        public int ParticleDamage { get; private set; } = 1;
        public int BallDamage { get; private set; } = 1;

        // Уровни прокачки
        public int SpeedLevel { get; private set; } = 0;
        public int LifetimeLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;

        private float _cooldownTimer = 0f;
        private float _baseCooldown = 4f;

        // Таймер для создания частичек
        private float _particleTimer = 0f;
        private const float PARTICLE_INTERVAL = 0.1f;

        // Ссылка на камеру для определения границ экрана
        private Camera _camera;
        private Viewport _viewport;

        public RouletteBall(Player player) : base(player, WeaponType.Regular, WeaponName.RouletteBall, 4f, 2)
        {
            Damage = 2;
            // Получаем камеру и вьюпорт из игрового контекста
            // Это временное решение - в реальной игре нужно передать камеру извне
        }

        // Метод для установки камеры и вьюпорта
        public void SetCamera(Camera camera, Viewport viewport)
        {
            _camera = camera;
            _viewport = viewport;
        }

        public override void LevelUp()
        {
        }

        public void UpgradeSpeed()
        {
            if (SpeedLevel >= 5) return;
            ProjectileSpeed += 100f;
            SpeedLevel++;
        }

        public void UpgradeLifetime()
        {
            if (LifetimeLevel >= 5) return;
            ParticleLifetime += 1f;
            LifetimeLevel++;
        }

        public void UpgradeDamage()
        {
            if (DamageLevel >= 5) return;
            ParticleDamage += 1;
            BallDamage += 1;
            DamageLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _cooldownTimer -= deltaTime;
            _particleTimer += deltaTime;

            // Обновляем ВСЕ шарики
            for (int i = ActiveBalls.Count - 1; i >= 0; i--)
            {
                var ball = ActiveBalls[i];
                if (ball.IsActive)
                {
                    // Обновляем границы экрана для шарика на основе камеры
                    if (_camera != null)
                    {
                        ball.ScreenBounds = new Rectangle(
                            (int)_camera.Position.X,
                            (int)_camera.Position.Y,
                            _viewport.Width,
                            _viewport.Height
                        );
                    }

                    ball.Update(gameTime);

                    // Создаем частички следа с интервалом 0.1 секунды
                    if (_particleTimer >= PARTICLE_INTERVAL)
                    {
                        CreateParticle(ball.Position);
                    }

                    // ПРОВЕРЯЕМ СТОЛКНОВЕНИЯ ШАРИКА С ВРАГАМИ
                    CheckBallCollisions(ball, Game1.CurrentEnemies);
                }
                else
                {
                    // Удаляем неактивные шарики
                    ActiveBalls.RemoveAt(i);
                    Debug.WriteLine($"Шарик завершил отскоки. Осталось шариков: {ActiveBalls.Count}");
                }
            }

            // Сбрасываем таймер частичек после создания
            if (_particleTimer >= PARTICLE_INTERVAL)
            {
                _particleTimer = 0f;
            }

            // Обновляем частички
            for (int i = ActiveParticles.Count - 1; i >= 0; i--)
            {
                var particle = ActiveParticles[i];
                particle.Update(gameTime);

                if (!particle.IsActive)
                {
                    ActiveParticles.RemoveAt(i);
                }
            }

            // Проверяем столкновения частичек с врагами
            CheckParticleCollisions(Game1.CurrentEnemies);
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // НОВЫЕ ШАРИКИ ТОЛЬКО КОГДА НЕТ АКТИВНЫХ И ЕСТЬ ВРАГИ НА ЭКРАНЕ
            if (_cooldownTimer > 0f || ActiveBalls.Count > 0) return;

            // ПРОВЕРЯЕМ, ЕСТЬ ЛИ ВРАГИ В ПРЕДЕЛАХ ЭКРАНА
            Enemy closestEnemy = FindClosestEnemyOnScreen(enemies);
            if (closestEnemy == null)
            {
                // Если врагов на экране нет, не создаем шарик
                Debug.WriteLine("Нет врагов на экране - шарик не создается");
                return;
            }

            // Летим к ближайшему врагу на экране
            Vector2 direction = Vector2.Normalize(closestEnemy.Position - Player.Position);
            CreateBall(Player.Position, direction);

            _cooldownTimer = _baseCooldown;
        }

        // ИЩЕМ ВРАГА БЛИЖАЙШЕГО К ИГРОКУ, НАХОДЯЩЕГОСЯ НА ЭКРАНЕ
        private Enemy FindClosestEnemyOnScreen(List<Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0) return null;

            Enemy closestEnemy = null;
            float minDistance = float.MaxValue;

            // Получаем текущие границы экрана
            Rectangle screenBounds = GetCurrentScreenBounds();

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                // Проверяем, находится ли враг в пределах экрана
                if (!IsEnemyOnScreen(enemy, screenBounds)) continue;

                float distance = Vector2.DistanceSquared(Player.Position, enemy.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        // Получаем текущие границы экрана на основе камеры
        private Rectangle GetCurrentScreenBounds()
        {
            if (_camera != null)
            {
                return new Rectangle(
                    (int)_camera.Position.X,
                    (int)_camera.Position.Y,
                    _viewport.Width,
                    _viewport.Height
                );
            }
            else
            {
                // Fallback: статические границы
                return new Rectangle(0, 0, 1280, 720);
            }
        }

        // Проверяем, находится ли враг в пределах экрана
        private bool IsEnemyOnScreen(Enemy enemy, Rectangle screenBounds)
        {
            Rectangle enemyBounds = enemy.GetBounds();
            return screenBounds.Intersects(enemyBounds);
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
                BallDamage
            );

            // УСТАНАВЛИВАЕМ ССЫЛКУ НА ОРУЖИЕ
            newBall.SetWeapon(this);

            // Устанавливаем начальные границы экрана
            newBall.ScreenBounds = GetCurrentScreenBounds();

            ActiveBalls.Add(newBall);
            Debug.WriteLine($"Создан новый шарик. Урон: {BallDamage}, Скорость: {ProjectileSpeed}, Отскоков: {MaxBounces}");
        }

        // Метод для поиска нового врага после отскока - ИЩЕМ БЛИЖАЙШЕГО К ИГРОКУ НА ЭКРАНЕ
        public Vector2? FindNextTarget(Vector2 currentPosition, List<Enemy> enemies)
        {
            Enemy closestEnemy = FindClosestEnemyOnScreen(enemies);
            if (closestEnemy != null)
            {
                return closestEnemy.Position;
            }

            // Если врагов на экране нет, возвращаем null - шарик отскочит в случайную сторону
            return null;
        }

        // НОВЫЙ МЕТОД: воспроизведение звука отскока (будет вызываться из RouletteBallProjectile)
        public void PlayBounceSound()
        {
            WeaponManager.PlayWeaponSound(WeaponName.RouletteBall);
        }

        // Проверка столкновений шарика с врагами
        private void CheckBallCollisions(RouletteBallProjectile ball, List<Enemy> enemies)
        {
            if (!ball.IsActive) return;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (!enemy.IsAlive) continue;

                if (ball.GetBounds().Intersects(enemy.GetBounds()))
                {
                    // ШАРИК НАНОСИТ УРОН, НО НЕ УНИЧТОЖАЕТСЯ
                    enemy.TakeDamage(ball.Damage);

                    Debug.WriteLine($"Шарик нанес урон {ball.Damage} врагу");
                    // НЕ break - шарик может поразить нескольких врагов за один кадр
                }
            }
        }

        private void CreateParticle(Vector2 position)
        {
            var particle = new RouletteParticle(
                position,
                0,
                Color.White,
                ParticleDamage,
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
                        particle.IsActive = false; // Уничтожаем частичку

                        Debug.WriteLine($"Частичка нанесла урон {particle.Damage} врагу");
                        break;
                    }
                }
            }
        }
    }
}