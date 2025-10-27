using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Survive_the_night.Managers
{
    public class MusicsManager
    {
        private Dictionary<int, Song> _levelSongs;
        private Song _mainMenuMusic; // Добавляем музыку для меню
        private int _currentLevel = 1;
        private bool _isMusicEnabled = true;
        private GameState _currentMusicState = GameState.MainMenu; // Отслеживаем состояние для музыки

        public MusicsManager()
        {
            _levelSongs = new Dictionary<int, Song>();
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                // Загружаем музыку для главного меню
                _mainMenuMusic = content.Load<Song>("Musics/MainMusic");

                // Загружаем музыку для всех 8 уровней
                for (int i = 1; i <= 8; i++)
                {
                    string songPath = $"Musics/HardLevel{i}";
                    _levelSongs[i] = content.Load<Song>(songPath);
                }

                // Настраиваем медиаплеер
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.75f; // 75% громкость

                // Сразу запускаем музыку меню
                PlayMenuMusic();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки музыки: {ex.Message}");
                // Продолжаем без музыки, если есть ошибки
            }
        }

        // Новый метод для воспроизведения музыки меню
        public void PlayMenuMusic()
        {
            if (!_isMusicEnabled) return;

            try
            {
                if (_mainMenuMusic != null && MediaPlayer.State != MediaState.Playing)
                {
                    MediaPlayer.Play(_mainMenuMusic);
                    _currentMusicState = GameState.MainMenu;
                    System.Diagnostics.Debug.WriteLine("🎵 Воспроизведение музыки главного меню");
                }
                else if (_mainMenuMusic != null && _currentMusicState != GameState.MainMenu)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_mainMenuMusic);
                    _currentMusicState = GameState.MainMenu;
                    System.Diagnostics.Debug.WriteLine("🎵 Переключение на музыку главного меню");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения музыки меню: {ex.Message}");
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

        // Новый метод для остановки музыки при завершении загрузки
        public void StopMusicForGameStart()
        {
            try
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                    _currentMusicState = GameState.Playing; // Устанавливаем состояние игры
                    System.Diagnostics.Debug.WriteLine("🎵 Музыка остановлена для начала игры");
                }
            }
            catch { }
        }
        public void StopMusic()
        {
            try
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                    _currentMusicState = GameState.MainMenu;
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
            else if (enabled && MediaPlayer.State != MediaState.Playing)
            {
                // Если музыка включена и не играет, запускаем музыку меню
                PlayMenuMusic();
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