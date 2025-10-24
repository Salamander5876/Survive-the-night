using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics; // ДОБАВИТЬ
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

        // Добавляем свойство для бонуса опыта
        public int ExperienceBonus { get; private set; } = 0;
        public int CoinBonus { get; private set; } = 0; // НОВЫЙ БОНУС

        public ItemManager(Player player)
        {
            _player = player;
        }

        public void AddExperienceOrb(Vector2 position, int value)
        {
            // Применяем бонус опыта
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
            // Применяем бонус монет
            int finalValue = value + CoinBonus;
            var coin = new Coin(position, finalValue);
            _activeItems.Add(coin);
            _itemRenderers.Add(new CoinRenderer(coin, _coinTexture));
        }

        // Методы для установки текстур
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

        // Метод для применения бонуса опыта из магазина
        public void ApplyExperienceBonus(int bonusAmount)
        {
            ExperienceBonus += bonusAmount;
        }

        // НОВЫЙ МЕТОД: для применения бонуса монет
        public void ApplyCoinBonus(int bonusAmount)
        {
            CoinBonus += bonusAmount;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                var item = _activeItems[i];

                if (!item.IsActive)
                {
                    _activeItems.RemoveAt(i);
                    _itemRenderers.RemoveAt(i);
                    continue;
                }

                item.Update(gameTime, _player);

                if (item.CheckCollision(_player))
                {
                    item.ApplyEffect(_player);
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