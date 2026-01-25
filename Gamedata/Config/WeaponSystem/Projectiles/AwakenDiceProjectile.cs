// Gamedata/Config/WeaponSystem/Projectiles/AwakenDiceProjectile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Entities;
using Survive_the_night.Gamedata.Config.WeaponSystem;

namespace Survive_the_night.Gamedata.Config.WeaponSystem.Projectiles
{
    public class AwakenDiceProjectile : Projectile
    {
        public int DiceValue { get; private set; }
        private Player Player { get; set; }
        private float OrbitRadius { get; set; } = 100f;
        private float OrbitAngle { get; set; }
        public bool OrbitClockwise { get; private set; } // ИСПРАВЛЕНО: сеттер private

        private static Texture2D[] _regularDiceTextures = new Texture2D[6];
        private static Texture2D[] _megaDiceTextures = new Texture2D[6];

        private float _orbitSpeed = 180f;
        private bool _useMegaTexture;
        private bool _isOrbiting = false;

        public AwakenDiceProjectile(int diceValue, Player player, bool orbitClockwise,
                                  int damage, int hitsLeft, bool useMegaTexture = false)
            : base(Vector2.Zero, 32, Color.White, damage, 0f, Vector2.Zero, hitsLeft)
        {
            DiceValue = diceValue;
            Player = player;
            OrbitClockwise = orbitClockwise; // Это теперь работает, т.к. сеттер private
            _useMegaTexture = useMegaTexture;

            SetLifeTime(90f);

            // Устанавливаем начальный угол
            float initialAngle = (float)(diceValue * MathHelper.TwoPi / 6f);
            OrbitAngle = initialAngle;

            // Сразу устанавливаем позицию на орбите
            UpdatePosition();

            // Размер для мега-костей больше
            Size = _useMegaTexture ? 40 : 32;

            // НАЧАЛЬНОЕ ВРАЩЕНИЕ = 0
            Rotation = 0f;
        }

        // Метод для загрузки обычных текстур
        public static void LoadRegularTextures(Texture2D dice1, Texture2D dice2, Texture2D dice3,
                                             Texture2D dice4, Texture2D dice5, Texture2D dice6)
        {
            _regularDiceTextures[0] = dice1;
            _regularDiceTextures[1] = dice2;
            _regularDiceTextures[2] = dice3;
            _regularDiceTextures[3] = dice4;
            _regularDiceTextures[4] = dice5;
            _regularDiceTextures[5] = dice6;
        }

        // Метод для загрузки мега-текстур
        public static void LoadMegaTextures(Texture2D dice1, Texture2D dice2, Texture2D dice3,
                                          Texture2D dice4, Texture2D dice5, Texture2D dice6)
        {
            _megaDiceTextures[0] = dice1;
            _megaDiceTextures[1] = dice2;
            _megaDiceTextures[2] = dice3;
            _megaDiceTextures[3] = dice4;
            _megaDiceTextures[4] = dice5;
            _megaDiceTextures[5] = dice6;
        }

        // Метод для обновления позиции на орбите
        private void UpdatePosition()
        {
            if (Player == null) return;

            Position = Player.Position + new Vector2(
                (float)System.Math.Cos(OrbitAngle) * OrbitRadius,
                (float)System.Math.Sin(OrbitAngle) * OrbitRadius
            );
        }

        // Агрессивное следование за игроком (во время спавна)
        public void UpdateAggressiveFollow()
        {
            if (!IsActive || Player == null) return;

            UpdateLifeTime(new GameTime());

            // Жестко привязываем к текущей позиции игрока
            UpdatePosition();

            // НЕТ ВРАЩЕНИЯ во время агрессивного следования
        }

        // Обновление орбиты с заданной скоростью и радиусом
        public void UpdateOrbitPosition(float deltaTime, float orbitSpeed, float orbitRadius)
        {
            if (!IsActive || Player == null) return;

            UpdateLifeTime(new GameTime());

            // Обновляем угол орбиты - orbitSpeed может быть как положительной, так и отрицательной
            float direction = OrbitClockwise ? 1f : -1f;
            OrbitAngle += MathHelper.ToRadians(orbitSpeed * direction * deltaTime);
            OrbitRadius = orbitRadius;

            // Обновляем позицию
            UpdatePosition();

            // Вращение начинается только когда кости крутятся вокруг игрока
            Rotation += 360f * deltaTime;
        }

        // Переключение в режим вращения
        public void StartOrbiting()
        {
            _isOrbiting = true;
        }

        public override void Update(GameTime gameTime)
        {
            // Базовый метод ничего не делает - вся логика в UpdateOrbitPosition
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (!IsActive) return;

            Texture2D textureToUse = null;

            if (_useMegaTexture)
            {
                // Используем мега-текстуры для второго круга
                if (DiceValue >= 1 && DiceValue <= 6 && _megaDiceTextures[DiceValue - 1] != null)
                {
                    textureToUse = _megaDiceTextures[DiceValue - 1];
                }
            }
            else
            {
                // Используем обычные текстуры для первого круга
                if (DiceValue >= 1 && DiceValue <= 6 && _regularDiceTextures[DiceValue - 1] != null)
                {
                    textureToUse = _regularDiceTextures[DiceValue - 1];
                }
            }

            if (textureToUse != null)
            {
                DrawWithTexture(spriteBatch, textureToUse);
            }
            else
            {
                base.Draw(spriteBatch, debugTexture);
            }
        }

        public void OnHitEnemy()
        {
            // Проигрываем звук попадания
            WeaponManager.PlayWeaponSound(WeaponName.Dice, 0.5f);
        }

        // Метод для установки радиуса орбиты
        public void SetOrbitRadius(float radius)
        {
            OrbitRadius = radius;
        }
    }
}