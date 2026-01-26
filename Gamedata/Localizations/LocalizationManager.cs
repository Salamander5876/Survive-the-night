// Localizations/LocalizationManager.cs
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Localizations;
using Survive_the_night.Localizations.RU.Weapons;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Localizations
{
    public enum Language
    {
        RU,
        EN
    }

    public static class LocalizationManager
    {
        private static Language _currentLanguage = Language.RU;
        private static Dictionary<Language, Dictionary<WeaponName, WeaponTextFile>> _weaponTexts;

        static LocalizationManager()
        {
            Initialize();
        }

        private static void Initialize()
        {
            _weaponTexts = new Dictionary<Language, Dictionary<WeaponName, WeaponTextFile>>();

            // »нициализаци€ русского €зыка
            _weaponTexts[Language.RU] = new Dictionary<WeaponName, WeaponTextFile>
            {
                { WeaponName.PlayingCards, new TextRuPlayingCards() },
                { WeaponName.CasinoChips, new TextRuCasinoChips() },
                { WeaponName.GoldenBullet, new TextRuGoldenBullet() },
                { WeaponName.GoldenSword, new TextRuGoldenSword() },
                { WeaponName.MolotovCocktail, new TextRuMolotovCocktail() },
                { WeaponName.BigLaser, new TextRuBigLaser() },
                { WeaponName.StickyBomb, new TextRuStickyBomb() },
                { WeaponName.Dice, new TextRuDiceWeapon() },
                { WeaponName.RouletteBall, new TextRuRouletteBall() },
                { WeaponName.GoldenTyphoon, new TextRuGoldenTyphoon() },
                { WeaponName.EventHorizon, new TextRuEventHorizon() },
                { WeaponName.BeerBottle, new TextRuBeerBottle() },
                { WeaponName.Breaker, new TextRuBreaker() },
                { WeaponName.Typhoon, new TextRuTyphoon() },
                { WeaponName.WealthArtifact, new TextRuWealthArtifact() },
                { WeaponName.Banknote, new TextRuBanknote() }
            };

            // TODO: ƒобавить инициализацию дл€ английского €зыка
            _weaponTexts[Language.EN] = new Dictionary<WeaponName, WeaponTextFile>();
        }

        public static void SetLanguage(Language language)
        {
            if (_currentLanguage != language)
            {
                _currentLanguage = language;
                // «десь можно добавить логику перезагрузки текстов при смене €зыка
            }
        }

        public static Language CurrentLanguage => _currentLanguage;

        public static WeaponTextFile GetWeaponText(WeaponName weaponName)
        {
            if (_weaponTexts.ContainsKey(_currentLanguage) &&
                _weaponTexts[_currentLanguage].TryGetValue(weaponName, out var text))
            {
                return text;
            }

            // ¬озвращаем текст на русском как fallback
            if (_weaponTexts[Language.RU].TryGetValue(weaponName, out var ruText))
            {
                return ruText;
            }

            // ≈сли совсем ничего нет, возвращаем заглушку
            return new TextRuPlayingCards();
        }

        public static string GetWeaponName(WeaponName weaponName)
        {
            return GetWeaponText(weaponName).Name;
        }

        public static string GetWeaponDescription(WeaponName weaponName)
        {
            return GetWeaponText(weaponName).Description;
        }

        public static string GetWeaponRouletteTitle(WeaponName weaponName)
        {
            return GetWeaponText(weaponName).RouletteTitle;
        }

        public static string GetWeaponRouletteDescription(WeaponName weaponName)
        {
            return GetWeaponText(weaponName).RouletteDescription;
        }
    }
}