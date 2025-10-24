using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survive_the_night.Entities;
using Survive_the_night.Interfaces;
using System;

namespace Survive_the_night.Managers
{
    public class GameHUD : IGameHUD
    {
        private readonly Player _player;
        private readonly LevelManager _levelManager;
        private readonly SpriteFont _font;
        private readonly Texture2D _debugTexture;
        private readonly Viewport _viewport;

        // Данные для отрисовки
        private float _survivalTime;
        private int _killCount;

        // Анимация этапа
        private string _stageAnnouncementText = "";
        private float _stageAnnouncementTimer = 0f;
        private const float STAGE_ANNOUNCEMENT_DURATION = 3f;
        private Color _stageAnnouncementColor = Color.Transparent;

        public GameHUD(Player player, LevelManager levelManager, SpriteFont font, Texture2D debugTexture, Viewport viewport)
        {
            _player = player;
            _levelManager = levelManager;
            _font = font;
            _debugTexture = debugTexture;
            _viewport = viewport;
        }

        public void UpdateGameStats(float survivalTime, int killCount)
        {
            _survivalTime = survivalTime;
            _killCount = killCount;
        }

        public void Update(GameTime gameTime)
        {
            UpdateStageAnnouncement(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawHealthBar(spriteBatch);
            DrawExperienceBar(spriteBatch);
            DrawTopHUD(spriteBatch);
        }

        public void DrawStageAnnouncement(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(_stageAnnouncementText) && _stageAnnouncementColor.A > 0)
            {
                Vector2 textSize = _font.MeasureString(_stageAnnouncementText);
                Vector2 position = new Vector2(
                    (_viewport.Width - textSize.X) / 2,
                    150 // Отступ сверху
                );

                // Тень текста
                spriteBatch.DrawString(_font, _stageAnnouncementText, position + new Vector2(2, 2),
                    Color.Black * (_stageAnnouncementColor.A / 255f));
                // Основной текст
                spriteBatch.DrawString(_font, _stageAnnouncementText, position, _stageAnnouncementColor);
            }
        }

        public void ShowStageAnnouncement(int stage)
        {
            _stageAnnouncementText = $"ЭТАП {stage}";
            _stageAnnouncementTimer = STAGE_ANNOUNCEMENT_DURATION;
            _stageAnnouncementColor = Color.Gold;
        }

        private void UpdateStageAnnouncement(GameTime gameTime)
        {
            if (_stageAnnouncementTimer > 0)
            {
                _stageAnnouncementTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Анимация появления/исчезания
                if (_stageAnnouncementTimer > STAGE_ANNOUNCEMENT_DURATION - 0.5f)
                {
                    // Появление (первые 0.5 секунды)
                    float progress = 1f - (_stageAnnouncementTimer - (STAGE_ANNOUNCEMENT_DURATION - 0.5f)) / 0.5f;
                    _stageAnnouncementColor = Color.Gold * progress;
                }
                else if (_stageAnnouncementTimer < 0.5f)
                {
                    // Исчезание (последние 0.5 секунды)
                    float progress = _stageAnnouncementTimer / 0.5f;
                    _stageAnnouncementColor = Color.Gold * progress;
                }
                else
                {
                    // Полная видимость
                    _stageAnnouncementColor = Color.Gold;
                }

                if (_stageAnnouncementTimer <= 0)
                {
                    _stageAnnouncementText = "";
                    _stageAnnouncementColor = Color.Transparent;
                }
            }
        }

        private void DrawTopHUD(SpriteBatch spriteBatch)
        {
            int screenWidth = _viewport.Width;

            // Отрисовка УРОВНЯ (слева вверху)
            spriteBatch.DrawString(_font, $"УРОВЕНЬ: {_player.Level}", new Vector2(10, 35), Color.White);

            // Отрисовка ТАЙМЕРА (по центру вверху)
            int minutes = (int)(_survivalTime / 60);
            int seconds = (int)(_survivalTime % 60);
            string timeString = $"{minutes:00}:{seconds:00}";

            Vector2 timeSize = _font.MeasureString(timeString);
            Vector2 timePosition = new Vector2(
                (screenWidth - timeSize.X) / 2,
                10
            );

            spriteBatch.DrawString(_font, timeString, timePosition, Color.Yellow);

            // Отрисовка СЧЕТЧИКА КИЛЛОВ (справа вверху)
            string killString = $"КИЛЛЫ: {_killCount}";
            Vector2 killSize = _font.MeasureString(killString);
            Vector2 killPosition = new Vector2(
                screenWidth - killSize.X - 10,
                10
            );

            spriteBatch.DrawString(_font, killString, killPosition, Color.Red);

            // Отрисовка МОНЕТ (под киллами)
            string coinString = $"МОНЕТЫ: {_player.Coins}";
            Vector2 coinSize = _font.MeasureString(coinString);
            Vector2 coinPosition = new Vector2(
                screenWidth - coinSize.X - 10,
                35 // Под счетчиком киллов
            );

            spriteBatch.DrawString(_font, coinString, coinPosition, Color.Gold);
        }

        private void DrawExperienceBar(SpriteBatch spriteBatch)
        {
            int screenWidth = _viewport.Width;
            const int BarHeight = 10;
            const int BarX = 0;
            const int BarY = 720 - BarHeight;

            int barWidth = screenWidth;

            float experienceRatio = (float)_player.CurrentExperience / _player.ExperienceToNextLevel;
            int currentExperienceWidth = (int)(barWidth * experienceRatio);

            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, barWidth, BarHeight), Color.DarkSlateBlue);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, currentExperienceWidth, BarHeight), Color.Purple);

            // Отрисовка текста XP
            string xpText = $"ОПЫТ: {_player.CurrentExperience} / {_player.ExperienceToNextLevel}";
            Vector2 textSize = _font.MeasureString(xpText);

            Vector2 textPosition = new Vector2(
                (screenWidth - textSize.X) / 2,
                BarY - textSize.Y - 5
            );

            spriteBatch.DrawString(_font, xpText, textPosition, Color.White);
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            const int BarWidth = 200;
            const int BarHeight = 20;
            const int BarX = 10;
            const int BarY = 10;

            float healthRatio = (float)_player.CurrentHealth / _player.MaxHealth;
            int currentHealthWidth = (int)(BarWidth * healthRatio);

            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, BarWidth, BarHeight), Color.DarkGray);
            spriteBatch.Draw(
                _debugTexture,
                new Rectangle(BarX, BarY, currentHealthWidth, BarHeight),
                Color.Lerp(Color.Red, Color.LimeGreen, healthRatio)
            );

            // Рамка
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY + BarHeight - 1, BarWidth, 1), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX, BarY, 1, BarHeight), Color.White);
            spriteBatch.Draw(_debugTexture, new Rectangle(BarX + BarWidth - 1, BarY, 1, BarHeight), Color.White);

            // Отрисовка текста HP
            string healthText = $"HP: {_player.CurrentHealth}/{_player.MaxHealth}";
            Vector2 textSize = _font.MeasureString(healthText);

            Vector2 textPosition = new Vector2(
                BarX + (BarWidth - textSize.X) / 2,
                BarY + (BarHeight - textSize.Y) / 2
            );

            spriteBatch.DrawString(_font, healthText, textPosition, Color.White);
        }
    }
}