using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survive_the_night.Weapons;
using System;
using System.Collections.Generic;

namespace Survive_the_night.Interfaces
{
    public class StartMenu
    {
        private GraphicsDevice _graphicsDevice;
        private Texture2D _debugTexture;
        private SpriteFont _font;

        // �������� ��� GUI
        private Texture2D _weaponCellTexture;
        private Texture2D _upButtonTexture;
        private Texture2D _downButtonTexture;

        // ��������� �����
        private MouseState _previousMouseState;
        private MouseState _currentMouseState;

        // ��������� UI
        private Rectangle _startButtonRect;
        private Rectangle _backButtonRect;
        private Rectangle _upButtonRect;
        private Rectangle _downButtonRect;
        private Rectangle _weaponCellRect;
        private Rectangle _descriptionRect;
        private Rectangle _scrollBarRect;
        private Rectangle _scrollThumbRect;

        // ������ ����
        private string _playerName = "�����";
        private List<WeaponName> _availableWeapons;
        private int _selectedWeaponIndex = 0;
        private float _scrollPosition = 0f;
        private float _maxScroll = 0f;
        private bool _isScrolling = false;

        // ���������
        private const int ButtonWidth = 200;
        private const int ButtonHeight = 50;
        private const int WeaponCellSize = 120;
        private const int ArrowButtonSize = 40;
        private const int DescriptionWidth = 400;
        private const int DescriptionHeight = 200;
        private const int ScrollBarWidth = 15;
        private const int ScrollThumbMinHeight = 30;

        public WeaponName SelectedWeapon => _availableWeapons[_selectedWeaponIndex];
        public string PlayerName => _playerName;

        // �������� ������
        private Dictionary<WeaponName, string> _weaponDescriptions = new Dictionary<WeaponName, string>
        {
            {
                WeaponName.PlayingCards,
                "��������� �����\n\n������ ������, ������� ��������� �� 3 ������ �� ���� �������. " +
                "����� ����� �� ������ ���������� � ������� ���� ���� ������ �� ����� ����. " +
                "������� �������� ��� ������ � ������� �����������."
            },
            {
                WeaponName.GoldenBullet,
                "������� ����\n\n������ ������ � ������� ������ �� ����� ����. " +
                "���� ����� � ������� ��������� � �������������� �������� ���������� �����. " +
                "�������� ��� ��������� ����������� ������� �����������."
            },
            {
                WeaponName.CasinoChips,
                "����� ������\n\n�����, ������� ����������� ����� �������. " +
                "������ ����� ����� �������� ���������� ������, ������������ ����� ����. " +
                "���������� ������ �����, ������������� ������ ���� � �����."
            }
        };

        public StartMenu(GraphicsDevice graphicsDevice, Texture2D debugTexture, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _debugTexture = debugTexture;
            _font = font;
            _previousMouseState = Mouse.GetState();
            _currentMouseState = Mouse.GetState();

            // ������������� ������ ������
            _availableWeapons = new List<WeaponName>
            {
                WeaponName.PlayingCards,
                WeaponName.GoldenBullet,
                WeaponName.CasinoChips
            };

            CalculateLayout();
        }

        public void LoadContent(Texture2D weaponCell, Texture2D upButton, Texture2D downButton)
        {
            _weaponCellTexture = weaponCell;
            _upButtonTexture = upButton;
            _downButtonTexture = downButton;
        }

        private void CalculateLayout()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // ����������� ������� ��� ������ ������
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;

            // Weapon selection (�� ������)
            _weaponCellRect = new Rectangle(
                centerX - WeaponCellSize / 2,
                centerY - 100,
                WeaponCellSize,
                WeaponCellSize
            );

            // ������ ������ ������
            _upButtonRect = new Rectangle(
                centerX - ArrowButtonSize / 2,
                _weaponCellRect.Y - ArrowButtonSize - 15,
                ArrowButtonSize,
                ArrowButtonSize
            );

            _downButtonRect = new Rectangle(
                centerX - ArrowButtonSize / 2,
                _weaponCellRect.Y + WeaponCellSize + 15,
                ArrowButtonSize,
                ArrowButtonSize
            );

            // ������� �������� (������ �����)
            _descriptionRect = new Rectangle(
                screenWidth - DescriptionWidth - 50,
                200,
                DescriptionWidth,
                DescriptionHeight
            );

            // ������ ���������
            _scrollBarRect = new Rectangle(
                _descriptionRect.X + _descriptionRect.Width - ScrollBarWidth,
                _descriptionRect.Y,
                ScrollBarWidth,
                _descriptionRect.Height
            );

            UpdateScrollThumb();

            // ������ ������ (������ ������ ����)
            _startButtonRect = new Rectangle(
                screenWidth - ButtonWidth - 30,
                screenHeight - ButtonHeight - 30,
                ButtonWidth,
                ButtonHeight
            );

