using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using System.Collections.Generic;

namespace Survive_the_night.Projectiles
{
    // Базовый класс для всех снарядов
    public abstract class Projectile : GameObject
    {
        // ИСПРАВЛЕНО: Свойство Damage для чтения из PlayingCards.CheckProjectileCollisions
        public int Damage { get; protected set; }
        public float Speed { get; protected set; }
        public bool IsActive { get; set; } = true;

        // ИСПРАВЛЕННЫЙ КОНСТРУКТОР: Соответствует базовому классу GameObject
        public Projectile(Vector2 initialPosition, int size, Color color, int damage, float speed)
            : base(initialPosition, size, color)
        {
            Damage = damage;
            Speed = speed;
        }

        // ИСПРАВЛЕНО: Реализация абстрактного метода GameObject.Update
        public override void Update(GameTime gameTime)
        {
            // Используется PlayingCard.Update(GameTime)
        }

        // ДОБАВЛЕНО: Этот метод будет использоваться в PlayingCards для запуска логики полета
        public virtual void Update(GameTime gameTime, Vector2 direction)
        {
            // Потомок (PlayingCard) реализует здесь всю логику полета.
        }

        protected virtual void OnHitEnemy(Enemy enemy)
        {
            // ПРЕДПОЛОЖЕНИЕ: У класса Enemy есть метод TakeDamage(int)
            enemy.TakeDamage(Damage);
            IsActive = false;
        }

        // ПРИМЕЧАНИЕ: Методы Draw и GetBounds наследуются от GameObject
    }
}