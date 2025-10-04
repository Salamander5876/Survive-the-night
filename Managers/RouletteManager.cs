using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Survive_the_night; // Подключаем корневое пространство имен
using System;

// Важно: Предполагаем, что класс UpgradeOption доступен.

namespace Survive_the_night.Managers
{
    // Здесь НЕТ определения public enum GameState!

    public class RouletteManager
    {
        private LevelUpMenu _levelUpMenu;

        // --- Поля Рулетки ---
        private const float SpinDuration = 3.0f;
        private float _spinTimer = 0f;
        private float _rotationSpeed = 600f;

        private int _selectedIndex = 0;
        private int _finalChoiceIndex = -1;
        private bool _isSpinning = false;

        // Используем List<UpgradeOption>
        private List<UpgradeOption> _currentOptions;
        private KeyboardState _oldKeyboardState;

        public RouletteManager(LevelUpMenu levelUpMenu)
        {
            _levelUpMenu = levelUpMenu;
            _oldKeyboardState = Keyboard.GetState();
            _currentOptions = _levelUpMenu.CurrentOptions;
        }

        public void StartRoulette()
        {
            _levelUpMenu.GenerateOptions();
            _currentOptions = _levelUpMenu.CurrentOptions;

            if (_currentOptions == null || _currentOptions.Count == 0)
            {
                // Принудительно используем корневой тип
                Game1.CurrentState = Survive_the_night.GameState.Playing;
                return;
            }

            _finalChoiceIndex = Game1.Random.Next(0, _currentOptions.Count);

            _spinTimer = SpinDuration;
            _isSpinning = true;
            // !!! КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: Принудительное использование корневого типа !!!
            Game1.CurrentState = Survive_the_night.GameState.Roulette;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState newKeyboardState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isSpinning)
            {
                _spinTimer -= deltaTime;

                if (_spinTimer <= 0)
                {
                    _isSpinning = false;
                    _selectedIndex = _finalChoiceIndex;
                    return;
                }

                // Логика вращения
                float normalizedTime = _spinTimer / SpinDuration;
                _rotationSpeed = MathHelper.Lerp(100f, 600f, normalizedTime);
                float rotationAdvance = _rotationSpeed * deltaTime;
                int steps = (int)(rotationAdvance / 100);

                if (steps > 0 && _currentOptions != null && _currentOptions.Count > 0)
                {
                    _selectedIndex = (_selectedIndex + steps) % _currentOptions.Count;
                }
            }
            else // Остановлено, ждем ввода
            {
                if (_spinTimer <= 0 && newKeyboardState.GetPressedKeys().Length > 0 && _oldKeyboardState.GetPressedKeys().Length == 0)
                {
                    _levelUpMenu.ApplyChoice(_finalChoiceIndex);
                    // Принудительно используем корневой тип
                    Game1.CurrentState = Survive_the_night.GameState.Playing;
                    _levelUpMenu.CurrentOptions.Clear();
                }
            }

            _oldKeyboardState = newKeyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D debugTexture, int screenWidth, int screenHeight)
        {
            // Принудительно используем корневой тип
            if (Game1.CurrentState != Survive_the_night.GameState.Roulette || _currentOptions == null || _currentOptions.Count == 0) return;

            Vector2 center = new Vector2(screenWidth / 2, screenHeight / 2);
            int boxWidth = 600;
            int boxHeight = 100;
            Rectangle box = new Rectangle((int)center.X - boxWidth / 2, (int)center.Y - boxHeight / 2, boxWidth, boxHeight);

            string headerText = _isSpinning ? "АВТОМАТ. ПОЛУЧЕНИЕ НАГРАДЫ!" : "НАГРАДА ПОЛУЧЕНА!";
            Vector2 headerSize = font.MeasureString(headerText);
            spriteBatch.DrawString(font, headerText, center - new Vector2(headerSize.X / 2, 200), Color.Yellow);

            // Отрисовка поля
            spriteBatch.Draw(debugTexture, box, Color.DarkViolet * 0.5f);
            spriteBatch.Draw(debugTexture, new Rectangle(box.X - 5, box.Y - 5, box.Width + 10, box.Height + 10), Color.Gold * 0.9f);

            // Определение отображаемого текста
            int displayIndex = _isSpinning ? _selectedIndex : _finalChoiceIndex;
            string currentItemTitle = (_currentOptions.Count > displayIndex && displayIndex >= 0)
                ? _currentOptions[displayIndex].Title
                : "ОШИБКА ДАННЫХ";

            Color textColor;

            if (_isSpinning)
            {
                textColor = Color.White;
            }
            else
            {
                textColor = Color.LimeGreen;

                string winText = "ПОБЕДА!";
                string pressKeyText = "Нажмите любую кнопку...";

                Vector2 winSize = font.MeasureString(winText);
                Vector2 pressKeySize = font.MeasureString(pressKeyText);

                spriteBatch.DrawString(font, winText, center - new Vector2(winSize.X / 2, 100), Color.LimeGreen);
                spriteBatch.DrawString(font, pressKeyText, center + new Vector2(-pressKeySize.X / 2, 100), Color.White);
            }

            Vector2 textPos = center - font.MeasureString(currentItemTitle) / 2;
            spriteBatch.DrawString(font, currentItemTitle, textPos, textColor);
        }
    }
}