// Localizations/RU/Weapons/TextRuWealthArtifact.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuWealthArtifact : WeaponTextFile
    {
        public override string Name => "Артефакт богатства";
        public override string Description => "Золотые артефакты, стреляющие монетами в ближайших врагов.\n\nСоздает настоящий дождь из золота, уничтожающий врагов.";
        public override string RouletteTitle => "Артефакт богатства [ЛЕГЕНДАРНЫЙ]";
        public override string RouletteDescription => "Добавляет новое легендарное оружие: золотые артефакты, стреляющие монетами в ближайших врагов.";
        public override string CountUpgradeTitle => "Количество артефактов +1";
        public override string DamageUpgradeTitle => "";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка групп -0.3с";
        public override string OtherUpgradeTitle => "Групп монет +5";
    }
}