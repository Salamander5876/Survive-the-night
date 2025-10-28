using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

// Убедитесь, что все эти пространства имен существуют
using Survive_the_night.Entities;
using Survive_the_night.Weapons;
using Survive_the_night.Projectiles;
using Survive_the_night.Managers;
using Survive_the_night.Interfaces;
using Survive_the_night.Items;
using Survive_the_night.Entities.Enemies.Regular;
using Survive_the_night.Entities.Enemies.Elite;

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

        // Звуки (только общие, не связанные с оружиями)
        public static SoundEffect SFXGoldenSword;
        public static SoundEffect SFXThrowMolotov;
        public static SoundEffect SFXFireBurn;
        public static SoundEffect SFXCasinoChips;
        public static SoundEffect SFXBigLaser;

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
        private LevelManager _levelManager;
        private GameHUD _gameHUD;

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
            // Сначала создаем debugTexture
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });

            // Загружаем шрифт (нужно создать временный SpriteBatch)
            var tempSpriteBatch = new SpriteBatch(GraphicsDevice);
            // Шрифт будет загружен в LoadContent, но для инициализации можно использовать временный

            // Синхронизация локального и статического состояния
            _currentGameState = GameState.MainMenu;
            Game1.CurrentState = GameState.MainMenu;

            Vector2 initialPlayerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
            );

            _player = new Player(initialPlayerPosition);
            _camera = new Camera(_player, GraphicsDevice.Viewport);

            // СНАЧАЛА инициализируем менеджеры
            _musicManager = new MusicsManager();
            _levelManager = new LevelManager();

            // ПОТОМ создаем SpawnManager и передаем LevelManager
            _spawnManager = new SpawnManager(_enemies, _player, _camera, GraphicsDevice.Viewport, _levelManager);

            // Новая система предметов
            _itemManager = new ItemManager(_player);

            // Магазин бонусов
            _bonusShop = new BonusShopMenu(_player, _itemManager);

            // Оружие будет инициализировано после выбора в StartMenu
            CurrentEnemies = _enemies;

            // GameOverScreen и VictoryScreen будут инициализированы в LoadContent
            // где будут доступны необходимые ресурсы

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

            // Загрузка музыки
            _musicManager.LoadContent(Content);

            // --- ЗАГРУЗКА ТЕКСТУР И ЗВУКОВ ДЛЯ ОРУЖИЙ ЧЕРЕЗ WEAPON MANAGER ---

            // Игральные карты
            var cardTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/Card1");
            var cardTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/Card2");
            var cardTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/Card3");
            var cardTexture4 = Content.Load<Texture2D>("Sprites/Projectiles/Card4");
            var cardSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXCardDeal");
            WeaponManager.LoadWeaponTextures(WeaponName.PlayingCards, cardTexture1, cardTexture2, cardTexture3, cardTexture4);
            WeaponManager.LoadWeaponSound(WeaponName.PlayingCards, cardSound);

            // Золотые пули
            var bulletTexture = Content.Load<Texture2D>("Sprites/Projectiles/Bullet");
            var gunSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXGunShooting");
            WeaponManager.LoadWeaponTextures(WeaponName.GoldenBullet, bulletTexture);
            WeaponManager.LoadWeaponSound(WeaponName.GoldenBullet, gunSound);

            // Фишки казино
            var chipTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsBlue");
            var chipTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsGreen");
            var chipTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/CasinoChipsRed");
            var chipsSound = Content.Load<SoundEffect>("Sounds/Weapons/SFCCasinoChips");
            WeaponManager.LoadWeaponTextures(WeaponName.CasinoChips, chipTexture1, chipTexture2, chipTexture3);
            WeaponManager.LoadWeaponSound(WeaponName.CasinoChips, chipsSound);

            // Золотой меч
            var swordTexture = Content.Load<Texture2D>("Sprites/Projectiles/GoldenSword");
            SFXGoldenSword = Content.Load<SoundEffect>("Sounds/Weapons/SFXGoldenSword");
            WeaponManager.LoadWeaponTextures(WeaponName.GoldenSword, swordTexture);
            WeaponManager.LoadWeaponSound(WeaponName.GoldenSword, SFXGoldenSword);

            // Молотов
            var molotovTexture = Content.Load<Texture2D>("Sprites/Projectiles/Molotov");
            var molotovFireTexture = Content.Load<Texture2D>("Sprites/Projectiles/MolotovFire");
            SFXThrowMolotov = Content.Load<SoundEffect>("Sounds/Weapons/SFXThrowMolotov");
            SFXFireBurn = Content.Load<SoundEffect>("Sounds/Weapons/SFXFireBurn");
            MolotovCocktail.SetTextures(molotovTexture, molotovFireTexture);
            WeaponManager.LoadWeaponTextures(WeaponName.MolotovCocktail, molotovTexture);
            WeaponManager.LoadWeaponSound(WeaponName.MolotovCocktail, SFXThrowMolotov);

            // Большой лазер
            var bigLaserTexture = Content.Load<Texture2D>("Sprites/Projectiles/BigLaser");
            SFXBigLaser = Content.Load<SoundEffect>("Sounds/Weapons/SFXBigLaser");
            WeaponManager.LoadWeaponTextures(WeaponName.BigLaser, bigLaserTexture);
            WeaponManager.LoadWeaponSound(WeaponName.BigLaser, SFXBigLaser);
            BigLaserProjectile.SetDefaultTexture(bigLaserTexture);

            // Установка текстур по умолчанию для проектов
            PlayingCard.SetDefaultTexture(cardTexture1);
            GoldenBulletProjectile.SetDefaultTexture(bulletTexture);
            GoldenSwordProjectile.SetDefaultTexture(swordTexture);
            CasinoChip.SetDefaultTexture(chipTexture1);

            // Загрузка текстур для предметов
            _heartTexture = Content.Load<Texture2D>("Sprites/Items/Heart");
            _goldenHeartTexture = Content.Load<Texture2D>("Sprites/Items/GoldenHeart");
            Texture2D coinTexture = Content.Load<Texture2D>("Sprites/Items/GoldMoney");
            Texture2D experienceOrbTexture = Content.Load<Texture2D>("Sprites/Items/ExperienceOrb"); // Новая текстура

            // Установка текстур для рендереров предметов
            HealthOrbRenderer.SetTexture(_heartTexture);
            GoldenHealthOrbRenderer.SetTexture(_goldenHeartTexture);
            _itemManager.SetCoinTexture(coinTexture);
            _itemManager.SetExperienceOrbTexture(experienceOrbTexture); // Устанавливаем текстуру эссенции

            // Динамит
            var dynamiteTexture = Content.Load<Texture2D>("Sprites/Items/Dynamite");
            var dynamiteExplosionTexture = Content.Load<Texture2D>("Sprites/Projectiles/DynamiteExplosion");
            var dynamiteExplosionSound = Content.Load<SoundEffect>("Sounds/Items/SFXDynamiteExplosion");

            _itemManager.SetDynamiteTexture(dynamiteTexture);
            Dynamite.SetExplosionSound(dynamiteExplosionSound);
            DynamiteExplosion.SetTexture(dynamiteExplosionTexture);

            // Магнит
            var magnetTexture = Content.Load<Texture2D>("Sprites/Items/Magnet");
            _itemManager.SetMagnetTexture(magnetTexture);

            // Липкая бомба
            var stickyBombTexture = Content.Load<Texture2D>("Sprites/Projectiles/StickyBomb");
            var bombExplosionTexture = Content.Load<Texture2D>("Sprites/Projectiles/BombExplosion");
            var bombThrowSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXBombThrow");
            var bombExplosionSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXBombExplosion");

            WeaponManager.LoadWeaponTextures(WeaponName.StickyBomb, stickyBombTexture);
            WeaponManager.LoadWeaponSound(WeaponName.StickyBomb, bombThrowSound);

            StickyBombProjectile.SetTextures(stickyBombTexture, bombExplosionTexture);
            StickyBomb.SetSounds(bombThrowSound, bombExplosionSound);

            // Игральные кости
            var diceTexture1 = Content.Load<Texture2D>("Sprites/Projectiles/Dice1");
            var diceTexture2 = Content.Load<Texture2D>("Sprites/Projectiles/Dice2");
            var diceTexture3 = Content.Load<Texture2D>("Sprites/Projectiles/Dice3");
            var diceTexture4 = Content.Load<Texture2D>("Sprites/Projectiles/Dice4");
            var diceTexture5 = Content.Load<Texture2D>("Sprites/Projectiles/Dice5");
            var diceTexture6 = Content.Load<Texture2D>("Sprites/Projectiles/Dice6");
            var diceSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXDiceDamage");

            WeaponManager.LoadWeaponTextures(WeaponName.Dice,
                diceTexture1, diceTexture2, diceTexture3,
                diceTexture4, diceTexture5, diceTexture6);
            WeaponManager.LoadWeaponSound(WeaponName.Dice, diceSound);

            // Устанавливаем текстуры и звук для DiceProjectile
            DiceProjectile.SetTextures(diceTexture1, diceTexture2, diceTexture3, diceTexture4, diceTexture5, diceTexture6);
            DiceProjectile.SetHitSound(diceSound);

            // Рулетка
            var rouletteBallTexture = Content.Load<Texture2D>("Sprites/Projectiles/RouletteBall");
            var rouletteSound = Content.Load<SoundEffect>("Sounds/Weapons/SFXRouletteDamage");

            // Загружаем все текстуры частичек
            for (int i = 1; i <= 15; i++)
            {
                var particleTexture = Content.Load<Texture2D>($"Sprites/Projectiles/Roulette{i}");
                RouletteParticle.AddParticleTexture(particleTexture);
            }

            WeaponManager.LoadWeaponTextures(WeaponName.RouletteBall, rouletteBallTexture);
            WeaponManager.LoadWeaponSound(WeaponName.RouletteBall, rouletteSound);
            RouletteBallProjectile.SetDefaultTexture(rouletteBallTexture);

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

            System.Diagnostics.Debug.WriteLine($"Загружены текстуры кнопок: Пауза={pauseButtonTexture != null} ({pauseButtonTexture?.Width}x{pauseButtonTexture?.Height}), Магазин={shopButtonTexture != null} ({shopButtonTexture?.Width}x{shopButtonTexture?.Height})");

            // Передаем текстуры в HUD
            _gameHUD.LoadButtonTextures(pauseButtonTexture, shopButtonTexture);
            

            // Инициализация рулетки
            _rouletteManager = new RouletteManager(_player, _weapons, GraphicsDevice, _debugTexture, _font);
            _rouletteMenu = new RouletteMenu(_rouletteManager, GraphicsDevice, _debugTexture, _font);

            // Инициализация магазина бонусов (после загрузки текстур)
            _bonusShopInterface = new BonusShopInterface(_bonusShop, GraphicsDevice, _debugTexture, _font);

            // Инициализация меню паузы
            _pauseMenu = new PauseMenu(GraphicsDevice, _debugTexture, _font);

            // Инициализация экрана загрузки
            _loadingScreen = new LoadingScreen(GraphicsDevice, _debugTexture, _font);

            // Инициализация экранов Game Over и Victory (если не были инициализированы в Initialize)
            if (_gameOverScreen == null)
                _gameOverScreen = new GameOverScreen(GraphicsDevice, _debugTexture, _font);
            if (_victoryScreen == null)
                _victoryScreen = new VictoryScreen(GraphicsDevice, _debugTexture, _font);
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
            MouseState currentMs = Mouse.GetState(); // Добавляем получение состояния мыши

            // Обработка ESC - новое поведение
            if (currentKs.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                if (_currentGameState == GameState.Playing)
                {
                    // В игре - открываем меню паузы
                    _pauseMenu.Show();
                    Game1.CurrentState = GameState.Paused;
                }
                else if (_currentGameState == GameState.Paused)
                {
                    // В паузе - закрываем меню паузы
                    _pauseMenu.Hide();
                    Game1.CurrentState = GameState.Playing;
                }
                else if (_currentGameState == GameState.BonusShop)
                {
                    // В магазине - закрываем магазин
                    _bonusShop.Hide();
                    Game1.CurrentState = GameState.Playing;
                }
                else if (_currentGameState == GameState.MainMenu)
                {
                    // В главном меню - выходим из игры
                    Exit();
                }
            }

            // Обработка клавиши B для открытия магазина из состояния Playing
            if (currentKs.IsKeyDown(Keys.B) && !_previousKeyboardState.IsKeyDown(Keys.B) &&
                _currentGameState == GameState.Playing)
            {
                _bonusShop.Show();
                Game1.CurrentState = GameState.BonusShop;
                System.Diagnostics.Debug.WriteLine("Магазин открыт по клавише B");
            }

            // Обработка кликов по кнопкам HUD
            if (currentMs.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                Point mousePos = currentMs.Position;

                if (_gameHUD.IsPauseButtonClicked(mousePos) && _currentGameState == GameState.Playing)
                {
                    // Открываем меню паузы по кнопке
                    _pauseMenu.Show();
                    Game1.CurrentState = GameState.Paused;
                    System.Diagnostics.Debug.WriteLine("Меню паузы открыто по кнопке");
                }
                else if (_gameHUD.IsShopButtonClicked(mousePos) && _currentGameState == GameState.Playing)
                {
                    // Открываем магазин бонусов
                    _bonusShop.Show();
                    Game1.CurrentState = GameState.BonusShop;
                    System.Diagnostics.Debug.WriteLine("Магазин бонусов открыт");
                }
            }

            // Обновляем приватное состояние из статического для работы switch'а
            _currentGameState = Game1.CurrentState;

            // Управление музыкой в зависимости от состояния
            UpdateMusicForState();

            switch (_currentGameState)
            {
                case GameState.MainMenu:
                    Game1.CurrentState = _mainMenu.Update(gameTime);
                    var menuState = _mainMenu.Update(gameTime);
                    if (menuState == GameState.StartMenu)
                    {
                        Game1.CurrentState = GameState.StartMenu;
                    }
                    else if (menuState == GameState.ExitGame)
                    {
                        Exit(); // Выход из игры
                    }
                    break;

                case GameState.StartMenu:
                    var newState = _startMenu.Update(gameTime);
                    if (newState == GameState.Loading) // Меняем проверку
                    {
                        // Сохраняем выбранное оружие
                        _selectedStartingWeapon = _startMenu.SelectedWeapon;
                    }
                    Game1.CurrentState = newState;
                    break;

                case GameState.Loading:
                    bool startGame = _loadingScreen.Update(gameTime);

                    if (startGame)
                    {
                        // Останавливаем музыку меню перед началом игры
                        _musicManager.StopMusicForGameStart();

                        // Инициализируем игрока с выбранным оружием
                        InitializePlayerWeapon();

                        // Сбрасываем менеджеры при начале новой игры
                        _levelManager.Reset();
                        _itemManager.Clear();
                        _musicManager.PlayLevelMusic(_levelManager.CurrentLevel);

                        Game1.CurrentState = GameState.Playing;
                        _loadingScreen.Reset();
                        System.Diagnostics.Debug.WriteLine("Игра начата по нажатию кнопки");
                    }
                    break;

                case GameState.Playing:
                    _survivalTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Обновляем HUD
                    _gameHUD.UpdateGameStats(_survivalTime, _killCount);
                    _gameHUD.Update(gameTime);

                    // ОТЛАДКА: проверяем состояние игрока
                    System.Diagnostics.Debug.WriteLine($"Состояние игрока: Alive={_player.IsAlive}, Health={_player.CurrentHealth}");

                    // Воспроизводим музыку текущего уровня
                    _musicManager.PlayLevelMusic(_levelManager.CurrentLevel);

                    if (currentKs.IsKeyDown(Keys.F8) && !_previousKeyboardState.IsKeyDown(Keys.F8))
                    {
                        // Принудительная победа для тестирования
                        Game1.CurrentState = GameState.Victory;
                        _victoryScreen.Show();
                        System.Diagnostics.Debug.WriteLine("🎉 Тестовый переход к победе по F8");
                        break;
                    }

                    // Проверки состояния
                    if (!_player.IsAlive)
                    {
                        System.Diagnostics.Debug.WriteLine("Переход в состояние GameOver!");
                        Game1.CurrentState = GameState.GameOver;
                        _gameOverScreen.Show();
                        _musicManager.StopMusic();
                        break;
                    }

                    // Обычный LevelUp
                    if (_player.IsLevelUpPending)
                    {
                        Game1.CurrentState = GameState.LevelUp;
                        break;
                    }

                    // Игровая логика
                    _player.Update(gameTime);
                    _camera.Follow();
                    _spawnManager.Update(gameTime);

                    // Обновление менеджера предметов
                    _itemManager.Update(gameTime);

                    // Обновление взрывов динамита
                    DynamiteExplosion.UpdateAll(gameTime, _enemies);

                    // Обработка кликов по кнопкам HUD
                    MouseState mouseState = Mouse.GetState();
                    if (mouseState.LeftButton == ButtonState.Pressed &&
                        _previousMouseState.LeftButton == ButtonState.Released)
                    {
                        Point mousePos = mouseState.Position;

                        if (_gameHUD.IsPauseButtonClicked(mousePos))
                        {
                            // TODO: Реализовать функционал паузы
                            System.Diagnostics.Debug.WriteLine("Кнопка паузы нажата");
                        }
                        else if (_gameHUD.IsShopButtonClicked(mousePos))
                        {
                            // Открываем магазин бонусов
                            _bonusShop.Show();
                            Game1.CurrentState = GameState.BonusShop;
                            System.Diagnostics.Debug.WriteLine("Магазин бонусов открыт");
                        }
                    }

                    for (int i = _enemies.Count - 1; i >= 0; i--)
                    {
                        Enemy enemy = _enemies[i];
                        enemy.Update(gameTime);

                        if (!enemy.IsAlive)
                        {
                            _killCount++;

                            // Дроп элитного врага
                            if (enemy is EliteEnemy)
                            {
                                // РЕГИСТРИРУЕМ УБИЙСТВО ЭЛИТНОГО ВРАГА
                                int oldLevel = _levelManager.CurrentLevel;
                                _levelManager.EliteKilled();

                                // ОБНОВЛЯЕМ ТЕКСТУРУ ПОЛА ЕСЛИ УРОВЕНЬ ИЗМЕНИЛСЯ
                                if (_levelManager.CurrentLevel != oldLevel)
                                {
                                    UpdateFloorTexture();
                                    _gameHUD.ShowStageAnnouncement(_levelManager.CurrentLevel);
                                }

                                // Проверка победы (8 этап + 2 элитных врага убито)
                                if (_levelManager.CurrentLevel == 8 && _levelManager.ElitesKilled >= 2)
                                {
                                    Game1.CurrentState = GameState.Victory;
                                    _victoryScreen.Show();
                                    System.Diagnostics.Debug.WriteLine("УСЛОВИЕ ПОБЕДЫ ВЫПОЛНЕНО! Активируем экран победы.");
                                    break;
                                }

                                // Элитные враги дропают 5 монет
                                for (int j = 0; j < 5; j++)
                                {
                                    _itemManager.AddCoin(enemy.Position, 1);
                                }

                                // 3 эссенции опыта от элитного врага
                                for (int j = 0; j < 3; j++)
                                {
                                    _itemManager.AddExperienceOrb(enemy.Position, 1);
                                }

                                // 100% шанс дропа золотого сердца от элитного врага
                                _itemManager.AddGoldenHealthOrb(enemy.Position, 1.0f);

                                _rouletteManager.StartRoulette();
                                Game1.CurrentState = GameState.Roulette;
                            }
                            else
                            {
                                // Обычный дроп опыта через менеджер предметов
                                _itemManager.AddExperienceOrb(enemy.Position, 1);

                                // Шанс дропа сердца 2%
                                if (Game1.Random.NextDouble() < 0.02)
                                {
                                    _itemManager.AddHealthOrb(enemy.Position, 0.25f);
                                }

                                // Шанс дропа монеты 25% (1 к 4)
                                if (Game1.Random.NextDouble() < 0.25)
                                {
                                    _itemManager.AddCoin(enemy.Position, 1);
                                }

                                // Шанс дропа эссенции опыта 50%
                                if (Game1.Random.NextDouble() < 0.5)
                                {
                                    _itemManager.AddExperienceOrb(enemy.Position, 1);
                                }

                                // Шанс дропа магнита 2% от обычных врагов
                                if (Game1.Random.NextDouble() < 0.02 && !(enemy is EliteEnemy))
                                {
                                    _itemManager.AddMagnet(enemy.Position);
                                }

                                // НОВЫЙ: Шанс дропа динамита 2% от обычных врагов
                                if (Game1.Random.NextDouble() < 0.02)
                                {
                                    _itemManager.AddDynamite(enemy.Position);
                                }
                            }

                            _enemies.RemoveAt(i);
                        }
                    }

                    // Обработка коллизий с врагами
                    if (_player.IsAlive)
                    {
                        var playerBounds = GetBounds(_player);
                        var collidingEnemies = _enemies.Where(enemy =>
                            enemy.IsAlive &&
                            GetBounds(enemy).Intersects(playerBounds)
                        ).ToList();

                        if (collidingEnemies.Any())
                        {
                            if (!_player.IsInvulnerable)
                            {
                                _player.TakeDamage(collidingEnemies.First().Damage);
                            }
                        }
                    }

                    // Обновление оружия
                    foreach (var weapon in _weapons)
                    {
                        weapon.Update(gameTime);
                        weapon.Attack(gameTime, _enemies);

                        // Особый случай для RouletteBall - обновляем границы шариков
                        if (weapon is RouletteBall rouletteBall)
                        {
                            // Создаем актуальные границы экрана
                            Rectangle screenBounds = new Rectangle(
                                (int)_camera.Position.X,
                                (int)_camera.Position.Y,
                                GraphicsDevice.Viewport.Width,
                                GraphicsDevice.Viewport.Height
                            );

                            // Обновляем границы для ВСЕХ активных шариков
                            foreach (var ball in rouletteBall.ActiveBalls) // ИСПОЛЬЗУЕМ ActiveBalls вместо ActiveBall
                            {
                                if (ball.IsActive)
                                {
                                    ball.ScreenBounds = screenBounds;
                                }
                            }
                        }
                    }
                    break; // ДОБАВЬТЕ break В КОНЦЕ case

                case GameState.Paused:
                    _pauseMenu.Update();

                    _musicManager.PauseMusic();

                    // Проверяем, не изменилось ли состояние игры через меню паузы
                    if (Game1.CurrentState == GameState.MainMenu)
                    {
                        // Сбрасываем игру
                        _enemies.Clear();
                        _weapons.Clear();
                        _levelManager.Reset();
                        _itemManager.Clear();
                        _survivalTime = 0f;
                        _killCount = 0;

                        // Запускаем музыку меню
                        _musicManager.PlayMenuMusic();

                        _pauseMenu.Hide();
                        System.Diagnostics.Debug.WriteLine("Возврат в главное меню, музыка меню запущена");
                    }
                    else if (!_pauseMenu.IsVisible)
                    {
                        // Если меню паузы скрылось, возвращаемся в игру
                        Game1.CurrentState = GameState.Playing;
                    }
                    break;

                case GameState.LevelUp:
                    if (_levelUpMenu.CurrentOptions.Count == 0) { _levelUpMenu.GenerateOptions(); }
                    _levelUpMenu.Update(gameTime);
                    if (!_player.IsLevelUpPending)
                    {
                        Game1.CurrentState = GameState.Playing;
                        _levelUpMenu.CurrentOptions.Clear();
                    }
                    break;

                case GameState.Roulette:
                    _rouletteManager.Update(gameTime);

                    // Проверяем, завершилась ли рулетка
                    if (!_rouletteManager.IsActive)
                    {
                        Game1.CurrentState = GameState.Playing;

                        // ПОСЛЕ РУЛЕТКИ показываем анимацию этапа, если уровень изменился
                        if (_levelManager.ElitesKilled > 0 && _levelManager.ElitesKilled % 2 == 0)
                        {
                            _gameHUD.ShowStageAnnouncement(_levelManager.CurrentLevel);
                        }
                    }
                    break;

                case GameState.BonusShop:
                    _bonusShop.Update(gameTime);
                    _bonusShopInterface.UpdateInput();

                    // Если магазин скрылся, возвращаемся в игру
                    if (!_bonusShop.IsVisible)
                    {
                        Game1.CurrentState = GameState.Playing;
                        System.Diagnostics.Debug.WriteLine("Возврат в игру после закрытия магазина");
                    }
                    break;

                case GameState.GameOver:
                    _musicManager.StopMusic();
                    _gameOverScreen.Update();

                    // Если экран скрылся (после нажатия кнопки), сбрасываем игру
                    if (!_gameOverScreen.IsVisible)
                    {
                        // Сбрасываем игру при выходе из Game Over
                        _enemies.Clear();
                        _weapons.Clear();
                        _levelManager.Reset();
                        _itemManager.Clear();
                        _survivalTime = 0f;
                        _killCount = 0;

                        // Состояние уже изменено кнопками в GameOverScreen
                        System.Diagnostics.Debug.WriteLine("Игра сброшена после Game Over");
                    }
                    break;
                case GameState.Victory:
                    _musicManager.StopMusic();
                    _victoryScreen.Update();

                    // Если экран скрылся (после нажатия кнопки), сбрасываем игру
                    if (!_victoryScreen.IsVisible)
                    {
                        // Сбрасываем игру при выходе из Victory
                        _enemies.Clear();
                        _weapons.Clear();
                        _levelManager.Reset();
                        _itemManager.Clear();
                        _survivalTime = 0f;
                        _killCount = 0;

                        // Состояние уже изменено кнопками в VictoryScreen
                        System.Diagnostics.Debug.WriteLine("Игра сброшена после победы");
                    }
                    break;
            }

            // В конце метода Update обновляем предыдущее состояние
            _previousKeyboardState = currentKs;
            _previousMouseState = currentMs; // Добавляем это
            base.Update(gameTime);
        }

        // Статический метод для активации магнита из других классов
        public static void ActivateMagnet(float duration, float speed)
        {
            // Получаем доступ к ItemManager через существующий экземпляр
            // Этот метод будет вызван из Magnet.ApplyEffect()
            // Временное решение - нужно передать ссылку на ItemManager в Magnet
        }

        // Метод для обновления текстуры пола
        private void UpdateFloorTexture()
        {
            try
            {
                Texture2D newFloorTexture = _levelManager.LoadCurrentLevelFloorTexture(Content);
                _worldGeneration.ChangeFloorTexture(newFloorTexture);
                Debug.WriteLine($"🔄 Обновлена текстура пола для уровня {_levelManager.CurrentLevel}");
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка обновления текстуры пола: {ex.Message}");
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

            // Визуализация зоны спавна (для отладки)
            Vector2 screenCenter = _player.Position;
            float screenLeft = screenCenter.X - GraphicsDevice.Viewport.Width / 2;
            float screenRight = screenCenter.X + GraphicsDevice.Viewport.Width / 2;
            float screenTop = screenCenter.Y - GraphicsDevice.Viewport.Height / 2;
            float screenBottom = screenCenter.Y + GraphicsDevice.Viewport.Height / 2;

            // Рисуем границы экрана
            Rectangle screenBounds = new Rectangle(
                (int)screenLeft, (int)screenTop,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height
            );
            _spriteBatch.Draw(_debugTexture, screenBounds, Color.Green * 0.1f);

            // Отрисовка предметов через менеджер
            _itemManager.Draw(_spriteBatch, _debugTexture);

            // Отрисовка огненных областей первыми (под всеми)
            foreach (var weapon in _weapons)
            {
                if (weapon is MolotovCocktail molotov)
                {
                    molotov.DrawProjectiles(_spriteBatch);
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

                    // Отрисовываем ВСЕ шарики (ActiveBalls вместо ActiveBall)
                    foreach (var ball in roulette.ActiveBalls)
                    {
                        if (ball.IsActive)
                        {
                            ball.Draw(_spriteBatch, _debugTexture);
                        }
                    }

                    // ОТЛАДКА: рисуем границы отскока (только если есть активные шарики)
                    if (roulette.ActiveBalls.Count > 0)
                    {
                        // Используем актуальные границы из первого шарика
                        Rectangle bounds = roulette.ActiveBalls[0].ScreenBounds;
                        DrawDebugRectangle(_spriteBatch, bounds, Color.Red * 0.2f);
                    }
                }
            }
        }

        private void DrawDebugRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            // Верхняя линия
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            // Нижняя линия
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), color);
            // Левая линия
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            // Правая линия
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), color);
        }

        private void DrawGameOverScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.DarkRed * 0.8f
            );

            string text = "ИГРА ОКОНЧЕНА";
            Vector2 size = _font.MeasureString(text);
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width - size.X) / 2,
                (GraphicsDevice.Viewport.Height - size.Y) / 2
            );
            spriteBatch.DrawString(_font, text, position, Color.White);
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

        // ДОБАВЬТЕ: фиксированные границы экрана для отскоков
        public static Rectangle GetScreenBounds(Camera camera, Viewport viewport)
        {
            return new Rectangle(
                (int)camera.Position.X,
                (int)camera.Position.Y,
                viewport.Width,
                viewport.Height
            );
        }

        protected override void UnloadContent()
        {
            _musicManager?.Dispose();
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
    }
}