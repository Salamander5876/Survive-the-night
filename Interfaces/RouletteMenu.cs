using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Managers;

namespace Survive_the_night.Interfaces
{
    public class RouletteMenu
    {
        private RouletteManager _rouletteManager;
        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public RouletteMenu(RouletteManager rouletteManager, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _rouletteManager = rouletteManager;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_rouletteManager.IsVisible) return;

            Vector2 startPosition = new Vector2(50, 50);
            int boxHeight = 150;
            int boxWidth = _graphicsDevice.Viewport.Width - 100;
            const int boxSpacing = 20;

            spriteBatch.DrawString(_font, "РУЛЕТКА - ВЫБЕРИТЕ НОВОЕ ОРУЖИЕ", startPosition - new Vector2(0, 40), Color.Gold);

            // Получаем текущую позицию мыши для выделения
            Point mousePosition = Mouse.GetState().Position;

            for (int i = 0; i < _rouletteManager.CurrentOptions.Count; i++)
            {
                var option = _rouletteManager.CurrentOptions[i];
                Rectangle box = new Rectangle((int)startPosition.X, (int)startPosition.Y + i * boxHeight + i * boxSpacing, boxWidth, boxHeight);

                // Фон
                Color boxColor = Color.DarkRed;
                if (box.Contains(mousePosition))
                {
                    boxColor = Color.DarkSlateGray; // Выделяем при наведении
                }

                spriteBatch.Draw(_debugTexture, box, boxColor);

                // Текст
                Vector2 textPos = new Vector2(box.X + 20, box.Y + 10);

                // Определяем цвет текста в зависимости от типа оружия
                Color titleColor = Color.White;
                if (option.Title.Contains("Золотой меч") || option.Title.Contains("Коктейль Молотова") || option.Title.Contains("Большой лазер"))
                {
                    titleColor = Color.Gold; // Легендарные оружия золотым цветом
                }

                spriteBatch.DrawString(_font, $"[{i + 1}] {option.Title}", textPos, titleColor);

                textPos.Y += 40;
                spriteBatch.DrawString(_font, option.Description, textPos, Color.LightGray);
            }
        }
    }
}