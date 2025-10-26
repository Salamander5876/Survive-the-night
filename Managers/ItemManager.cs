using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Survive_the_night.Items;
using Survive_the_night.Entities;

namespace Survive_the_night.Managers
{
    public class ItemManager
    {
        private List<Item> _activeItems = new List<Item>();
        private List<GameObject> _itemRenderers = new List<GameObject>();
        private Player _player;
        private Texture2D _coinTexture;
        private Texture2D _experienceOrbTexture;
        private Texture2D _dynamiteTexture;
        private Texture2D _magnetTexture;

        public int ExperienceBonus { get; private set; } = 0;
        public int CoinBonus { get; private set; } = 0;

        // Система магнита
        private float _magnetTimer = 0f;
        private float _magnetDuration = 0f;
        private float _magnetSpeed = 0f;
        private float _currentRotation = 0f;
        public bool IsMagnetActive => _magnetTimer > 0f;

        public ItemManager(Player player)
        {
            _player = player;
        }

        public void AddMagnet(Vector2 position)
        {
            var magnet = new Magnet(position);
            _activeItems.Add(magnet);
            _itemRenderers.Add(new MagnetRenderer(magnet, _magnetTexture));
        }

        public void SetMagnetTexture(Texture2D texture)
        {
            _magnetTexture = texture;
            Magnet.SetTexture(texture);
        }

        // Активация магнитного эффекта
        public void ActivateMagnet(float duration, float speed)
        {
            _magnetTimer = duration;
            _magnetDuration = duration;
            _magnetSpeed = speed;
            _currentRotation = 0f;

            Debug.WriteLine($"Магнит активирован! Длительность: {duration}сек, Скорость: {speed}");
        }

        // Обновление магнитного эффекта
        private void UpdateMagnet(GameTime gameTime)
        {
            if (_magnetTimer > 0f)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _magnetTimer -= deltaTime;

                // Обновляем вращение
                _currentRotation += 5f * deltaTime;

                if (_magnetTimer <= 0f)
                {
                    _magnetTimer = 0f;
                    _currentRotation = 0f;
                    Debug.WriteLine("Магнит закончил работу");
                }
            }
        }

        // Обновление притяжения предметов при активном магните
        private void UpdateMagnetAttraction(GameTime gameTime, Player player)
        {
            if (!IsMagnetActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // БЕЗГРАНИЧНЫЙ радиус - магнит притягивает все предметы на карте
            foreach (var item in _activeItems)
            {
                // ИСПРАВЛЕНИЕ: Применяем магнит ТОЛЬКО к опыту и монетам, НЕ к динамиту
                if (item is ExperienceOrb || item is Coin) // УБРАЛИ Dynamite
                {
                    Vector2 direction = player.Position - item.Position;

                    // Всегда притягиваем, независимо от расстояния
                    if (direction != Vector2.Zero)
                    {
                        direction.Normalize();
                        item.Position += direction * _magnetSpeed * deltaTime;
                    }
                }
            }
        }

        // Остальные методы остаются без изменений...
        public void AddDynamite(Vector2 position)
        {
            var dynamite = new Dynamite(position);
            _activeItems.Add(dynamite);
            _itemRenderers.Add(new DynamiteRenderer(dynamite, _dynamiteTexture));
        }

        public void SetDynamiteTexture(Texture2D texture)
        {
            _dynamiteTexture = texture;
            Dynamite.SetTexture(texture);
        }

        public void AddExperienceOrb(Vector2 position, int value)
        {
            int finalValue = value + ExperienceBonus;
            var orb = new ExperienceOrb(position, finalValue);
            _activeItems.Add(orb);
            _itemRenderers.Add(new ExperienceOrbRenderer(orb, _experienceOrbTexture));
        }

        public void AddHealthOrb(Vector2 position, float healPercentage)
        {
            var orb = new HealthOrb(position, healPercentage);
            _activeItems.Add(orb);
            _itemRenderers.Add(new HealthOrbRenderer(orb));
        }

        public void AddGoldenHealthOrb(Vector2 position, float healPercentage)
        {
            var orb = new GoldenHealthOrb(position, healPercentage);
            _activeItems.Add(orb);
            _itemRenderers.Add(new GoldenHealthOrbRenderer(orb));
        }

        public void AddCoin(Vector2 position, int value = 1)
        {
            int finalValue = value + CoinBonus;
            var coin = new Coin(position, finalValue);
            _activeItems.Add(coin);
            _itemRenderers.Add(new CoinRenderer(coin, _coinTexture));
        }

        public void SetCoinTexture(Texture2D texture)
        {
            _coinTexture = texture;
            Coin.SetTexture(texture);
        }

        public void SetExperienceOrbTexture(Texture2D texture)
        {
            _experienceOrbTexture = texture;
            ExperienceOrb.SetTexture(texture);
        }

        public void ApplyExperienceBonus(int bonusAmount)
        {
            ExperienceBonus += bonusAmount;
        }

        public void ApplyCoinBonus(int bonusAmount)
        {
            CoinBonus += bonusAmount;
        }

        public void Update(GameTime gameTime)
        {
            // Обновление магнитного эффекта
            UpdateMagnet(gameTime);
            UpdateMagnetAttraction(gameTime, _player);

            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                var item = _activeItems[i];

                if (!item.IsActive)
                {
                    _activeItems.RemoveAt(i);
                    _itemRenderers.RemoveAt(i);
                    continue;
                }

                // Обычное обновление предмета (без магнита)
                // НЕ вызываем item.Update для предметов, которые примагничиваются
                // ИСПРАВЛЕНИЕ: Только опыт и монеты, динамит обновляется нормально
                if (!IsMagnetActive || !(item is ExperienceOrb || item is Coin)) // УБРАЛИ Dynamite
                {
                    item.Update(gameTime, _player);
                }

                if (item.CheckCollision(_player))
                {
                    // Особый случай для магнита
                    if (item is Magnet)
                    {
                        ActivateMagnet(10f, Magnet.TotalAttractionSpeed); // ИСПРАВЛЕНИЕ: 10 секунд!
                    }
                    else
                    {
                        item.ApplyEffect(_player);
                    }
                    item.IsActive = false;
                }
            }

            foreach (var renderer in _itemRenderers)
            {
                renderer.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            foreach (var renderer in _itemRenderers)
            {
                // Если магнит активен и это монета или опыт - применяем вращение
                // ИСПРАВЛЕНИЕ: Только монеты и опыт, динамит не вращается
                if (IsMagnetActive && (renderer is CoinRenderer coinRenderer))
                {
                    coinRenderer.SetRotation(_currentRotation);
                }
                else if (IsMagnetActive && (renderer is ExperienceOrbRenderer orbRenderer))
                {
                    orbRenderer.SetRotation(_currentRotation);
                }
                // Динамит не вращается!

                renderer.Draw(spriteBatch, debugTexture);
            }
        }

        public void Clear()
        {
            _activeItems.Clear();
            _itemRenderers.Clear();
        }
    }
}