using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Gamedata.Managers
{
    public class RouletteManager
    {
        private Player _player;
        private List<Weapon> _weapons;

        // Переменные состояния для надежного ввода с клавиатуры и мыши
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        // Используем статический Random из Game1
        private Random _random => Game1.Random;

        public List<UpgradeOption> CurrentOptions { get; private set; } = new List<UpgradeOption>();
        public bool IsVisible { get; private set; }

        // ДОБАВЛЕНО: Свойство IsActive для отслеживания активности рулетки
        public bool IsActive => IsVisible;

        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public RouletteManager(Player player, List<Weapon> allWeapons, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _player = player;
            _weapons = allWeapons;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
        }

        public void StartRoulette()
        {
            GenerateRouletteOptions();
            IsVisible = true;
            // Явно указываем полное пространство имен для GameState
            Game1.CurrentState = GameState.Roulette;
        }

        public void GenerateRouletteOptions()
        {
            CurrentOptions.Clear();
            List<UpgradeOption> pool = new List<UpgradeOption>();

            // Проверяем, есть ли еще обычные оружия для получения
            bool hasAllRegularWeapons = _weapons.Any(w => w is PlayingCards) &&
                                      _weapons.Any(w => w is CasinoChips) &&
                                      _weapons.Any(w => w is GoldenBullet) &&
                                      _weapons.Any(w => w is StickyBomb) &&
                                      _weapons.Any(w => w is DiceWeapon) &&
                                      _weapons.Any(w => w is RouletteBall);

            // Если все обычные оружия получены, показываем только легендарные
            if (hasAllRegularWeapons)
            {
                // Легендарные оружия (гарантированно)
                if (!_weapons.Any(w => w is GoldenSword))
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Золотой меч [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: золотые мечи, летящие с автонаводкой.",
                        ApplyUpgrade = () => _weapons.Add(new GoldenSword(_player))
                    });
                }

                if (!_weapons.Any(w => w is MolotovCocktail))
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Коктейль Молотова [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: бросает бутылки, создающие огненные зоны.",
                        ApplyUpgrade = () => _weapons.Add(new MolotovCocktail(_player))
                    });
                }

                if (!_weapons.Any(w => w is BigLaser))
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Большой лазер [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: мощный лазер, который автоматически наводится на врагов.",
                        ApplyUpgrade = () => _weapons.Add(new BigLaser(_player))
                    });
                }

                if (!_weapons.Any(w => w is GoldenTyphoon))
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Золотой Тайфун [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: снаряды, летящие по 8 направлениям с огромной скоростью вращения.",
                        ApplyUpgrade = () => _weapons.Add(new GoldenTyphoon(_player))
                    });
                }

                if (!_weapons.Any(w => w is EventHorizon))
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Горизонт Событий [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: звезды, вращающиеся по расширяющимся кольцам с самонаведением на врагов.",
                        ApplyUpgrade = () => _weapons.Add(new EventHorizon(_player))
                    });
                }

                if (!_weapons.Any(w => w is Breaker) && !pool.Any(o => o.Title.Contains("Разрушитель")) && pool.Count < 3)
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Разрушитель [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: массивный меч, совершающий сокрушительные взмахи вокруг игрока.",
                        ApplyUpgrade = () => _weapons.Add(new Breaker(_player))
                    });
                }
            }
            else
            {
                // Смешанный пул: обычные оружия + 10% шанс на легендарные
                List<UpgradeOption> regularPool = new List<UpgradeOption>();
                List<UpgradeOption> legendaryPool = new List<UpgradeOption>();

                // Обычные оружия
                if (!_weapons.Any(w => w is StickyBomb))
                {
                    regularPool.Add(new UpgradeOption
                    {
                        Title = "Липкая бомба",
                        Description = "Бомбы, которые прилипают к врагам и взрываются через время.",
                        ApplyUpgrade = () => _weapons.Add(new StickyBomb(_player))
                    });
                }

                if (!_weapons.Any(w => w is PlayingCards))
                {
                    regularPool.Add(new UpgradeOption
                    {
                        Title = "Игральные карты",
                        Description = "Игральные карты, которые пробивают врагов.",
                        ApplyUpgrade = () => _weapons.Add(new PlayingCards(_player))
                    });
                }

                if (!_weapons.Any(w => w is CasinoChips))
                {
                    regularPool.Add(new UpgradeOption
                    {
                        Title = "Фишки казино",
                        Description = "Фишки, которые отскакивают между врагами.",
                        ApplyUpgrade = () => _weapons.Add(new CasinoChips(_player))
                    });
                }

                if (!_weapons.Any(w => w is GoldenBullet))
                {
                    regularPool.Add(new UpgradeOption
                    {
                        Title = "Золотая пуля",
                        Description = "Точные золотые пули, которые могут отталкивать врагов.",
                        ApplyUpgrade = () => _weapons.Add(new GoldenBullet(_player))
                    });
                }

                if (!_weapons.Any(w => w is RouletteBall))
                {
                    regularPool.Add(new UpgradeOption
                    {
                        Title = "Рулетка",
                        Description = "Шарик рулетки летит случайно и отскакивает от стен.\n" +
                                     "Оставляет след из частичек, которые наносят урон и уничтожаются при столкновении.",
                        ApplyUpgrade = () => _weapons.Add(new RouletteBall(_player))
                    });
                }

                if (!_weapons.Any(w => w is DiceWeapon))
                {
                    regularPool.Add(new UpgradeOption
                    {
                        Title = "Игральные кости",
                        Description = "Кости, вращающиеся вокруг игрока и имеют разное количество пробития и урона",
                        ApplyUpgrade = () => _weapons.Add(new DiceWeapon(_player))
                    });
                }

                if (!_weapons.Any(w => w is BeerBottle))
                {
                    regularPool.Add(new UpgradeOption
                    {
                        Title = "Бутылка пива",
                        Description = "Бутылки пива, создающие липкие лужи с периодическим уроном.",
                        ApplyUpgrade = () => _weapons.Add(new BeerBottle(_player))
                    });
                }

                // Легендарные оружия (10% шанс появления каждого)
                if (!_weapons.Any(w => w is GoldenSword) && _random.NextDouble() < 0.1)
                {
                    legendaryPool.Add(new UpgradeOption
                    {
                        Title = "Золотой меч [ЛЕГЕНДАРНЫЙ]",
                        Description = "Золотые мечи, летящие с автонаводкой.",
                        ApplyUpgrade = () => _weapons.Add(new GoldenSword(_player))
                    });
                }

                if (!_weapons.Any(w => w is MolotovCocktail) && _random.NextDouble() < 0.1)
                {
                    legendaryPool.Add(new UpgradeOption
                    {
                        Title = "Коктейль Молотова [ЛЕГЕНДАРНЫЙ]",
                        Description = "Бросает бутылки, создающие огненные зоны.",
                        ApplyUpgrade = () => _weapons.Add(new MolotovCocktail(_player))
                    });
                }

                if (!_weapons.Any(w => w is BigLaser) && _random.NextDouble() < 0.1)
                {
                    legendaryPool.Add(new UpgradeOption
                    {
                        Title = "Большой лазер [ЛЕГЕНДАРНЫЙ]",
                        Description = "Мощный лазер, который автоматически наводится и имеет приоритет атаки на врагов с большим хп.",
                        ApplyUpgrade = () => _weapons.Add(new BigLaser(_player))
                    });
                }

                if (!_weapons.Any(w => w is GoldenTyphoon) && _random.NextDouble() < 0.1)
                {
                    legendaryPool.Add(new UpgradeOption
                    {
                        Title = "Золотой Тайфун [ЛЕГЕНДАРНЫЙ]",
                        Description = "Снаряды, летящие по 8 направлениям с огромной скоростью вращения и с бесконечным пробитием.",
                        ApplyUpgrade = () => _weapons.Add(new GoldenTyphoon(_player))
                    });
                }

                if (!_weapons.Any(w => w is EventHorizon) && _random.NextDouble() < 0.1)
                {
                    legendaryPool.Add(new UpgradeOption
                    {
                        Title = "Горизонт Событий [ЛЕГЕНДАРНЫЙ]",
                        Description = "Звезды, вращающиеся по расширяющимся кольцам с самонаведением на врагов.",
                        ApplyUpgrade = () => _weapons.Add(new EventHorizon(_player))
                    });
                }

                if (!_weapons.Any(w => w is Breaker) && _random.NextDouble() < 0.1)
                {
                    legendaryPool.Add(new UpgradeOption
                    {
                        Title = "Разрушитель [ЛЕГЕНДАРНЫЙ]",
                        Description = "Массивный меч, совершающий сокрушительные взмахи вокруг игрока и наносит дополнительный урон врагам с полным хп",
                        ApplyUpgrade = () => _weapons.Add(new Breaker(_player))
                    });
                }

                // Объединяем пулы, сначала легендарные (если есть), затем обычные
                pool.AddRange(legendaryPool);
                pool.AddRange(regularPool);
            }

            // --- ИСПРАВЛЕНИЕ: Гарантируем, что всегда будет 3 варианта ---

            // Если доступных опций меньше 3, добавляем легендарные оружия (если они еще не добавлены)
            if (pool.Count < 3)
            {
                // Проверяем и добавляем недостающие легендарные оружия
                if (!_weapons.Any(w => w is GoldenSword) && !pool.Any(o => o.Title.Contains("Золотой меч")))
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Золотой меч [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: золотые мечи, летящие с автонаводкой.",
                        ApplyUpgrade = () => _weapons.Add(new GoldenSword(_player))
                    });
                }

                if (!_weapons.Any(w => w is MolotovCocktail) && !pool.Any(o => o.Title.Contains("Коктейль Молотова")) && pool.Count < 3)
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Коктейль Молотова [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: бросает бутылки, создающие огненные зоны.",
                        ApplyUpgrade = () => _weapons.Add(new MolotovCocktail(_player))
                    });
                }

                if (!_weapons.Any(w => w is BigLaser) && !pool.Any(o => o.Title.Contains("Большой лазер")) && pool.Count < 3)
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Большой лазер [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: мощный лазер, который автоматически наводится на врагов.",
                        ApplyUpgrade = () => _weapons.Add(new BigLaser(_player))
                    });
                }

                if (!_weapons.Any(w => w is GoldenTyphoon) && !pool.Any(o => o.Title.Contains("Золотой Тайфун")) && pool.Count < 3)
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Золотой Тайфун [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: снаряды, летящие по 8 направлениям с огромной скоростью вращения.",
                        ApplyUpgrade = () => _weapons.Add(new GoldenTyphoon(_player))
                    });
                }

                // === ДОБАВЛЕНО: Горизонт Событий (резервное добавление) ===
                if (!_weapons.Any(w => w is EventHorizon) && !pool.Any(o => o.Title.Contains("Горизонт Событий")) && pool.Count < 3)
                {
                    pool.Add(new UpgradeOption
                    {
                        Title = "Горизонт Событий [ЛЕГЕНДАРНЫЙ]",
                        Description = "Добавляет новое легендарное оружие: звезды, вращающиеся по расширяющимся кольцам с самонаведением на врагов.",
                        ApplyUpgrade = () => _weapons.Add(new EventHorizon(_player))
                    });
                }
            }

            // Если все равно меньше 3 опций, добавляем кнопку-пустышку
            while (pool.Count < 3)
            {
                pool.Add(new UpgradeOption
                {
                    Title = "Пропустить выбор",
                    Description = "Продолжить без получения нового оружия. Удача улыбнется в следующий раз!",
                    ApplyUpgrade = () => { /* Ничего не делаем - пустышка */ }
                });
            }

            // Выбираем 3 случайных уникальных опции
            int count = Math.Min(3, pool.Count);

            if (pool.Count <= count)
            {
                CurrentOptions.AddRange(pool);
            }
            else
            {
                HashSet<int> indices = new HashSet<int>();
                while (indices.Count < count)
                {
                    indices.Add(_random.Next(0, pool.Count));
                }

                foreach (int index in indices)
                {
                    CurrentOptions.Add(pool[index]);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            KeyboardState currentKs = Keyboard.GetState();
            MouseState currentMs = Mouse.GetState();

            bool choiceMade = false;

            // Определяем размеры и позиции для расчета кликов
            Vector2 startPosition = new Vector2(50, 50);
            const int boxHeight = 150;
            const int boxSpacing = 20;
            int boxWidth = _graphicsDevice.Viewport.Width - 100;

            // --- 1. Логика выбора с помощью клавиш D1-D3 (только при первом нажатии) ---
            if (currentKs.IsKeyDown(Keys.D1) && !_previousKeyboardState.IsKeyDown(Keys.D1) && CurrentOptions.Count > 0)
            {
                ApplyChoice(0);
                choiceMade = true;
            }
            else if (currentKs.IsKeyDown(Keys.D2) && !_previousKeyboardState.IsKeyDown(Keys.D2) && CurrentOptions.Count > 1)
            {
                ApplyChoice(1);
                choiceMade = true;
            }
            else if (currentKs.IsKeyDown(Keys.D3) && !_previousKeyboardState.IsKeyDown(Keys.D3) && CurrentOptions.Count > 2)
            {
                ApplyChoice(2);
                choiceMade = true;
            }

            // --- 2. Логика выбора с помощью мыши (при клике) ---
            if (!choiceMade && currentMs.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                Point mousePosition = currentMs.Position;

                for (int i = 0; i < CurrentOptions.Count; i++)
                {
                    Rectangle box = new Rectangle(
                        (int)startPosition.X,
                        (int)startPosition.Y + i * boxHeight + i * boxSpacing,
                        boxWidth,
                        boxHeight
                    );

                    if (box.Contains(mousePosition))
                    {
                        ApplyChoice(i);
                        choiceMade = true;
                        break;
                    }
                }
            }

            // Обновляем состояния для следующего кадра
            _previousKeyboardState = currentKs;
            _previousMouseState = currentMs;
        }

        public void ApplyChoice(int index)
        {
            CurrentOptions[index].ApplyUpgrade.Invoke();
            IsVisible = false;
            // Явно указываем полное пространство имен для GameState
            Game1.CurrentState = GameState.Playing;
            CurrentOptions.Clear();
        }
    }
}