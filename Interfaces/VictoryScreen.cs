using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survive_the_night.Interfaces
{
    public class VictoryScreen
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        // Кнопки
        private Rectangle _restartButtonRect;
        private Rectangle _mainMenuButtonRect;

        private bool _isRestartButtonHovered = false;
        private bool _isMainMenuButtonHovered = false;

        public bool IsVisible { get; set; }

        public VictoryScreen(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
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
            _restartButtonRect = new Rectangle(
                centerX - buttonWidth / 2,
                centerY - buttonHeight / 2,
                buttonWidth,
                buttonHeight
            );

            _mainMenuButtonRect = new Rectangle(
                centerX - buttonWidth / 2,
                _restartButtonRect.Bottom + buttonSpacing,
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
            _isRestartButtonHovered = _restartButtonRect.Contains(mousePos);
            _isMainMenuButtonHovered = _mainMenuButtonRect.Contains(mousePos);

            // Обработка кликов
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isRestartButtonHovered)
                {
                    // Начать заново - переходим в стартовое меню
                    Game1.CurrentState = GameState.StartMenu;
                    Hide();
                    System.Diagnostics.Debug.WriteLine("Кнопка 'Начать заново' нажата на экране победы");
                }
                else if (_isMainMenuButtonHovered)
                {
                    // Выйти в главное меню
                    Game1.CurrentState = GameState.MainMenu;
                    Hide();
                    System.Diagnostics.Debug.WriteLine("Кнопка 'Выйти в меню' нажата на экране победы");
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // Полупрозрачный золотой фон
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Gold * 0.9f);

            // Заголовок победы
            string title = "ПОБЕДА!";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (screenWidth - titleSize.X) / 2,
                150
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.DarkBlue);

            // Сообщение о победе
            string message = "Вы пережили все испытания казино!";
            Vector2 messageSize = _font.MeasureString(message);
            Vector2 messagePos = new Vector2(
                (screenWidth - messageSize.X) / 2,
                220
            );
            spriteBatch.DrawString(_font, message, messagePos, Color.DarkBlue);

            // Статистика
            string stats = $"Время выживания: {(int)Game1.Instance.SurvivalTime} секунд";
            Vector2 statsSize = _font.MeasureString(stats);
            Vector2 statsPos = new Vector2(
                (screenWidth - statsSize.X) / 2,
                270
            );
            spriteBatch.DrawString(_font, stats, statsPos, Color.DarkBlue);

            // Кнопка "Начать заново"
            Color restartColor = _isRestartButtonHovered ? Color.LightGreen : Color.Green;
            spriteBatch.Draw(_debugTexture, _restartButtonRect, restartColor);

            string restartText = "НАЧАТЬ ЗАНОВО";
            Vector2 restartTextSize = _font.MeasureString(restartText);
            Vector2 restartTextPos = new Vector2(
                _restartButtonRect.Center.X - restartTextSize.X / 2,
                _restartButtonRect.Center.Y - restartTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, restartText, restartTextPos, Color.White);

            // Кнопка "Выйти в меню"
            Color menuColor = _isMainMenuButtonHovered ? Color.LightSalmon : Color.Red;
            spriteBatch.Draw(_debugTexture, _mainMenuButtonRect, menuColor);

            string menuText = "ВЫЙТИ В МЕНЮ";
            Vector2 menuTextSize = _font.MeasureString(menuText);
            Vector2 menuTextPos = new Vector2(
                _mainMenuButtonRect.Center.X - menuTextSize.X / 2,
                _mainMenuButtonRect.Center.Y - menuTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, menuText, menuTextPos, Color.White);
        }
    }
}