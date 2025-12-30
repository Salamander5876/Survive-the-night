// Gamedata/Config/WeaponSystem/Awaken/AwakenPlayingCards.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem.Weapons;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Awaken
{
    public class AwakenPlayingCards : Weapon
    {
        public List<AwakenPlayingCard> ActiveProjectiles { get; private set; } = new List<AwakenPlayingCard>();

        // Параметры веера
        private const float FAN_TOTAL_ANGLE = 45f;
        private readonly float[] _groupAnglesDegrees = { 0f, 22.5f, 45f };
        private const int GROUPS_COUNT = 3;
        private const int CARDS_PER_GROUP = 6;
        private const int TOTAL_CARDS_PER_BURST = GROUPS_COUNT * CARDS_PER_GROUP;

        // Тайминги
        private const float LAYER_INTERVAL = 0.3f;
        private const float BURST_COOLDOWN = 0.5f;

        // Состояние стрельбы
        private bool _isBurstActive = false;
        private int _currentLayer = 0;
        private float _nextLayerTimer = 0f;
        private Vector2 _currentTargetDirection = Vector2.Zero;

        private static List<Texture2D> _awakenCardTextures;

        public AwakenPlayingCards(Player player)
            : base(player, WeaponType.Regular, WeaponName.PlayingCards, BURST_COOLDOWN, 7)
        {
            CooldownTimer = 0f;

            System.Diagnostics.Debug.WriteLine($"=== AWAKEN CARDS WITH PIERCE ===");
            System.Diagnostics.Debug.WriteLine($"Pierce: 3 enemies (hits 3 different enemies then destroys)");
            System.Diagnostics.Debug.WriteLine($"Damage per hit: {Damage}");
        }

        public static void LoadAwakenTextures(Texture2D texture1, Texture2D texture2,
                                              Texture2D texture3, Texture2D texture4)
        {
            _awakenCardTextures = new List<Texture2D> { texture1, texture2, texture3, texture4 };
        }

        private Texture2D GetRandomAwakenTexture()
        {
            if (_awakenCardTextures != null && _awakenCardTextures.Count > 0)
            {
                return _awakenCardTextures[Game1.Random.Next(0, _awakenCardTextures.Count)];
            }
            return null;
        }

        public override void LevelUp()
        {
            // Пробужденное оружие не прокачивается
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Обновляем активные снаряды
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                var card = ActiveProjectiles[i];
                if (card.IsActive)
                {
                    card.Update(gameTime);
                }
                else
                {
                    ActiveProjectiles.RemoveAt(i);
                }
            }

            // Управление стрельбой
            UpdateShooting(gameTime);

            // Проверка коллизий
            CheckProjectileCollisions(Game1.CurrentEnemies);
        }

        private void UpdateShooting(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isBurstActive && CooldownTimer <= 0f)
            {
                StartNewBurst();
            }

            if (_isBurstActive)
            {
                _nextLayerTimer -= deltaTime;

                if (_nextLayerTimer <= 0f)
                {
                    ShootLayer();

                    _currentLayer++;

                    if (_currentLayer >= CARDS_PER_GROUP)
                    {
                        _isBurstActive = false;
                        CooldownTimer = CooldownTime;
                    }
                    else
                    {
                        _nextLayerTimer = LAYER_INTERVAL;
                    }
                }
            }
        }

        private void StartNewBurst()
        {
            Enemy target = FindClosestEnemy(Game1.CurrentEnemies);
            if (target != null)
            {
                _currentTargetDirection = Vector2.Normalize(target.Position - Player.Position);
                _isBurstActive = true;
                _currentLayer = 0;
                _nextLayerTimer = 0f;
            }
            else
            {
                CooldownTimer = 0.2f;
            }
        }

        private void ShootLayer()
        {
            for (int group = 0; group < GROUPS_COUNT; group++)
            {
                float groupAngleDegrees = _groupAnglesDegrees[group];
                float adjustedAngle = groupAngleDegrees - (FAN_TOTAL_ANGLE / 2);
                float angleRadians = MathHelper.ToRadians(adjustedAngle);

                Vector2 groupDirection = RotateVector(_currentTargetDirection, angleRadians);
                float cardSpread = MathHelper.ToRadians((float)Game1.Random.NextDouble() * 3f - 1.5f);
                Vector2 cardDirection = RotateVector(groupDirection, cardSpread);

                // Создаем карту с пробитием 3 врагов
                var card = new AwakenPlayingCard(
                    Player.Position,
                    0,
                    Color.White,
                    Damage,
                    300f,
                    cardDirection,
                    3, // ПРОБИТИЕ 3 ВРАГОВ
                    GetRandomAwakenTexture()
                );

                ActiveProjectiles.Add(card);
            }

            WeaponManager.PlayWeaponSound(WeaponName.PlayingCards, 0.7f);
        }

        private void CheckProjectileCollisions(List<Enemy> enemies)
        {
            if (enemies == null) return;

            int totalHits = 0;
            int cardsDestroyed = 0;

            // Проверяем каждую карту
            foreach (var card in ActiveProjectiles)
            {
                if (!card.IsActive) continue;

                // Проверяем каждого врага
                foreach (var enemy in enemies.ToList())
                {
                    if (!enemy.IsAlive) continue;

                    // Пробуем поразить врага (метод сам проверит пересечение и нанесет урон)
                    bool hit = card.TryHitEnemy(enemy);

                    if (hit)
                    {
                        totalHits++;

                        // Если карта уничтожилась после попадания
                        if (!card.IsActive)
                        {
                            cardsDestroyed++;
                            break; // Перестаем проверять эту карту
                        }
                    }
                }
            }

            if (totalHits > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Awaken cards: {totalHits} hits, {cardsDestroyed} cards destroyed");
            }
        }

        private Vector2 RotateVector(Vector2 vector, float angleRadians)
        {
            float cos = (float)System.Math.Cos(angleRadians);
            float sin = (float)System.Math.Sin(angleRadians);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // Вся логика теперь в Update
        }
    }
}