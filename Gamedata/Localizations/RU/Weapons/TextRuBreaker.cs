// Localizations/RU/Weapons/TextRuBreaker.cs
using Survive_the_night.Gamedata.Localizations;

namespace Survive_the_night.Localizations.RU.Weapons
{
    public class TextRuBreaker : WeaponTextFile
    {
        public override string Name => "Разрушитель";
        public override string Description => "Массивный меч, совершающий сокрушительные взмахи вокруг игрока.\n\nНаносит дополнительный урон врагам с полным здоровьем.\n\nИдеально для уничтожения свежепоявившихся врагов.";
        public override string RouletteTitle => "Разрушитель [ЛЕГЕНДАРНЫЙ]";
        public override string RouletteDescription => "Добавляет новое легендарное оружие: массивный меч, совершающий сокрушительные взмахи вокруг игрока и наносит дополнительный урон врагам с полным хп";
        public override string CountUpgradeTitle => "";
        public override string DamageUpgradeTitle => "Урон +5";
        public override string ReloadSpeedUpgradeTitle => "Перезарядка -0.2с";
        public override string OtherUpgradeTitle => "Скорость атаки +150";
    }
}