using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Gamedata.Config.WeaponSystem;
using Survive_the_night.Gamedata.Managers;
using Survive_the_night.Localizations;
using System;

namespace Survive_the_night.Scripts.Interfaces
{
    public class RouletteMenu
    {
        private RouletteManager _rouletteManager;
        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public RouletteMenu(RouletteManager rouletteManager, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _rouletteManager = rouletteManager;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_rouletteManager.IsVisible) return;

            Vector2 startPosition = new Vector2(50, 50);
            int boxHeight = 150;
            int boxWidth = _graphicsDevice.Viewport.Width - 100;
            const int boxSpacing = 20;

            spriteBatch.DrawString(_font, "РУЛЕТКА - ВЫБЕРИТЕ НОВОЕ ОРУЖИЕ", startPosition - new Vector2(0, 40), Color.Gold);

            Point mousePosition = Mouse.GetState().Position;

            for (int i = 0; i < _rouletteManager.CurrentOptions.Count; i++)
            {
                var option = _rouletteManager.CurrentOptions[i];
                Rectangle box = new Rectangle((int)startPosition.X, (int)startPosition.Y + i * boxHeight + i * boxSpacing, boxWidth, boxHeight);

                // --- ВЫБОР ЦВЕТА В ЗАВИСИМОСТИ ОТ ТИПА ---
                Color boxColor;

                if (option.IsSkipOption)
                {
                    boxColor = Color.DarkGray; // Пропуск
                }
                else if (option.IsAwakenOption)
                {
                    boxColor = new Color(180, 70, 220); // ПУРПУРНЫЙ для пробуждения
                }
                else
                {
                    // Проверяем тип обычного/легендарного оружия
                    WeaponName weaponName = GetWeaponNameFromTitle(option.Title);
                    boxColor = WeaponManager.LegendaryWeapons.Contains(weaponName)
                        ? Color.DarkGoldenrod // Легендарное
                        : Color.DarkRed;      // Обычное
                }

                // Подсветка при наведении
                if (box.Contains(mousePosition))
                {
                    boxColor = boxColor * 1.3f; // Делаем ярче
                }

                spriteBatch.Draw(_debugTexture, box, boxColor);

                // --- ТЕКСТ ---
                Vector2 textPos = new Vector2(box.X + 20, box.Y + 10);

                // Цвет текста
                Color titleColor = option.IsAwakenOption
                    ? Color.Purple  // Пурпурный текст для пробуждения
                    : Color.White;

                spriteBatch.DrawString(_font, $"[{i + 1}] {option.Title}", textPos, titleColor);

                // Описание (можно добавить спец. цвет для описания пробуждения)
                textPos.Y += 40;
                Color descColor = option.IsAwakenOption ? Color.Lavender : Color.LightGray;
                spriteBatch.DrawString(_font, option.Description, textPos, descColor);

                // Дополнительная надпись для пробуждения
                if (option.IsAwakenOption)
                {
                    textPos.Y += 30;
                    //spriteBatch.DrawString(_font, "МЕГА-УЛУЧШЕНИЕ", textPos, Color.Gold);
                }
            }
        }

        // Вспомогательный метод для получения WeaponName из заголовка
        private WeaponName GetWeaponNameFromTitle(string title)
        {
            // Проходим по всем оружиям и ищем совпадение
            foreach (WeaponName weaponName in Enum.GetValues(typeof(WeaponName)))
            {
                string weaponTitle = LocalizationManager.GetWeaponRouletteTitle(weaponName);
                if (title == weaponTitle || title.Contains(weaponTitle))
                {
                    return weaponName;
                }
            }

            // Если не нашли, возвращаем первое оружие (заглушка)
            return WeaponName.PlayingCards;
        }
    }
}