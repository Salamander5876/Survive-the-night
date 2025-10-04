using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities; // Для доступа к игроку
using Survive_the_night.Weapons; // Для доступа к PlayingCards

namespace Survive_the_night.Managers
{
    // Класс, описывающий одно доступное улучшение
    public class UpgradeOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; } // Идентификатор для применения эффекта
        public Color Color { get; set; }
    }

    // Класс, управляющий меню выбора улучшений при повышении уровня
    public class LevelUpMenu
    {
        private Player _player;
        // НОВОЕ ПОЛЕ: Ссылка на наше оружие
        private PlayingCards _cardsWeapon;
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        // НОВОЕ ПОЛЕ: Ссылка на шрифт
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
            _cardsWeapon = cardsWeapon; // СОХРАНЯЕМ ССЫЛКУ НА ОРУЖИЕ
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font; // СОХРАНЯЕМ ССЫЛКУ НА ШРИФТ
            _previousMouseState = Mouse.GetState();
        }

        // ... (Метод GenerateOptions остается прежним) ...
        public void GenerateOptions()
        {
            _currentOptions = new UpgradeOption[OptionCount];

            // Вариант 1: Увеличение HP (Id = 1, Value = 10)
            _currentOptions[0] = new UpgradeOption
            {
                Name = "Здоровье +10",
                Description = "Увеличивает максимальное HP на 10 и лечит.",
                Id = 1,
                Color = Color.Red
            };

            // Вариант 2: Увеличение скорости (Id = 2, Value = 50f)
            _currentOptions[1] = new UpgradeOption
            {
                Name = "Скорость +50",
                Description = "Увеличивает скорость передвижения на 50.",
                Id = 2,
                Color = Color.LimeGreen
            };

            // Вариант 3: Урон +1 (Id = 3, Value = 1f)
            _currentOptions[2] = new UpgradeOption
            {
                Name = "Урон Карт +1",
                Description = "Увеличивает урон метательных карт на 1.",
                Id = 3,
                Color = Color.Gold
            };
        }

        /// <summary>
        /// Обновляет логику меню (обрабатывает ввод от мыши).
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!_player.IsLevelUpPending) return;

            MouseState currentMouseState = Mouse.GetState();

            // Если меню еще не сгенерировано (первый вызов)
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
                    if (currentMouseState.LeftButton == ButtonState.Released &&
                        _previousMouseState.LeftButton == ButtonState.Pressed)
                    {
                        ApplyUpgrade(_currentOptions[i]);
                        _player.CompleteLevelUp();
                        _currentOptions = null;

                        break;
                    }
                }
            }

            _previousMouseState = currentMouseState;
        }

        /// <summary>
        /// Применяет выбранное улучшение к игроку или оружию.
        /// </summary>
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
                    // !!! БУДУЩИЙ КОД: Здесь мы вызовем метод _cardsWeapon.UpgradeDamage()
                    // Пока просто выводим в консоль
                    System.Diagnostics.Debug.WriteLine($"Улучшение: {option.Name} (Урон) применено.");
                    break;
            }
        }


        /// <summary>
        /// Отрисовывает меню улучшений.
        /// </summary>
        // ИЗМЕНЕНИЕ: Метод принимает SpriteFont, но делает его необязательным, если мы не можем его передать (на всякий случай)
        public void Draw(SpriteBatch spriteBatch, SpriteFont font = null)
        {
            if (!_player.IsLevelUpPending || _currentOptions == null) return;

            Rectangle menuArea = GetMenuArea();

            for (int i = 0; i < OptionCount; i++)
            {
                Rectangle optionRect = GetOptionRectangle(i, menuArea);
                UpgradeOption option = _currentOptions[i];

                // Рисуем прямоугольник улучшения с проверкой наведения
                Color boxColor = option.Color * 0.7f;
                MouseState currentMouseState = Mouse.GetState();

                if (optionRect.Contains(currentMouseState.Position))
                {
                    boxColor = option.Color * 1.0f; // Ярче при наведении
                }

                spriteBatch.Draw(_debugTexture, optionRect, boxColor);

                // Рисуем рамку
                spriteBatch.Draw(_debugTexture, new Rectangle(optionRect.X, optionRect.Y, optionRect.Width, 2), Color.White); // Верх
                spriteBatch.Draw(_debugTexture, new Rectangle(optionRect.X, optionRect.Y + OptionHeight - 2, optionRect.Width, 2), Color.White); // Низ

                // === НОВОЕ: Отрисовка текста ===
                if (font != null)
                {
                    // Название улучшения
                    spriteBatch.DrawString(
                        font,
                        option.Name,
                        new Vector2(optionRect.X + 10, optionRect.Y + 10),
                        Color.White
                    );

                    // Описание улучшения
                    spriteBatch.DrawString(
                        font,
                        option.Description,
                        new Vector2(optionRect.X + 10, optionRect.Y + 35),
                        Color.LightGray
                    );

                    // Уровень игрока (для контекста)
                    spriteBatch.DrawString(
                        font,
                        $"Уровень: {_player.Level}",
                        new Vector2(optionRect.X + 10, optionRect.Y + 55),
                        Color.Cyan
                    );
                }
            }
        }

        // ... (Методы GetMenuArea() и GetOptionRectangle() остаются прежними) ...

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

        private Rectangle GetOptionRectangle(int index, Rectangle menuArea)
        {
            int x = menuArea.X;
            int y = menuArea.Y + index * (OptionHeight + OptionSpacing);

            return new Rectangle(x, y, OptionWidth, OptionHeight);
        }
    }
}