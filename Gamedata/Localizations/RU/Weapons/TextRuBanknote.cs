using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuBanknote : WeaponTextFile
    {
        public override string Name => "Банкнота";
        public override string Description => "Выпускает веер из 5 банкнот под углом 20 градусов.\n" +
                                              "\nКаждая банкнота наносит урон при попадании и уничтожается.\n" +
                                              "\nОтличное оружие для быстрого поражения ближайших врагов.";
        public override string RouletteTitle => "Банкнота";
        public override string RouletteDescription => "Веер из 5 банкнот, наносящих урон врагам.";

        public override string AwakenName => "";
        public override string AwakenDescription => "";

        public override string CountUpgradeTitle => "Количество банкнот +1";
        public override string DamageUpgradeTitle => "Урон банкноты +1";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка -0.4с";
        public override string OtherUpgradeTitle => "";
    }
}