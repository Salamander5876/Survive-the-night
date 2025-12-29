// Localizations/RU/Weapons/TextRuBigLaser.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuBigLaser : WeaponTextFile
    {
        public override string Name => "Большой лазер";
        public override string Description => "Мощное лазерное оружие с автоматическим наведением.\n\nЛазер отслеживает врагов с наибольшим здоровьем и наносит им постоянный урон.\n\nИмеет приоритетную систему таргетинга.";
        public override string RouletteTitle => "Большой лазер [ЛЕГЕНДАРНЫЙ]";
        public override string RouletteDescription => "Добавляет новое легендарное оружие: мощный лазер, который автоматически наводится и имеет приоритет атаки на врагов с большим хп.";
        public override string CountUpgradeTitle => "";
        public override string DamageUpgradeTitle => "Урон +1";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка -5с";
        public override string OtherUpgradeTitle => "Длительность +10с";
    }
}