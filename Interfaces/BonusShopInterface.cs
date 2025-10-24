using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Managers;

namespace Survive_the_night.Interfaces
{
    public class BonusShopInterface
    {
        private BonusShopMenu _shopMenu;
        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        // Кнопки
        private Rectangle _bonusButtonRect;
        private Rectangle _skillButtonRect;
        private Rectangle _exitButtonRect;

        private bool _isBonusButtonHovered = false;
        private bool _isSkillButtonHovered = false;
        private bool _isExitButtonHovered = false;

        public BonusShopInterface(BonusShopMenu shopMenu, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _shopMenu = shopMenu;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;

            int centerX = graphicsDevice.Viewport.Width / 2;
            int centerY = graphicsDevice.Viewport.Height / 2;

            // Кнопка бонусов (левая половина)
            _bonusButtonRect = new Rectangle(centerX - 300, centerY + 100, 200, 50);

            // Кнопка навыков (правая половина)
            _skillButtonRect = new Rectangle(centerX + 100, centerY + 100, 200, 50);

            // Кнопка выхода
            _exitButtonRect = new Rectangle(centerX - 100, centerY + 200, 200, 40);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_shopMenu.IsVisible) return;

            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            int centerX = screenWidth / 2;

            // Фон всего меню
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.8f);

