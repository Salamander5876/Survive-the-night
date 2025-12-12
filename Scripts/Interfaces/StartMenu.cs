using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Weapons;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Scripts.Interfaces
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

        // Новые элементы для выбора режима игры
        private Rectangle _gameModeDropdownRect;
        private Rectangle _gameModeDescriptionRect;
        private Rectangle _gameModeScrollBarRect;
        private Rectangle _gameModeScrollThumbRect;
        private bool _isDropdownOpen = false;
        private Rectangle[] _gameModeOptionRects;

        // Данные меню
        private List<WeaponName> _availableWeapons;
        private int _selectedWeaponIndex = 0;
        private float _scrollPosition = 0f;
        private float _maxScroll = 0f;
        private bool _isScrolling = false;

        // Данные для прокрутки описания режима
        private float _gameModeScrollPosition = 0f;
        private float _gameModeMaxScroll = 0f;
        private bool _isGameModeScrolling = false;

        // Данные режимов игры
        public enum GameMode
        {
            Easy,
            Hard,
            Insane,
            Survival,
            Custom,
            Endless
        }

        private GameMode _selectedGameMode = GameMode.Easy;
        private Dictionary<GameMode, string> _gameModeDescriptions;

        // Константы
        private const int ButtonWidth = 200;
        private const int ButtonHeight = 50;
        private const int WeaponCellSize = 150;
        private const int ArrowButtonSize = 40;
        private const int DescriptionWidth = 400;
        private const int DescriptionHeight = 400;
        private const int ScrollBarWidth = 15;
        private const int ScrollThumbMinHeight = 30;
        private const int GameModeDropdownWidth = 400;
        private const int GameModeDropdownHeight = 40;
        private const int GameModeOptionHeight = 35;
        private const int GameModeDescriptionHeight = 330;

        public WeaponName SelectedWeapon => _availableWeapons[_selectedWeaponIndex];
        public GameMode SelectedGameMode => _selectedGameMode;

        // Описания оружий с использованием \n для переносов
        private Dictionary<WeaponName, string> _weaponDescriptions = new Dictionary<WeaponName, string>
        {
            {
                WeaponName.PlayingCards,
                "Мощное оружие, которое пробивает до 3 врагов за один выстрел.\n\n" +
                "Карты летят по прямой траектории и наносят урон всем врагам на своем пути.\n\n" +
                "Отлично подходит для борьбы с толпами противников."
            },
            {
                WeaponName.GoldenBullet,
                "Точное оружие с высоким уроном по одной цели.\n\n" +
                "Пули летят с большой скоростью и гарантированно поражают ближайшего врага.\n\n" +
                "Идеально для точечного уничтожения сильных противников."
            },
            {
                WeaponName.CasinoChips,
                "Фишки, которые отскакивают между врагами.\n\n" +
                "Каждая фишка может поразить нескольких врагов, перескакивая между ними.\n\n" +
                "Эффективны против групп, расположенных близко друг к другу."
            },
            {
                WeaponName.StickyBomb,
                "Тактическое оружие с отложенным взрывом.\n\n" +
                "Бомба прилипает к врагу и взрывается через 10 секунд, нанося урон всем врагам в радиусе.\n\n" +
                "Новые бомбы не появляются, пока все предыдущие не взорвались.\n\n" +
                "Отлично подходит для контроля толп и стратегического планирования."
            },
            {
                WeaponName.Dice,
                "Магические кости с уникальными характеристиками для каждого значения.\n\n" +
                "Кость 1: Урон 2, Пробитие 6\n" +
                "Кость 2: Урон 4, Пробитие 5\n" +
                "Кость 3: Урон 6, Пробитие 4\n" +
                "Кость 4: Урон 8, Пробитие 3\n" +
                "Кость 5: Урон 10, Пробитие 2\n" +
                "Кость 6: Урон 12, Пробитие 1\n\n" +
                "Перезарядка: 5,1 сек (после уничтожения всех костей)\n" +
                "Меняют направление вращения после каждого перезапуска."
            },
            {
                WeaponName.RouletteBall,
                "Шарик рулетки, который летит в случайном направлении и отскакивает от стен экрана.\n\n" +
                "Шарик не наносит урон, но оставляет след из частичек, которые наносят урон врагам.\n\n" +
                "Частички существуют 0.5 секунды и уничтожаются при столкновении с врагами.\n\n" +
                "Стандартные отскоки: 10\n" +
                "Стандартный урон: 1\n\n" +
                "Прокачка: скорость, время жизни частичек, урон частичек."
            },
            {
                WeaponName.BeerBottle,
                "Бросает бутылки пива, которые разбиваются и создают липкие лужи.\n\n" +
                "Лужи наносят 3 урона врагам с интервалом 2 секунды.\n\n" +
                "Каждый враг имеет свой независимый таймер урона при нахождении в луже.\n\n" +
                "Прокачка: количество бутылок, время жизни лужи, скорость нанесения урона."
            },
            {
                WeaponName.Typhoon,
                "Мощные снаряды, летящие к ближайшему врагу.\n\n" +
                "Особенности:\n" +
                "Бесконечное пробитие врагов\n" +
                "Наносит урон каждые 0.3 секунды\n" +
                "Автоматическое наведение на ближайшего врага\n\n" +
                "Прокачка: урон, количество снарядов, скорость перезарядки."
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
                WeaponName.BeerBottle,
                WeaponName.Typhoon,

                WeaponName.BigLaser,
                WeaponName.MolotovCocktail,
                WeaponName.GoldenSword,
                WeaponName.GoldenTyphoon,
                WeaponName.EventHorizon,
                WeaponName.Breaker,
                WeaponName.WealthArtifact
            };

            // Инициализация описаний режимов игры
            _gameModeDescriptions = new Dictionary<GameMode, string>
            {
                {
                    GameMode.Easy,
                    "Идеально для новичков и обучения механике игры.\n\n" +
                    "Особенности:\n" +
                    "Характеристики врагов: стандартные\n" +
                    "Элитные враги: только 1 тип\n" +
                    "Для перехода на этап: 1 элитный враг\n" +
                    "Спаун врагов: стандартная скорость\n" +
                    "Победа: 8 этап, 1 элитный враг\n\n" +
                    "Рекомендуется для первого знакомства с игрой."
                },
                {
                    GameMode.Hard,
                    "Для опытных игроков. Баланс сложности и удовольствия.\n\n" +
                    "Особенности:\n" +
                    "Характеристики врагов: +50% к HP и урону\n" +
                    "Элитные враги: 2 типа (стандартный и усиленный)\n" +
                    "Для перехода на этап: 2 элитных врага\n" +
                    "Спаун врагов: в 2 раза чаще\n" +
                    "Победа: 8 этап, 2 элитных врага\n\n" +
                    "Стандартный игровой опыт для любителей вызова."
                },
                {
                    GameMode.Insane,
                    "Экстремальная сложность для настоящих мастеров.\n\n" +
                    "Особенности:\n" +
                    "Характеристики врагов: +100% к HP и урону\n" +
                    "Элитные враги: 2 типа (стандартный и усиленный)\n" +
                    "Для перехода на этап: 2 элитных врага\n" +
                    "Спаун врагов: в 4 раза чаще\n" +
                    "Победа: 8 этап, 2 элитных врага\n\n" +
                    "Только для экспертов, готовых к настоящему испытанию."
                },
                {
                    GameMode.Survival,
                    "Сражайтесь так долго, как сможете!\n\n" +
                    "Особенности:\n" +
                    "Начальная сложность: как в Лёгком режиме\n" +
                    "После 8 этапа: цикл повторяется с усилением\n" +
                    "Цикл 2: сложность как в Сложном режиме\n" +
                    "Цикл 3-4: сложность как в Безумном режиме\n" +
                    "Элитные враги: 2 типа\n" +
                    "Для перехода на этап: 2 элитных врага\n" +
                    "Победа: 8 этап в 4 цикле\n" +
                    "Уникальная музыка для каждого цикла!\n\n" +
                    "Проверьте свой предел выживания!"
                },
                {
                    GameMode.Custom,
                    "Режим в разработке. Скоро будет доступен!"
                },
                {
                    GameMode.Endless,
                    "Режим в разработке. Скоро будет доступен!"
                }
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
                _weaponSprites[WeaponName.BeerBottle] = content.Load<Texture2D>("Sprites/GUI/WeaponBottleBeer");
                _weaponSprites[WeaponName.Typhoon] = content.Load<Texture2D>("Sprites/GUI/WeaponTyphoon");
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

            // Область описания оружия (правая часть) - уменьшила ширину чтобы освободить место
            _descriptionRect = new Rectangle(
                screenWidth - DescriptionWidth - 50,
                180,
                DescriptionWidth - 50, // Уменьшил ширину на 50
                DescriptionHeight
            );

            // Полоса прокрутки для оружия
            _scrollBarRect = new Rectangle(
                _descriptionRect.X + _descriptionRect.Width - ScrollBarWidth,
                _descriptionRect.Y,
                ScrollBarWidth,
                _descriptionRect.Height
            );

            // Новые элементы: выбор режима игры (левая часть) - УВЕЛИЧЕННАЯ ШИРИНА
            _gameModeDropdownRect = new Rectangle(
                50,
                180,
                GameModeDropdownWidth, // Теперь 400 вместо 250
                GameModeDropdownHeight
            );

            _gameModeDescriptionRect = new Rectangle(
                50,
                _gameModeDropdownRect.Y + GameModeDropdownHeight + 20,
                GameModeDropdownWidth, // Такая же ширина как у выпадающего списка
                GameModeDescriptionHeight
            );

            // Полоса прокрутки для описания режима
            _gameModeScrollBarRect = new Rectangle(
                _gameModeDescriptionRect.X + _gameModeDescriptionRect.Width - ScrollBarWidth,
                _gameModeDescriptionRect.Y,
                ScrollBarWidth,
                _gameModeDescriptionRect.Height
            );

            // Инициализация прямоугольников для опций выпадающего списка - ТАКАЯ ЖЕ ШИРИНА
            _gameModeOptionRects = new Rectangle[6]; // Было 4, теперь 6
            for (int i = 0; i < 6; i++) // Было 4, теперь 6
            {
                _gameModeOptionRects[i] = new Rectangle(
                    _gameModeDropdownRect.X,
                    _gameModeDropdownRect.Y + GameModeDropdownHeight + i * GameModeOptionHeight,
                    GameModeDropdownWidth,
                    GameModeOptionHeight
                );
            }

            UpdateScrollThumb();
            UpdateGameModeScrollThumb();

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

        // Метод для получения прямоугольника отрисовки спрайта оружия в оригинальном размере
        private Rectangle GetWeaponSpriteRectangle()
        {
            if (!_weaponSprites.ContainsKey(_availableWeapons[_selectedWeaponIndex]))
                return _weaponCellRect;

            Texture2D weaponTexture = _weaponSprites[_availableWeapons[_selectedWeaponIndex]];

            // Используем оригинальные размеры текстуры
            int width = weaponTexture.Width;
            int height = weaponTexture.Height;

            // Центрируем спрайт в ячейке
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

        private void UpdateGameModeScrollThumb()
        {
            float contentHeight = GetGameModeDescriptionTextHeight();
            float visibleRatio = _gameModeDescriptionRect.Height / contentHeight;

            if (visibleRatio >= 1f)
            {
                _gameModeMaxScroll = 0f;
                _gameModeScrollPosition = 0f;
                _gameModeScrollThumbRect = Rectangle.Empty;
                return;
            }

            _gameModeMaxScroll = contentHeight - _gameModeDescriptionRect.Height;

            int thumbHeight = (int)(_gameModeDescriptionRect.Height * visibleRatio);
            thumbHeight = Math.Max(thumbHeight, ScrollThumbMinHeight);

            float scrollRatio = _gameModeScrollPosition / _gameModeMaxScroll;
            int thumbY = _gameModeScrollBarRect.Y + (int)((_gameModeDescriptionRect.Height - thumbHeight) * scrollRatio);

            _gameModeScrollThumbRect = new Rectangle(
                _gameModeScrollBarRect.X,
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

        private float GetGameModeDescriptionTextHeight()
        {
            string description = _gameModeDescriptions[_selectedGameMode];
            return MeasureTextHeight(description, _gameModeDescriptionRect.Width - ScrollBarWidth - 20);
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

            // Обработка прокрутки колесиком мыши для описания оружия
            if (_descriptionRect.Contains(_currentMouseState.Position))
            {
                _scrollPosition -= (_currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue) / 10f;
                _scrollPosition = MathHelper.Clamp(_scrollPosition, 0, _maxScroll);
                UpdateScrollThumb();
            }

            // Обработка прокрутки колесиком мыши для описания режима
            if (_gameModeDescriptionRect.Contains(_currentMouseState.Position) && !_isDropdownOpen)
            {
                _gameModeScrollPosition -= (_currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue) / 10f;
                _gameModeScrollPosition = MathHelper.Clamp(_gameModeScrollPosition, 0, _gameModeMaxScroll);
                UpdateGameModeScrollThumb();
            }

            // Обработка перетаскивания ползунка для оружия
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (_scrollThumbRect.Contains(_currentMouseState.Position) && !_isScrolling)
                {
                    _isScrolling = true;
                }
                else if (_gameModeScrollThumbRect.Contains(_currentMouseState.Position) && !_isGameModeScrolling && !_isDropdownOpen)
                {
                    _isGameModeScrolling = true;
                }
            }
            else
            {
                _isScrolling = false;
                _isGameModeScrolling = false;
            }

            if (_isScrolling)
            {
                float relativeY = _currentMouseState.Y - _scrollBarRect.Y;
                float scrollRatio = MathHelper.Clamp(relativeY / (_scrollBarRect.Height - _scrollThumbRect.Height), 0, 1);
                _scrollPosition = scrollRatio * _maxScroll;
                UpdateScrollThumb();
            }

            if (_isGameModeScrolling)
            {
                float relativeY = _currentMouseState.Y - _gameModeScrollBarRect.Y;
                float scrollRatio = MathHelper.Clamp(relativeY / (_gameModeScrollBarRect.Height - _gameModeScrollThumbRect.Height), 0, 1);
                _gameModeScrollPosition = scrollRatio * _gameModeMaxScroll;
                UpdateGameModeScrollThumb();
            }

            // Обработка кликов по кнопкам
            if (_currentMouseState.LeftButton == ButtonState.Released &&
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                // Обработка выбора режима игры
                if (_gameModeDropdownRect.Contains(_currentMouseState.Position))
                {
                    _isDropdownOpen = !_isDropdownOpen;
                }
                else if (_isDropdownOpen)
                {
                    // Проверяем клик по опциям выпадающего списка
                    for (int i = 0; i < _gameModeOptionRects.Length; i++)
                    {
                        if (_gameModeOptionRects[i].Contains(_currentMouseState.Position))
                        {
                            _selectedGameMode = (GameMode)i;
                            _isDropdownOpen = false;
                            _gameModeScrollPosition = 0f; // Сброс прокрутки при смене режима
                            UpdateGameModeScrollThumb();
                            break;
                        }
                    }

                    // Если кликнули вне выпадающего списка, закрываем его
                    if (!IsMouseOverGameModeDropdownArea())
                    {
                        _isDropdownOpen = false;
                    }
                }

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
                    return GameState.Loading; // Меняем на Loading вместо Playing
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

        private bool IsMouseOverGameModeDropdownArea()
        {
            Point mousePos = _currentMouseState.Position;

            // Проверяем основную кнопку выбора режима
            if (_gameModeDropdownRect.Contains(mousePos))
                return true;

            // Если выпадающий список открыт, проверяем все опции
            if (_isDropdownOpen)
            {
                foreach (var optionRect in _gameModeOptionRects)
                {
                    if (optionRect.Contains(mousePos))
                        return true;
                }
            }

            return false;
        }

        private string GetGameModeDisplayName(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Easy: return "ЛЕГКИЙ";
                case GameMode.Hard: return "СЛОЖНЫЙ";
                case GameMode.Insane: return "БЕЗУМНЫЙ";
                case GameMode.Survival: return "ВЫЖИВАНИЕ";
                case GameMode.Custom: return "СВОЙ РЕЖИМ";
                case GameMode.Endless: return "БЕСКОНЕЧНЫЙ";
                default: return "НЕИЗВЕСТНО";
            }
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

            // Отрисовка выбора режима игры (ВСЕГДА ПЕРВЫМ - под выпадающим списком)
            DrawGameModeSelection(spriteBatch);

            // Отрисовка выпадающего списка поверх всего (ЕСЛИ ОТКРЫТ)
            if (_isDropdownOpen)
            {
                DrawGameModeDropdown(spriteBatch);
            }

            // Кнопки
            DrawButton(spriteBatch, _startButtonRect, "НАЧАТЬ ИГРУ", Color.Green);
            DrawButton(spriteBatch, _backButtonRect, "НАЗАД", Color.Gray);
        }

        private void DrawGameModeSelection(SpriteBatch spriteBatch)
        {
            // Заголовок выбора режима
            string modeTitle = "ВЫБОР РЕЖИМА ИГРЫ";
            Vector2 modeTitleSize = _font.MeasureString(modeTitle);
            Vector2 modeTitlePos = new Vector2(
                _gameModeDropdownRect.Center.X - modeTitleSize.X / 2,
                _gameModeDropdownRect.Y - modeTitleSize.Y - 10
            );
            spriteBatch.DrawString(_font, modeTitle, modeTitlePos, Color.Yellow);

            // Основная кнопка выбора режима
            Color dropdownColor = _gameModeDropdownRect.Contains(_currentMouseState.Position) ?
                Color.LightGray : Color.Gray;

            spriteBatch.Draw(_debugTexture, _gameModeDropdownRect, dropdownColor);

            // Текст выбранного режима
            string selectedModeText = GetGameModeDisplayName(_selectedGameMode);
            Vector2 textSize = _font.MeasureString(selectedModeText);
            Vector2 textPos = new Vector2(
                _gameModeDropdownRect.Center.X - textSize.X / 2,
                _gameModeDropdownRect.Center.Y - textSize.Y / 2
            );
            spriteBatch.DrawString(_font, selectedModeText, textPos, Color.White);

            // Стрелка вниз (индикатор выпадающего списка)
            int arrowSize = 10;
            Rectangle arrowRect = new Rectangle(
                _gameModeDropdownRect.Right - arrowSize - 5,
                _gameModeDropdownRect.Center.Y - arrowSize / 2,
                arrowSize,
                arrowSize
            );
            DrawTriangle(spriteBatch, arrowRect, Color.White, _isDropdownOpen);

            // Описание выбранного режима (ПОД выпадающим списком)
            DrawGameModeDescription(spriteBatch);
        }

        private void DrawGameModeDropdown(SpriteBatch spriteBatch)
        {
            // Фон для всего выпадающего списка (полупрозрачный)
            Rectangle dropdownBackground = new Rectangle(
                _gameModeDropdownRect.X,
                _gameModeDropdownRect.Y + _gameModeDropdownRect.Height,
                _gameModeDropdownRect.Width,
                GameModeOptionHeight * 6 // Было 4, теперь 6
            );

            spriteBatch.Draw(_debugTexture, dropdownBackground, Color.Black * 0.9f);

            for (int i = 0; i < 6; i++) // Было 4, теперь 6
            {
                GameMode mode = (GameMode)i;
                Rectangle optionRect = _gameModeOptionRects[i];

                Color optionColor = optionRect.Contains(_currentMouseState.Position) ?
                    Color.LightBlue : Color.DarkGray;

                spriteBatch.Draw(_debugTexture, optionRect, optionColor);

                string modeText = GetGameModeDisplayName(mode);
                Vector2 modeTextSize = _font.MeasureString(modeText);
                Vector2 modeTextPos = new Vector2(
                    optionRect.Center.X - modeTextSize.X / 2,
                    optionRect.Center.Y - modeTextSize.Y / 2
                );

                Color textColor = mode == _selectedGameMode ? Color.Yellow : Color.White;

                // Для режимов в разработке делаем текст серым
                if (mode == GameMode.Custom || mode == GameMode.Endless)
                {
                    textColor = Color.Gray;
                }

                spriteBatch.DrawString(_font, modeText, modeTextPos, textColor);

                // Рамка для опции
                DrawRectangle(spriteBatch, optionRect, Color.White);
            }
        }

        private void DrawGameModeDescription(SpriteBatch spriteBatch)
        {
            // Фон области описания режима
            spriteBatch.Draw(_debugTexture, _gameModeDescriptionRect, Color.DarkSlateBlue * 0.8f);

            // Рамка
            DrawRectangle(spriteBatch, _gameModeDescriptionRect, Color.White);

            // Полоса прокрутки (если нужно)
            if (_gameModeMaxScroll > 0)
            {
                spriteBatch.Draw(_debugTexture, _gameModeScrollBarRect, Color.Gray);
                spriteBatch.Draw(_debugTexture, _gameModeScrollThumbRect, Color.LightGray);
            }

            // Текст описания режима с учетом прокрутки
            string description = _gameModeDescriptions[_selectedGameMode];
            Rectangle textArea = new Rectangle(
                _gameModeDescriptionRect.X + 10,
                _gameModeDescriptionRect.Y + 10,
                _gameModeDescriptionRect.Width - (_gameModeMaxScroll > 0 ? ScrollBarWidth + 5 : 20),
                _gameModeDescriptionRect.Height - 20
            );

            DrawTextWithNewlines(spriteBatch, description, textArea, _gameModeScrollPosition);
        }

        private void DrawTriangle(SpriteBatch spriteBatch, Rectangle rect, Color color, bool pointingDown)
        {
            // Простая реализация треугольника через линии
            Vector2 top, left, right;

            if (pointingDown)
            {
                // Стрелка вверх (когда список открыт)
                top = new Vector2(rect.Center.X, rect.Top);
                left = new Vector2(rect.Left, rect.Bottom);
                right = new Vector2(rect.Right, rect.Bottom);
            }
            else
            {
                // Стрелка вниз (когда список закрыт)
                top = new Vector2(rect.Center.X, rect.Bottom);
                left = new Vector2(rect.Left, rect.Top);
                right = new Vector2(rect.Right, rect.Top);
            }

            // Рисуем треугольник с помощью debug texture
            spriteBatch.Draw(_debugTexture, new Rectangle((int)top.X, (int)top.Y, 1, 1), color);
            spriteBatch.Draw(_debugTexture, new Rectangle((int)left.X, (int)left.Y, 1, 1), color);
            spriteBatch.Draw(_debugTexture, new Rectangle((int)right.X, (int)right.Y, 1, 1), color);
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
                _descriptionRect.X + 15,
                _descriptionRect.Y + 15,
                _descriptionRect.Width - ScrollBarWidth - 25,
                _descriptionRect.Height - 30
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

                // Рисуем часть слова если она видима
                if (currentPos.Y >= minY && currentPos.Y + lineHeight <= maxY)
                {
                    spriteBatch.DrawString(_font, linePart, new Vector2(textArea.X, currentPos.Y), Color.White);
                }

                currentPos.Y += lineHeight;
                if (currentPos.Y > maxY) return "";

                remainingWord = remainingWord.Substring(charsToTake);
            }

            return remainingWord;
        }
    }
}