using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survive_the_night.Interfaces
{
    public class PauseMenu
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        // Кнопки
        private Rectangle _resumeButtonRect;
        private Rectangle _exitButtonRect;

        private bool _isResumeButtonHovered = false;
        private bool _isExitButtonHovered = false;

        public bool IsVisible { get; set; }

        public PauseMenu(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;

            CalculateLayout();
        }

        private void CalculateLayout()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;

            // Размеры кнопок
            int buttonWidth = 300;
            int buttonHeight = 60;
            int buttonSpacing = 20;

            // Позиции кнопок
            _resumeButtonRect = new Rectangle(
                centerX - buttonWidth / 2,
                centerY - buttonHeight / 2,
                buttonWidth,
                buttonHeight
            );

            _exitButtonRect = new Rectangle(
                centerX - buttonWidth / 2,
                _resumeButtonRect.Bottom + buttonSpacing,
                buttonWidth,
                buttonHeight
            );
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Update()
        {
            if (!IsVisible) return;

            MouseState mouseState = Mouse.GetState();
            Point mousePos = mouseState.Position;

            // Обновляем состояние кнопок
            _isResumeButtonHovered = _resumeButtonRect.Contains(mousePos);
            _isExitButtonHovered = _exitButtonRect.Contains(mousePos);

            // Обработка кликов
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isResumeButtonHovered)
                {
                    Hide();
                }
                else if (_isExitButtonHovered)
                {
                    // Меняем глобальное состояние игры
                    Game1.CurrentState = GameState.MainMenu;
                    Hide();
                    System.Diagnostics.Debug.WriteLine("Кнопка 'Выйти в меню' нажата, состояние изменено на MainMenu");
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // Полупрозрачный фон
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.7f);

            // Заголовок
            string title = "ПАУЗА";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (screenWidth - titleSize.X) / 2,
                200
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            // Кнопка "Продолжить"
            Color resumeColor = _isResumeButtonHovered ? Color.LightGreen : Color.Green;
            spriteBatch.Draw(_debugTexture, _resumeButtonRect, resumeColor);

            string resumeText = "ПРОДОЛЖИТЬ";
            Vector2 resumeTextSize = _font.MeasureString(resumeText);
            Vector2 resumeTextPos = new Vector2(
                _resumeButtonRect.Center.X - resumeTextSize.X / 2,
                _resumeButtonRect.Center.Y - resumeTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, resumeText, resumeTextPos, Color.White);

            // Кнопка "Выйти в меню"
            Color exitColor = _isExitButtonHovered ? Color.LightSalmon : Color.Red;
            spriteBatch.Draw(_debugTexture, _exitButtonRect, exitColor);

            string exitText = "ВЫЙТИ В МЕНЮ";
            Vector2 exitTextSize = _font.MeasureString(exitText);
            Vector2 exitTextPos = new Vector2(
                _exitButtonRect.Center.X - exitTextSize.X / 2,
                _exitButtonRect.Center.Y - exitTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, exitText, exitTextPos, Color.White);
        }
    }
}