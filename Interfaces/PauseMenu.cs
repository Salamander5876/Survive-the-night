using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survive_the_night.Interfaces
{
    public class PauseMenu
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        // ������
        private Rectangle _resumeButtonRect;
        private Rectangle _exitButtonRect;

        private bool _isResumeButtonHovered = false;
        private bool _isExitButtonHovered = false;

        public bool IsVisible { get; set; }

        public PauseMenu(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;

            CalculateLayout();
        }

        private void CalculateLayout()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;

            // ������� ������
            int buttonWidth = 300;
            int buttonHeight = 60;
            int buttonSpacing = 20;

            // ������� ������
            _resumeButtonRect = new Rectangle(
                centerX - buttonWidth / 2,
                centerY - buttonHeight / 2,
                buttonWidth,
                buttonHeight
            );

            _exitButtonRect = new Rectangle(
                centerX - buttonWidth / 2,
                _resumeButtonRect.Bottom + buttonSpacing,
                buttonWidth,
                buttonHeight
            );
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Update()
        {
            if (!IsVisible) return;

            MouseState mouseState = Mouse.GetState();
            Point mousePos = mouseState.Position;

            // ��������� ��������� ������
            _isResumeButtonHovered = _resumeButtonRect.Contains(mousePos);
            _isExitButtonHovered = _exitButtonRect.Contains(mousePos);

            // ��������� ������
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isResumeButtonHovered)
                {
                    Hide();
                }
                else if (_isExitButtonHovered)
                {
                    // ������ ���������� ��������� ����
                    Game1.CurrentState = GameState.MainMenu;
                    Hide();
                    System.Diagnostics.Debug.WriteLine("������ '����� � ����' ������, ��������� �������� �� MainMenu");
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // �������������� ���
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.7f);

            // ���������
            string title = "�����";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2(
                (screenWidth - titleSize.X) / 2,
                200
            );
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            // ������ "����������"
            Color resumeColor = _isResumeButtonHovered ? Color.LightGreen : Color.Green;
            spriteBatch.Draw(_debugTexture, _resumeButtonRect, resumeColor);

            string resumeText = "����������";
            Vector2 resumeTextSize = _font.MeasureString(resumeText);
            Vector2 resumeTextPos = new Vector2(
                _resumeButtonRect.Center.X - resumeTextSize.X / 2,
                _resumeButtonRect.Center.Y - resumeTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, resumeText, resumeTextPos, Color.White);

            // ������ "����� � ����"
            Color exitColor = _isExitButtonHovered ? Color.LightSalmon : Color.Red;
            spriteBatch.Draw(_debugTexture, _exitButtonRect, exitColor);

            string exitText = "����� � ����";
            Vector2 exitTextSize = _font.MeasureString(exitText);
            Vector2 exitTextPos = new Vector2(
                _exitButtonRect.Center.X - exitTextSize.X / 2,
                _exitButtonRect.Center.Y - exitTextSize.Y / 2
            );
            spriteBatch.DrawString(_font, exitText, exitTextPos, Color.White);
        }
    }
}