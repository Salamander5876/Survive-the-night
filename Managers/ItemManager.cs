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
        private Texture2D _heartTexture;
        private Texture2D _goldenHeartTexture;
        private Texture2D _debugTexture;

        public int ExperienceBonus { get; private set; } = 0;
        public int CoinBonus { get; private set; } = 0;

        // Система магнита
        private float _magnetTimer = 0f;
        private float _magnetDuration = 0f;
        private float _magnetSpeed = 0f;
        private float _currentRotation = 0f;
        public bool IsMagnetActive => _magnetTimer > 0f;

        // Свойство для доступа к активным предметам (для отладки)
        public List<Item> ActiveItems => _activeItems;

        public ItemManager(Player player)
        {
            _player = player;
        }

        public void SetTextures(Texture2D coinTexture, Texture2D experienceOrbTexture, Texture2D dynamiteTexture,
                              Texture2D magnetTexture, Texture2D heartTexture, Texture2D goldenHeartTexture)
        {
            _coinTexture = coinTexture;
            _experienceOrbTexture = experienceOrbTexture;
            _dynamiteTexture = dynamiteTexture;
            _magnetTexture = magnetTexture;
            _heartTexture = heartTexture;
            _goldenHeartTexture = goldenHeartTexture;

            // Устанавливаем статические текстуры
            Coin.SetTexture(coinTexture);
            ExperienceOrb.SetTexture(experienceOrbTexture);
            Dynamite.SetTexture(dynamiteTexture);
            Magnet.SetTexture(magnetTexture);
            HealthOrbRenderer.SetTexture(heartTexture);
            GoldenHealthOrbRenderer.SetTexture(goldenHeartTexture);
        }

        public void SetDebugTexture(Texture2D debugTexture)
        {
            _debugTexture = debugTexture;
        }

        public void AddMagnet(Vector2 position)
        {
            var magnet = new Magnet(position);
            _activeItems.Add(magnet);
            _itemRenderers.Add(new MagnetRenderer(magnet, _magnetTexture));
            Debug.WriteLine($"Магнит создан на позиции {position}, текстура: {_magnetTexture != null}");
        }

        public void UpdatePlayerReference(Player newPlayer)
        {
            _player = newPlayer;
            Debug.WriteLine($"ItemManager: ссылка на игрока обновлена");
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

        public void AddDynamite(Vector2 position)
        {
            var dynamite = new Dynamite(position);
            _activeItems.Add(dynamite);
            _itemRenderers.Add(new DynamiteRenderer(dynamite, _dynamiteTexture));
            Debug.WriteLine($"Динамит создан на позиции {position}, текстура: {_dynamiteTexture != null}");
        }

        public void AddExperienceOrb(Vector2 position, int value)
        {
            int finalValue = value + ExperienceBonus;
            var orb = new ExperienceOrb(position, finalValue);
            _activeItems.Add(orb);

            // Убедитесь, что рендерер создается правильно
            if (_experienceOrbTexture != null)
            {
                _itemRenderers.Add(new ExperienceOrbRenderer(orb, _experienceOrbTexture));
                Debug.WriteLine($"Опыт создан: {finalValue} на позиции {position}, текстура: {_experienceOrbTexture.Width}x{_experienceOrbTexture.Height}, всего предметов: {_activeItems.Count}, рендереров: {_itemRenderers.Count}");
            }
            else
            {
                Debug.WriteLine($"ОШИБКА: Текстура опыта не загружена!");
                // Создаем рендерер с debug текстурой
                _itemRenderers.Add(new ExperienceOrbRenderer(orb, _debugTexture));
            }
        }

        public void AddHealthOrb(Vector2 position, float healPercentage)
        {
            var orb = new HealthOrb(position, healPercentage);
            _activeItems.Add(orb);

            // Убедитесь, что рендерер создается правильно
            var renderer = new HealthOrbRenderer(orb);
            _itemRenderers.Add(renderer);
            Debug.WriteLine($"HealthOrb создан на позиции {position}, текстура сердца: {_heartTexture != null}, всего предметов: {_activeItems.Count}, рендереров: {_itemRenderers.Count}");
        }

        public void AddGoldenHealthOrb(Vector2 position, float healPercentage)
        {
            var orb = new GoldenHealthOrb(position, healPercentage);
            _activeItems.Add(orb);
            var renderer = new GoldenHealthOrbRenderer(orb);
            _itemRenderers.Add(renderer);
            Debug.WriteLine($"GoldenHealthOrb создан на позиции {position}, текстура: {_goldenHeartTexture != null}");
        }

        public void AddCoin(Vector2 position, int value = 1)
        {
            int finalValue = value + CoinBonus;
            var coin = new Coin(position, finalValue);
            _activeItems.Add(coin);

            // Убедитесь, что рендерер создается правильно
            if (_coinTexture != null)
            {
                _itemRenderers.Add(new CoinRenderer(coin, _coinTexture));
                Debug.WriteLine($"Монета создана: {value} на позиции {position}, текстура: {_coinTexture.Width}x{_coinTexture.Height}, всего предметов: {_activeItems.Count}, рендереров: {_itemRenderers.Count}");
            }
            else
            {
                Debug.WriteLine($"ОШИБКА: Текстура монеты не загружена!");
                // Создаем рендерер с debug текстурой
                _itemRenderers.Add(new CoinRenderer(coin, _debugTexture));
            }
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
            Debug.WriteLine($"Обновление ItemManager: активных предметов={_activeItems.Count}, рендереров={_itemRenderers.Count}");
            // Обновление магнитного эффекта
            UpdateMagnet(gameTime);
            UpdateMagnetAttraction(gameTime, _player);

            int expCollected = 0;
            int coinsCollected = 0;
            int healthCollected = 0;

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
                if (!IsMagnetActive || !(item is ExperienceOrb || item is Coin))
                {
                    item.Update(gameTime, _player);
                }

                // Проверка коллизии с игроком
                bool isColliding = item.CheckCollision(_player);
                if (isColliding)
                {
                    Debug.WriteLine($"Коллизия с предметом: {item.GetType().Name} на позиции {item.Position}");

                    // Особый случай для магнита
                    if (item is Magnet)
                    {
                        ActivateMagnet(10f, Magnet.TotalAttractionSpeed);
                        Debug.WriteLine($"Магнит активирован!");
                    }
                    else
                    {
                        // Применяем эффект предмета
                        Debug.WriteLine($"Применение эффекта предмета {item.GetType().Name}");
                        item.ApplyEffect(_player);

                        // Считаем статистику
                        if (item is ExperienceOrb)
                        {
                            expCollected++;
                            Debug.WriteLine($"Опыт подобран! Всего в этом кадре: {expCollected}");
                        }
                        else if (item is Coin)
                        {
                            coinsCollected++;
                            Debug.WriteLine($"Монета подобрана! Всего в этом кадре: {coinsCollected}");
                        }
                        else if (item is HealthOrb || item is GoldenHealthOrb)
                        {
                            healthCollected++;
                            Debug.WriteLine($"Здоровье подобрано! Всего в этом кадре: {healthCollected}");
                        }
                        else if (item is Dynamite)
                        {
                            Debug.WriteLine($"Динамит поднят!");
                        }
                    }

                    item.IsActive = false;
                    Debug.WriteLine($"Предмет {item.GetType().Name} деактивирован");
                }
            }

            // Вывод итоговой статистики за кадр
            if (expCollected > 0 || coinsCollected > 0 || healthCollected > 0)
            {
                Debug.WriteLine($"Итог кадра: опыт={expCollected}, монеты={coinsCollected}, здоровье={healthCollected}");
            }

            foreach (var renderer in _itemRenderers)
            {
                renderer.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            Debug.WriteLine($"Отрисовка предметов: активных предметов={_activeItems.Count}, рендереров={_itemRenderers.Count}");

            foreach (var renderer in _itemRenderers)
            {
                try
                {
                    // Если магнит активен и это монета или опыт - применяем вращение
                    if (IsMagnetActive && renderer is CoinRenderer coinRenderer)
                    {
                        coinRenderer.SetRotation(_currentRotation);
                    }
                    else if (IsMagnetActive && renderer is ExperienceOrbRenderer orbRenderer)
                    {
                        orbRenderer.SetRotation(_currentRotation);
                    }

                    renderer.Draw(spriteBatch, debugTexture);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"Ошибка отрисовки рендерера {renderer.GetType().Name}: {ex.Message}");
                }
            }
        }

        public void Clear()
        {
            _activeItems.Clear();
            _itemRenderers.Clear();
            Debug.WriteLine("ItemManager очищен");
        }
    }
}