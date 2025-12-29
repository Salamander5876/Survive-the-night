// Localizations/RU/Weapons/TextRuStickyBomb.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuStickyBomb : WeaponTextFile
    {
        public override string Name => "Липкая бомба";
        public override string Description => "Тактическое оружие с отложенным взрывом.\n\nБомба прилипает к врагу и взрывается через 10 секунд, нанося урон всем врагам в радиусе.\n\nНовые бомбы не появляются, пока все предыдущие не взорвались.";
        public override string RouletteTitle => "Липкая бомба";
        public override string RouletteDescription => "Бомбы, которые прилипают к врагам и взрываются через время.";
        public override string CountUpgradeTitle => "Количество +1";
        public override string DamageUpgradeTitle => "Урон +4";
        public override string ReloadSpeedUpgradeTitle => "";
        public override string OtherUpgradeTitle => "Время взрыва -1.5с";
    }
}