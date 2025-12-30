// Gamedata/Config/WeaponSystem/AwakenWeapon.cs
using Survive_the_night.Entities;

namespace Survive_the_night.Gamedata.Config.WeaponSystem
{
    public abstract class AwakenWeapon : Weapon
    {
        public WeaponName OriginalWeaponName { get; protected set; }

        protected AwakenWeapon(Player player, WeaponType type, WeaponName name,
                              float cooldownTime, int damage, WeaponName originalWeaponName)
            : base(player, type, name, cooldownTime, damage)
        {
            OriginalWeaponName = originalWeaponName;
        }

        // Метод для проверки совместимости с исходным оружием
        public virtual bool IsCompatibleWith(Weapon originalWeapon)
        {
            return originalWeapon.Name == OriginalWeaponName;
        }
    }
}