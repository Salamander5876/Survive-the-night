// Localizations/RU/Weapons/TextRuTyphoon.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuTyphoon : WeaponTextFile
    {
        public override string Name => "Тайфун";
        public override string Description => "Снаряды, летящие к ближайшему врагу с бесконечным пробитием.\n\nНаносят урон каждые 0.3 секунды при контакте с врагом.";
        public override string RouletteTitle => "Тайфун";
        public override string RouletteDescription => "Снаряды, летящие к ближайшему врагу с бесконечным пробитием.\nНаносят урон каждые 0.3 секунды при контакте с врагом.";
        public override string CountUpgradeTitle => "Количество снарядов +1";
        public override string DamageUpgradeTitle => "Урон +1";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка -0.2с";
        public override string OtherUpgradeTitle => "";
    }
}