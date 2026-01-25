// Gamedata/Config/WeaponSystem/AwakenManager.cs
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem.Weapons;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Gamedata.Config.WeaponSystem
{
    public static class AwakenManager
    {
        // Список оружий, у которых ЕСТЬ пробуждение
        private static readonly HashSet<WeaponName> _weaponsWithAwaken = new()
        {
            WeaponName.PlayingCards,
            WeaponName.Dice
            // Добавлять другие оружия по мере реализации
        };

        // Проверка, есть ли у оружия пробуждение
        public static bool HasAwaken(WeaponName weaponName)
        {
            return _weaponsWithAwaken.Contains(weaponName);
        }

        // Проверка, готово ли конкретное оружие к пробуждению
        public static bool IsWeaponReadyForAwaken(Weapon weapon)
        {
            if (weapon == null || !HasAwaken(weapon.Name))
                return false;

            switch (weapon.Name)
            {
                case WeaponName.PlayingCards:
                    return IsPlayingCardsReady(weapon as PlayingCards);
                case WeaponName.Dice:
                    return IsDiceWeaponReady(weapon as DiceWeapon);
                default:
                    return false;
            }
        }

        private static bool IsDiceWeaponReady(Weapon weapon)
        {
            if (weapon is not DiceWeapon dice) return false;

            // Все 3 ветки должны быть максимально прокачаны
            return dice.DamageBonusLevel >= 5 &&
                   dice.PierceBonusLevel >= 5 &&
                   dice.CooldownLevel >= 5;
        }

        // Проверка для Игральных карт
        private static bool IsPlayingCardsReady(PlayingCards cards)
        {
            if (cards == null) return false;

            // Все 3 ветки должны быть максимально прокачаны
            return cards.CountLevel >= 5 &&
                   cards.DamageLevel >= 5 &&
                   cards.ReloadSpeedLevel >= 5;
        }

        // Создание пробужденного оружия
        public static Weapon CreateAwakenWeapon(WeaponName weaponName, Player player)
        {
            return weaponName switch
            {
                WeaponName.PlayingCards => new Awaken.AwakenPlayingCards(player),
                WeaponName.Dice => new Awaken.AwakenDiceWeapon(player),
                _ => null
            };
        }

        // Получение оружий из инвентаря, которые готовы к пробуждению
        public static List<WeaponName> GetReadyWeapons(List<Weapon> weapons)
        {
            List<WeaponName> readyWeapons = new List<WeaponName>();

            foreach (var weapon in weapons)
            {
                if (HasAwaken(weapon.Name) && IsWeaponReadyForAwaken(weapon))
                {
                    readyWeapons.Add(weapon.Name);
                }
            }

            return readyWeapons;
        }
    }
}