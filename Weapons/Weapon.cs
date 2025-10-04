using Microsoft.Xna.Framework;
using Survive_the_night.Entities;
using System.Collections.Generic; // Важно для использования List<Enemy>

namespace Survive_the_night.Weapons
{
    // Базовый класс для всего оружия в игре
    public abstract class Weapon
    {
        // Ссылка на игрока, чтобы оружие знало, откуда стрелять
        protected Player Player { get; private set; }

        // Время между атаками в секундах
        public float CooldownTime { get; protected set; }

        // Текущий урон
        public int Damage { get; protected set; }

        // Таймер, отслеживающий, когда можно будет атаковать снова
        protected float CooldownTimer { get; set; } = 0f;

        // Конструктор
        public Weapon(Player player, float cooldownTime, int damage)
        {
            Player = player;
            CooldownTime = cooldownTime;
            Damage = damage;
        }

        // Метод обновления логики (перезарядка)
        public virtual void Update(GameTime gameTime)
        {
            CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (CooldownTimer < 0f)
            {
                CooldownTimer = 0f;
            }
        }

        // Абстрактный метод: реализует саму атаку (спаун снарядов, эффект)
        // Принимает список врагов для самонаведения или проверки попадания.
        public abstract void Attack(GameTime gameTime, List<Enemy> enemies);
    }
}