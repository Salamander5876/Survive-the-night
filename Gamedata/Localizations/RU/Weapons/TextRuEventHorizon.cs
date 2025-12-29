// Localizations/RU/Weapons/TextRuEventHorizon.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuEventHorizon : WeaponTextFile
    {
        public override string Name => "Горизонт Событий";
        public override string Description => "Звезды, вращающиеся по расширяющимся кольцам.\n\nИмеют систему самонаведения на врагов и наносят сокрушительный урон.\n\nПо-настоящему космическое оружие.";
        public override string RouletteTitle => "Горизонт Событий [ЛЕГЕНДАРНЫЙ]";
        public override string RouletteDescription => "Добавляет новое легендарное оружие: звезды, вращающиеся по расширяющимся кольцам с самонаведением на врагов.";
        public override string CountUpgradeTitle => "Количество звезд +2";
        public override string DamageUpgradeTitle => "Урон +5";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка -0.2с";
        public override string OtherUpgradeTitle => "";
    }
}