// Localizations/RU/Weapons/TextRuPlayingCards.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuPlayingCards : WeaponTextFile
    {
        public override string Name => "Игральные карты";
        public override string Description => "Хорошее оружие, которое пробивает до 3 врагов за один выстрел.\n" +
                                              "\nКарты летят по прямой траектории и наносят урон всем врагам на своем пути.\n" +
                                              "\nОтлично подходит для борьбы с толпами противников.";
        public override string RouletteTitle => "Игральные карты";
        public override string RouletteDescription => "Оружие, которое может пробивать несколько врагов.";

        // ДОБАВИТЬ:
        public override string AwakenName => "Пробужденные Карты";
        public override string AwakenDescription => "Карты вылетают веером на 45 градусов, 3 группы по 6 карт каждая.\n" +
                                                    "Размер карт увеличен в 2 раза, а также улучшены характеристики";

        public override string CountUpgradeTitle => "Количество карт +1";
        public override string DamageUpgradeTitle => "Урон карты +1";
        public override string ReloadSpeedUpgradeTitle => "Скорость перезарядки -0.2с";
        public override string OtherUpgradeTitle => "";
    }
}