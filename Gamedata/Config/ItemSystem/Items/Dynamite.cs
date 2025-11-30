using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.Items;
using Survive_the_night.Gamedata.Config.ItemSystem.Renderers;

namespace Survive_the_night.Gamedata.Config.ItemSystem.Items
{
    public class Dynamite : Item
    {
        private const float AttractionSpeed = 400f;
        private const float AttractionRadius = 50f;
        private static Texture2D _texture;

        public static int BaseDamage { get; private set; } = 5;
        public static int DamageBonus { get; private set; } = 0;
        public static int TotalDamage => BaseDamage + DamageBonus;

        public Dynamite(Vector2 position)
        {
            Position = position;
            Value = TotalDamage;
        }

        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }

        public static void ApplyDamageBonus(int bonusAmount)
        {
            DamageBonus += bonusAmount;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();

            if (distance < AttractionRadius)
            {
                direction.Normalize();
                Position += direction * AttractionSpeed * deltaTime;
            }
        }

        public override void ApplyEffect(Player player)
        {
            // Используем новый менеджер звуков
            var explosion = new DynamiteExplosion(player.Position, TotalDamage, null); // SoundEffect больше не нужен
            DynamiteExplosion.ActiveExplosions.Add(explosion);
            IsActive = false;
        }

        public override bool CheckCollision(Player player)
        {
            return Vector2.Distance(Position, player.Position) < 25f;
        }
    }
}