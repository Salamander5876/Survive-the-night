using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survive_the_night.Interfaces
{
    public class MainMenuRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;
        private MouseState _previousMouseState;

        // Параметры кнопки
        private Rectangle _startButtonRect;
        private const int ButtonWidth = 300;
        private const int ButtonHeight = 80;

        // Новая кнопка выхода
        private Rectangle _exitButtonRect;

        public MainMenuRenderer(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
            _previousMouseState = Mouse.GetState();

            // Расчет позиции кнопки по центру экрана
            int centerX = graphicsDevice.Viewport.Width / 2;
            int centerY = graphicsDevice.Viewport.Height / 2;
            _startButtonRect = new Rectangle(
                centerX - ButtonWidth / 2,
                centerY - ButtonHeight / 2,
                ButtonWidth,
                ButtonHeight
            );

            // Кнопка выхода (под кнопкой старта)
            _exitButtonRect = new Rectangle(
                centerX - ButtonWidth / 2,
                centerY + ButtonHeight + 20, // Под кнопкой старта с отступом
                ButtonWidth,
                ButtonHeight
            );
        }

        /// <summary>
        /// Обновляет логику меню и возвращает следующее состояние игры.
        /// </summary>
        public GameState Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();

            if (_startButtonRect.Contains(currentMouseState.Position))
            {
                if (currentMouseState.LeftButton == ButtonState.Released &&
                    _previousMouseState.LeftButton == ButtonState.Pressed)
                {
                    return GameState.StartMenu;
                }
            }

            // Обработка кнопки выхода
            if (_exitButtonRect.Contains(currentMouseState.Position))
            {
                if (currentMouseState.LeftButton == ButtonState.Released &&
                    _previousMouseState.LeftButton == ButtonState.Pressed)
                {
                    // Выход из игры
                    // Нужно получить доступ к Game1 для выхода
                    // Пока просто возвращаем состояние, обработку выхода добавим в Game1
                    return GameState.ExitGame;
                }
            }

            _previousMouseState = currentMouseState;
            return GameState.MainMenu;
        }

        /// <summary>
        /// Отрисовывает экран главного меню.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // Фон (темно-серый)
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkSlateGray);

            // Отрисовка заголовка
            string title = "CASINO SURVIVORS";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (screenWidth - 100) / 3,
                screenHeight / 4
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

            // Отрисовка кнопки
            Color buttonColor = Color.Green * 0.7f;
            if (_startButtonRect.Contains(Mouse.GetState().Position))
            {
                buttonColor = Color.Green; // Эффект наведения
            }
            spriteBatch.Draw(_debugTexture, _startButtonRect, buttonColor);

            // Рисуем рамку кнопки
            spriteBatch.Draw(_debugTexture, new Rectangle(_startButtonRect.X, _startButtonRect.Y, ButtonWidth, 2), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_startButtonRect.X, _startButtonRect.Y + ButtonHeight - 2, ButtonWidth, 2), Color.White);

            // Текст на кнопке
            string buttonText = "НАЧАТЬ ИГРУ";
            Vector2 textSize = _font.MeasureString(buttonText);
            Vector2 textPos = new Vector2(
                _startButtonRect.X + (_startButtonRect.Width - textSize.X) / 2,
                _startButtonRect.Y + (_startButtonRect.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(_font, buttonText, textPos, Color.White);

            // Отрисовка кнопки выхода
            Color exitButtonColor = _exitButtonRect.Contains(Mouse.GetState().Position) ?
            Color.Red : Color.Red * 0.7f;

            spriteBatch.Draw(_debugTexture, _exitButtonRect, exitButtonColor);

            // Рамка кнопки выхода
            spriteBatch.Draw(_debugTexture, new Rectangle(_exitButtonRect.X, _exitButtonRect.Y, ButtonWidth, 2), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_exitButtonRect.X, _exitButtonRect.Y + ButtonHeight - 2, ButtonWidth, 2), Color.White);

            // Текст на кнопке выхода
            string exitButtonText = "ВЫЙТИ";
            Vector2 exitTextSize = _font.MeasureString(exitButtonText);
            Vector2 exitTextPos = new Vector2(
                _exitButtonRect.X + (_exitButtonRect.Width - exitTextSize.X) / 2,
                _exitButtonRect.Y + (_exitButtonRect.Height - exitTextSize.Y) / 2
            );
            spriteBatch.DrawString(_font, exitButtonText, exitTextPos, Color.White);
        }
    }
}