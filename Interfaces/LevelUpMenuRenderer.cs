using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Managers;

namespace Survive_the_night.Interfaces
{
    public class LevelUpMenuRenderer
    {
        private LevelUpMenu _levelUpMenu;
        private Texture2D _debugTexture;
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public LevelUpMenuRenderer(LevelUpMenu levelUpMenu, GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _levelUpMenu = levelUpMenu;
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_levelUpMenu.IsVisible) return;

            Vector2 startPosition = new Vector2(50, 50);
            int boxHeight = 150;
            int boxWidth = _graphicsDevice.Viewport.Width - 100;
            const int boxSpacing = 20;

            spriteBatch.DrawString(_font, "�������� ���������", startPosition - new Vector2(0, 40), Color.Yellow);

            // �������� ������� ������� ���� ��� ���������
            Point mousePosition = Mouse.GetState().Position;

            for (int i = 0; i < _levelUpMenu.CurrentOptions.Count; i++)
            {
                var option = _levelUpMenu.CurrentOptions[i];
                Rectangle box = new Rectangle((int)startPosition.X, (int)startPosition.Y + i * boxHeight + i * boxSpacing, boxWidth, boxHeight);

                // ���
                Color boxColor = Color.DarkBlue;
                if (box.Contains(mousePosition))
                {
                    boxColor = Color.DarkSlateGray; // �������� ��� ���������
                }

                spriteBatch.Draw(_debugTexture, box, boxColor);

                // �����
                Vector2 textPos = new Vector2(box.X + 20, box.Y + 10);
                spriteBatch.DrawString(_font, $"[{i + 1}] {option.Title}", textPos, Color.White);

                textPos.Y += 40;
                spriteBatch.DrawString(_font, option.Description, textPos, Color.LightGray);
            }
        }
    }
}