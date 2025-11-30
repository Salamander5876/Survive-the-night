using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// Убедитесь, что все эти пространства имен существуют
using Survive_the_night.Entities;
using Survive_the_night.Entities.Enemies.Elite;
using Survive_the_night.Entities.Enemies.Regular;
using Survive_the_night.Gamedata.Config.Items;
using Survive_the_night.Gamedata.Config.ItemSystem.Items;
using Survive_the_night.Gamedata.Config.ItemSystem.Renderers;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using Survive_the_night.Gamedata.Config.WeaponSystem.Weapons;
using Survive_the_night.Gamedata.Managers;
using Survive_the_night.Scripts.Interfaces;
using Survive_the_night.Scripts.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Survive_the_night
{
    /// <summary>
    /// Перечисление состояний игры. Определено ТОЛЬКО здесь, в корневом пространстве имен.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        StartMenu,
        Playing,
        LevelUp,
        GameOver,
        Roulette,
        BonusShop,
        Paused,
        Loading,
        ExitGame,
        Victory
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // --- СТАТИЧЕСКИЕ ПОЛЯ ДЛЯ ГЛОБАЛЬНОГО ДОСТУПА ---
        public static System.Random Random { get; private set; } = new System.Random();
        public static Vector2 WorldSize { get; private set; }
        public static List<Enemy> CurrentEnemies { get; private set; }

        public MusicsManager GetMusicManager()
        {
            return _musicManager;
        }

        /// <summary>
        /// Глобальное статическое поле, которое используется для управления состоянием игры из других классов.
        /// </summary>
        public static GameState CurrentState = GameState.MainMenu;
        // ------------------------------------------------

        // Game State Management
        private GameState _currentGameState;
        private MainMenuRenderer _mainMenu;
        private StartMenu _startMenu;
        private RouletteManager _rouletteManager;
        private RouletteMenu _rouletteMenu;
        private MusicsManager _musicManager;
        private SoundManager _soundManager;
        private LevelManager _levelManager;
        private GameHUD _gameHUD;
        private DifficultyManager _difficultyManager;

        // Game World Entities
        private Player _player;
        private SpawnManager _spawnManager;
        private Camera _camera;
        private List<Enemy> _enemies = new List<Enemy>();

        // Новая система предметов
        private ItemManager _itemManager;

        // Магазин бонусов
        private BonusShopMenu _bonusShop;
        private BonusShopInterface _bonusShopInterface;

        // World Generation
        private WorldGeneration _worldGeneration;

        // HUD Data
        private float _survivalTime = 0f;
        private int _killCount = 0;

        // Weapons and Upgrades
        private List<Weapon> _weapons = new List<Weapon>();
        private LevelUpMenu _levelUpMenu;
        private LevelUpMenuRenderer _levelUpMenuRenderer;

        // Content
        private Texture2D _debugTexture;
        private SpriteFont _font;
        // Textures
        private Texture2D _heartTexture;
        private Texture2D _goldenHeartTexture;

        private Texture2D _beerBottleTexture;
        private Texture2D _bottleTexture;
        private Texture2D _puddleTexture;

        // Стартовое оружие
        private WeaponName _selectedStartingWeapon = WeaponName.PlayingCards;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        // Меню паузы
        private PauseMenu _pauseMenu;

        // Экран загрузки
        private LoadingScreen _loadingScreen;

        // Экран Game Over
        private GameOverScreen _gameOverScreen;

        // Экран победы
        private VictoryScreen _victoryScreen;

        // Статическое свойство для доступа из GameOverScreen
        public static Game1 Instance { get; private set; }

        // Свойство для доступа к времени выживания
        public float SurvivalTime => _survivalTime;

        public Game1()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            _currentGameState = GameState.MainMenu;
            Game1.CurrentState = GameState.MainMenu;

            Vector2 initialPlayerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
            );

            _player = new Player(initialPlayerPosition);
            _camera = new Camera(_player, GraphicsDevice.Viewport);

            // Создаем DifficultyManager
            _difficultyManager = new DifficultyManager();

            _soundManager = SoundManager.Instance;
            _musicManager = new MusicsManager();
            _levelManager = new LevelManager(_difficultyManager);

            _spawnManager = new SpawnManager(_enemies, _player, _camera, GraphicsDevice.Viewport, _levelManager, _difficultyManager);

            _itemManager = new ItemManager(_player);
            _bonusShop = new BonusShopMenu(_player, _itemManager);

            Window.Title = "Casino Survivors";

            CurrentEnemies = _enemies;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            // Загрузка шрифта
            _font = Content.Load<SpriteFont>("Fonts/Default");

            // Создание HUD
            _gameHUD = new GameHUD(_player, _levelManager, _font, _debugTexture, GraphicsDevice.Viewport);

            // ЗАМЕНА: Загружаем текстуру пола через LevelManager
            Texture2D casinoFloorTexture = _levelManager.LoadCurrentLevelFloorTexture(Content);
            _worldGeneration = new WorldGeneration(casinoFloorTexture, _camera, GraphicsDevice.Viewport);

            // Загрузка музыки и звуков
            _musicManager.LoadContent(Content);
            _soundManager.LoadContent(Content); // SoundManager загрузит все звуки автоматически, включая звуки предметов

            // --- ЗАГРУЗКА ТЕКСТУР ДЛЯ ОРУЖИЙ (звуки теперь через SoundManager) ---

            // Игральные карты
            var cardTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/Card1");
            var cardTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/Card2");
            var cardTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/Card3");
            var cardTexture4 = Content.Load<Texture2D>("Sprites/Projectiles/Card4");
            WeaponManager.LoadWeaponTextures(WeaponName.PlayingCards, cardTexture1, cardTexture2, cardTexture3, cardTexture4);

            // Золотые пули
            var bulletTexture = Content.Load<Texture2D>("Sprites/Projectiles/Bullet");
            WeaponManager.LoadWeaponTextures(WeaponName.GoldenBullet, bulletTexture);

            // Фишки казино
            var chipTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsBlue");
            var chipTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsGreen");
            var chipTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsRed");
            WeaponManager.LoadWeaponTextures(WeaponName.CasinoChips, chipTexture1, chipTexture2, chipTexture3);

            // Золотой меч
            var swordTexture = Content.Load<Texture2D>("Sprites/Projectiles/GoldenSword");
            WeaponManager.LoadWeaponTextures(WeaponName.GoldenSword, swordTexture);

            // Молотов
            var molotovTexture = Content.Load<Texture2D>("Sprites/Projectiles/Molotov");
            var molotovFireTexture = Content.Load<Texture2D>("Sprites/Projectiles/MolotovFire");
            MolotovCocktail.SetTextures(molotovTexture, molotovFireTexture);
            WeaponManager.LoadWeaponTextures(WeaponName.MolotovCocktail, molotovTexture);

            // Большой лазер
            var bigLaserTexture = Content.Load<Texture2D>("Sprites/Projectiles/BigLaser");
            WeaponManager.LoadWeaponTextures(WeaponName.BigLaser, bigLaserTexture);
            BigLaserProjectile.SetDefaultTexture(bigLaserTexture);

            // Золотой Тайфун
            var goldenTyphoonTexture = Content.Load<Texture2D>("Sprites/Projectiles/GoldenTyphoon");
            WeaponManager.LoadWeaponTextures(WeaponName.GoldenTyphoon, goldenTyphoonTexture);
            GoldenTyphoonProjectile.SetDefaultTexture(goldenTyphoonTexture);

            // Горизонт Событий
            var eventHorizonTexture = Content.Load<Texture2D>("Sprites/Projectiles/EventHorizonStar");
            WeaponManager.LoadWeaponTextures(WeaponName.EventHorizon, eventHorizonTexture);
            EventHorizonStarProjectile.SetDefaultTexture(eventHorizonTexture);

            // Тайфун
            var typhoonTexture = Content.Load<Texture2D>("Sprites/Projectiles/Typhoon");
            WeaponManager.LoadWeaponTextures(WeaponName.Typhoon, typhoonTexture);
            TyphoonProjectile.SetDefaultTexture(typhoonTexture);

            // Установка текстур по умолчанию для проектов
            PlayingCard.SetDefaultTexture(cardTexture1);
            GoldenBulletProjectile.SetDefaultTexture(bulletTexture);
            GoldenSwordProjectile.SetDefaultTexture(swordTexture);
            CasinoChip.SetDefaultTexture(chipTexture1);

            // Загрузка текстур для предметов
            _heartTexture = Content.Load<Texture2D>("Sprites/Items/Heart");
            _goldenHeartTexture = Content.Load<Texture2D>("Sprites/Items/GoldenHeart");
            Texture2D coinTexture = Content.Load<Texture2D>("Sprites/Items/GoldMoney");
            Texture2D experienceOrbTexture = Content.Load<Texture2D>("Sprites/Items/ExperienceOrb");
            Texture2D dynamiteTexture = Content.Load<Texture2D>("Sprites/Items/Dynamite");
            Texture2D magnetTexture = Content.Load<Texture2D>("Sprites/Items/Magnet");

            // Устанавливаем ВСЕ текстуры для ItemManager через один метод
            _itemManager.SetTextures(coinTexture, experienceOrbTexture, dynamiteTexture, magnetTexture, _heartTexture, _goldenHeartTexture);
            _itemManager.SetDebugTexture(_debugTexture);

            var dynamiteExplosionTexture = Content.Load<Texture2D>("Sprites/Projectiles/DynamiteExplosion");
            DynamiteExplosion.SetTexture(dynamiteExplosionTexture);

            // Липкая бомба
            var stickyBombTexture = Content.Load<Texture2D>("Sprites/Projectiles/StickyBomb");
            var bombExplosionTexture = Content.Load<Texture2D>("Sprites/Projectiles/BombExplosion");
            WeaponManager.LoadWeaponTextures(WeaponName.StickyBomb, stickyBombTexture);
            StickyBombProjectile.SetTextures(stickyBombTexture, bombExplosionTexture);

            // Игральные кости
            var diceTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/Dice1");
            var diceTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/Dice2");
            var diceTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/Dice3");
            var diceTexture4 = Content.Load<Texture2D>("Sprites/Projectiles/Dice4");
            var diceTexture5 = Content.Load<Texture2D>("Sprites/Projectiles/Dice5");
            var diceTexture6 = Content.Load<Texture2D>("Sprites/Projectiles/Dice6");
            WeaponManager.LoadWeaponTextures(WeaponName.Dice,
                diceTexture1, diceTexture2, diceTexture3,
                diceTexture4, diceTexture5, diceTexture6);

            // Устанавливаем текстуры для DiceProjectile
            DiceProjectile.SetTextures(diceTexture1, diceTexture2, diceTexture3, diceTexture4, diceTexture5, diceTexture6);

            // Рулетка
            var rouletteBallTexture = Content.Load<Texture2D>("Sprites/Projectiles/RouletteBall");
            WeaponManager.LoadWeaponTextures(WeaponName.RouletteBall, rouletteBallTexture);
            RouletteBallProjectile.SetDefaultTexture(rouletteBallTexture);

            // Beer Bottle
            var beerBottleTexture = Content.Load<Texture2D>("Sprites/Projectiles/BottleBeer");
            var beerPuddleTexture = Content.Load<Texture2D>("Sprites/Projectiles/PuddleBeer");
            BeerBottle.SetTexturesAndSounds(beerBottleTexture, beerPuddleTexture, null, null); // Звуки теперь null
            WeaponManager.LoadWeaponTextures(WeaponName.BeerBottle, beerBottleTexture);

            _beerBottleTexture = beerBottleTexture;
            _bottleTexture = beerBottleTexture;
            _puddleTexture = beerPuddleTexture;

            // Разрушитель
            var breakerTexture = Content.Load<Texture2D>("Sprites/Projectiles/BreakerBlade");
            WeaponManager.LoadWeaponTextures(WeaponName.Breaker, breakerTexture);
            BreakerBladeProjectile.SetDefaultTexture(breakerTexture);

            // Загружаем все текстуры частичек
            for (int i = 1; i <= 15; i++)
            {
                var particleTexture = Content.Load<Texture2D>($"Sprites/Projectiles/Roulette{i}");
                RouletteParticle.AddParticleTexture(particleTexture);
            }

            // Загрузка текстур для StartMenu
            var weaponCellTexture = Content.Load<Texture2D>("Sprites/GUI/CellWeapon");
            var upButtonTexture = Content.Load<Texture2D>("Sprites/GUI/UpButton");
            var downButtonTexture = Content.Load<Texture2D>("Sprites/GUI/DownButton");

            // Инициализация менеджеров и меню
            _mainMenu = new MainMenuRenderer(GraphicsDevice, _debugTexture, _font);
            _startMenu = new StartMenu(GraphicsDevice, _debugTexture, _font);
            _startMenu.LoadContent(weaponCellTexture, upButtonTexture, downButtonTexture);

            // ЗАГРУЗКА СПРАЙТОВ ОРУЖИЙ ДЛЯ СТАРТОВОГО МЕНЮ
            _startMenu.LoadWeaponSprites(Content);

            _levelUpMenu = new LevelUpMenu(_player, _weapons, GraphicsDevice, _debugTexture, _font);
            _levelUpMenuRenderer = new LevelUpMenuRenderer(_levelUpMenu, GraphicsDevice, _debugTexture, _font);

            // Загрузка текстур для кнопок HUD
            var pauseButtonTexture = Content.Load<Texture2D>("Sprites/GUI/ButtonPause");
            var shopButtonTexture = Content.Load<Texture2D>("Sprites/GUI/ButtonShop");

            // Передаем текстуры в HUD
            _gameHUD.LoadButtonTextures(pauseButtonTexture, shopButtonTexture);

            // Инициализация рулетки
            _rouletteManager = new RouletteManager(_player, _weapons, GraphicsDevice, _debugTexture, _font);
            _rouletteMenu = new RouletteMenu(_rouletteManager, GraphicsDevice, _debugTexture, _font);

            // Инициализация магазина бонусов (после загрузки текстур)
            _bonusShopInterface = new BonusShopInterface(_bonusShop, GraphicsDevice, _debugTexture, _font);

            // Инициализация меню паузы
            _pauseMenu = new PauseMenu(GraphicsDevice, _debugTexture, _font);
            _pauseMenu.SetInitialVolumes(0.75f, 1.0f);

            // Инициализация экрана загрузки
            _loadingScreen = new LoadingScreen(GraphicsDevice, _debugTexture, _font);

            // Инициализация экранов Game Over и Victory
            _gameOverScreen = new GameOverScreen(GraphicsDevice, _debugTexture, _font);
            _victoryScreen = new VictoryScreen(GraphicsDevice, _debugTexture, _font);

            // ПРОВЕРКА ЗАГРУЗКИ ЗВУКОВ ПРЕДМЕТОВ
            Debug.WriteLine("=== ПРОВЕРКА ЗАГРУЗКИ ЗВУКОВ ПРЕДМЕТОВ ===");
            Debug.WriteLine($"Звук монеты: {(_soundManager.ContainsSound("take_coin") ? "✓ Загружен" : "✗ Ошибка")}");
            Debug.WriteLine($"Звук опыта: {(_soundManager.ContainsSound("take_experience") ? "✓ Загружен" : "✗ Ошибка")}");
            Debug.WriteLine($"Звук лечения: {(_soundManager.ContainsSound("healing") ? "✓ Загружен" : "✗ Ошибка")}");
            Debug.WriteLine($"Звук взрыва: {(_soundManager.ContainsSound("dynamite_explosion") ? "✓ Загружен" : "✗ Ошибка")}");
            Debug.WriteLine($"Звук магнита: {(_soundManager.ContainsSound("magnetic_sound") ? "✓ Загружен" : "✗ Ошибка")}");
            Debug.WriteLine("=========================================");
        }

        // Метод для инициализации выбранного оружия
        private void InitializePlayerWeapon()
        {
            _weapons.Clear();
            var selectedWeapon = WeaponManager.CreateWeapon(_selectedStartingWeapon, _player);
            _weapons.Add(selectedWeapon);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKs = Keyboard.GetState();
            MouseState currentMs = Mouse.GetState();

            // Сохраняем предыдущее состояние для обнаружения изменений
            GameState previousState = _currentGameState;
            _currentGameState = Game1.CurrentState;

            // Обработка ESC - ПРИОРИТЕТНАЯ
            if (currentKs.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                if (_currentGameState == GameState.Playing)
                {
                    // В игре - открываем меню паузы
                    _pauseMenu.Show();
                    Game1.CurrentState = GameState.Paused;
                    System.Diagnostics.Debug.WriteLine("🔄 ESC: Playing -> Paused");
                }
                else if (_currentGameState == GameState.Paused)
                {
                    // В паузе - закрываем меню паузы
                    _pauseMenu.Hide();
                    Game1.CurrentState = GameState.Playing;
                    System.Diagnostics.Debug.WriteLine("🔄 ESC: Paused -> Playing");
                }
                else if (_currentGameState == GameState.BonusShop)
                {
                    // В магазине - закрываем магазин
                    _bonusShop.Hide();
                    Game1.CurrentState = GameState.Playing;
                    System.Diagnostics.Debug.WriteLine("🔄 ESC: BonusShop -> Playing");
                }
                else if (_currentGameState == GameState.MainMenu)
                {
                    // В главном меню - выходим из игры
                    Exit();
                }
            }

            // Обработка клавиши B для открытия магазина
            if (currentKs.IsKeyDown(Keys.B) && !_previousKeyboardState.IsKeyDown(Keys.B) &&
                _currentGameState == GameState.Playing)
            {
                _bonusShop.Show();
                Game1.CurrentState = GameState.BonusShop;
                System.Diagnostics.Debug.WriteLine("🔄 B: Playing -> BonusShop");
            }

            // Обработка кликов по кнопкам HUD
            if (currentMs.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                Point mousePos = currentMs.Position;

                if (_gameHUD.IsPauseButtonClicked(mousePos) && _currentGameState == GameState.Playing)
                {
                    // Открываем меню паузы по кнопке
                    _pauseMenu.Show();
                    Game1.CurrentState = GameState.Paused;
                    System.Diagnostics.Debug.WriteLine("🔄 Кнопка паузы: Playing -> Paused");
                }
                else if (_gameHUD.IsShopButtonClicked(mousePos) && _currentGameState == GameState.Playing)
                {
                    // Открываем магазин бонусов
                    _bonusShop.Show();
                    Game1.CurrentState = GameState.BonusShop;
                    System.Diagnostics.Debug.WriteLine("🔄 Кнопка магазина: Playing -> BonusShop");
                }
            }

            // ОБНОВЛЯЕМ состояние после всех изменений
            _currentGameState = Game1.CurrentState;

            // Управление звуками при переходе между состояниями - ВСЕГДА ВЫЗЫВАЕТСЯ
            if (previousState != _currentGameState)
            {
                System.Diagnostics.Debug.WriteLine($"🔄 ОБНАРУЖЕНО ИЗМЕНЕНИЕ: {previousState} -> {_currentGameState}");
                HandleSoundStateTransition(previousState, _currentGameState);
            }

            // Управление музыкой в зависимости от состояния
            UpdateMusicForState();

            // ОСНОВНАЯ ЛОГИКА СОСТОЯНИЙ
            switch (_currentGameState)
            {
                case GameState.MainMenu:
                    var menuState = _mainMenu.Update(gameTime);
                    if (menuState == GameState.StartMenu)
                    {
                        Game1.CurrentState = GameState.StartMenu;
                    }
                    else if (menuState == GameState.ExitGame)
                    {
                        Exit();
                    }
                    break;

                case GameState.StartMenu:
                    var newState = _startMenu.Update(gameTime);
                    if (newState == GameState.Loading)
                    {
                        _selectedStartingWeapon = _startMenu.SelectedWeapon;
                    }
                    Game1.CurrentState = newState;
                    break;

                case GameState.Loading:
                    bool startGame = _loadingScreen.Update(gameTime);
                    if (startGame)
                    {
                        ResetGameToInitialState();
                        _difficultyManager.SetDifficulty(_startMenu.SelectedGameMode);
                        _musicManager.SetGameMode(_startMenu.SelectedGameMode);
                        _musicManager.StopMusicForGameStart();
                        InitializePlayerWeapon();

                        if (_startMenu.SelectedGameMode == StartMenu.GameMode.Survival)
                        {
                            _musicManager.ForcePlaySurvivalCycleMusic(1);
                        }
                        else
                        {
                            _musicManager.PlayLevelMusic(1);
                        }

                        Game1.CurrentState = GameState.Playing;
                        _loadingScreen.Reset();
                    }
                    break;

                case GameState.Playing:
                    UpdatePlayingState(gameTime, currentKs, currentMs);
                    break;

                case GameState.Paused:
                    UpdatePausedState();
                    break;

                case GameState.LevelUp:
                    UpdateLevelUpState(gameTime);
                    break;

                case GameState.Roulette:
                    UpdateRouletteState(gameTime);
                    break;

                case GameState.BonusShop:
                    UpdateBonusShopState(gameTime);
                    break;

                case GameState.GameOver:
                    UpdateGameOverState();
                    break;

                case GameState.Victory:
                    UpdateVictoryState();
                    break;
            }

            // В конце метода Update обновляем предыдущее состояние
            _previousKeyboardState = currentKs;
            _previousMouseState = currentMs;
            base.Update(gameTime);
        }

        private void UpdatePlayingState(GameTime gameTime, KeyboardState currentKs, MouseState currentMs)
        {
            _survivalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем HUD
            _gameHUD.UpdateGameStats(_survivalTime, _killCount);
            _gameHUD.Update(gameTime);
            _itemManager.Update(gameTime);

            // Тестовые команды
            if (currentKs.IsKeyDown(Keys.F9) && !_previousKeyboardState.IsKeyDown(Keys.F9))
            {
                Vector2 playerPos = _player.Position;
                _itemManager.AddExperienceOrb(playerPos + new Vector2(50, 0), 25);
                _itemManager.AddCoin(playerPos + new Vector2(-50, 0), 1);
                _itemManager.AddHealthOrb(playerPos + new Vector2(0, 50), 0.25f);
                _itemManager.AddDynamite(playerPos + new Vector2(0, -50));
            }

            if (currentKs.IsKeyDown(Keys.F10) && !_previousKeyboardState.IsKeyDown(Keys.F10))
            {
                _spawnManager.SpawnEliteEnemy();
            }

            if (currentKs.IsKeyDown(Keys.F8) && !_previousKeyboardState.IsKeyDown(Keys.F8))
            {
                Game1.CurrentState = GameState.Victory;
                _victoryScreen.Show();
                return;
            }

            // Музыка
            if (_difficultyManager.CurrentDifficulty != StartMenu.GameMode.Survival)
            {
                _musicManager.PlayLevelMusic(_levelManager.CurrentLevel);
            }

            // Проверки состояния
            if (!_player.IsAlive)
            {
                Game1.CurrentState = GameState.GameOver;
                _gameOverScreen.Show();
                _musicManager.StopMusic();
                return;
            }

            if (_player.IsLevelUpPending)
            {
                Game1.CurrentState = GameState.LevelUp;
                return;
            }

            // Игровая логика
            _player.Update(gameTime);
            _camera.Follow();
            _spawnManager.Update(gameTime);
            _itemManager.Update(gameTime);
            DynamiteExplosion.UpdateAll(gameTime, _enemies);

            // Обновление врагов
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _enemies[i];
                enemy.Update(gameTime);

                if (!enemy.IsAlive)
                {
                    _killCount++;
                    HandleEnemyDeath(enemy);
                    _enemies.RemoveAt(i);
                }
            }

            // Коллизии с врагами
            if (_player.IsAlive)
            {
                var playerBounds = GetBounds(_player);
                var collidingEnemies = _enemies.Where(enemy =>
                    enemy.IsAlive && GetBounds(enemy).Intersects(playerBounds)
                ).ToList();

                if (collidingEnemies.Any() && !_player.IsInvulnerable)
                {
                    _player.TakeDamage(collidingEnemies.First().Damage);
                }
            }

            // Обновление оружия
            foreach (var weapon in _weapons)
            {
                weapon.Update(gameTime);
                weapon.Attack(gameTime, _enemies);
                UpdateWeaponSpecifics(weapon);
            }
        }

        private void UpdatePausedState()
        {
            _pauseMenu.Update();
            _musicManager.PauseMusic();

            // Проверяем, не изменилось ли состояние через меню паузы
            if (Game1.CurrentState == GameState.MainMenu)
            {
                ResetToMainMenu();
            }
            else if (!_pauseMenu.IsVisible)
            {
                // Если меню паузы скрылось, возвращаемся в игру
                _musicManager.ResumeMusic();
                Game1.CurrentState = GameState.Playing;
                System.Diagnostics.Debug.WriteLine("🔄 Пауза закрыта: Paused -> Playing");
            }
        }

        private void UpdateBonusShopState(GameTime gameTime)
        {
            _bonusShop.Update(gameTime);
            _bonusShopInterface.UpdateInput();

            // Если магазин скрылся, возвращаемся в игру
            if (!_bonusShop.IsVisible)
            {
                Game1.CurrentState = GameState.Playing;
                System.Diagnostics.Debug.WriteLine("🔄 Магазин закрыт: BonusShop -> Playing");
            }
        }

        private void UpdateLevelUpState(GameTime gameTime)
        {
            if (_levelUpMenu.CurrentOptions.Count == 0)
            {
                _levelUpMenu.GenerateOptions();
            }
            _levelUpMenu.Update(gameTime);

            if (!_player.IsLevelUpPending)
            {
                Game1.CurrentState = GameState.Playing;
                _levelUpMenu.CurrentOptions.Clear();
            }
        }

        private void UpdateRouletteState(GameTime gameTime)
        {
            _rouletteManager.Update(gameTime);

            if (!_rouletteManager.IsActive)
            {
                Game1.CurrentState = GameState.Playing;

                if (_levelManager.ElitesKilled > 0 && _levelManager.ElitesKilled % 2 == 0)
                {
                    _gameHUD.ShowStageAnnouncement(_levelManager.CurrentLevel);
                }
            }
        }

        private void UpdateGameOverState()
        {
            _musicManager.StopMusic();
            _gameOverScreen.Update();

            if (!_gameOverScreen.IsVisible)
            {
                StopAllGameSounds();
            }
        }

        private void UpdateVictoryState()
        {
            _musicManager.StopMusic();
            _victoryScreen.Update();

            if (!_victoryScreen.IsVisible)
            {
                StopAllGameSounds();
            }
        }


        private void HandleEnemyDeath(Enemy enemy)
        {
            if (enemy is EliteEnemy)
            {
                HandleEliteEnemyDeath(enemy);
            }
            else
            {
                HandleRegularEnemyDeath(enemy);
            }
        }

        private void HandleEliteEnemyDeath(Enemy enemy)
        {
            int elitesBefore = _levelManager.ElitesKilled;
            int levelBefore = _levelManager.CurrentLevel;
            int cycleBefore = _difficultyManager.CurrentCycle;

            _levelManager.EliteKilled();

            int elitesAfter = _levelManager.ElitesKilled;
            int levelAfter = _levelManager.CurrentLevel;
            int cycleAfter = _difficultyManager.CurrentCycle;

            System.Diagnostics.Debug.WriteLine($"Элитный враг убит! Уровень: {levelBefore}->{levelAfter}, Элитных: {elitesBefore}->{elitesAfter}, Цикл: {cycleBefore}->{cycleAfter}");

            if (levelAfter != levelBefore)
            {
                UpdateFloorTexture();
                _gameHUD.ShowStageAnnouncement(levelAfter);
            }

            if (_difficultyManager.CurrentDifficulty == StartMenu.GameMode.Survival && cycleAfter != cycleBefore)
            {
                _difficultyManager.UpdateSurvivalBaseHealth(cycleAfter);
                _musicManager.ForcePlaySurvivalCycleMusic(cycleAfter);
            }

            if (_difficultyManager.CheckVictoryCondition(levelBefore, elitesBefore + 1))
            {
                Game1.CurrentState = GameState.Victory;
                _victoryScreen.Show();
                _musicManager.StopMusic();
                return;
            }

            // Дроп от элитного врага
            for (int j = 0; j < 5; j++)
            {
                _itemManager.AddCoin(enemy.Position, 1);
            }
            for (int j = 0; j < 3; j++)
            {
                _itemManager.AddExperienceOrb(enemy.Position, 1);
            }
            _itemManager.AddGoldenHealthOrb(enemy.Position, 1.0f);

            _rouletteManager.StartRoulette();
            Game1.CurrentState = GameState.Roulette;
        }

        private void HandleRegularEnemyDeath(Enemy enemy)
        {
            Vector2 worldPosition = enemy.Position;
            _itemManager.AddExperienceOrb(worldPosition, 1);

            if (Game1.Random.NextDouble() < 0.02)
            {
                _itemManager.AddHealthOrb(enemy.Position, 0.25f);
            }
            if (Game1.Random.NextDouble() < 0.25)
            {
                _itemManager.AddCoin(enemy.Position, 1);
            }
            if (Game1.Random.NextDouble() < 0.5)
            {
                _itemManager.AddExperienceOrb(enemy.Position, 1);
            }
            if (Game1.Random.NextDouble() < 0.02 && !(enemy is EliteEnemy))
            {
                _itemManager.AddMagnet(enemy.Position);
            }
            if (Game1.Random.NextDouble() < 0.02)
            {
                _itemManager.AddDynamite(enemy.Position);
            }
        }

        private void UpdateWeaponSpecifics(Weapon weapon)
        {
            if (weapon is RouletteBall rouletteBall)
            {
                var rouletteField = weapon.GetType().GetField("_camera",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (rouletteField != null && rouletteField.GetValue(weapon) == null)
                {
                    weapon.GetType().GetMethod("SetCamera")?.Invoke(weapon,
                        new object[] { _camera, GraphicsDevice.Viewport });
                }

                Rectangle screenBounds = new Rectangle(
                    (int)_camera.Position.X,
                    (int)_camera.Position.Y,
                    GraphicsDevice.Viewport.Width,
                    GraphicsDevice.Viewport.Height
                );

                foreach (var ball in rouletteBall.ActiveBalls)
                {
                    if (ball.IsActive)
                    {
                        ball.ScreenBounds = screenBounds;
                    }
                }
            }
        }

        private void ResetToMainMenu()
        {
            _enemies.Clear();
            _weapons.Clear();
            _levelManager.Reset();
            _itemManager.Clear();
            _survivalTime = 0f;
            _killCount = 0;

            StopAllGameSounds();
            StopWeaponSpecificSounds();

            _musicManager.PlayMenuMusic();
            _pauseMenu.Hide();
            System.Diagnostics.Debug.WriteLine("Возврат в главное меню, музыка меню запущена");
        }


        // Метод для обновления текстуры пола
        private void UpdateFloorTexture()
        {
            try
            {
                if (_levelManager != null && Content != null)
                {
                    Texture2D newFloorTexture = _levelManager.LoadCurrentLevelFloorTexture(Content);
                    if (_worldGeneration != null)
                    {
                        _worldGeneration.ChangeFloorTexture(newFloorTexture);
                        Debug.WriteLine($"Обновлена текстура пола для уровня {_levelManager.CurrentLevel}");

                        // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: выводим информацию о текстуре
                        if (newFloorTexture != null)
                        {
                            Debug.WriteLine($"Текстура пола: {newFloorTexture.Width}x{newFloorTexture.Height}, Уровень: {_levelManager.CurrentLevel}");
                        }
                        else
                        {
                            Debug.WriteLine("ОШИБКА: Текстура пола не загружена!");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ОШИБКА: WorldGeneration не инициализирован!");
                    }
                }
                else
                {
                    Debug.WriteLine("ОШИБКА: LevelManager или Content не доступны!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"ОШИБКА обновления текстуры пола: {ex.Message}");
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (_currentGameState)
            {
                case GameState.MainMenu:
                    _spriteBatch.Begin();
                    _mainMenu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.StartMenu:
                    _spriteBatch.Begin();
                    _startMenu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.Loading:
                    _spriteBatch.Begin();
                    _loadingScreen.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.Playing:
                case GameState.LevelUp:
                case GameState.Roulette:
                case GameState.BonusShop:
                case GameState.Paused:
                    // Отрисовка игрового мира (с камерой)
                    _spriteBatch.Begin(transformMatrix: _camera.Transform);
                    DrawWorldObjects();
                    _spriteBatch.End();

                    // Отрисовка HUD и UI (без камеры)
                    _spriteBatch.Begin();

                    // Используем новый HUD для отрисовки интерфейса
                    if (_currentGameState == GameState.Playing)
                    {
                        _gameHUD.Draw(_spriteBatch);
                        _gameHUD.DrawStageAnnouncement(_spriteBatch);
                    }

                    if (_currentGameState == GameState.LevelUp)
                    {
                        DrawLevelUpPendingScreen(_spriteBatch);
                        _levelUpMenuRenderer.Draw(_spriteBatch);
                    }

                    if (_currentGameState == GameState.Roulette)
                    {
                        DrawLevelUpPendingScreen(_spriteBatch);
                        _rouletteMenu.Draw(_spriteBatch);
                    }

                    if (_currentGameState == GameState.BonusShop)
                    {
                        DrawLevelUpPendingScreen(_spriteBatch);
                        _bonusShopInterface.Draw(_spriteBatch);
                    }

                    if (_currentGameState == GameState.Paused)
                    {
                        DrawLevelUpPendingScreen(_spriteBatch);
                        _pauseMenu.Draw(_spriteBatch);
                    }

                    _spriteBatch.End();
                    break;

                case GameState.GameOver:
                    // Отрисовываем только экран Game Over
                    _spriteBatch.Begin();
                    _gameOverScreen.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.Victory:
                    // Отрисовываем только экран Victory
                    _spriteBatch.Begin();
                    _victoryScreen.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ DRAW ---

        private void DrawWorldObjects()
        {
            // Отрисовка фона мира
            _worldGeneration.Draw(_spriteBatch);

            

            // ОТЛАДКА: проверяем состояние WorldGeneration
            if (_worldGeneration == null)
            {
                Debug.WriteLine("ОШИБКА: WorldGeneration is NULL в DrawWorldObjects!");
                return;
            }

            // Отрисовка фона мира
            try
            {
                _worldGeneration.Draw(_spriteBatch);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА отрисовки WorldGeneration: {ex.Message}");
            }

            // Отрисовка огненных областей первыми (под всеми)
            foreach (var weapon in _weapons)
            {
                if (weapon is MolotovCocktail molotov)
                {
                    molotov.DrawProjectiles(_spriteBatch);
                }

                // ДОБАВЛЕНО: Отрисовка пивных луж (только лужи, бутылки отрисуются позже)
                if (weapon is BeerBottle beerBottle)
                {
                    // Отрисовываем только лужи здесь (они будут ПОД всеми)
                    foreach (var puddle in beerBottle.ActivePuddles)
                    {
                        if (puddle.IsActive)
                        {
                            puddle.DrawWithTexture(_spriteBatch, _puddleTexture);
                        }
                    }
                }
            }

            DynamiteExplosion.DrawAll(_spriteBatch);

            // Отрисовка врагов
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    enemy.Draw(_spriteBatch, _debugTexture);
                }
            }

            // Отрисовка игрока (САМЫЙ ВЕРХНИЙ СЛОЙ)
            Color playerTint = Color.White;
            if (_player.IsInvulnerable)
            {
                if ((int)(_survivalTime * 10) % 2 == 0)
                {
                    playerTint = Color.Red * 0.5f;
                }
            }
            _player.Draw(_spriteBatch, _debugTexture, playerTint);

            // Отрисовка предметов через менеджер
            _itemManager.Draw(_spriteBatch, _debugTexture);

            if (_survivalTime % 5f < 0.1f) // Каждые 5 секунд
            {
                //Debug.WriteLine($"Камера: {_camera.Position}, Игрок: {_player.Position}");
            }

            // Отрисовка остального оружия (НАД ВСЕМИ)
            foreach (var weapon in _weapons)
            {
                if (weapon is PlayingCards cards)
                {
                    foreach (var card in cards.ActiveProjectiles)
                    {
                        if (card.IsActive)
                        {
                            card.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is GoldenBullet goldenBullet)
                {
                    foreach (var bullet in goldenBullet.ActiveProjectiles)
                    {
                        if (bullet.IsActive)
                        {
                            bullet.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is StickyBomb stickyBomb)
                {
                    foreach (var bomb in stickyBomb.ActiveBombs)
                    {
                        if (bomb.IsActive)
                        {
                            bomb.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is GoldenSword sword)
                {
                    foreach (var projectile in sword.ActiveProjectiles)
                    {
                        if (projectile.IsActive)
                        {
                            projectile.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is CasinoChips casinoChips)
                {
                    foreach (var chip in casinoChips.ActiveProjectiles)
                    {
                        if (chip.IsActive)
                        {
                            chip.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                if (weapon is BigLaser bigLaser)
                {
                    bigLaser.DrawLaser(_spriteBatch, _debugTexture);
                }

                if (weapon is DiceWeapon diceWeapon)
                {
                    foreach (var dice in diceWeapon.ActiveDice)
                    {
                        if (dice.IsActive)
                        {
                            dice.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                // Отрисовка рулетки
                if (weapon is RouletteBall roulette)
                {
                    // Отрисовываем частички ПОД шариками
                    foreach (var particle in roulette.ActiveParticles)
                    {
                        if (particle.IsActive)
                        {
                            particle.Draw(_spriteBatch, _debugTexture);
                        }
                    }

                    // Отрисовываем ВСЕ шарики
                    foreach (var ball in roulette.ActiveBalls)
                    {
                        if (ball.IsActive)
                        {
                            ball.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                // Отрисовка Золотого Тайфуна
                if (weapon is GoldenTyphoon goldenTyphoon)
                {
                    foreach (var projectile in goldenTyphoon.ActiveProjectiles)
                    {
                        if (projectile.IsActive)
                        {
                            projectile.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                // Отрисовка Горизонта Событий
                if (weapon is EventHorizon eventHorizon)
                {
                    foreach (var star in eventHorizon.ActiveProjectiles)
                    {
                        if (star.IsActive)
                        {
                            star.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                // Отрисовка летящих бутылок пива
                if (weapon is BeerBottle beerBottle)
                {
                    foreach (var bottle in beerBottle.ActiveBottles)
                    {
                        if (bottle.IsActive)
                        {
                            bottle.DrawWithTexture(_spriteBatch, _bottleTexture);
                        }
                    }
                }

                if (weapon is Breaker breaker)
                {
                    foreach (var blade in breaker.ActiveBlades)
                    {
                        if (blade.IsActive)
                        {
                            blade.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }

                // Отрисовка Тайфуна
                if (weapon is Typhoon typhoon)
                {
                    foreach (var projectile in typhoon.ActiveProjectiles)
                    {
                        if (projectile.IsActive)
                        {
                            projectile.Draw(_spriteBatch, _debugTexture);
                        }
                    }
                }
            }
        }

        private void DrawLevelUpPendingScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.Black * 0.6f
            );
        }

        private Rectangle GetBounds(object obj)
        {
            const int Size = 24;
            Vector2 position = Vector2.Zero;

            if (obj is Player p)
            {
                position = p.Position;
            }
            else if (obj is Enemy e)
            {
                position = e.Position;
            }
            else
            {
                return Rectangle.Empty;
            }

            return new Rectangle(
                (int)(position.X - Size),
                (int)(position.Y - Size),
                Size * 2,
                Size * 2
            );
        }

        protected override void UnloadContent()
        {
            StopAllGameSounds();
            _musicManager?.Dispose();
            _soundManager?.Dispose();
            base.UnloadContent();
        }

        private void UpdateMusicForState()
        {
            switch (_currentGameState)
            {
                case GameState.MainMenu:
                case GameState.StartMenu:
                case GameState.Loading:
                    // В этих состояниях играет музыка главного меню
                    _musicManager.PlayMenuMusic();
                    break;

                case GameState.Playing:
                    // Музыка уровня уже управляется через PlayLevelMusic в состоянии Playing
                    break;

                case GameState.Paused:
                case GameState.BonusShop:
                case GameState.LevelUp:
                case GameState.Roulette:
                    // В этих состояниях музыка продолжает играть (если это музыка уровня)
                    break;

                case GameState.GameOver:
                    // В GameOver можно остановить музыку или переключить на другую
                    _musicManager.StopMusic();
                    break;
            }
        }

        //Полный сброс всех параметров игры к начальным значениям
        private void ResetGameToInitialState()
        {
            System.Diagnostics.Debug.WriteLine("ПОЛНЫЙ СБРОС ИГРЫ К НАЧАЛЬНОМУ СОСТОЯНИЮ");

            // ОСТАНАВЛИВАЕМ ВСЕ ЗВУКИ ПЕРЕД СБРОСОМ
            StopAllGameSounds();

            // СБРОС ЭКРАНОВ СМЕРТИ И ПОБЕДЫ - ДОБАВЛЕНО ДЛЯ ИСПРАВЛЕНИЯ БАГА
            if (_gameOverScreen != null && _gameOverScreen.IsVisible)
            {
                _gameOverScreen.Hide();
                _gameOverScreen.Reset();
                System.Diagnostics.Debug.WriteLine("Экран Game Over сброшен");
            }

            if (_victoryScreen != null && _victoryScreen.IsVisible)
            {
                _victoryScreen.Hide();
                System.Diagnostics.Debug.WriteLine("Экран Victory сброшен");
            }

            Vector2 initialPlayerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
            );

            // ПОЛНЫЙ СБРОС СОСТОЯНИЯ ИГРОКА - КРИТИЧЕСКИ ВАЖНО!
            if (_player != null)
            {
                _player.SetPosition(initialPlayerPosition);
                _player.ResetExperienceRequirements();
                _player.ResetToInitialState();

                // ЯВНЫЙ СБРОС ЖИЗНЕННЫХ ПАРАМЕТРОВ
                _player.IsAlive = true; // ГАРАНТИРУЕМ, ЧТО ИГРОК ЖИВ
                _player.CurrentHealth = _player.MaxHealth; // ПОЛНОЕ ЗДОРОВЬЕ

                System.Diagnostics.Debug.WriteLine($"Игрок восстановлен: HP={_player.CurrentHealth}/{_player.MaxHealth}, IsAlive={_player.IsAlive}");
            }

            if (_camera != null)
            {
                _camera = new Camera(_player, GraphicsDevice.Viewport);
                UpdateWorldGenerationCamera();
            }

            if (_levelManager != null)
            {
                _levelManager.Reset();
                System.Diagnostics.Debug.WriteLine($"LevelManager сброшен. Текущий уровень: {_levelManager.CurrentLevel}");
            }

            if (_difficultyManager != null)
            {
                _difficultyManager.Reset();
                System.Diagnostics.Debug.WriteLine($"DifficultyManager сброшен. Режим: {_difficultyManager.CurrentDifficulty}");
            }

            if (_spawnManager != null)
            {
                _spawnManager = new SpawnManager(_enemies, _player, _camera, GraphicsDevice.Viewport, _levelManager, _difficultyManager);
                System.Diagnostics.Debug.WriteLine("SpawnManager сброшен");
            }

            if (_itemManager != null)
            {
                _itemManager.Clear();
                _itemManager.UpdatePlayerReference(_player);
                System.Diagnostics.Debug.WriteLine("ItemManager очищен и обновлен");
            }

            if (_bonusShop != null)
            {
                _bonusShop.ResetPrices();
                System.Diagnostics.Debug.WriteLine("Магазин бонусов сброшен, цены восстановлены");
            }

            _enemies.Clear();
            _weapons.Clear();

            _survivalTime = 0f;
            _killCount = 0;

            if (_levelUpMenu != null)
            {
                _levelUpMenu = new LevelUpMenu(_player, _weapons, GraphicsDevice, _debugTexture, _font);
                _levelUpMenuRenderer = new LevelUpMenuRenderer(_levelUpMenu, GraphicsDevice, _debugTexture, _font);
                System.Diagnostics.Debug.WriteLine("Меню прокачки сброшено");
            }

            if (_rouletteManager != null)
            {
                _rouletteManager = new RouletteManager(_player, _weapons, GraphicsDevice, _debugTexture, _font);
                _rouletteMenu = new RouletteMenu(_rouletteManager, GraphicsDevice, _debugTexture, _font);
                System.Diagnostics.Debug.WriteLine("Рулетка сброшена");
            }

            UpdateFloorTexture();

            // СБРАСЫВАЕМ СОСТОЯНИЕ ИГРЫ
            Game1.CurrentState = GameState.Playing;
            _currentGameState = GameState.Playing;

            _musicManager.PlayLevelMusic(1);

            System.Diagnostics.Debug.WriteLine("ВСЕ ПАРАМЕТРЫ ИГРЫ СБРОШЕНЫ К НАЧАЛЬНОМУ СОСТОЯНИЮ");
        }

        // Обновляет камеру в WorldGeneration после сброса игры
        private void UpdateWorldGenerationCamera()
        {
            if (_worldGeneration != null && _camera != null)
            {
                // Получаем тип WorldGeneration через рефлексию и устанавливаем камеру
                var field = _worldGeneration.GetType().GetField("_camera",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(_worldGeneration, _camera);
                    Debug.WriteLine("Камера в WorldGeneration обновлена");
                }
                else
                {
                    Debug.WriteLine("ОШИБКА: Не найден поле _camera в WorldGeneration");
                }
            }
        }

        private void StopAllGameSounds()
        {
            System.Diagnostics.Debug.WriteLine("=== ПОЛНАЯ ОСТАНОВКА ВСЕХ ИГРОВЫХ ЗВУКОВ ===");

            // Останавливаем все звуки через SoundManager
            _soundManager.StopAllGameSounds();
            _soundManager.StopAllSounds();

            // Особые случаи - останавливаем специфические звуки оружий
            StopWeaponSpecificSounds();

            // Останавливаем звуки предметов
            ItemSoundManager.StopAllItemSounds();
            _itemManager.StopMagnetSound();
        }

        private void StopWeaponSpecificSounds()
        {
            foreach (var weapon in _weapons)
            {
                if (weapon is BigLaser bigLaser)
                {
                    bigLaser.StopLaserSound();
                    System.Diagnostics.Debug.WriteLine($"Остановлен звук лазера");
                }

                if (weapon is MolotovCocktail molotov)
                {
                    molotov.StopAllSounds();
                    System.Diagnostics.Debug.WriteLine($"Остановлены звуки MolotovCocktail");
                }
            }
        }

        private void PauseWeaponSpecificSounds()
        {
            int lasersPaused = 0;
            int molotovsPaused = 0;

            foreach (var weapon in _weapons)
            {
                if (weapon is BigLaser bigLaser)
                {
                    bigLaser.PauseLaserSound();
                    lasersPaused++;
                    System.Diagnostics.Debug.WriteLine($"Приостановлен звук лазера");
                }

                if (weapon is MolotovCocktail molotov)
                {
                    molotov.PauseAllSounds();
                    molotovsPaused++;
                    System.Diagnostics.Debug.WriteLine($"Приостановлены звуки MolotovCocktail");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Пауза оружий: лазеров={lasersPaused}, молотовов={molotovsPaused}");
        }

        private void ResumeWeaponSpecificSounds()
        {
            int lasersResumed = 0;
            int molotovsResumed = 0;

            foreach (var weapon in _weapons)
            {
                if (weapon is BigLaser bigLaser)
                {
                    bigLaser.ResumeLaserSound();
                    lasersResumed++;
                    System.Diagnostics.Debug.WriteLine($"▶Возобновлен звук лазера");
                }

                if (weapon is MolotovCocktail molotov)
                {
                    molotov.ResumeAllSounds();
                    molotovsResumed++;
                    System.Diagnostics.Debug.WriteLine($"▶Возобновлены звуки MolotovCocktail");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Возобновление оружий: лазеров={lasersResumed}, молотовов={molotovsResumed}");
        }

        private void HandleSoundStateTransition(GameState previousState, GameState newState)
        {
            System.Diagnostics.Debug.WriteLine($"=== ПЕРЕХОД СОСТОЯНИЯ: {previousState} -> {newState} ===");

            // Пауза звуков при переходе в меню-состояния (включая BonusShop)
            if (newState == GameState.LevelUp ||
                newState == GameState.Roulette ||
                newState == GameState.BonusShop ||  // <- магазин здесь!
                newState == GameState.Paused)
            {
                if (previousState == GameState.Playing)
                {
                    System.Diagnostics.Debug.WriteLine($"ПАУЗА звуков при переходе в {newState}");
                    _soundManager.PauseAllGameSounds();
                    PauseWeaponSpecificSounds();

                    ItemSoundManager.PauseAllItemSounds();
                    _itemManager.PauseMagnetSound();
                }
            }

            // Возобновление звуков при возврате в игру
            if (newState == GameState.Playing)
            {
                if (previousState == GameState.LevelUp ||
                    previousState == GameState.Roulette ||
                    previousState == GameState.BonusShop ||  // <- магазин здесь!
                    previousState == GameState.Paused)
                {
                    System.Diagnostics.Debug.WriteLine($"ВОЗОБНОВЛЕНИЕ звуков при возврате в игру из {previousState}");
                    _soundManager.ResumeAllGameSounds();
                    ResumeWeaponSpecificSounds();

                    ItemSoundManager.ResumeAllItemSounds();
                    _itemManager.ResumeMagnetSound();
                }
            }

            // Полная остановка звуков при переходе в финальные состояния
            if (newState == GameState.GameOver ||
                newState == GameState.Victory ||
                newState == GameState.MainMenu)
            {
                if (previousState == GameState.Playing)
                {
                    System.Diagnostics.Debug.WriteLine($"ПОЛНАЯ ОСТАНОВКА звуков при переходе в {newState}");
                    StopAllGameSounds();
                    StopWeaponSpecificSounds();

                    ItemSoundManager.StopAllItemSounds();
                    _itemManager.StopMagnetSound();
                }
            }
        }
    }
}