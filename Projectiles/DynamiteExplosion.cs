using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Diagnostics;
using Survive_the_night.Entities;

namespace Survive_the_night.Projectiles
{
    public class DynamiteExplosion
    {
        public static List<DynamiteExplosion> ActiveExplosions = new List<DynamiteExplosion>();

        public Vector2 Position { get; private set; }
        public int Damage { get; private set; }
        public bool IsActive { get; private set; }
        public Rectangle Hitbox { get; private set; }

        private float _duration = 0.2f;
        private float _timer = 0f;
        private static Texture2D _explosionTexture;
        private SoundEffect _explosionSound;
        private bool _soundPlayed = false;
        private bool _damageApplied = false;
        private List<Enemy> _damagedEnemies = new List<Enemy>(); // Список уже поврежденных врагов

        public DynamiteExplosion(Vector2 position, int damage, SoundEffect explosionSound)
        {
            Position = position;
            Damage = damage;
            IsActive = true;
            _explosionSound = explosionSound;
            _damageApplied = false;

            // Хитбокс будет установлен после загрузки текстуры
            if (_explosionTexture != null)
            {
                UpdateHitbox();
            }
        }

        public static void SetTexture(Texture2D texture)
        {
            _explosionTexture = texture;
            // Обновляем хитбоксы всех активных взрывов
            foreach (var explosion in ActiveExplosions)
            {
                explosion.UpdateHitbox();
            }
        }

        private void UpdateHitbox()
        {
            if (_explosionTexture != null)
            {
                // Хитбокс соответствует размеру спрайта взрыва
                int width = _explosionTexture.Width;
                int height = _explosionTexture.Height;
                Hitbox = new Rectangle(
                    (int)(Position.X - width / 2f),
                    (int)(Position.Y - height / 2f),
                    width,
                    height
                );
                Debug.WriteLine($"💥 Хитбокс взрыва: {Hitbox} (размер текстуры: {width}x{height})");
            }
        }

        public void Update(GameTime gameTime, List<Enemy> enemies)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timer += deltaTime;

            // Проигрываем звук при создании взрыва
            if (!_soundPlayed && _explosionSound != null)
            {
                _explosionSound.Play(0.3f, 0f, 0f);
                _soundPlayed = true;
            }

            // Наносим урон врагам только один раз при создании взрыва
            if (!_damageApplied)
            {
                ApplyDamageToEnemies(enemies);
                _damageApplied = true;
            }

            if (_timer >= _duration)
            {
                IsActive = false;
            }
        }

        private void ApplyDamageToEnemies(List<Enemy> enemies)
        {
            int enemiesHit = 0;

            foreach (var enemy in enemies)
            {
                // Не наносим урон элитным врагам
                if (enemy is EliteEnemy) continue;

                // Проверяем не наносили ли уже урон этому врагу
                if (_damagedEnemies.Contains(enemy)) continue;

                // Получаем хитбокс врага
                Rectangle enemyBounds = GetEnemyBounds(enemy);

                // Проверяем пересечение хитбоксов
                if (enemy.IsAlive && Hitbox.Intersects(enemyBounds))
                {
                    enemy.TakeDamage(Damage);
                    _damagedEnemies.Add(enemy); // Запоминаем что нанесли урон этому врагу
                    enemiesHit++;
                    Debug.WriteLine($"💥 Динамит нанес {Damage} урона врагу! Хитбокс взрыва: {Hitbox}, Хитбокс врага: {enemyBounds}");
                }
            }

            Debug.WriteLine($"💥 Взрыв динамита: поражено {enemiesHit} врагов");
        }

        private Rectangle GetEnemyBounds(Enemy enemy)
        {
            // Используем тот же метод что и в Game1.cs для получения хитбокса врага
            const int Size = 24;
            return new Rectangle(
                (int)(enemy.Position.X - Size),
                (int)(enemy.Position.Y - Size),
                Size * 2,
                Size * 2
            );
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || _explosionTexture == null) return;

            float alpha = 1f - (_timer / _duration);
            Color color = Color.White * alpha;

            Vector2 origin = new Vector2(_explosionTexture.Width / 2f, _explosionTexture.Height / 2f);

            spriteBatch.Draw(
                _explosionTexture,
                Position,
                null,
                color,
                0f,
                origin,
                1.0f,
                SpriteEffects.None,
                0f
            );

            // Отрисовка хитбокса для отладки (можно убрать потом)
            //DrawDebugHitbox(spriteBatch);
        }

        private void DrawDebugHitbox(SpriteBatch spriteBatch)
        {
            // Создаем текстуру для отладки хитбокса
            Texture2D debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            debugTexture.SetData(new[] { Color.Red });

            spriteBatch.Draw(debugTexture, Hitbox, Color.Red * 0.3f);
        }

        public static void UpdateAll(GameTime gameTime, List<Enemy> enemies)
        {
            for (int i = ActiveExplosions.Count - 1; i >= 0; i--)
            {
                var explosion = ActiveExplosions[i];
                explosion.Update(gameTime, enemies);

                if (!explosion.IsActive)
                {
                    ActiveExplosions.RemoveAt(i);
                }
            }
        }

        public static void DrawAll(SpriteBatch spriteBatch)
        {
            foreach (var explosion in ActiveExplosions)
            {
                explosion.Draw(spriteBatch);
            }
        }
    }
}