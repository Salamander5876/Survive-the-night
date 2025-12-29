// Localizations/RU/Weapons/TextRuDiceWeapon.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuDiceWeapon : WeaponTextFile
    {
        public override string Name => "Игральные кости";
        public override string Description => "Магические кости с уникальными характеристиками для каждого значения.\n\nКость 1: Урон 2, Пробитие 6\nКость 2: Урон 4, Пробитие 5\nКость 3: Урон 6, Пробитие 4\nКость 4: Урон 8, Пробитие 3\nКость 5: Урон 10, Пробитие 2\nКость 6: Урон 12, Пробитие 1";
        public override string RouletteTitle => "Игральные кости";
        public override string RouletteDescription => "Кости, вращающиеся вокруг игрока и имеют разное количество пробития и урона";
        public override string CountUpgradeTitle => "";
        public override string DamageUpgradeTitle => "Дополнительный урон +1";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка -1с";
        public override string OtherUpgradeTitle => "Дополнительное пробитие +1";
    }
}