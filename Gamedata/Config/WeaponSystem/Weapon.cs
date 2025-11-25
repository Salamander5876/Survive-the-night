using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem.Weapons;
using Survive_the_night.Scripts.Managers;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem
{
    // Перечисление типов оружия
    public enum WeaponType
    {
        Regular,
        Legendary
    }

    // Перечисление всех доступных оружий
    public enum WeaponName
    {
        PlayingCards,
        CasinoChips,
        GoldenBullet,
        GoldenSword,
        MolotovCocktail,
        BigLaser,
        StickyBomb,
        Dice,
        RouletteBall,
        GoldenTyphoon,
        EventHorizon,
        BeerBottle,
        Breaker
    }

    public abstract class Weapon
    {
        protected Player Player { get; private set; }
        public WeaponType Type { get; protected set; }
        public WeaponName Name { get; protected set; }

        public float CooldownTime { get; protected set; }
        public int Damage { get; protected set; }
        public int Level { get; protected set; } = 1;
        public const int MAX_LEVEL = 10;

        protected float CooldownTimer { get; set; } = 0f;

        public Weapon(Player player, WeaponType type, WeaponName name, float cooldownTime, int damage)
        {
            Player = player;
            Type = type;
            Name = name;
            CooldownTime = cooldownTime;
            Damage = damage;
        }

        public abstract void LevelUp();

        // Сделать виртуальным вместо абстрактного
        public virtual void Update(GameTime gameTime)
        {
            CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (CooldownTimer < 0f)
            {
                CooldownTimer = 0f;
            }
        }

        public abstract void Attack(GameTime gameTime, List<Enemy> enemies);

        protected Enemy FindClosestEnemy(List<Enemy> enemies)
        {
            float minDistanceSquared = float.MaxValue;
            Enemy closestEnemy = null;

            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                float distanceSquared = Vector2.DistanceSquared(Player.Position, enemy.Position);

                if (distanceSquared < minDistanceSquared && distanceSquared < 490000) // 700x700
                {
                    minDistanceSquared = distanceSquared;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }
    }

    // Статический класс для управления всеми оружиями
    public static class WeaponManager
    {
        // Группы оружий
        public static List<WeaponName> RegularWeapons { get; private set; } = new List<WeaponName>
        {
            WeaponName.PlayingCards,
            WeaponName.CasinoChips,
            WeaponName.GoldenBullet,
            WeaponName.StickyBomb,
            WeaponName.Dice,
            WeaponName.RouletteBall,
            WeaponName.BeerBottle
        };

        public static List<WeaponName> LegendaryWeapons { get; private set; } = new List<WeaponName>
        {
            WeaponName.GoldenSword,
            WeaponName.MolotovCocktail,
            WeaponName.BigLaser,
            WeaponName.GoldenTyphoon,
            WeaponName.EventHorizon,
            WeaponName.Breaker
        };

        // Текстуры для оружий
        private static Dictionary<WeaponName, List<Texture2D>> _weaponTextures = new Dictionary<WeaponName, List<Texture2D>>();

        // Методы для загрузки контента
        public static void LoadWeaponTextures(WeaponName weaponName, params Texture2D[] textures)
        {
            if (!_weaponTextures.ContainsKey(weaponName))
            {
                _weaponTextures[weaponName] = new List<Texture2D>();
            }

            foreach (var texture in textures)
            {
                if (texture != null && !_weaponTextures[weaponName].Contains(texture))
                {
                    _weaponTextures[weaponName].Add(texture);
                }
            }
        }

        // Методы для получения контента
        public static Texture2D GetRandomWeaponTexture(WeaponName weaponName)
        {
            if (_weaponTextures.ContainsKey(weaponName) && _weaponTextures[weaponName].Count > 0)
            {
                return _weaponTextures[weaponName][Game1.Random.Next(0, _weaponTextures[weaponName].Count)];
            }
            return null;
        }

        // === НОВЫЕ МЕТОДЫ ДЛЯ РАБОТЫ СО ЗВУКАМИ ЧЕРЕЗ SOUNDMANAGER ===

        public static void PlayWeaponSound(WeaponName weaponName, float volumeMultiplier = 1.0f)
        {
            string soundKey = GetWeaponSoundKey(weaponName);
            SoundManager.Instance.PlaySound(soundKey, volumeMultiplier);
        }

        // Специальный метод для BigLaser и других оружий с длительными звуками
        public static SoundEffectInstance PlayWeaponSoundLooping(WeaponName weaponName, float volumeMultiplier = 1.0f)
        {
            string soundKey = GetWeaponSoundKey(weaponName);
            var instance = SoundManager.Instance.PlaySoundInstance(soundKey, volumeMultiplier, true);

            // Добавляем в группу оружий для управления паузой
            if (instance != null)
            {
                SoundManager.Instance.AddToSoundGroup("weapon_sounds", instance);
            }

            return instance;
        }

        public static void StopWeaponSoundInstance(SoundEffectInstance instance)
        {
            if (instance != null && !instance.IsDisposed)
            {
                instance.Stop();
                instance.Dispose();

                // Удаляем из группы звуков
                SoundManager.Instance.RemoveFromSoundGroup("weapon_sounds", instance);
            }
        }

        private static string GetWeaponSoundKey(WeaponName weaponName)
        {
            switch (weaponName)
            {
                case WeaponName.PlayingCards: return "card_deal";
                case WeaponName.GoldenBullet: return "gun_shoot";
                case WeaponName.CasinoChips: return "casino_chips";
                case WeaponName.GoldenSword: return "golden_sword";
                case WeaponName.MolotovCocktail: return "molotov_throw";
                case WeaponName.BigLaser: return "big_laser";
                case WeaponName.StickyBomb: return "bomb_throw";
                case WeaponName.Dice: return "dice_damage";
                case WeaponName.RouletteBall: return "roulette_damage";
                case WeaponName.GoldenTyphoon: return "golden_typhoon";
                case WeaponName.EventHorizon: return "event_horizon";
                case WeaponName.BeerBottle: return "beer_throw";
                case WeaponName.Breaker: return "breaker_swing";
                default: return "card_deal";
            }
        }

        // Метод для получения звука горения огня
        public static SoundEffectInstance PlayFireBurnSound(float volumeMultiplier = 1.0f)
        {
            var instance = SoundManager.Instance.PlaySoundInstance("fire_burn", volumeMultiplier, true);

            // Добавляем в группу звуков огня для управления паузой
            if (instance != null)
            {
                SoundManager.Instance.AddToSoundGroup("fire_sounds", instance);
            }

            return instance;
        }

        // Фабричный метод для создания оружия
        public static Weapon CreateWeapon(WeaponName weaponName, Player player)
        {
            switch (weaponName)
            {
                case WeaponName.PlayingCards:
                    return new PlayingCards(player);
                case WeaponName.CasinoChips:
                    return new CasinoChips(player);
                case WeaponName.GoldenBullet:
                    return new GoldenBullet(player);
                case WeaponName.GoldenSword:
                    return new GoldenSword(player);
                case WeaponName.MolotovCocktail:
                    return new MolotovCocktail(player);
                case WeaponName.BigLaser:
                    return new BigLaser(player);
                case WeaponName.StickyBomb:
                    return new StickyBomb(player);
                case WeaponName.Dice:
                    return new DiceWeapon(player);
                case WeaponName.RouletteBall:
                    return new RouletteBall(player);
                case WeaponName.GoldenTyphoon:
                    return new GoldenTyphoon(player);
                case WeaponName.EventHorizon:
                    return new EventHorizon(player);
                case WeaponName.BeerBottle:
                    return new BeerBottle(player);
                case WeaponName.Breaker:
                    return new Breaker(player);
                default:
                    return new PlayingCards(player);
            }
        }

        // Получение отображаемого имени оружия
        public static string GetDisplayName(WeaponName weaponName)
        {
            switch (weaponName)
            {
                case WeaponName.PlayingCards: return "Игральные карты";
                case WeaponName.CasinoChips: return "Фишки казино";
                case WeaponName.GoldenBullet: return "Золотые пули";
                case WeaponName.GoldenSword: return "Золотой меч";
                case WeaponName.MolotovCocktail: return "Коктейль Молотова";
                case WeaponName.BigLaser: return "Большой лазер";
                case WeaponName.StickyBomb: return "Липкая бомба";
                case WeaponName.Dice: return "Игральные кости";
                case WeaponName.RouletteBall: return "Рулетка";
                case WeaponName.GoldenTyphoon: return "Золотой Тайфун";
                case WeaponName.EventHorizon: return "Горизонт Событий";
                case WeaponName.BeerBottle: return "Пивная бутылка";
                case WeaponName.Breaker: return "Разрушитель";
                default: return "Неизвестное оружие";
            }
        }

        // Проверка доступности оружия (для меню выбора)
        public static bool IsWeaponAvailable(WeaponName weaponName)
        {
            // Все обычные оружия доступны с начала
            if (RegularWeapons.Contains(weaponName))
                return true;

            // Легендарные оружия получаются через прокачку
            return false;
        }
    }
}