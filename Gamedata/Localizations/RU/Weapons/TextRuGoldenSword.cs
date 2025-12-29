// Localizations/RU/Weapons/TextRuGoldenSword.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuGoldenSword : WeaponTextFile
    {
        public override string Name => "Золотой меч";
        public override string Description => "Легендарное оружие, создающее вокруг игрока вращающиеся золотые мечи.\n\nМечи автоматически наводятся на ближайших врагов и наносят значительный урон.\n\nКоличество целей увеличивается с уровнем.";
        public override string RouletteTitle => "Золотой меч [ЛЕГЕНДАРНЫЙ]";
        public override string RouletteDescription => "Добавляет новое легендарное оружие: золотые мечи, летящие с автонаводкой.";
        public override string CountUpgradeTitle => "Количество мечей +2";
        public override string DamageUpgradeTitle => "Урон +4";
        public override string ReloadSpeedUpgradeTitle => "";
        public override string OtherUpgradeTitle => "Целей +10";
    }
}