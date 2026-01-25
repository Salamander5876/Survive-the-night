using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class DiceWeapon : Weapon
    {
        public List<DiceProjectile> ActiveDice { get; private set; } = new List<DiceProjectile>();

        public int DamageBonusLevel { get; private set; } = 0;
        public int PierceBonusLevel { get; private set; } = 0;
        public int CooldownLevel { get; private set; } = 0;


        private float _baseCooldown = 3.0f;
        public float CurrentCooldown => Math.Max(0.1f, _baseCooldown - CooldownLevel * 0.5f); // Минимум 0.5 сек

        private float _cooldownTimer = 0f;
        private float _spawnTimer = 0f;
        private int _currentDiceValue = 0;
        private bool _isSpawning = false;
        private bool _currentOrbitDirection = true;

        private enum WeaponState
        {
            Ready,
            Spawning,
            Active,
            Cooldown
        }

        private WeaponState _currentState = WeaponState.Ready;

        // ИЗМЕНЕНИЕ: вместо отслеживания всех пораженных врагов, 
        // отслеживаем врагов, которые СЕЙЧАС находятся в коллизии
        private Dictionary<DiceProjectile, HashSet<Enemy>> _currentlyCollidingEnemies = new Dictionary<DiceProjectile, HashSet<Enemy>>();

        public DiceWeapon(Player player) : base(player, WeaponType.Regular, WeaponName.Dice, 0f, 1)
        {
            _currentState = WeaponState.Ready;
            _cooldownTimer = 0f;
        }

        public override void LevelUp() { }

        public void UpgradeDamage()
        {
            if (DamageBonusLevel >= 5) return;
            DamageBonusLevel++;
        }

        public void UpgradePierce()
        {
            if (PierceBonusLevel >= 5) return;
            PierceBonusLevel++;
        }

        public void UpgradeCooldown()
        {
            if (CooldownLevel >= 5) return;
            CooldownLevel++;
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
                    _currentOrbitDirection = !_currentOrbitDirection;

                    // Очищаем словарь коллизий при старте нового цикла
                    _currentlyCollidingEnemies.Clear();
                    break;

                case WeaponState.Spawning:
                    UpdateDiceAggressiveFollow();

                    _spawnTimer -= deltaTime;

                    if (_spawnTimer <= 0f && _currentDiceValue < 6)
                    {
                        SpawnDice(_currentDiceValue + 1);
                        _currentDiceValue++;
                        _spawnTimer = 0.2f;

                        if (_currentDiceValue >= 6)
                        {
                            _currentState = WeaponState.Active;
                            foreach (var dice in ActiveDice)
                            {
                                dice.StartOrbiting();
                                // Инициализируем множество для отслеживания коллизий
                                if (!_currentlyCollidingEnemies.ContainsKey(dice))
                                {
                                    _currentlyCollidingEnemies[dice] = new HashSet<Enemy>();
                                }
                            }
                        }
                    }
                    break;

                case WeaponState.Active:
                    UpdateDiceNormalOrbit(deltaTime);

                    bool allDiceDestroyed = true;
                    for (int i = ActiveDice.Count - 1; i >= 0; i--)
                    {
                        var dice = ActiveDice[i];
                        if (dice.IsActive)
                        {
                            allDiceDestroyed = false;
                        }
                        else
                        {
                            // Удаляем кубик из словаря коллизий при уничтожении
                            if (_currentlyCollidingEnemies.ContainsKey(dice))
                            {
                                _currentlyCollidingEnemies.Remove(dice);
                            }
                            ActiveDice.RemoveAt(i);
                        }
                    }

                    if (allDiceDestroyed && ActiveDice.Count == 0)
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
            foreach (var dice in ActiveDice)
            {
                if (dice.IsActive)
                {
                    dice.UpdateAggressiveFollow();
                }
            }
        }

        private void UpdateDiceNormalOrbit(float deltaTime)
        {
            foreach (var dice in ActiveDice)
            {
                if (dice.IsActive)
                {
                    dice.UpdateNormalOrbit(deltaTime);
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

        private void SpawnDice(int diceValue)
        {
            if (diceValue < 1 || diceValue > 6 || Player == null) return;

            // ИЗМЕНЕНО: базовый урон костей увеличен
            int baseDiceDamage = GetBaseDiceDamage(diceValue); // Новый метод для расчета урона
            int diceDamage = baseDiceDamage + DamageBonusLevel;
            int basePierce = 7 - diceValue;
            int totalPierce = basePierce + PierceBonusLevel;

            var dice = new DiceProjectile(
                diceValue,
                Player,
                _currentOrbitDirection,
                diceDamage,
                totalPierce
            );

            ActiveDice.Add(dice);
            // Инициализируем множество для этого кубика
            _currentlyCollidingEnemies[dice] = new HashSet<Enemy>();
        }

        // НОВЫЙ МЕТОД: расчет базового урона для кости
        private int GetBaseDiceDamage(int diceValue)
        {
            switch (diceValue)
            {
                case 1: return 1;
                case 2: return 1;
                case 3: return 3;
                case 4: return 4;
                case 5: return 5;
                case 6: return 6;
                default: return 1;
            }
        }

        private void CheckCollisions(List<Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0) return;

            foreach (var dice in ActiveDice)
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

                // 3. Определяем врагов, которые только что вошли в коллизию (новые коллизии)
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

                        System.Diagnostics.Debug.WriteLine($"Dice {dice.DiceValue} hit enemy for {dice.Damage} damage. Hits left: {dice.HitsLeft}");

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

            // 6. Очищаем словарь от деактивированных кубиков
            var keysToRemove = new List<DiceProjectile>();
            foreach (var kvp in _currentlyCollidingEnemies)
            {
                if (!kvp.Key.IsActive || !ActiveDice.Contains(kvp.Key))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _currentlyCollidingEnemies.Remove(key);
            }
        }

        public string GetDebugInfo()
        {
            return $"State: {_currentState}, Dice: {ActiveDice.Count}, Cooldown: {_cooldownTimer:0.0}s";
        }
    }
}