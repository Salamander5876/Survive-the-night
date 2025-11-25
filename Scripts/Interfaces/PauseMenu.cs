using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Scripts.Managers;

namespace Survive_the_night.Scripts.Interfaces
{
    public class PauseMenu
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        // Кнопки
        private Rectangle _resumeButtonRect;
        private Rectangle _exitButtonRect;

        // Ползунки громкости
        private Rectangle _musicSliderRect;
        private Rectangle _musicSliderTrackRect;
        private Rectangle _soundSliderRect;
        private Rectangle _soundSliderTrackRect;

        // Состояние ползунков
        private bool _isMusicSliderDragging = false;
        private bool _isSoundSliderDragging = false;
        private float _musicVolume = 1.0f;
        private float _soundVolume = 1.0f;

        private bool _isResumeButtonHovered = false;
        private bool _isExitButtonHovered = false;
        private bool _isMusicSliderHovered = false;
        private bool _isSoundSliderHovered = false;

        public bool IsVisible { get; set; }

        public PauseMenu(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;

            // Загружаем текущие значения громкости
            _musicVolume = 0.75f; // Значение по умолчанию
            _soundVolume = 1.0f;  // Значение по умолчанию

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

            // Размеры ползунков
            int sliderTrackWidth = 300;
            int sliderTrackHeight = 10;
            int sliderHandleWidth = 20;
            int sliderHandleHeight = 30;

            // Позиции элементов
            int startY = centerY - 100;

            // Ползунок музыки
            _musicSliderTrackRect = new Rectangle(
                centerX - sliderTrackWidth / 2,
                startY,
                sliderTrackWidth,
                sliderTrackHeight
            );

            // Ползунок звуков
            _soundSliderTrackRect = new Rectangle(
                centerX - sliderTrackWidth / 2,
                startY + 60,
                sliderTrackWidth,
                sliderTrackHeight
            );

            // Обновляем позиции ползунков на основе текущей громкости
            UpdateSliderPositions();

            // Кнопки
            _resumeButtonRect = new Rectangle(
                centerX - buttonWidth / 2,
                startY + 140,
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

        private void UpdateSliderPositions()
        {
            int centerX = _graphicsDevice.Viewport.Width / 2;
            int sliderTrackWidth = 300;
            int sliderHandleWidth = 20;

            // Позиция ползунка музыки
            int musicSliderX = centerX - sliderTrackWidth / 2 + (int)(_musicVolume * sliderTrackWidth) - sliderHandleWidth / 2;
            _musicSliderRect = new Rectangle(
                musicSliderX,
                _musicSliderTrackRect.Y - 10,
                sliderHandleWidth,
                30
            );

            // Позиция ползунка звуков
            int soundSliderX = centerX - sliderTrackWidth / 2 + (int)(_soundVolume * sliderTrackWidth) - sliderHandleWidth / 2;
            _soundSliderRect = new Rectangle(
                soundSliderX,
                _soundSliderTrackRect.Y - 10,
                sliderHandleWidth,
                30
            );
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
            // Сбрасываем состояние перетаскивания
            _isMusicSliderDragging = false;
            _isSoundSliderDragging = false;
        }

        public void Update()
        {
            if (!IsVisible) return;

            MouseState mouseState = Mouse.GetState();
            Point mousePos = mouseState.Position;

            // Обновляем состояние элементов
            _isResumeButtonHovered = _resumeButtonRect.Contains(mousePos);
            _isExitButtonHovered = _exitButtonRect.Contains(mousePos);
            _isMusicSliderHovered = _musicSliderRect.Contains(mousePos) || _musicSliderTrackRect.Contains(mousePos);
            _isSoundSliderHovered = _soundSliderRect.Contains(mousePos) || _soundSliderTrackRect.Contains(mousePos);

            // Обработка перетаскивания ползунков
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isMusicSliderHovered || _isMusicSliderDragging)
                {
                    _isMusicSliderDragging = true;
                    UpdateMusicVolumeFromMouse(mousePos.X);
                }
                else if (_isSoundSliderHovered || _isSoundSliderDragging)
                {
                    _isSoundSliderDragging = true;
                    UpdateSoundVolumeFromMouse(mousePos.X);
                }
                else if (_isResumeButtonHovered)
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
            else
            {
                // Сбрасываем состояние перетаскивания при отпускании кнопки мыши
                _isMusicSliderDragging = false;
                _isSoundSliderDragging = false;
            }

            // Обновляем позиции ползунков
            UpdateSliderPositions();
        }

        private void UpdateMusicVolumeFromMouse(int mouseX)
        {
            int centerX = _graphicsDevice.Viewport.Width / 2;
            int sliderTrackWidth = 300;
            int trackStartX = centerX - sliderTrackWidth / 2;

            // Вычисляем новую громкость на основе позиции мыши
            float newVolume = (float)(mouseX - trackStartX) / sliderTrackWidth;
            _musicVolume = MathHelper.Clamp(newVolume, 0f, 1f);

            // Применяем громкость к музыке
            MusicsManager musicManager = GetMusicManager();
            if (musicManager != null)
            {
                musicManager.SetVolume(_musicVolume);
            }

            System.Diagnostics.Debug.WriteLine($"Громкость музыки: {_musicVolume:P0}");
        }

        private void UpdateSoundVolumeFromMouse(int mouseX)
        {
            int centerX = _graphicsDevice.Viewport.Width / 2;
            int sliderTrackWidth = 300;
            int trackStartX = centerX - sliderTrackWidth / 2;

            // Вычисляем новую громкость на основе позиции мыши
            float newVolume = (float)(mouseX - trackStartX) / sliderTrackWidth;
            _soundVolume = MathHelper.Clamp(newVolume, 0f, 1f);

            // Применяем громкость к звукам
            SoundManager.Instance.SetVolume(_soundVolume);

            System.Diagnostics.Debug.WriteLine($"Громкость звуков: {_soundVolume:P0}");
        }

        private MusicsManager GetMusicManager()
        {
            // Получаем MusicManager из Game1 через рефлексию
            // Это временное решение - в идеале нужно передать ссылку на MusicManager в конструктор
            var game1Type = typeof(Game1);
            var musicManagerField = game1Type.GetField("_musicManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (musicManagerField != null && Game1.Instance != null)
            {
                return musicManagerField.GetValue(Game1.Instance) as MusicsManager;
            }

            return null;
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
                150
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            // Ползунок громкости музыки
            DrawVolumeSlider(spriteBatch, "МУЗЫКА", _musicSliderTrackRect, _musicSliderRect, _isMusicSliderHovered || _isMusicSliderDragging, _musicVolume);

            // Ползунок громкости звуков
            DrawVolumeSlider(spriteBatch, "ЗВУКИ", _soundSliderTrackRect, _soundSliderRect, _isSoundSliderHovered || _isSoundSliderDragging, _soundVolume);

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

        private void DrawVolumeSlider(SpriteBatch spriteBatch, string label, Rectangle trackRect, Rectangle sliderRect, bool isActive, float volume)
        {
            // Текст метки
            string labelText = $"{label}: {volume:P0}";
            Vector2 labelSize = _font.MeasureString(labelText);
            Vector2 labelPos = new Vector2(
                trackRect.X,
                trackRect.Y - labelSize.Y - 5
            );
            spriteBatch.DrawString(_font, labelText, labelPos, Color.White);

            // Дорожка ползунка
            Color trackColor = Color.Gray;
            spriteBatch.Draw(_debugTexture, trackRect, trackColor);

            // Заполненная часть дорожки (визуальная индикация громкости)
            Rectangle filledTrackRect = new Rectangle(
                trackRect.X,
                trackRect.Y,
                (int)(trackRect.Width * volume),
                trackRect.Height
            );
            Color filledColor = isActive ? Color.LightBlue : Color.CornflowerBlue;
            spriteBatch.Draw(_debugTexture, filledTrackRect, filledColor);

            // Ползунок
            Color sliderColor = isActive ? Color.Gold : Color.Yellow;
            spriteBatch.Draw(_debugTexture, sliderRect, sliderColor);
        }

        // Методы для установки начальных значений громкости
        public void SetInitialVolumes(float musicVolume, float soundVolume)
        {
            _musicVolume = MathHelper.Clamp(musicVolume, 0f, 1f);
            _soundVolume = MathHelper.Clamp(soundVolume, 0f, 1f);
            UpdateSliderPositions();
        }
    }
}