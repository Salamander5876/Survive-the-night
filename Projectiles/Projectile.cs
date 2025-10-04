using Microsoft.Xna.Framework;
using Survive_the_night.Entities; // Важно: для доступа к GameObject
using System.Collections.Generic;

namespace Survive_the_night.Projectiles
{
    // Базовый класс для всех снарядов
    public abstract class Projectile : GameObject
    {
        protected int Damage { get; private set; }
        public float Speed { get; protected set; }
        public bool IsActive { get; set; } = true;

        public Projectile(Vector2 initialPosition, int size, Color color, int damage, float speed)
            : base(initialPosition, size, color)
        {
            Damage = damage;
            Speed = speed;
        }

        // ИСПРАВЛЕНИЕ: Реализуем абстрактный метод из GameObject!
        // Этот метод обязателен для всех GameObject.
        public override void Update(GameTime gameTime)
        {
            // Поскольку снаряды будут обновляться через специфический метод 
            // Attack() в Weapon, здесь оставляем его пустым.
        }

        // Дополнительный метод для сложной логики (с врагами). 
        // Его будет вызывать класс PlayingCards.
        public virtual void Update(GameTime gameTime, List<Enemy> enemies)
        {
            // Потомок (CardProjectile) реализует здесь всю логику полета.
        }

        protected virtual void OnHitEnemy(Enemy enemy)
        {
            enemy.TakeDamage(Damage);
            IsActive = false;
        }
    }
}