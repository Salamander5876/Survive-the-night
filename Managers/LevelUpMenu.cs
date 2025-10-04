using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities;
using Survive_the_night.Weapons;

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
        // УДАЛЕНО: private SpriteFont _font; 

        private UpgradeOption[] _currentOptions;
        private const int OptionCount = 3;
        private const int OptionHeight = 80;
        private const int OptionWidth = 300;
        private const int OptionSpacing = 20;

        private MouseState _previousMouseState;

        /// <summary>
        /// Конструктор меню улучшений.
        /// </summary>
        // ИЗМЕНЕНИЕ: Убрали SpriteFont из аргументов
        public LevelUpMenu(Player player, PlayingCards cardsWeapon, GraphicsDevice graphicsDevice, Texture2D debugTexture)
        {
            _player = player;
            _cardsWeapon = cardsWeapon;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _previousMouseState = Mouse.GetState();
        }

        // ... (GenerateOptions остается прежним) ...
        public void GenerateOptions()
        {
            _currentOptions = new UpgradeOption[OptionCount];

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

        // ... (Update остается прежним) ...
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
                    // БУДУЩИЙ КОД: Здесь мы вызовем метод _cardsWeapon.UpgradeDamage()
                    System.Diagnostics.Debug.WriteLine($"Улучшение: {option.Name} (Урон) применено. (Эффект не реализован)");
                    break;
            }
        }


        /// <summary>
        /// Отрисовывает меню улучшений (только прямоугольники).
        /// </summary>
        // ИЗМЕНЕНИЕ: Метод Draw больше не принимает SpriteFont
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_player.IsLevelUpPending || _currentOptions == null) return;

            Rectangle menuArea = GetMenuArea();

            for (int i = 0; i < OptionCount; i++)
            {
                Rectangle optionRect = GetOptionRectangle(i, menuArea);
                UpgradeOption option = _currentOptions[i];

                Color boxColor = option.Color * 0.7f;
                MouseState currentMouseState = Mouse.GetState();

                if (optionRect.Contains(currentMouseState.Position))
                {
                    boxColor = option.Color * 1.0f;
                }

                spriteBatch.Draw(_debugTexture, optionRect, boxColor);

                // Рисуем рамку
                spriteBatch.Draw(_debugTexture, new Rectangle(optionRect.X, optionRect.Y, optionRect.Width, 2), Color.White);
                spriteBatch.Draw(_debugTexture, new Rectangle(optionRect.X, optionRect.Y + OptionHeight - 2, optionRect.Width, 2), Color.White);

                // УДАЛЕНО: Код отрисовки текста
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