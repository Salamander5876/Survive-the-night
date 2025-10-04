using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System.Collections.Generic;

namespace Survive_the_night.Projectiles
{
    // Базовый класс для всех снарядов
    public abstract class Projectile : GameObject
    {
        public int Damage { get; protected set; }
        public float Speed { get; protected set; }
        public bool IsActive { get; set; } = true;

        // Добавлено для пробития (для PlayingCards)
        public int HitsLeft { get; protected set; }

        // !!! ИСПРАВЛЕННЫЙ КОНСТРУКТОР: Вызывает базовый конструктор GameObject !!!
        // Добавлен аргумент hitsLeft для MolotovProjectile и PlayingCard
        public Projectile(Vector2 initialPosition, int size, Color color, int damage, float speed, int hitsLeft)
            : base(initialPosition, size, color) // Вызов конструктора GameObject
        {
            Damage = damage;
            Speed = speed;
            HitsLeft = hitsLeft;
        }

        // Реализация абстрактного метода Update
        public override void Update(GameTime gameTime)
        {
            // Этот метод будет переопределен в PlayingCard и MolotovProjectile
        }

        // Метод для логики пробития
        public virtual void HitTarget()
        {
            HitsLeft--;
            if (HitsLeft <= 0)
            {
                IsActive = false;
            }
        }

        protected virtual void OnHitEnemy(Enemy enemy)
        {
            // ПРЕДПОЛОЖЕНИЕ: У класса Enemy есть метод TakeDamage(int)
            // enemy.TakeDamage(Damage);
            // HitTarget(); 
        }

        // ПРИМЕЧАНИЕ: Методы Draw и GetBounds наследуются от GameObject
    }
}