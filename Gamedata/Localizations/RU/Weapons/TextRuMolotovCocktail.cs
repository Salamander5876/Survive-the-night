// Localizations/RU/Weapons/TextRuMolotovCocktail.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuMolotovCocktail : WeaponTextFile
    {
        public override string Name => "Коктейль Молотова";
        public override string Description => "Тактическое оружие, создающее зоны огня на поле боя.\n\nБутылки разбиваются и создают огненные области, которые наносят постоянный урон врагам.\n\nЭффективно для контроля территории.";
        public override string RouletteTitle => "Коктейль Молотова [ЛЕГЕНДАРНЫЙ]";
        public override string RouletteDescription => "Добавляет новое легендарное оружие: бросает бутылки, создающие огненные зоны.";
        public override string CountUpgradeTitle => "Количество бутылок +2";
        public override string DamageUpgradeTitle => "Урон огня +1";
        public override string ReloadSpeedUpgradeTitle => "";
        public override string OtherUpgradeTitle => "Время горения +10с";
    }
}