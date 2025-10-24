using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public ItemManager(Player player)
        {
            _player = player;
        }

        public void AddExperienceOrb(Vector2 position, int value)
        {
            var orb = new ExperienceOrb(position, value);
            _activeItems.Add(orb);
            _itemRenderers.Add(new ExperienceOrbRenderer(orb));
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
            var coin = new Coin(position, value);
            _activeItems.Add(coin);
            _itemRenderers.Add(new CoinRenderer(coin, _coinTexture));
        }

        public void SetCoinTexture(Texture2D texture)
        {
            _coinTexture = texture;
            Coin.SetTexture(texture);
        }

        // Остальные методы остаются без изменений
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