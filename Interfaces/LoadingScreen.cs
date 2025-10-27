using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survive_the_night.Interfaces
{
    public class LoadingScreen
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        // Прогресс загрузки
        private float _loadingProgress = 0f;
        private const float LOADING_DURATION = 10f; // 10 секунд
        private float _loadingTimer = 0f;

        // Полоска загрузки
        private Rectangle _progressBarBackground;
        private Rectangle _progressBarFill;
        private const int PROGRESS_BAR_WIDTH = 600;
        private const int PROGRESS_BAR_HEIGHT = 30;

        // Кнопка начала игры
        private Rectangle _startButtonRect;
        private const int BUTTON_WIDTH = 300;
        private const int BUTTON_HEIGHT = 60;
        private bool _isStartButtonHovered = false;

        public bool IsComplete => _loadingProgress >= 1f;

        public LoadingScreen(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
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

            // Позиция полоски загрузки
            _progressBarBackground = new Rectangle(
                centerX - PROGRESS_BAR_WIDTH / 2,
                centerY + 100,
                PROGRESS_BAR_WIDTH,
                PROGRESS_BAR_HEIGHT
            );

            _progressBarFill = new Rectangle(
                _progressBarBackground.X,
                _progressBarBackground.Y,
                0, // Начинаем с 0 ширины
                PROGRESS_BAR_HEIGHT
            );

            // Позиция кнопки начала игры
            _startButtonRect = new Rectangle(
                centerX - BUTTON_WIDTH / 2,
                _progressBarBackground.Bottom + 50,
                BUTTON_WIDTH,
                BUTTON_HEIGHT
            );
        }

        public bool Update(GameTime gameTime)
        {
            if (!IsComplete)
            {
                // Обновляем таймер загрузки
                _loadingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _loadingProgress = MathHelper.Clamp(_loadingTimer / LOADING_DURATION, 0f, 1f);

                // Обновляем ширину полоски загрузки
                _progressBarFill.Width = (int)(_progressBarBackground.Width * _loadingProgress);
            }

            // Обновляем состояние кнопки (только когда загрузка завершена)
            if (IsComplete)
            {
                MouseState mouseState = Mouse.GetState();
                Point mousePos = mouseState.Position;

                _isStartButtonHovered = _startButtonRect.Contains(mousePos);

                // Обработка клика по кнопке начала игры
                if (mouseState.LeftButton == ButtonState.Pressed && _isStartButtonHovered)
                {
                    return true; // Сигнализируем о начале игры
                }
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // Фон
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkBlue);

            // Заголовок
            string title = "ЗАГРУЗКА ИГРЫ";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (screenWidth - titleSize.X) / 2,
                150
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            // Полоска загрузки - фон
            spriteBatch.Draw(_debugTexture, _progressBarBackground, Color.DarkGray);

            // Полоска загрузки - заполнение
            Color progressColor = Color.Lerp(Color.Red, Color.Green, _loadingProgress);
            spriteBatch.Draw(_debugTexture, _progressBarFill, progressColor);

            // Рамка полоски загрузки
            spriteBatch.Draw(_debugTexture, new Rectangle(_progressBarBackground.X, _progressBarBackground.Y, _progressBarBackground.Width, 2), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_progressBarBackground.X, _progressBarBackground.Y + _progressBarBackground.Height - 2, _progressBarBackground.Width, 2), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_progressBarBackground.X, _progressBarBackground.Y, 2, _progressBarBackground.Height), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_progressBarBackground.X + _progressBarBackground.Width - 2, _progressBarBackground.Y, 2, _progressBarBackground.Height), Color.White);

            // Процент загрузки
            string progressText = $"{(int)(_loadingProgress * 100)}%";
            Vector2 progressTextSize = _font.MeasureString(progressText);
            Vector2 progressTextPos = new Vector2(
                _progressBarBackground.Center.X - progressTextSize.X / 2,
                _progressBarBackground.Center.Y - progressTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, progressText, progressTextPos, Color.White);

            // Инструкции по управлению
            DrawControls(spriteBatch, screenWidth);

            // Кнопка начала игры (только когда загрузка завершена)
            if (IsComplete)
            {
                DrawStartButton(spriteBatch);
            }
            else
            {
                // Сообщение о загрузке
                string loadingMessage = "Идет загрузка...";
                Vector2 messageSize = _font.MeasureString(loadingMessage);
                Vector2 messagePos = new Vector2(
                    (screenWidth - messageSize.X) / 2,
                    _progressBarBackground.Bottom + 20
                );
                spriteBatch.DrawString(_font, loadingMessage, messagePos, Color.Yellow);
            }
        }

        private void DrawControls(SpriteBatch spriteBatch, int screenWidth)
        {
            int centerX = screenWidth / 2;
            int controlsY = 250;

            // Заголовок инструкций
            string controlsTitle = "УПРАВЛЕНИЕ";
            Vector2 controlsTitleSize = _font.MeasureString(controlsTitle);
            Vector2 controlsTitlePos = new Vector2(
                centerX - controlsTitleSize.X / 2,
                controlsY
            );
            spriteBatch.DrawString(_font, controlsTitle, controlsTitlePos, Color.Yellow);

            // Список управлений (убрали "ЦЕЛЬ: Курсор мыши")
            string[] controls = {
                "ПЕРЕДВИЖЕНИЕ: WASD",
                "МАГАЗИН: B",
                "ПАУЗА: ESC",
                "АТАКА: Автоматическая"
            };

            int lineHeight = 30;
            for (int i = 0; i < controls.Length; i++)
            {
                Vector2 controlSize = _font.MeasureString(controls[i]);
                Vector2 controlPos = new Vector2(
                    centerX - controlSize.X / 2,
                    controlsY + 40 + (i * lineHeight)
                );
                spriteBatch.DrawString(_font, controls[i], controlPos, Color.LightGray);
            }
        }

        private void DrawStartButton(SpriteBatch spriteBatch)
        {
            // Цвет кнопки в зависимости от наведения
            Color buttonColor = _isStartButtonHovered ? Color.LightGreen : Color.Green;

            // Фон кнопки
            spriteBatch.Draw(_debugTexture, _startButtonRect, buttonColor);

            // Рамка кнопки
            spriteBatch.Draw(_debugTexture, new Rectangle(_startButtonRect.X, _startButtonRect.Y, _startButtonRect.Width, 2), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_startButtonRect.X, _startButtonRect.Y + _startButtonRect.Height - 2, _startButtonRect.Width, 2), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_startButtonRect.X, _startButtonRect.Y, 2, _startButtonRect.Height), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(_startButtonRect.X + _startButtonRect.Width - 2, _startButtonRect.Y, 2, _startButtonRect.Height), Color.White);

            // Текст кнопки
            string buttonText = "НАЧАТЬ ИГРУ";
            Vector2 textSize = _font.MeasureString(buttonText);
            Vector2 textPos = new Vector2(
                _startButtonRect.Center.X - textSize.X / 2,
                _startButtonRect.Center.Y - textSize.Y / 2
            );
            spriteBatch.DrawString(_font, buttonText, textPos, Color.White);

            // Подсказка
            string hint = "Загрузка завершена! Нажмите чтобы начать";
            Vector2 hintSize = _font.MeasureString(hint);
            Vector2 hintPos = new Vector2(
                (spriteBatch.GraphicsDevice.Viewport.Width - hintSize.X) / 2,
                _startButtonRect.Bottom + 10
            );
            spriteBatch.DrawString(_font, hint, hintPos, Color.Yellow);
        }

        public void Reset()
        {
            _loadingProgress = 0f;
            _loadingTimer = 0f;
            _progressBarFill.Width = 0;
            _isStartButtonHovered = false;
        }
    }
}