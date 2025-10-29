using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Items;

public class CoinRenderer : GameObject
{
    private Coin _coin;
    private Texture2D _texture;
    private float _rotation = 0f;

    public CoinRenderer(Coin coin, Texture2D texture)
        : base(coin.Position, texture?.Width ?? 20, texture?.Height ?? 20, Color.Gold)
    {
        _coin = coin;
        _texture = texture;

        // Отладочная информация
        System.Diagnostics.Debug.WriteLine($"CoinRenderer создан: texture={texture != null}, size={texture?.Width}x{texture?.Height}");
    }

    public override void Update(GameTime gameTime)
    {
        Position = _coin.Position; // Это мировые координаты

        // ДЕБАГ: выводим информацию о позиции рендерера
        System.Diagnostics.Debug.WriteLine($"Рендерер монеты обновлен: мировые координаты = {Position}, активен = {_coin.IsActive}");
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
    {
        if (_texture != null && _coin.IsActive)
        {
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

            spriteBatch.Draw(
                _texture,
                Position,
                null,
                Color.White,
                _rotation,
                origin,
                1.0f,
                SpriteEffects.None,
                0f
            );

            System.Diagnostics.Debug.WriteLine($"Отрисовка монеты: позиция={Position}, текстура={_texture.Width}x{_texture.Height}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ОШИБКА отрисовки монеты: текстура={_texture != null}, активна={_coin.IsActive}");
            // Резервная отрисовка
            base.Draw(spriteBatch, debugTexture, Color.Gold);
        }
    }

    public void SetRotation(float rotation)
    {
        _rotation = rotation;
    }

    public override Rectangle GetBounds()
    {
        if (_texture != null)
        {
            return new Rectangle(
                (int)(Position.X - _texture.Width / 2f),
                (int)(Position.Y - _texture.Height / 2f),
                _texture.Width,
                _texture.Height
            );
        }
        else
        {
            return base.GetBounds();
        }
    }
}