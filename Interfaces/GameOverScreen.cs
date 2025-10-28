using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survive_the_night.Interfaces
{
    public class GameOverScreen
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

        public GameOverScreen(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
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
            System.Diagnostics.Debug.WriteLine("Game Over экран показан");
        }

        public void Hide()
        {
            IsVisible = false;
            System.Diagnostics.Debug.WriteLine("Game Over экран скрыт");
        }

        public void Reset()
        {
            // Сброс состояния кнопок
            _isRestartButtonHovered = false;
            _isMainMenuButtonHovered = false;
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
                    Reset();
                    System.Diagnostics.Debug.WriteLine("Кнопка 'Начать заново' нажата, состояние изменено на StartMenu");
                }
                else if (_isMainMenuButtonHovered)
                {
                    // Выйти в главное меню
                    Game1.CurrentState = GameState.MainMenu;
                    Hide();
                    Reset();
                    System.Diagnostics.Debug.WriteLine("Кнопка 'Выйти в меню' нажата, состояние изменено на MainMenu");
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // Полупрозрачный темно-красный фон
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkRed * 0.8f);

            // Заголовок
            string title = "ИГРА ОКОНЧЕНА";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (screenWidth - titleSize.X) / 2,
                150
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            // Статистика
            string stats = $"Вы продержались: {(int)Game1.Instance.SurvivalTime} секунд";
            Vector2 statsSize = _font.MeasureString(stats);
            Vector2 statsPos = new Vector2(
                (screenWidth - statsSize.X) / 2,
                220
            );
            spriteBatch.DrawString(_font, stats, statsPos, Color.LightGray);

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

            // Инструкция
            string instruction = "Нажмите на кнопку для продолжения";
            Vector2 instructionSize = _font.MeasureString(instruction);
            Vector2 instructionPos = new Vector2(
                (screenWidth - instructionSize.X) / 2,
                screenHeight - 50
            );
            spriteBatch.DrawString(_font, instruction, instructionPos, Color.LightGray * 0.7f);
        }
    }
}