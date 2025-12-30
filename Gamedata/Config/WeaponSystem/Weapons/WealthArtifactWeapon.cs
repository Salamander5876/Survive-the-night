// WealthArtifactWeapon.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Weapons
{
    public class WealthArtifactWeapon : Weapon
    {
        // Уровни прокачки
        public int ArtifactCountLevel { get; private set; } = 0; // +1 артефакт за уровень
        public int GroupCountLevel { get; private set; } = 0;   // +5 групп за уровень
        public int CooldownLevel { get; private set; } = 0;     // -0.3с за уровень

        // Базовая характеристика
        private int _baseArtifactCount = 1;
        private float _baseGroupCooldown = 1.5f;
        private int _baseGroupCount = 5;
        private float _baseMiniGroupInterval = 0.2f; // Интервал между мини-группами
        private int _baseMiniGroupsPerShot = 5; // 5 мини-групп за выстрел
        private int _baseMoneyPerMiniGroup = 4; // 4 монеты в мини-группе
        private int _baseMoneyDamage = 5;

        // Текущие характеристики
        public int CurrentArtifactCount => _baseArtifactCount + ArtifactCountLevel;
        public float CurrentGroupCooldown => Math.Max(0.3f, _baseGroupCooldown - CooldownLevel * 0.3f);
        public int CurrentGroupCount => _baseGroupCount + GroupCountLevel * 5;

        // Состояние
        private List<WealthArtifactProjectile> _activeArtifacts = new List<WealthArtifactProjectile>();
        private List<WealthArtifactMoneyProjectile> _activeMoney = new List<WealthArtifactMoneyProjectile>();
        private float _artifactSpawnTimer = 0f;
        private float _groupCooldownTimer = 0f;
        private float _miniGroupTimer = 0f;
        private int _groupsFired = 0; // Сколько групп уже выстрелили
        private int _totalGroupsToFire = 0; // Сколько всего групп нужно выстрелить
        private int _currentMiniGroup = 0; // Текущая мини-группа внутри группы
        private bool _isShootingGroup = false; // Идет стрельба внутри группы
        private bool _isReturning = false;
        private float _rotationTimer = 0f;
        private float _returnTimer = 0f;

        // Углы для артефактов (в радианах) - ИСПРАВЛЕНО!
        private readonly float[] _artifactAngles = {
            MathHelper.ToRadians(315f),  // Северо-запад (315°)
            MathHelper.ToRadians(45f),   // Северо-восток (45°)
            MathHelper.ToRadians(135f),  // Юго-восток (135°)
            MathHelper.ToRadians(225f)   // Юго-запад (225°)
        };

        private enum WeaponState
        {
            Waiting,
            SpawningArtifacts,
            ArtifactsActive,
            ShootingGroup,   // Стрельба внутри группы (мини-группы с интервалом)
            Rotating,        // Вращение квадрата между группами
            AllGroupsFired,  // Все группы выстрелили
            Returning,
            Cooldown
        }

        private WeaponState _currentState = WeaponState.Waiting;

        public WealthArtifactWeapon(Player player)
            : base(player, WeaponType.Legendary, WeaponName.WealthArtifact, 0f, 0)
        {
            _currentState = WeaponState.Waiting;
            _groupCooldownTimer = 0f;
        }

        public override void LevelUp()
        {
            // Базовая реализация - не используется для этого оружия
        }

        // Методы прокачки
        public void UpgradeArtifactCount()
        {
            if (ArtifactCountLevel >= 3) return;
            ArtifactCountLevel++;

            // Если артефакты активны, нужно пересоздать их с новым количеством
            if (_currentState != WeaponState.Waiting && _currentState != WeaponState.Cooldown)
            {
                ReturnArtifacts();
            }
        }

        public void UpgradeGroupCount()
        {
            if (GroupCountLevel >= 3) return;
            GroupCountLevel++;
        }

        public void UpgradeCooldown()
        {
            if (CooldownLevel >= 3) return;
            CooldownLevel++;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (_currentState)
            {
                case WeaponState.Waiting:
                    _groupCooldownTimer -= deltaTime;
                    if (_groupCooldownTimer <= 0f)
                    {
                        _currentState = WeaponState.SpawningArtifacts;
                        _artifactSpawnTimer = 0f;
                        _groupsFired = 0;
                        _totalGroupsToFire = CurrentGroupCount;
                    }
                    break;

                case WeaponState.SpawningArtifacts:
                    // Сразу спавним артефакты
                    SpawnArtifacts();
                    _currentState = WeaponState.ArtifactsActive;
                    _groupCooldownTimer = 0.5f; // Маленькая задержка перед первой группой
                    break;

                case WeaponState.ArtifactsActive:
                    UpdateArtifacts(deltaTime);
                    UpdateMoney(deltaTime);

                    _groupCooldownTimer -= deltaTime;
                    if (_groupCooldownTimer <= 0f)
                    {
                        _currentState = WeaponState.ShootingGroup;
                        _miniGroupTimer = 0f;
                        _currentMiniGroup = 0;
                        _isShootingGroup = true;

                        // Ускоряем вращение артефактов
                        foreach (var artifact in _activeArtifacts)
                        {
                            artifact.StartShooting();
                        }
                    }
                    break;

                case WeaponState.ShootingGroup:
                    UpdateArtifacts(deltaTime);
                    UpdateMoney(deltaTime);

                    if (_isShootingGroup)
                    {
                        _miniGroupTimer -= deltaTime;
                        if (_miniGroupTimer <= 0f && _currentMiniGroup < _baseMiniGroupsPerShot)
                        {
                            // Выстреливаем одну мини-группу
                            ShootMiniGroup(_currentMiniGroup);
                            _currentMiniGroup++;
                            _miniGroupTimer = _baseMiniGroupInterval;

                            // Проверяем, закончили ли все мини-группы в этой группе
                            if (_currentMiniGroup >= _baseMiniGroupsPerShot)
                            {
                                _isShootingGroup = false;
                                _groupsFired++;

                                // Проверяем, закончили ли все группы
                                if (_groupsFired >= _totalGroupsToFire)
                                {
                                    _currentState = WeaponState.AllGroupsFired;
                                    _returnTimer = 1f; // 1 секунда задержки перед возвратом

                                    // Замедляем вращение артефактов
                                    foreach (var artifact in _activeArtifacts)
                                    {
                                        artifact.StopShooting();
                                    }
                                }
                                else
                                {
                                    // Еще есть группы - начинаем вращение квадрата
                                    _currentState = WeaponState.Rotating;
                                    _rotationTimer = CurrentGroupCooldown;

                                    // Замедляем вращение артефактов на время вращения квадрата
                                    foreach (var artifact in _activeArtifacts)
                                    {
                                        artifact.StopShooting();
                                    }
                                }
                            }
                        }
                    }
                    break;

                case WeaponState.Rotating:
                    UpdateArtifacts(deltaTime);
                    UpdateMoney(deltaTime);

                    // Вращаем артефакты
                    RotateArtifacts(deltaTime);

                    _rotationTimer -= deltaTime;
                    if (_rotationTimer <= 0f)
                    {
                        // Возвращаем артефакты в исходное положение
                        ResetArtifactRotation();

                        // Переходим к следующей группе
                        _currentState = WeaponState.ShootingGroup;
                        _isShootingGroup = true;
                        _miniGroupTimer = 0f;
                        _currentMiniGroup = 0;

                        // Ускоряем вращение артефактов для стрельбы
                        foreach (var artifact in _activeArtifacts)
                        {
                            artifact.StartShooting();
                        }
                    }
                    break;

                case WeaponState.AllGroupsFired:
                    UpdateArtifacts(deltaTime);
                    UpdateMoney(deltaTime);

                    // Все группы выстрелили, ждем 1 секунду перед возвратом
                    _returnTimer -= deltaTime;
                    if (_returnTimer <= 0f)
                    {
                        _currentState = WeaponState.Returning;
                    }
                    break;

                case WeaponState.Returning:
                    UpdateArtifacts(deltaTime);
                    UpdateMoney(deltaTime);

                    ReturnArtifacts();
                    _currentState = WeaponState.Cooldown;
                    _groupCooldownTimer = 1f; // 1 секунда задержки перед повторным вылетом
                    break;

                case WeaponState.Cooldown:
                    UpdateArtifacts(deltaTime);
                    UpdateMoney(deltaTime);

                    _groupCooldownTimer -= deltaTime;
                    if (_groupCooldownTimer <= 0f)
                    {
                        _currentState = WeaponState.Waiting;
                    }
                    break;
            }
        }

        public override void Attack(GameTime gameTime, List<Enemy> enemies)
        {
            // Проверяем коллизии монет с врагами
            CheckMoneyCollisions(enemies);
        }

        private void SpawnArtifacts()
        {
            _activeArtifacts.Clear();

            int artifactsToSpawn = Math.Min(CurrentArtifactCount, _artifactAngles.Length);

            for (int i = 0; i < artifactsToSpawn; i++)
            {
                var artifact = new WealthArtifactProjectile(
                    Player.Position,
                    Player,
                    _artifactAngles[i]
                );

                _activeArtifacts.Add(artifact);
            }
        }

        private void UpdateArtifacts(float deltaTime)
        {
            for (int i = _activeArtifacts.Count - 1; i >= 0; i--)
            {
                var artifact = _activeArtifacts[i];

                // Создаем GameTime для обновления
                var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(deltaTime));
                artifact.Update(gameTime);

                if (!artifact.IsActive)
                {
                    _activeArtifacts.RemoveAt(i);
                }
            }
        }

        private void ShootMiniGroup(int miniGroupIndex)
        {
            if (Player == null || _activeArtifacts.Count == 0) return;

            // Каждый артефакт выстреливает одну мини-группу (4 монеты)
            foreach (var artifact in _activeArtifacts)
            {
                // Находим ближайшего врага к артефакту
                Enemy closestEnemy = FindClosestEnemyToPosition(artifact.Position);

                Vector2 baseDirection;

                if (closestEnemy != null && closestEnemy.IsAlive)
                {
                    // Стреляем в ближайшего врага
                    baseDirection = Vector2.Normalize(closestEnemy.Position - artifact.Position);
                }
                else
                {
                    // Если врагов нет, стреляем в случайном направлении
                    float randomAngle = MathHelper.ToRadians(Game1.Random.Next(0, 360));
                    baseDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
                }

                // Каждая мини-группа имеет свой угол смещения (веер)
                float miniGroupAngle = MathHelper.ToRadians((miniGroupIndex * 10f) - 20f); // -20, -10, 0, +10, +20 градусов
                Vector2 miniGroupDirection = RotateVector(baseDirection, miniGroupAngle);

                // Выстреливаем 4 монеты в мини-группе
                for (int i = 0; i < _baseMoneyPerMiniGroup; i++)
                {
                    // Небольшой разброс внутри мини-группы
                    float inGroupSpread = MathHelper.ToRadians(Game1.Random.Next(-5, 5));
                    Vector2 finalDirection = RotateVector(miniGroupDirection, inGroupSpread);

                    var money = new WealthArtifactMoneyProjectile(
                        artifact.Position,
                        _baseMoneyDamage,
                        finalDirection
                    );

                    _activeMoney.Add(money);
                }
            }

            // Проигрываем звук вылета монеты (один раз на мини-группу)
            WeaponManager.PlayWeaponSound(WeaponName.WealthArtifact, 0.3f);

            //System.Diagnostics.Debug.WriteLine($"Выстрелена мини-группа {miniGroupIndex + 1}/5 в группе {_groupsFired + 1}/{_totalGroupsToFire}");
        }

        private Enemy FindClosestEnemyToPosition(Vector2 position)
        {
            float minDistanceSquared = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in Game1.CurrentEnemies)
            {
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(position, enemy.Position);

                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }

        private void UpdateMoney(float deltaTime)
        {
            for (int i = _activeMoney.Count - 1; i >= 0; i--)
            {
                var money = _activeMoney[i];

                // Создаем GameTime для обновления
                var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(deltaTime));
                money.Update(gameTime);

                if (!money.IsActive)
                {
                    _activeMoney.RemoveAt(i);
                }
            }
        }

        private void RotateArtifacts(float deltaTime)
        {
            float rotationSpeed = MathHelper.TwoPi / CurrentGroupCooldown; // Полный оборот за время перезарядки

            for (int i = 0; i < _activeArtifacts.Count; i++)
            {
                var artifact = _activeArtifacts[i];
                artifact.OrbitAngle += rotationSpeed * deltaTime;

                // Ограничиваем угол в диапазоне 0-2π
                if (artifact.OrbitAngle > MathHelper.TwoPi)
                {
                    artifact.OrbitAngle -= MathHelper.TwoPi;
                }
                else if (artifact.OrbitAngle < 0)
                {
                    artifact.OrbitAngle += MathHelper.TwoPi;
                }
            }
        }

        private void ResetArtifactRotation()
        {
            for (int i = 0; i < _activeArtifacts.Count; i++)
            {
                var artifact = _activeArtifacts[i];
                if (i < _artifactAngles.Length)
                {
                    artifact.OrbitAngle = _artifactAngles[i];
                }
            }
        }

        private void ReturnArtifacts()
        {
            _isReturning = true;
            foreach (var artifact in _activeArtifacts)
            {
                artifact.StartReturn();
            }
        }

        private void CheckMoneyCollisions(List<Enemy> enemies)
        {
            for (int i = _activeMoney.Count - 1; i >= 0; i--)
            {
                var money = _activeMoney[i];
                if (!money.IsActive) continue;

                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var enemy = enemies[j];
                    if (!enemy.IsAlive) continue;

                    if (money.GetBounds().Intersects(enemy.GetBounds()))
                    {
                        enemy.TakeDamage(money.Damage);
                        money.IsActive = false; // Монета уничтожается после попадания

                        //System.Diagnostics.Debug.WriteLine($"Монета попала! Урон: {money.Damage}");
                        break; // Монета может попасть только в одного врага
                    }
                }
            }
        }

        private Vector2 RotateVector(Vector2 vector, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }

        public void DrawArtifacts(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            foreach (var artifact in _activeArtifacts)
            {
                artifact.Draw(spriteBatch, debugTexture);
            }
        }

        public void DrawMoney(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            foreach (var money in _activeMoney)
            {
                money.Draw(spriteBatch, debugTexture);
            }
        }

        public string GetDebugInfo()
        {
            return $"State: {_currentState}, Artifacts: {_activeArtifacts.Count}, Money: {_activeMoney.Count}, Groups: {_groupsFired}/{_totalGroupsToFire}, MiniGroup: {_currentMiniGroup}/5";
        }
    }
}