            // ������ ����� (����� ������ ����)
            _backButtonRect = new Rectangle(
                30,
                screenHeight - ButtonHeight - 30,
                ButtonWidth,
                ButtonHeight
            );
        }

        private void UpdateScrollThumb()
        {
            float contentHeight = GetDescriptionTextHeight();
            float visibleRatio = _descriptionRect.Height / contentHeight;

            if (visibleRatio >= 1f)
            {
                _maxScroll = 0f;
                _scrollPosition = 0f;
                _scrollThumbRect = Rectangle.Empty;
                return;
            }

            _maxScroll = contentHeight - _descriptionRect.Height;

            int thumbHeight = (int)(_descriptionRect.Height * visibleRatio);
            thumbHeight = Math.Max(thumbHeight, ScrollThumbMinHeight);

            float scrollRatio = _scrollPosition / _maxScroll;
            int thumbY = _scrollBarRect.Y + (int)((_descriptionRect.Height - thumbHeight) * scrollRatio);

            _scrollThumbRect = new Rectangle(
                _scrollBarRect.X,
                thumbY,
                ScrollBarWidth,
                thumbHeight
            );
        }

        private float GetDescriptionTextHeight()
        {
            WeaponName currentWeapon = _availableWeapons[_selectedWeaponIndex];
            string description = _weaponDescriptions.ContainsKey(currentWeapon)
                ? _weaponDescriptions[currentWeapon]
                : "�������� �����������.";

            return MeasureWrappedTextHeight(description, _descriptionRect.Width - ScrollBarWidth - 20);
        }

        private float MeasureWrappedTextHeight(string text, float maxWidth)
        {
            string[] words = text.Split(' ');
            string line = "";
            float lineHeight = _font.MeasureString("A").Y;
            float totalHeight = 0f;
            int lineCount = 0;

            foreach (string word in words)
            {
                string testLine = line + (line == "" ? "" : " ") + word;
                Vector2 size = _font.MeasureString(testLine);

                if (size.X > maxWidth && line != "")
                {
                    totalHeight += lineHeight;
                    lineCount++;
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }

            if (line != "")
            {
                totalHeight += lineHeight;
                lineCount++;
            }

            return totalHeight;
        }

        public GameState Update(GameTime gameTime)
        {
            _currentMouseState = Mouse.GetState();

            // ��������� ��������� ��������� ����
            if (_descriptionRect.Contains(_currentMouseState.Position))
            {
                _scrollPosition -= _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
                _scrollPosition = MathHelper.Clamp(_scrollPosition, 0, _maxScroll);
                UpdateScrollThumb();
            }

            // ��������� �������������� ��������
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (_scrollThumbRect.Contains(_currentMouseState.Position) && !_isScrolling)
                {
                    _isScrolling = true;
                }
            }
            else
            {
                _isScrolling = false;
            }

            if (_isScrolling)
            {
                float relativeY = _currentMouseState.Y - _scrollBarRect.Y;
                float scrollRatio = relativeY / (_scrollBarRect.Height - _scrollThumbRect.Height);
                _scrollPosition = scrollRatio * _maxScroll;
                _scrollPosition = MathHelper.Clamp(_scrollPosition, 0, _maxScroll);
                UpdateScrollThumb();
            }

            // ��������� ������ �� �������
            if (_currentMouseState.LeftButton == ButtonState.Released &&
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                // ������ �����
                if (_upButtonRect.Contains(_currentMouseState.Position))
                {
                    _selectedWeaponIndex--;
                    if (_selectedWeaponIndex < 0)
                        _selectedWeaponIndex = _availableWeapons.Count - 1;
                    _scrollPosition = 0f;
                    UpdateScrollThumb();
                }

                // ������ ����
                if (_downButtonRect.Contains(_currentMouseState.Position))
                {
                    _selectedWeaponIndex++;
                    if (_selectedWeaponIndex >= _availableWeapons.Count)
                        _selectedWeaponIndex = 0;
                    _scrollPosition = 0f;
                    UpdateScrollThumb();
                }

                // ������ �����
                if (_startButtonRect.Contains(_currentMouseState.Position))
                {
                    return GameState.Playing;
                }

                // ������ �����
                if (_backButtonRect.Contains(_currentMouseState.Position))
                {
                    return GameState.MainMenu;
                }
            }

            _previousMouseState = _currentMouseState;
            return GameState.StartMenu;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            // ���
            spriteBatch.Draw(_debugTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkBlue);

            // ���������
            string title = "����� ���������� ������";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePos = new Vector2((screenWidth - titleSize.X) / 2, 80);
            spriteBatch.DrawString(_font, title, titlePos, Color.White);

            // ��� ������
            string nameText = $"���: {_playerName}";
            Vector2 namePos = new Vector2((screenWidth - _font.MeasureString(nameText).X) / 2, 120);
            spriteBatch.DrawString(_font, nameText, namePos, Color.LightGray);

            // �������� ���������� ������ (��� �������� ��������)
            string weaponName = WeaponManager.GetDisplayName(_availableWeapons[_selectedWeaponIndex]);
            Vector2 weaponNameSize = _font.MeasureString(weaponName);
            Vector2 weaponNamePos = new Vector2(
                _descriptionRect.Center.X - weaponNameSize.X / 2,
                _descriptionRect.Y - weaponNameSize.Y - 10
            );
            spriteBatch.DrawString(_font, weaponName, weaponNamePos, Color.Yellow);

            // ��������� ������ ������
            DrawWeaponSelection(spriteBatch);

            // ��������� �������� ������
            DrawWeaponDescription(spriteBatch);

            // ������
            DrawButton(spriteBatch, _startButtonRect, "������ ����", Color.Green);
            DrawButton(spriteBatch, _backButtonRect, "�����", Color.Gray);
        }

