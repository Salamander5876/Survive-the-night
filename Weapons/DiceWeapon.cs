using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Projectiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Survive_the_night.Weapons
{
    public class DiceWeapon : Weapon
    {
        public List<DiceProjectile> ActiveDice { get; private set; } = new List<DiceProjectile>();

        public int DamageBonusLevel { get; private set; } = 0;
        public int PierceBonusLevel { get; private set; } = 0;
        public int CooldownLevel { get; private set; } = 0;

        // ИЗМЕНЕНО: начальная перезарядка 5.1 сек, улучшение -0.5 сек
        private float _baseCooldown = 5.1f;
        public float CurrentCooldown => Math.Max(0.1f, _baseCooldown - (CooldownLevel * 0.5f)); // Минимум 0.1 сек

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

        private static SoundEffect _hitSound;
        private Dictionary<DiceProjectile, List<Enemy>> _hitEnemies = new Dictionary<DiceProjectile, List<Enemy>>();

        public DiceWeapon(Player player) : base(player, WeaponType.Regular, WeaponName.Dice, 0f, 1)
        {
            _currentState = WeaponState.Ready;
            _cooldownTimer = 0f;
        }

        public static void SetHitSound(SoundEffect sound)
        {
            _hitSound = sound;
        }

        public override void LevelUp() { }

        public void UpgradeDamage()
        {
            if (DamageBonusLevel >= 10) return;
            DamageBonusLevel++;
        }

        public void UpgradePierce()
        {
            if (PierceBonusLevel >= 10) return;
            PierceBonusLevel++;
        }

        public void UpgradeCooldown()
        {
            if (CooldownLevel >= 10) return;
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
                            if (_hitEnemies.ContainsKey(dice))
                            {
                                _hitEnemies.Remove(dice);
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
            _hitEnemies[dice] = new List<Enemy>();
        }

        // НОВЫЙ МЕТОД: расчет базового урона для кости
        private int GetBaseDiceDamage(int diceValue)
        {
            switch (diceValue)
            {
                case 1: return 2; // УВЕЛИЧЕНО с 1 до 2
                case 2: return 4; // УВЕЛИЧЕНО с 2 до 4
                case 3: return 6; // УВЕЛИЧЕНО с 3 до 6
                case 4: return 8; // УВЕЛИЧЕНО с 4 до 8
                case 5: return 10; // УВЕЛИЧЕНО с 5 до 10
                case 6: return 12; // УВЕЛИЧЕНО с 6 до 12
                default: return 1;
            }
        }

        private void CheckCollisions(List<Enemy> enemies)
        {
            for (int i = ActiveDice.Count - 1; i >= 0; i--)
            {
                var dice = ActiveDice[i];
                if (!dice.IsActive) continue;

                List<Enemy> alreadyHit = _hitEnemies.ContainsKey(dice) ? _hitEnemies[dice] : new List<Enemy>();

                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var enemy = enemies[j];
                    if (!enemy.IsAlive) continue;

                    if (alreadyHit.Contains(enemy)) continue;

                    if (dice.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(dice.Damage);

                        alreadyHit.Add(enemy);
                        _hitEnemies[dice] = alreadyHit;

                        dice.HitsLeft--;
                        dice.OnHitEnemy();

                        if (dice.HitsLeft <= 0)
                        {
                            dice.IsActive = false;
                            break;
                        }
                    }
                }
            }
        }

        public string GetDebugInfo()
        {
            return $"State: {_currentState}, Dice: {ActiveDice.Count}, Cooldown: {_cooldownTimer:0.0}s";
        }
    }
}