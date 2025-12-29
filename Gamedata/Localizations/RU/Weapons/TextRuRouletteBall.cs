// Localizations/RU/Weapons/TextRuRouletteBall.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuRouletteBall : WeaponTextFile
    {
        public override string Name => "Рулетка";
        public override string Description => "Шарик рулетки летит случайно и отскакивает от стен.\n\nОставляет след из частичек, которые наносят урон и уничтожаются при столкновении.";
        public override string RouletteTitle => "Рулетка";
        public override string RouletteDescription => "Шарик рулетки летит случайно и отскакивает от стен.\nОставляет след из частичек, которые наносят урон и уничтожаются при столкновении.";
        public override string CountUpgradeTitle => "";
        public override string DamageUpgradeTitle => "Урон частичек +1";
        public override string ReloadSpeedUpgradeTitle => "";
        public override string OtherUpgradeTitle => "Скорость +100";
    }
}