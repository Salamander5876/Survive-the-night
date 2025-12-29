// Localizations/RU/Weapons/TextRuCasinoChips.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuCasinoChips : WeaponTextFile
    {
        public override string Name => "Фишки казино";
        public override string Description => "Фишки, которые отскакивают между врагами.\n\nКаждая фишка может поразить нескольких врагов, перескакивая между ними.\n\nЭффективны против групп, расположенных близко друг к другу.";
        public override string RouletteTitle => "Фишки казино";
        public override string RouletteDescription => "Фишки, которые отскакивают между врагами.";
        public override string CountUpgradeTitle => "";
        public override string DamageUpgradeTitle => "Урон +2";
        public override string ReloadSpeedUpgradeTitle => "Скорость перезарядки -0.3с";
        public override string OtherUpgradeTitle => "Отскоки +1";
    }
}