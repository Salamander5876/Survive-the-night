// Localizations/RU/Weapons/TextRuBeerBottle.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuBeerBottle : WeaponTextFile
    {
        public override string Name => "Пивная бутылка";
        public override string Description => "Бутылки пива, создающие липкие лужи с периодическим уроном.\n\nКаждый враг имеет свой независимый таймер урона при нахождении в луже.";
        public override string RouletteTitle => "Бутылка пива";
        public override string RouletteDescription => "Бутылки пива, создающие липкие лужи с периодическим уроном.";
        public override string CountUpgradeTitle => "Количество +1";
        public override string DamageUpgradeTitle => "";
        public override string ReloadSpeedUpgradeTitle => "";
        public override string OtherUpgradeTitle => "Время лужи +5с";
    }
}