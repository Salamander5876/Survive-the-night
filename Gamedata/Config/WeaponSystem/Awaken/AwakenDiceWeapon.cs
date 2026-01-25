// Gamedata/Config/WeaponSystem/Awaken/AwakenDiceWeapon.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Awaken
{
    public class AwakenDiceWeapon : Weapon
    {
        // Списки кубиков для двух кругов
        public List<AwakenDiceProjectile> ActiveDiceCircle1 { get; private set; } = new List<AwakenDiceProjectile>();
        public List<AwakenDiceProjectile> ActiveDiceCircle2 { get; private set; } = new List<AwakenDiceProjectile>();

        // Параметры пробужденной версии
        private const float BASE_COOLDOWN = 3.0f;
        private const float COOLDOWN_REDUCTION_PER_LEVEL = 0.5f;
        private const int COOLDOWN_LEVEL = 5; // Максимально прокачано
        public float CurrentCooldown => Math.Max(0.5f, BASE_COOLDOWN - COOLDOWN_LEVEL * COOLDOWN_REDUCTION_PER_LEVEL);

        private float _cooldownTimer = 0f;
        private float _spawnTimer = 0f;
        private int _currentDiceValue = 0;
        private bool _firstCircleOrbitDirection = true;

        // Скорость вращения (в градусах в секунду) - теперь ОДИНАКОВАЯ для обоих кругов
        private const float ORBIT_SPEED = 300f;

        // Радиусы орбит - второй круг БОЛЬШЕ
        private const float ORBIT_RADIUS_CIRCLE1 = 100f;
        private const float ORBIT_RADIUS_CIRCLE2 = 150f;

        // Состояние оружия
        private enum WeaponState
        {
            Ready,
            Spawning,
            Active,
            Cooldown
        }

        private WeaponState _currentState = WeaponState.Ready;

        // Отслеживание коллизий для обоих кругов
        private Dictionary<AwakenDiceProjectile, HashSet<Enemy>> _currentlyCollidingEnemies = new Dictionary<AwakenDiceProjectile, HashSet<Enemy>>();

        public AwakenDiceWeapon(Player player) : base(player, WeaponType.Regular, WeaponName.Dice, 0f, 1)
        {
            _currentState = WeaponState.Ready;
            _cooldownTimer = 0f;
            _firstCircleOrbitDirection = true;
        }

        public override void LevelUp()
        {
            // Пробужденное оружие не прокачивается дальше
        }

        // Расчет базового урона для пробужденной версии
        private int GetAwakenBaseDiceDamage(int diceValue)
        {
            switch (diceValue)
            {
                case 1: return 6;
                case 2: return 7;
                case 3: return 8;
                case 4: return 9;
                case 5: return 10;
                case 6: return 11;
                default: return 6;
            }
        }

        // Расчет пробитий для пробужденной версии
        private int GetAwakenPierceCount(int diceValue)
        {
            int basePierce = 7 - diceValue; // Оригинальная формула
            int pierceBonus = 5; // Максимально прокачано
            return basePierce + pierceBonus;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (_currentState)
            {
                case WeaponState.Ready:
                    _currentState = WeaponState.Spawning;
                    _currentDiceValue = 0;
                    _spawnTimer = 0f;
                    _firstCircleOrbitDirection = !_firstCircleOrbitDirection;

                    // Очищаем словарь коллизий при старте нового цикла
                    _currentlyCollidingEnemies.Clear();
                    break;

                case WeaponState.Spawning:
                    UpdateDiceAggressiveFollow();

                    _spawnTimer -= deltaTime;

                    if (_spawnTimer <= 0f && _currentDiceValue < 6)
                    {
                        // Спавним кубик в обоих кругах одновременно
                        SpawnDicePair(_currentDiceValue + 1);
                        _currentDiceValue++;
                        _spawnTimer = 0.15f; // Быстрее спавн для пробужденной версии

                        if (_currentDiceValue >= 6)
                        {
                            _currentState = WeaponState.Active;
                            foreach (var dice in ActiveDiceCircle1)
                            {
                                dice.StartOrbiting();
                                if (!_currentlyCollidingEnemies.ContainsKey(dice))
                                {
                                    _currentlyCollidingEnemies[dice] = new HashSet<Enemy>();
                                }
                            }
                            foreach (var dice in ActiveDiceCircle2)
                            {
                                dice.StartOrbiting();
                                if (!_currentlyCollidingEnemies.ContainsKey(dice))
                                {
                                    _currentlyCollidingEnemies[dice] = new HashSet<Enemy>();
                                }
                            }
                        }
                    }
                    break;

                case WeaponState.Active:
                    UpdateDiceOrbit(deltaTime);

                    bool allDiceDestroyed = true;

                    // Проверяем первый круг
                    for (int i = ActiveDiceCircle1.Count - 1; i >= 0; i--)
                    {
                        var dice = ActiveDiceCircle1[i];
                        if (dice.IsActive)
                        {
                            allDiceDestroyed = false;
                        }
                        else
                        {
                            if (_currentlyCollidingEnemies.ContainsKey(dice))
                            {
                                _currentlyCollidingEnemies.Remove(dice);
                            }
                            ActiveDiceCircle1.RemoveAt(i);
                        }
                    }

                    // Проверяем второй круг
                    for (int i = ActiveDiceCircle2.Count - 1; i >= 0; i--)
                    {
                        var dice = ActiveDiceCircle2[i];
                        if (dice.IsActive)
                        {
                            allDiceDestroyed = false;
                        }
                        else
                        {
                            if (_currentlyCollidingEnemies.ContainsKey(dice))
                            {
                                _currentlyCollidingEnemies.Remove(dice);
                            }
                            ActiveDiceCircle2.RemoveAt(i);
                        }
                    }

                    // Перезарядка начинается только когда все кубики обоих кругов уничтожены
                    if (allDiceDestroyed && ActiveDiceCircle1.Count == 0 && ActiveDiceCircle2.Count == 0)
                    {
                        _currentState = WeaponState.Cooldown;
                        _cooldownTimer = CurrentCooldown;
                    }
                    break;

                case WeaponState.Cooldown:
                    _cooldownTimer -= deltaTime;
                    if (_cooldownTimer <= 0f)
                    {
                        _cooldownTimer = 0f;
                        _currentState = WeaponState.Ready;
                    }
                    break;
            }
        }

        private void UpdateDiceAggressiveFollow()
        {
            foreach (var dice in ActiveDiceCircle1)
            {
                if (dice.IsActive)
                {
                    dice.UpdateAggressiveFollow();
                }
            }
            foreach (var dice in ActiveDiceCircle2)
            {
                if (dice.IsActive)
                {
                    dice.UpdateAggressiveFollow();
                }
            }
        }

        private void UpdateDiceOrbit(float deltaTime)
        {
            // Первый круг вращается в направлении _firstCircleOrbitDirection
            foreach (var dice in ActiveDiceCircle1)
            {
                if (dice.IsActive)
                {
                    // Скорость положительная для первого круга
                    dice.UpdateOrbitPosition(deltaTime, ORBIT_SPEED, ORBIT_RADIUS_CIRCLE1);
                }
            }

            // Второй круг вращается в ПРОТИВОПОЛОЖНУЮ сторону
            foreach (var dice in ActiveDiceCircle2)
            {
                if (dice.IsActive)
                {
                    // Скорость ОТРИЦАТЕЛЬНАЯ для второго круга
                    dice.UpdateOrbitPosition(deltaTime, -ORBIT_SPEED, ORBIT_RADIUS_CIRCLE2);
                }
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            if (_currentState == WeaponState.Active)
            {
                CheckCollisions(enemies);
            }
        }

        private void SpawnDicePair(int diceValue)
        {
            if (diceValue < 1 || diceValue > 6 || Player == null) return;

            // Параметры для пробужденной версии
            int baseDamage = GetAwakenBaseDiceDamage(diceValue);
            int pierceCount = GetAwakenPierceCount(diceValue);

            // ПЕРВЫЙ круг: вращается в направлении _firstCircleOrbitDirection
            bool firstCircleClockwise = _firstCircleOrbitDirection;

            // ВТОРОЙ круг: вращается в ПРОТИВОПОЛОЖНУЮ сторону
            bool secondCircleClockwise = !_firstCircleOrbitDirection;

            // Создаем кубик для первого круга (обычные текстуры)
            var dice1 = new AwakenDiceProjectile(
                diceValue,
                Player,
                firstCircleClockwise, // Направление вращения первого круга
                baseDamage,
                pierceCount,
                false // Первый круг - обычные текстуры
            );
            dice1.SetOrbitRadius(ORBIT_RADIUS_CIRCLE1);
            ActiveDiceCircle1.Add(dice1);
            _currentlyCollidingEnemies[dice1] = new HashSet<Enemy>();

            // Создаем кубик для второго круга (мега текстуры)
            var dice2 = new AwakenDiceProjectile(
                diceValue,
                Player,
                secondCircleClockwise, // Направление вращения второго круга (противоположное)
                baseDamage,
                pierceCount,
                true // Второй круг - мега текстуры
            );
            dice2.SetOrbitRadius(ORBIT_RADIUS_CIRCLE2);
            ActiveDiceCircle2.Add(dice2);
            _currentlyCollidingEnemies[dice2] = new HashSet<Enemy>();
        }

        private void CheckCollisions(List<Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0) return;

            // Проверяем коллизии для всех кубиков обоих кругов
            CheckCircleCollisions(ActiveDiceCircle1, enemies);
            CheckCircleCollisions(ActiveDiceCircle2, enemies);
        }

        private void CheckCircleCollisions(List<AwakenDiceProjectile> diceList, List<Enemy> enemies)
        {
            foreach (var dice in diceList)
            {
                if (!dice.IsActive) continue;

                // 1. Определяем врагов, которые СЕЙЧАС находятся в коллизии
                var currentCollisions = new HashSet<Enemy>();

                foreach (var enemy in enemies)
                {
                    if (!enemy.IsAlive) continue;

                    // Проверяем коллизию
                    if (dice.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        currentCollisions.Add(enemy);
                    }
                }

                // 2. Получаем врагов, которые были в коллизии в прошлом кадре
                var previousCollisions = _currentlyCollidingEnemies.ContainsKey(dice)
                    ? _currentlyCollidingEnemies[dice]
                    : new HashSet<Enemy>();

                // 3. Определяем врагов, которые только что вошли в коллизию
                var newCollisions = new HashSet<Enemy>(currentCollisions);
                newCollisions.ExceptWith(previousCollisions);

                // 4. Наносим урон только тем врагам, которые только что вошли в коллизию
                foreach (var enemy in newCollisions)
                {
                    if (dice.HitsLeft > 0)
                    {
                        enemy.TakeDamage(dice.Damage);
                        dice.OnHitEnemy();
                        dice.HitsLeft--;

                        Debug.WriteLine($"Awaken Dice {dice.DiceValue} hit enemy for {dice.Damage} damage. Hits left: {dice.HitsLeft}");

                        // Если пробития закончились, деактивируем кубик
                        if (dice.HitsLeft <= 0)
                        {
                            dice.IsActive = false;
                            break;
                        }
                    }
                }

                // 5. Обновляем множество коллизий для следующего кадра
                _currentlyCollidingEnemies[dice] = currentCollisions;
            }
        }

        public string GetDebugInfo()
        {
            return $"Awaken State: {_currentState}, Circle1: {ActiveDiceCircle1.Count}, Circle2: {ActiveDiceCircle2.Count}, Cooldown: {_cooldownTimer:0.0}s";
        }
    }
}