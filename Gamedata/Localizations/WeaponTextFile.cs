// Localizations/Weapons/WeaponTextFile.cs
using Survive_the_night.Localizations;

namespace Survive_the_night.Gamedata.Localizations
{
    public abstract class WeaponTextFile : BaseTextFile
    {
        // Основные свойства
        public abstract string RouletteTitle { get; }
        public abstract string RouletteDescription { get; }

        // Описание улучшений
        public abstract string CountUpgradeTitle { get; }
        public abstract string DamageUpgradeTitle { get; }
        public abstract string ReloadSpeedUpgradeTitle { get; }
        public abstract string OtherUpgradeTitle { get; } // Для специфических улучшений

        public virtual string AwakenName => "";
        public virtual string AwakenDescription => "";

        // Форматированные строки (с параметрами)
        public virtual string GetCountUpgradeText(int currentLevel, int maxLevel) =>
            $"{CountUpgradeTitle} (Ур. {currentLevel}/{maxLevel})";

        public virtual string GetDamageUpgradeText(int currentLevel, int maxLevel) =>
            $"{DamageUpgradeTitle} (Ур. {currentLevel}/{maxLevel})";

        public virtual string GetReloadSpeedUpgradeText(int currentLevel, int maxLevel) =>
            $"{ReloadSpeedUpgradeTitle} (Ур. {currentLevel}/{maxLevel})";
    }
}