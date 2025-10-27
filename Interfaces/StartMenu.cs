using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Weapons;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Interfaces
{
    public class StartMenu
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        // Текстуры для GUI
        private Texture2D _weaponCellTexture;
        private Texture2D _upButtonTexture;
        private Texture2D _downButtonTexture;

        // Текстуры спрайтов оружий для отображения в ячейке
        private Dictionary<WeaponName, Texture2D> _weaponSprites;

        // Состояние ввода
        private MouseState _previousMouseState;
        private MouseState _currentMouseState;

        // Параметры UI
        private Rectangle _startButtonRect;
        private Rectangle _backButtonRect;
        private Rectangle _upButtonRect;
        private Rectangle _downButtonRect;
        private Rectangle _weaponCellRect;
        private Rectangle _descriptionRect;
        private Rectangle _scrollBarRect;
        private Rectangle _scrollThumbRect;

        // Данные меню
        private List<WeaponName> _availableWeapons;
        private int _selectedWeaponIndex = 0;
        private float _scrollPosition = 0f;
        private float _maxScroll = 0f;
        private bool _isScrolling = false;

        // Константы
        private const int ButtonWidth = 200;
        private const int ButtonHeight = 50;
        private const int WeaponCellSize = 150; // Увеличил размер для лучшего отображения спрайтов
        private const int ArrowButtonSize = 40;
        private const int DescriptionWidth = 400;
        private const int DescriptionHeight = 300;
        private const int ScrollBarWidth = 15;
        private const int ScrollThumbMinHeight = 30;

        public WeaponName SelectedWeapon => _availableWeapons[_selectedWeaponIndex];

        // Описания оружий с использованием \n для переносов
        private Dictionary<WeaponName, string> _weaponDescriptions = new Dictionary<WeaponName, string>
        {
            {
                WeaponName.PlayingCards,
                "ИГРАЛЬНЫЕ КАРТЫ\n\n" +
                "Мощное оружие, которое пробивает до 3 врагов за один выстрел.\n\n" +
                "Карты летят по прямой траектории и наносят урон всем врагам на своем пути.\n\n" +
                "Отлично подходит для борьбы с толпами противников."
            },
            {
                WeaponName.GoldenBullet,
                "ЗОЛОТЫЕ ПУЛИ\n\n" +
                "Точное оружие с высоким уроном по одной цели.\n\n" +
                "Пули летят с большой скоростью и гарантированно поражают ближайшего врага.\n\n" +
                "Идеально для точечного уничтожения сильных противников."
            },
            {
                WeaponName.CasinoChips,
                "ФИШКИ КАЗИНО\n\n" +
                "Фишки, которые отскакивают между врагами.\n\n" +
                "Каждая фишка может поразить нескольких врагов, перескакивая между ними.\n\n" +
                "Эффективны против групп, расположенных близко друг к другу."
            },
            {
                WeaponName.StickyBomb,
                "ЛИПКАЯ БОМБА\n\n" +
                "Тактическое оружие с отложенным взрывом.\n\n" +
                "Бомба прилипает к врагу и взрывается через 60 секунд, нанося урон всем врагам в радиусе.\n\n" +
                "Новые бомбы не появляются, пока все предыдущие не взорвались.\n\n" +
                "Отлично подходит для контроля толп и стратегического планирования."
            },
            {
                WeaponName.Dice,
                "ИГРАЛЬНЫЕ КОСТИ\n\n" +
                "Магические кости с уникальными характеристиками для каждого значения.\n\n" +
                "Кость 1: Урон 1, Пробитие 6\n" +
                "Кость 2: Урон 2, Пробитие 5\n" +
                "Кость 3: Урон 3, Пробитие 4\n" +
                "Кость 4: Урон 4, Пробитие 3\n" +
                "Кость 5: Урон 5, Пробитие 2\n" +
                "Кость 6: Урон 6, Пробитие 1\n\n" +
                "Перезарядка: 20 сек (после уничтожения всех костей)\n" +
                "Меняют направление вращения после каждого перезапуска."
            },
            {
                WeaponName.RouletteBall,
                "РУЛЕТКА\n\n" +
                "Шарик рулетки, который летит в случайном направлении и отскакивает от стен экрана.\n\n" +
                "Шарик не наносит урон, но оставляет след из частичек, которые наносят урон врагам.\n\n" +
                "Частички существуют 0.5 секунды и уничтожаются при столкновении с врагами.\n\n" +
                "Стандартные отскоки: 10\n" +
                "Стандартный урон: 1\n\n" +
                "Прокачка: скорость, время жизни частичек, урон частичек."
            }
        };

        public StartMenu(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
            _previousMouseState = Mouse.GetState();
            _currentMouseState = Mouse.GetState();
            _weaponSprites = new Dictionary<WeaponName, Texture2D>();

            // Инициализация списка оружия
            _availableWeapons = new List<WeaponName>
            {
                WeaponName.PlayingCards,
                WeaponName.GoldenBullet,
                WeaponName.CasinoChips,
                WeaponName.StickyBomb,
                WeaponName.Dice,
                WeaponName.RouletteBall,
                WeaponName.BigLaser,
                WeaponName.MolotovCocktail,
                WeaponName.GoldenSword
            };

            CalculateLayout();
        }

        public void LoadContent(Texture2D weaponCell, Texture2D upButton, Texture2D downButton)
        {
            _weaponCellTexture = weaponCell;
            _upButtonTexture = upButton;
            _downButtonTexture = downButton;
        }

        // Новый метод для загрузки спрайтов оружий
        public void LoadWeaponSprites(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                _weaponSprites[WeaponName.PlayingCards] = content.Load<Texture2D>("Sprites/GUI/WeaponPlayingCards");
                _weaponSprites[WeaponName.GoldenBullet] = content.Load<Texture2D>("Sprites/GUI/WeaponGoldenBullet");
                _weaponSprites[WeaponName.CasinoChips] = content.Load<Texture2D>("Sprites/GUI/WeaponCasinoChips");
                _weaponSprites[WeaponName.StickyBomb] = content.Load<Texture2D>("Sprites/GUI/WeaponStickyBomb");
                _weaponSprites[WeaponName.Dice] = content.Load<Texture2D>("Sprites/GUI/WeaponDice");
                _weaponSprites[WeaponName.RouletteBall] = content.Load<Texture2D>("Sprites/GUI/WeaponRoulette");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки спрайтов оружий: {ex.Message}");
            }
        }

        private void CalculateLayout()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // Центральная позиция для выбора оружия
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;

            // Weapon selection (по центру)
            _weaponCellRect = new Rectangle(
                centerX - WeaponCellSize / 2,
                centerY - 100,
                WeaponCellSize,
                WeaponCellSize
            );

            // Кнопки выбора оружия
            _upButtonRect = new Rectangle(
                centerX - ArrowButtonSize / 2,
                _weaponCellRect.Y - ArrowButtonSize - 15,
                ArrowButtonSize,
                ArrowButtonSize
            );

            _downButtonRect = new Rectangle(
                centerX - ArrowButtonSize / 2,
                _weaponCellRect.Y + WeaponCellSize + 15,
                ArrowButtonSize,
                ArrowButtonSize
            );

            // Область описания (правая часть)
            _descriptionRect = new Rectangle(
                screenWidth - DescriptionWidth - 50,
                180,
                DescriptionWidth,
                DescriptionHeight
            );

            // Полоса прокрутки
            _scrollBarRect = new Rectangle(
                _descriptionRect.X + _descriptionRect.Width - ScrollBarWidth,
                _descriptionRect.Y,
                ScrollBarWidth,
                _descriptionRect.Height
            );

            UpdateScrollThumb();

            // Кнопка старта (правый нижний угол)
            _startButtonRect = new Rectangle(
                screenWidth - ButtonWidth - 30,
                screenHeight - ButtonHeight - 30,
                ButtonWidth,
                ButtonHeight
            );

            // Кнопка назад (левый нижний угол)
            _backButtonRect = new Rectangle(
                30,
                screenHeight - ButtonHeight - 30,
                ButtonWidth,
                ButtonHeight
            );
        }

        // Метод для получения прямоугольника отрисовки спрайта оружия с сохранением пропорций
        private Rectangle GetWeaponSpriteRectangle()
        {
            if (!_weaponSprites.ContainsKey(_availableWeapons[_selectedWeaponIndex]))
                return _weaponCellRect;

            Texture2D weaponTexture = _weaponSprites[_availableWeapons[_selectedWeaponIndex]];

            // Вычисляем размеры с сохранением пропорций
            float scale = Math.Min(
                (float)(WeaponCellSize - 20) / weaponTexture.Width,
                (float)(WeaponCellSize - 20) / weaponTexture.Height
            );

            int width = (int)(weaponTexture.Width * scale);
            int height = (int)(weaponTexture.Height * scale);

            return new Rectangle(
                _weaponCellRect.Center.X - width / 2,
                _weaponCellRect.Center.Y - height / 2,
                width,
                height
            );
        }

        private void UpdateScrollThumb()
        {
            float contentHeight = GetDescriptionTextHeight();
            float visibleRatio = _descriptionRect.Height / contentHeight;

            if (visibleRatio >= 1f)
            {
                _maxScroll = 0f;
                _scrollPosition = 0f;
                _scrollThumbRect = Rectangle.Empty;
                return;
            }

            _maxScroll = contentHeight - _descriptionRect.Height;

            int thumbHeight = (int)(_descriptionRect.Height * visibleRatio);
            thumbHeight = Math.Max(thumbHeight, ScrollThumbMinHeight);

            float scrollRatio = _scrollPosition / _maxScroll;
            int thumbY = _scrollBarRect.Y + (int)((_descriptionRect.Height - thumbHeight) * scrollRatio);

            _scrollThumbRect = new Rectangle(
                _scrollBarRect.X,
                thumbY,
                ScrollBarWidth,
                thumbHeight
            );
        }

        private float GetDescriptionTextHeight()
        {
            WeaponName currentWeapon = _availableWeapons[_selectedWeaponIndex];
            string description = _weaponDescriptions.ContainsKey(currentWeapon)
                ? _weaponDescriptions[currentWeapon]
                : "Описание отсутствует.";

            return MeasureTextHeight(description, _descriptionRect.Width - ScrollBarWidth - 20);
        }

        private float MeasureTextHeight(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            string[] lines = text.Split('\n');
            float lineHeight = _font.MeasureString("A").Y;
            float totalHeight = 0f;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    totalHeight += lineHeight * 0.5f; // Пустая строка - половина высоты
                    continue;
                }

                // Разбиваем строку на слова
                string[] words = line.Split(' ');
                string currentLine = "";

                foreach (string word in words)
                {
                    string testLine = currentLine + (currentLine == "" ? "" : " ") + word;
                    Vector2 testSize = _font.MeasureString(testLine);

                    // Если строка становится слишком длинной, переносим
                    if (testSize.X > maxWidth)
                    {
                        if (currentLine == "")
                        {
                            // Если даже одно слово не помещается, разбиваем его по символам
                            currentLine = ProcessLongWord(word, maxWidth, ref totalHeight, lineHeight);
                        }
                        else
                        {
                            // Добавляем высоту для текущей строки и начинаем новую
                            totalHeight += lineHeight;
                            currentLine = word;
                        }
                    }
                    else
                    {
                        currentLine = testLine;
                    }
                }

                // Добавляем высоту для последней строки в абзаце
                if (!string.IsNullOrEmpty(currentLine))
                {
                    totalHeight += lineHeight;
                }
            }

            return totalHeight;
        }

        public GameState Update(GameTime gameTime)
        {
            _currentMouseState = Mouse.GetState();

            // Обработка прокрутки колесиком мыши
            if (_descriptionRect.Contains(_currentMouseState.Position))
            {
                _scrollPosition -= (_currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue) / 10f;
                _scrollPosition = MathHelper.Clamp(_scrollPosition, 0, _maxScroll);
                UpdateScrollThumb();
            }

            // Обработка перетаскивания ползунка
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (_scrollThumbRect.Contains(_currentMouseState.Position) && !_isScrolling)
                {
                    _isScrolling = true;
                }
            }
            else
            {
                _isScrolling = false;
            }

            if (_isScrolling)
            {
                float relativeY = _currentMouseState.Y - _scrollBarRect.Y;
                float scrollRatio = MathHelper.Clamp(relativeY / (_scrollBarRect.Height - _scrollThumbRect.Height), 0, 1);
                _scrollPosition = scrollRatio * _maxScroll;
                UpdateScrollThumb();
            }

            // Обработка кликов по кнопкам
            if (_currentMouseState.LeftButton == ButtonState.Released &&
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                // Кнопка Вверх
                if (_upButtonRect.Contains(_currentMouseState.Position))
                {
                    _selectedWeaponIndex--;
                    if (_selectedWeaponIndex < 0)
                        _selectedWeaponIndex = _availableWeapons.Count - 1;
                    _scrollPosition = 0f;
                    UpdateScrollThumb();
                }

                // Кнопка Вниз
                if (_downButtonRect.Contains(_currentMouseState.Position))
                {
                    _selectedWeaponIndex++;
                    if (_selectedWeaponIndex >= _availableWeapons.Count)
                        _selectedWeaponIndex = 0;
                    _scrollPosition = 0f;
                    UpdateScrollThumb();
                }

                // Кнопка Старт
                if (_startButtonRect.Contains(_currentMouseState.Position))
                {
                    return GameState.Playing;
                }

                // Кнопка Назад
                if (_backButtonRect.Contains(_currentMouseState.Position))
                {
                    return GameState.MainMenu;
                }
            }

            _previousMouseState = _currentMouseState;
            return GameState.StartMenu;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // Фон
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkBlue);

            // Заголовок
            string title = "ВЫБОР СТАРТОВОГО ОРУЖИЯ";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2((screenWidth - titleSize.X) / 2, 80);
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            // Название выбранного оружия (над областью описания)
            string weaponName = WeaponManager.GetDisplayName(_availableWeapons[_selectedWeaponIndex]);
            Vector2 weaponNameSize = _font.MeasureString(weaponName);
            Vector2 weaponNamePos = new Vector2(
                _descriptionRect.Center.X - weaponNameSize.X / 2,
                _descriptionRect.Y - weaponNameSize.Y - 10
            );
            spriteBatch.DrawString(_font, weaponName, weaponNamePos, Color.Yellow);

            // Отрисовка выбора оружия
            DrawWeaponSelection(spriteBatch);

            // Отрисовка описания оружия
            DrawWeaponDescription(spriteBatch);

            // Кнопки
            DrawButton(spriteBatch, _startButtonRect, "НАЧАТЬ ИГРУ", Color.Green);
            DrawButton(spriteBatch, _backButtonRect, "НАЗАД", Color.Gray);
        }

        private void DrawWeaponSelection(SpriteBatch spriteBatch)
        {
            // Ячейка оружия
            if (_weaponCellTexture != null)
                spriteBatch.Draw(_weaponCellTexture, _weaponCellRect, Color.White);
            else
                spriteBatch.Draw(_debugTexture, _weaponCellRect, Color.DarkGray);

            // Отрисовка спрайта выбранного оружия
            if (_weaponSprites.ContainsKey(_availableWeapons[_selectedWeaponIndex]))
            {
                Texture2D weaponSprite = _weaponSprites[_availableWeapons[_selectedWeaponIndex]];
                Rectangle spriteRect = GetWeaponSpriteRectangle();
                spriteBatch.Draw(weaponSprite, spriteRect, Color.White);
            }

            // Кнопки выбора
            if (_upButtonTexture != null)
                spriteBatch.Draw(_upButtonTexture, _upButtonRect, Color.White);
            else
                spriteBatch.Draw(_debugTexture, _upButtonRect, Color.LightGray);

            if (_downButtonTexture != null)
                spriteBatch.Draw(_downButtonTexture, _downButtonRect, Color.White);
            else
                spriteBatch.Draw(_debugTexture, _downButtonRect, Color.LightGray);
        }

        private void DrawWeaponDescription(SpriteBatch spriteBatch)
        {
            // Фон области описания
            spriteBatch.Draw(_debugTexture, _descriptionRect, Color.DarkSlateBlue * 0.8f);

            // Рамка
            DrawRectangle(spriteBatch, _descriptionRect, Color.White);

            // Полоса прокрутки (если нужно)
            if (_maxScroll > 0)
            {
                spriteBatch.Draw(_debugTexture, _scrollBarRect, Color.Gray);
                spriteBatch.Draw(_debugTexture, _scrollThumbRect, Color.LightGray);
            }

            // Текст описания с учетом прокрутки
            WeaponName currentWeapon = _availableWeapons[_selectedWeaponIndex];
            string description = _weaponDescriptions.ContainsKey(currentWeapon)
                ? _weaponDescriptions[currentWeapon]
                : "Описание отсутствует.";

            // Увеличиваем отступы для лучшего вида
            Rectangle textArea = new Rectangle(
                _descriptionRect.X + 15,  // Увеличил отступ слева
                _descriptionRect.Y + 15,  // Увеличил отступ сверху
                _descriptionRect.Width - ScrollBarWidth - 25, // Увеличил отступ справа
                _descriptionRect.Height - 30  // Увеличил отступ снизу
            );

            DrawTextWithNewlines(spriteBatch, description, textArea, _scrollPosition);
        }

        private void DrawTextWithNewlines(SpriteBatch spriteBatch, string text, Rectangle textArea, float scrollOffset)
        {
            if (string.IsNullOrEmpty(text))
                return;

            string[] paragraphs = text.Split('\n');
            float lineHeight = _font.MeasureString("A").Y;
            Vector2 currentPos = new Vector2(textArea.X, textArea.Y - scrollOffset);

            // Границы области отрисовки
            float minY = textArea.Y;
            float maxY = textArea.Y + textArea.Height;

            foreach (string paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    currentPos.Y += lineHeight * 0.5f;
                    continue;
                }

                // Разбиваем параграф на строки
                string[] words = paragraph.Split(' ');
                string currentLine = "";

                foreach (string word in words)
                {
                    string testLine = currentLine + (currentLine == "" ? "" : " ") + word;
                    Vector2 testSize = _font.MeasureString(testLine);

                    if (testSize.X > textArea.Width)
                    {
                        if (currentLine == "")
                        {
                            // Очень длинное слово - разбиваем по символам
                            currentLine = DrawLongWord(spriteBatch, word, textArea, ref currentPos, lineHeight, minY, maxY);
                            if (currentPos.Y > maxY) return;
                        }
                        else
                        {
                            // Рисуем текущую строку если она видима
                            if (currentPos.Y >= minY && currentPos.Y + lineHeight <= maxY)
                            {
                                spriteBatch.DrawString(_font, currentLine, new Vector2(textArea.X, currentPos.Y), Color.White);
                            }
                            currentPos.Y += lineHeight;
                            if (currentPos.Y > maxY) return;
                            currentLine = word;
                        }
                    }
                    else
                    {
                        currentLine = testLine;
                    }
                }

                // Рисуем последнюю строку параграфа если она видима
                if (!string.IsNullOrEmpty(currentLine))
                {
                    if (currentPos.Y >= minY && currentPos.Y + lineHeight <= maxY)
                    {
                        spriteBatch.DrawString(_font, currentLine, new Vector2(textArea.X, currentPos.Y), Color.White);
                    }
                    currentPos.Y += lineHeight;
                    if (currentPos.Y > maxY) return;
                }

                // Добавляем отступ между параграфами
                currentPos.Y += lineHeight * 0.3f;
                if (currentPos.Y > maxY) return;
            }
        }

        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), color);
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), color);
        }

        private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text, Color baseColor)
        {
            Color buttonColor = baseColor;
            if (rect.Contains(_currentMouseState.Position))
                buttonColor = Color.Lerp(baseColor, Color.White, 0.3f);

            spriteBatch.Draw(_debugTexture, rect, buttonColor);

            Vector2 textSize = _font.MeasureString(text);
            Vector2 textPos = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2,
                rect.Y + (rect.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(_font, text, textPos, Color.White);
        }

        // Обработка очень длинных слов, которые не помещаются в строку
        private string ProcessLongWord(string word, float maxWidth, ref float totalHeight, float lineHeight)
        {
            string remainingWord = word;

            while (remainingWord.Length > 0)
            {
                string testPart = "";
                int charsToTake = 0;

                // Находим максимальное количество символов, которые помещаются
                for (int i = 1; i <= remainingWord.Length; i++)
                {
                    testPart = remainingWord.Substring(0, i);
                    if (_font.MeasureString(testPart).X > maxWidth)
                    {
                        break;
                    }
                    charsToTake = i;
                }

                if (charsToTake == 0)
                {
                    // Если даже один символ не помещается, берем минимум 1 символ
                    charsToTake = 1;
                }

                string linePart = remainingWord.Substring(0, charsToTake);
                totalHeight += lineHeight;
                remainingWord = remainingWord.Substring(charsToTake);
            }

            return remainingWord; // Возвращаем оставшуюся часть (обычно пустую)
        }

        // Отрисовка очень длинных слов посимвольно
        // Отрисовка очень длинных слов посимвольно с проверкой границ
        private string DrawLongWord(SpriteBatch spriteBatch, string word, Rectangle textArea, ref Vector2 currentPos, float lineHeight, float minY, float maxY)
        {
            string remainingWord = word;

            while (remainingWord.Length > 0)
            {
                string testPart = "";
                int charsToTake = 0;

                // Находим максимальное количество символов, которые помещаются
                for (int i = 1; i <= remainingWord.Length; i++)
                {
                    testPart = remainingWord.Substring(0, i);
                    if (_font.MeasureString(testPart).X > textArea.Width)
                    {
                        break;
                    }
                    charsToTake = i;
                }

                if (charsToTake == 0)
                {
                    charsToTake = 1;
                }

                string linePart = remainingWord.Substring(0, charsToTake);

                // Рисуем часть слова только если она видима
                if (currentPos.Y >= minY && currentPos.Y + lineHeight <= maxY)
                {
                    spriteBatch.DrawString(_font, linePart, new Vector2(textArea.X, currentPos.Y), Color.White);
                }

                currentPos.Y += lineHeight;
                if (currentPos.Y > maxY) return "";

                remainingWord = remainingWord.Substring(charsToTake);
            }

            return "";
        }
    }
}