        private void DrawWeaponSelection(SpriteBatch spriteBatch)
        {
            // ������ ������
            if (_weaponCellTexture != null)
                spriteBatch.Draw(_weaponCellTexture, _weaponCellRect, Color.White);
            else
                spriteBatch.Draw(_debugTexture, _weaponCellRect, Color.DarkGray);

            // ������ ������
            if (_upButtonTexture != null)
                spriteBatch.Draw(_upButtonTexture, _upButtonRect, Color.White);
            else
                spriteBatch.Draw(_debugTexture, _upButtonRect, Color.LightGray);

            if (_downButtonTexture != null)
                spriteBatch.Draw(_downButtonTexture, _downButtonRect, Color.White);
            else
                spriteBatch.Draw(_debugTexture, _downButtonRect, Color.LightGray);
        }

        private void DrawWeaponDescription(SpriteBatch spriteBatch)
        {
            // ��� ������� ��������
            spriteBatch.Draw(_debugTexture, _descriptionRect, Color.DarkSlateBlue * 0.8f);

            // �����
            DrawRectangle(spriteBatch, _descriptionRect, Color.White);

            // ������ ��������� (���� �����)
            if (_maxScroll > 0)
            {
                spriteBatch.Draw(_debugTexture, _scrollBarRect, Color.Gray);
                spriteBatch.Draw(_debugTexture, _scrollThumbRect, Color.LightGray);
            }

            // ����� �������� � ������ ���������
            WeaponName currentWeapon = _availableWeapons[_selectedWeaponIndex];
            string description = _weaponDescriptions.ContainsKey(currentWeapon)
                ? _weaponDescriptions[currentWeapon]
                : "�������� �����������.";

            // ������� ��� ������ (�������� ������ ���������)
            Rectangle textArea = new Rectangle(
                _descriptionRect.X + 10,
                _descriptionRect.Y + 10,
                _descriptionRect.Width - ScrollBarWidth - 20,
                _descriptionRect.Height - 20
            );

            DrawWrappedText(spriteBatch, description, textArea, _scrollPosition);
        }

        private void DrawWrappedText(SpriteBatch spriteBatch, string text, Rectangle textArea, float scrollOffset)
        {
            string[] words = text.Split(' ');
            string line = "";
            float lineHeight = _font.MeasureString("A").Y;
            Vector2 currentPos = new Vector2(textArea.X, textArea.Y - scrollOffset);

            foreach (string word in words)
            {
                string testLine = line + (line == "" ? "" : " ") + word;
                Vector2 size = _font.MeasureString(testLine);

                if (size.X > textArea.Width && line != "")
                {
                    // ������ �����, ���� ��� ������
                    if (currentPos.Y + lineHeight >= textArea.Y && currentPos.Y <= textArea.Y + textArea.Height)
                    {
                        spriteBatch.DrawString(_font, line, new Vector2(textArea.X, currentPos.Y), Color.White);
                    }

                    currentPos.Y += lineHeight;
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }

            // ������ ��������� �����, ���� ������
            if (line != "" && currentPos.Y + lineHeight >= textArea.Y && currentPos.Y <= textArea.Y + textArea.Height)
            {
                spriteBatch.DrawString(_font, line, new Vector2(textArea.X, currentPos.Y), Color.White);
            }
        }

        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            // ������� �����
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            // ������ �����
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), color);
            // ����� �����
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X, rect.Y, 2, rect.Height), color);
            // ������ �����
            spriteBatch.Draw(_debugTexture, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), color);
        }

        private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text, Color baseColor)
        {
            Color buttonColor = baseColor;
            if (rect.Contains(_currentMouseState.Position))
                buttonColor = Color.Lerp(baseColor, Color.White, 0.3f);

            spriteBatch.Draw(_debugTexture, rect, buttonColor);

            Vector2 textSize = _font.MeasureString(text);
            Vector2 textPos = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2,
                rect.Y + (rect.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(_font, text, textPos, Color.White);
        }
    }
}