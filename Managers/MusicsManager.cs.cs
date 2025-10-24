using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Survive_the_night.Managers
{
    public class MusicsManager
    {
        private Dictionary<int, Song> _levelSongs;
        private int _currentLevel = 1;
        private bool _isMusicEnabled = true;

        public MusicsManager()
        {
            _levelSongs = new Dictionary<int, Song>();
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                // Загружаем музыку для всех 8 уровней
                for (int i = 1; i <= 8; i++)
                {
                    string songPath = $"Musics/HardLevel{i}";
                    _levelSongs[i] = content.Load<Song>(songPath);
                }

                // Настраиваем медиаплеер
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.5f; // 50% громкость
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки музыки: {ex.Message}");
                // Продолжаем без музыки, если есть ошибки
            }
        }

        public void PlayLevelMusic(int level)
        {
            if (!_isMusicEnabled) return;

            try
            {
                if (_currentLevel == level && MediaPlayer.State == MediaState.Playing)
                    return;

                _currentLevel = level;

                if (_levelSongs.ContainsKey(level) && _levelSongs[level] != null)
                {
                    // Плавная смена музыки
                    if (MediaPlayer.State == MediaState.Playing)
                    {
                        MediaPlayer.Stop();
                    }

                    MediaPlayer.Play(_levelSongs[level]);
                    System.Diagnostics.Debug.WriteLine($"🎵 Воспроизведение музыки уровня {level}");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения музыки: {ex.Message}");
            }
        }

        public void StopMusic()
        {
            try
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                }
            }
            catch { }
        }

        public void PauseMusic()
        {
            try
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                }
            }
            catch { }
        }

        public void ResumeMusic()
        {
            try
            {
                if (MediaPlayer.State == MediaState.Paused)
                {
                    MediaPlayer.Resume();
                }
            }
            catch { }
        }

        public void SetMusicEnabled(bool enabled)
        {
            _isMusicEnabled = enabled;
            if (!enabled)
            {
                StopMusic();
            }
        }

        public void SetVolume(float volume)
        {
            MediaPlayer.Volume = MathHelper.Clamp(volume, 0f, 1f);
        }

        // ДОБАВЛЕНО: Метод Dispose для очистки ресурсов
        public void Dispose()
        {
            try
            {
                StopMusic();
                _levelSongs?.Clear();
            }
            catch { }
        }
    }
}