using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Managers;
using Survive_the_night.Projectiles;
using System.Collections.Generic;

namespace Survive_the_night.Weapons
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
        RouletteBall
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
            WeaponName.RouletteBall
        };

        public static List<WeaponName> LegendaryWeapons { get; private set; } = new List<WeaponName>
        {
            WeaponName.GoldenSword,
            WeaponName.MolotovCocktail,
            WeaponName.BigLaser
        };

        // Текстуры для оружий
        private static Dictionary<WeaponName, List<Texture2D>> _weaponTextures = new Dictionary<WeaponName, List<Texture2D>>();
        private static Dictionary<WeaponName, SoundEffect> _weaponSounds = new Dictionary<WeaponName, SoundEffect>();

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

        public static void LoadWeaponSound(WeaponName weaponName, SoundEffect sound)
        {
            _weaponSounds[weaponName] = sound;
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

        public static SoundEffect GetWeaponSound(WeaponName weaponName)
        {
            return _weaponSounds.ContainsKey(weaponName) ? _weaponSounds[weaponName] : null;
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
                default:
                    return new PlayingCards(player);
            }
        }

        // Временный метод - нужно будет заменить на получение реального GameBoundaries
        private static GameBoundaries CreateTemporaryGameBoundaries()
        {
            // Это временное решение - в реальной игре нужно получить GameBoundaries из Game1
            return null;
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