            // Заголовок
            string title = "МАГАЗИН УЛУЧШЕНИЙ";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(centerX - titleSize.X / 2, 50), Color.Gold);

            // Разделительная линия
            spriteBatch.Draw(_debugTexture, new Rectangle(centerX, 100, 2, screenHeight - 200), Color.Gray);

            // === ЛЕВАЯ ПОЛОВИНА - БОНУСЫ ===
            DrawBonusSection(spriteBatch, centerX);

            // === ПРАВАЯ ПОЛОВИНА - НАВЫКИ ===
            DrawSkillSection(spriteBatch, centerX);

            // === КНОПКИ ===
            DrawButtons(spriteBatch);

            // === ИНФОРМАЦИЯ О МОНЕТАХ ===
            DrawCoinInfo(spriteBatch);
        }

        private void DrawBonusSection(SpriteBatch spriteBatch, int centerX)
        {
            // Заголовок бонусов с текущей ценой
            string bonusTitle = $"БОНУСЫ - {_shopMenu.CurrentBonusPrice} монет";
            Vector2 bonusTitleSize = _font.MeasureString(bonusTitle);
            spriteBatch.DrawString(_font, bonusTitle, new Vector2(centerX / 2 - bonusTitleSize.X / 2, 100), Color.Cyan);

            // Текущий выпавший бонус
            if (_shopMenu.CurrentBonus != null)
            {
                var bonus = _shopMenu.CurrentBonus;

                // Название бонуса и уровень
                string bonusName = $"{bonus.Name} (Ур. {bonus.LevelText})";
                Vector2 nameSize = _font.MeasureString(bonusName);
                spriteBatch.DrawString(_font, bonusName, new Vector2(centerX / 2 - nameSize.X / 2, 150), Color.LightGreen);

                // Описание
                spriteBatch.DrawString(_font, bonus.Description, new Vector2(centerX / 2 - 150, 180), Color.LightGray);

                if (bonus.IsMaxLevel)
                {
                    spriteBatch.DrawString(_font, "МАКСИМАЛЬНЫЙ УРОВЕНЬ!", new Vector2(centerX / 2 - 80, 210), Color.Gold);
                }
            }
            else
            {
                string hint = "Нажмите кнопку чтобы выпал бонус";
                Vector2 hintSize = _font.MeasureString(hint);
                spriteBatch.DrawString(_font, hint, new Vector2(centerX / 2 - hintSize.X / 2, 150), Color.Gray);
            }

            // Список всех бонусов и их уровней
            int yPos = 250;
            foreach (var bonus in _shopMenu.PlayerBonuses.Values)
            {
                string bonusInfo = $"{bonus.Name}: {bonus.LevelText}";
                Color color = bonus.IsMaxLevel ? Color.Gold : Color.White;
                spriteBatch.DrawString(_font, bonusInfo, new Vector2(50, yPos), color);
                yPos += 25;
            }
        }

        private void DrawSkillSection(SpriteBatch spriteBatch, int centerX)
        {
            // Заголовок навыков с текущей ценой
            string skillTitle = $"НАВЫКИ - {_shopMenu.CurrentSkillPrice} монет";
            Vector2 skillTitleSize = _font.MeasureString(skillTitle);
            spriteBatch.DrawString(_font, skillTitle, new Vector2(centerX + centerX / 2 - skillTitleSize.X / 2, 100), Color.Orange);

            // Текущий выпавший навык
            if (_shopMenu.CurrentSkill != null)
            {
                var skill = _shopMenu.CurrentSkill;

                string skillName = $"{skill.Name} (Ур. {skill.LevelText})";
                Vector2 nameSize = _font.MeasureString(skillName);
                spriteBatch.DrawString(_font, skillName, new Vector2(centerX + centerX / 2 - nameSize.X / 2, 150), Color.LightGreen);

                spriteBatch.DrawString(_font, skill.Description, new Vector2(centerX + 50, 180), Color.LightGray);

                if (skill.IsMaxLevel)
                {
                    spriteBatch.DrawString(_font, "МАКСИМАЛЬНЫЙ УРОВЕНЬ!", new Vector2(centerX + 70, 210), Color.Gold);
                }
            }
            else
            {
                string hint = "Система навыков в разработке";
                Vector2 hintSize = _font.MeasureString(hint);
                spriteBatch.DrawString(_font, hint, new Vector2(centerX + centerX / 2 - hintSize.X / 2, 150), Color.Gray);
            }
        }

        private void DrawButtons(SpriteBatch spriteBatch)
        {
            // Кнопка бонусов
            Color bonusButtonColor = _isBonusButtonHovered ? Color.LightBlue : Color.Cyan;
            if (_shopMenu.PlayerCoins < _shopMenu.CurrentBonusPrice) bonusButtonColor = Color.Gray;

            spriteBatch.Draw(_debugTexture, _bonusButtonRect, bonusButtonColor);
            string bonusButtonText = $"Купить бонус ({_shopMenu.CurrentBonusPrice})";
            Vector2 bonusTextSize = _font.MeasureString(bonusButtonText);
            Vector2 bonusTextPos = new Vector2(
                _bonusButtonRect.Center.X - bonusTextSize.X / 2,
                _bonusButtonRect.Center.Y - bonusTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, bonusButtonText, bonusTextPos, Color.Black);

            // Кнопка навыков
            Color skillButtonColor = _isSkillButtonHovered ? Color.LightSalmon : Color.Orange;
            if (_shopMenu.PlayerCoins < _shopMenu.CurrentSkillPrice) skillButtonColor = Color.Gray;

            spriteBatch.Draw(_debugTexture, _skillButtonRect, skillButtonColor);
            string skillButtonText = $"Купить навык ({_shopMenu.CurrentSkillPrice})";
            Vector2 skillTextSize = _font.MeasureString(skillButtonText);
            Vector2 skillTextPos = new Vector2(
                _skillButtonRect.Center.X - skillTextSize.X / 2,
                _skillButtonRect.Center.Y - skillTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, skillButtonText, skillTextPos, Color.Black);

            // Кнопка выхода
            Color exitButtonColor = _isExitButtonHovered ? Color.LightGray : Color.White;
            spriteBatch.Draw(_debugTexture, _exitButtonRect, exitButtonColor);
            string exitButtonText = "Выйти (ESC/B)"; // Обновляем подсказку
            Vector2 exitTextSize = _font.MeasureString(exitButtonText);
            Vector2 exitTextPos = new Vector2(
                _exitButtonRect.Center.X - exitTextSize.X / 2,
                _exitButtonRect.Center.Y - exitTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, exitButtonText, exitTextPos, Color.Black);
        }

        private void DrawCoinInfo(SpriteBatch spriteBatch)
        {
            string coinText = $"Монеты: {_shopMenu.PlayerCoins}";
            Vector2 coinTextSize = _font.MeasureString(coinText);
            Vector2 coinTextPos = new Vector2(
                _graphicsDevice.Viewport.Width - coinTextSize.X - 20,
                20
            );
            spriteBatch.DrawString(_font, coinText, coinTextPos, Color.Gold);
        }

        public void UpdateInput()
        {
            if (!_shopMenu.IsVisible) return;

            MouseState mouseState = Mouse.GetState();
            Point mousePos = mouseState.Position;

            // Обновляем состояние кнопок с учетом текущих цен
            _isBonusButtonHovered = _bonusButtonRect.Contains(mousePos) && _shopMenu.PlayerCoins >= _shopMenu.CurrentBonusPrice;
            _isSkillButtonHovered = _skillButtonRect.Contains(mousePos) && _shopMenu.PlayerCoins >= _shopMenu.CurrentSkillPrice;
            _isExitButtonHovered = _exitButtonRect.Contains(mousePos);

            // Обработка кликов
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isBonusButtonHovered)
                {
                    _shopMenu.RollBonus();
                }
                else if (_isSkillButtonHovered)
                {
                    _shopMenu.RollSkill();
                }
                else if (_isExitButtonHovered)
                {
                    _shopMenu.Hide();
                }
            }
        }
    }
}