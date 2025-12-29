// Localizations/RU/Weapons/TextRuGoldenTyphoon.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuGoldenTyphoon : WeaponTextFile
    {
        public override string Name => "Золотой Тайфун";
        public override string Description => "Мощные золотые снаряды, летящие по 8 направлениям.\n\nВращаются с огромной скоростью и имеют бесконечное пробитие.\n\nСоздают настоящую бурю из урона.";
        public override string RouletteTitle => "Золотой Тайфун [ЛЕГЕНДАРНЫЙ]";
        public override string RouletteDescription => "Добавляет новое легендарное оружие: снаряды, летящие по 8 направлениям с огромной скоростью вращения и с бесконечным пробитием.";
        public override string CountUpgradeTitle => "Количество снарядов +2";
        public override string DamageUpgradeTitle => "Урон +4";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка -0.2с";
        public override string OtherUpgradeTitle => "";
    }
}