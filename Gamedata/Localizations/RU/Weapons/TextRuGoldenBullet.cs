// Localizations/RU/Weapons/TextRuGoldenBullet.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuGoldenBullet : WeaponTextFile
    {
        public override string Name => "Золотые пули";
        public override string Description => "Точное оружие с высоким уроном по одной цели.\n\nПули летят с большой скоростью и гарантированно поражают ближайшего врага.\n\nИдеально для точечного уничтожения сильных противников.";
        public override string RouletteTitle => "Золотая пуля";
        public override string RouletteDescription => "Точные золотые пули, которые могут отталкивать врагов.";
        public override string CountUpgradeTitle => "Количество пуль +1";
        public override string DamageUpgradeTitle => "Урон +2";
        public override string ReloadSpeedUpgradeTitle => "";
        public override string OtherUpgradeTitle => "Отталкивание +5px";
    }
}