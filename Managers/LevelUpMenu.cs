using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities;
using Survive_the_night.Weapons;
using System.Diagnostics; // Для System.Diagnostics.Debug.WriteLine

namespace Survive_the_night.Managers
{
    public class UpgradeOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public Color Color { get; set; }
    }

    public class LevelUpMenu
    {
        private Player _player;
        private PlayingCards _cardsWeapon;
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        private UpgradeOption[] _currentOptions;
        private const int OptionCount = 3;
        private const int OptionHeight = 80;
        private const int OptionWidth = 300;
        private const int OptionSpacing = 20;

        private MouseState _previousMouseState;

        /// <summary>
        /// Конструктор меню улучшений.
        /// </summary>
        public LevelUpMenu(Player player, PlayingCards cardsWeapon, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _player = player;
            _cardsWeapon = cardsWeapon;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
            _previousMouseState = Mouse.GetState();
        }

        // ----------------- ЛОГИКА -----------------

        /// <summary>
        /// Генерирует три случайных или фиксированных улучшения для выбора.
        /// </summary>
        public void GenerateOptions()
        {
            _currentOptions = new UpgradeOption[OptionCount];

            // Фиксированные опции для начала разработки
            _currentOptions[0] = new UpgradeOption
            {
                Name = "Здоровье +10",
                Description = "Увеличивает максимальное HP на 10 и лечит.",
                Id = 1,
                Color = Color.Red
            };

            _currentOptions[1] = new UpgradeOption
            {
                Name = "Скорость +50",
                Description = "Увеличивает скорость передвижения на 50.",
                Id = 2,
                Color = Color.LimeGreen
            };

            _currentOptions[2] = new UpgradeOption
            {
                Name = "Урон Карт +1",
                Description = "Увеличивает урон метательных карт на 1.",
                Id = 3,
                Color = Color.Gold
            };
        }

        /// <summary>
        /// **МЕТОД, ВЫЗЫВАЮЩИЙ ОШИБКУ:** Обновляет логику меню и обрабатывает выбор.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!_player.IsLevelUpPending) return;

            MouseState currentMouseState = Mouse.GetState();

            if (_currentOptions == null)
            {
                GenerateOptions();
            }

            Rectangle menuArea = GetMenuArea();

            for (int i = 0; i < OptionCount; i++)
            {
                Rectangle optionRect = GetOptionRectangle(i, menuArea);

                if (optionRect.Contains(currentMouseState.Position))
                {
                    // Обработка клика мыши (кнопка была отпущена)
                    if (currentMouseState.LeftButton == ButtonState.Released &&
                        _previousMouseState.LeftButton == ButtonState.Pressed)
                    {
                        ApplyUpgrade(_currentOptions[i]);
                        _player.CompleteLevelUp();
                        _currentOptions = null;

                        break; // Выходим из цикла после выбора
                    }
                }
            }

            _previousMouseState = currentMouseState;
        }

        private void ApplyUpgrade(UpgradeOption option)
        {
            switch (option.Id)
            {
                case 1: // Здоровье
                    _player.ApplyUpgrade(option.Id, 10f);
                    break;
                case 2: // Скорость
                    _player.ApplyUpgrade(option.Id, 50f);
                    break;
                case 3: // Урон (Применяется к PlayingCards)
                    _cardsWeapon.UpgradeDamage(1); // Реализованное нами улучшение урона
                    Debug.WriteLine($"Улучшение: {option.Name} (Урон) применено. Новый урон: {_cardsWeapon.Damage}");
                    break;
            }
        }

        // ----------------- ОТРИСОВКА -----------------

        /// <summary>
        /// Отрисовывает меню улучшений.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!_player.IsLevelUpPending || _currentOptions == null) return;

            Rectangle menuArea = GetMenuArea();

            for (int i = 0; i < OptionCount; i++)
            {
                Rectangle optionRect = GetOptionRectangle(i, menuArea);
                UpgradeOption option = _currentOptions[i];

                Color boxColor = option.Color * 0.7f;
                MouseState currentMouseState = Mouse.GetState();

                // Эффект наведения
                if (optionRect.Contains(currentMouseState.Position))
                {
                    boxColor = option.Color * 1.0f;
                }

                // Отрисовка фона опции
                spriteBatch.Draw(_debugTexture, optionRect, boxColor);

                // Рисуем рамку
                spriteBatch.Draw(_debugTexture, new Rectangle(optionRect.X, optionRect.Y, optionRect.Width, 2), Color.White);
                spriteBatch.Draw(_debugTexture, new Rectangle(optionRect.X, optionRect.Y + OptionHeight - 2, optionRect.Width, 2), Color.White);

                // Отрисовка имени улучшения
                spriteBatch.DrawString(font, option.Name,
                    new Vector2(optionRect.X + 10, optionRect.Y + 5),
                    Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);

                // Отрисовка описания (меньший размер)
                spriteBatch.DrawString(font, option.Description,
                    new Vector2(optionRect.X + 10, optionRect.Y + 30),
                    Color.LightGray, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            }
        }

        // ----------------- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ -----------------

        /// <summary>
        /// Рассчитывает общую область, которую занимает меню.
        /// </summary>
        private Rectangle GetMenuArea()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            int totalHeight = OptionCount * OptionHeight + (OptionCount - 1) * OptionSpacing;
            int totalWidth = OptionWidth;

            int startX = (screenWidth - totalWidth) / 2;
            int startY = (screenHeight - totalHeight) / 2;

            return new Rectangle(startX, startY, totalWidth, totalHeight);
        }

        /// <summary>
        /// Рассчитывает прямоугольник для конкретной опции.
        /// </summary>
        private Rectangle GetOptionRectangle(int index, Rectangle menuArea)
        {
            int x = menuArea.X;
            int y = menuArea.Y + index * (OptionHeight + OptionSpacing);

            return new Rectangle(x, y, OptionWidth, OptionHeight);
        }
    }
}