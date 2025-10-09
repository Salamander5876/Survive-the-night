using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survive_the_night.Entities
{
    public abstract class BaseHealthOrb : GameObject
    {
        protected const int OrbWidth = 22;
        protected const int OrbHeight = 22;
        protected const float AttractionSpeed = 400f;
        public bool IsActive { get; set; }
        public float HealAmount { get; protected set; } // Процент от MaxHealth

        public BaseHealthOrb(Vector2 initialPosition, int size, Color color, float healAmount)
            : base(initialPosition, size, color)
        {
            HealAmount = healAmount;
            IsActive = true;
        }

        public override void Update(GameTime gameTime) { /* Логика не используется */ }

        public void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();
            const float NewAttractionRadius = 50f;
            if (distance < NewAttractionRadius)
            {
                direction.Normalize();
                Position += direction * AttractionSpeed * deltaTime;
            }
            if (GetBounds().Intersects(player.GetBounds()))
            {
                IsActive = false;
            }
        }
    }

    public class HealthOrb : BaseHealthOrb
    {
        private static Texture2D _texture;
        public HealthOrb(Vector2 initialPosition, float healPercentage)
            : base(initialPosition, OrbHeight, Color.LimeGreen, healPercentage) { }
        public static void SetTexture(Texture2D texture) { _texture = texture; }
        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            if (_texture != null)
            {
                Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
                float scaleX = OrbWidth / (float)_texture.Width;
                float scaleY = OrbHeight / (float)_texture.Height;
                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    0f,
                    origin,
                    new Vector2(scaleX, scaleY),
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                base.Draw(spriteBatch, debugTexture, color ?? this.Color);
            }
        }
        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(Position.X - OrbWidth / 2f),
                (int)(Position.Y - OrbHeight / 2f),
                OrbWidth,
                OrbHeight
            );
        }
    }

    // Новый класс: Золотое сердце
    public class GoldenHeartOrb : GameObject
    {
        private const int OrbWidth = 22;
        private const int OrbHeight = 22;
        private const float AttractionSpeed = 400f;
        private static Texture2D _texture;

        public bool IsActive { get; set; }
        public float HealAmount { get; private set; } // Процент от MaxHealth

        public GoldenHeartOrb(Vector2 initialPosition, float healPercentage = 0.5f)
            : base(initialPosition, OrbHeight, Color.Gold)
        {
            HealAmount = healPercentage;
            IsActive = true;
        }

        public override void Update(GameTime gameTime) { /* Логика не используется */ }

        public void Update(GameTime gameTime, Player player)
        {
            if (!IsActive) return;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = player.Position - Position;
            float distance = direction.Length();
            const float NewAttractionRadius = 50f;
            if (distance < NewAttractionRadius)
            {
                direction.Normalize();
                Position += direction * AttractionSpeed * deltaTime;
            }
            if (GetBounds().Intersects(player.GetBounds()))
            {
                IsActive = false;
            }
        }

        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture, Color? color = null)
        {
            if (_texture != null)
            {
                Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
                float scaleX = OrbWidth / (float)_texture.Width;
                float scaleY = OrbHeight / (float)_texture.Height;
                spriteBatch.Draw(
                    _texture,
                    Position,
                    null,
                    Color.White,
                    0f,
                    origin,
                    new Vector2(scaleX, scaleY),
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                base.Draw(spriteBatch, debugTexture, color ?? this.Color);
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(Position.X - OrbWidth / 2f),
                (int)(Position.Y - OrbHeight / 2f),
                OrbWidth,
                OrbHeight
            );
        }
    }
}