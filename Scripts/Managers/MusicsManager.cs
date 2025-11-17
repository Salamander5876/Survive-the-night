using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Survive_the_night.Scripts.Interfaces;
using System.Collections.Generic;

namespace Survive_the_night.Scripts.Managers
{
    public class MusicsManager
    {
        private Dictionary<int, Song> _levelSongs;
        private Dictionary<int, Song> _survivalCycleSongs;
        private Song _mainMenuMusic;
        private int _currentLevel = 1;
        private int _currentCycle = 1;
        private bool _isMusicEnabled = true;
        private GameState _currentMusicState = GameState.MainMenu;
        private StartMenu.GameMode _currentGameMode = StartMenu.GameMode.Easy;

        public MusicsManager()
        {
            _levelSongs = new Dictionary<int, Song>();
            _survivalCycleSongs = new Dictionary<int, Song>();
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
                    System.Diagnostics.Debug.WriteLine($"Загружена музыка уровня {i}: {songPath}");
                }

                // Загружаем музыку для циклов Survival
                for (int i = 1; i <= 4; i++)
                {
                    string songPath = $"Musics/SurvivalCycle{i}";
                    try
                    {
                        _survivalCycleSongs[i] = content.Load<Song>(songPath);
                        System.Diagnostics.Debug.WriteLine($"Загружена музыка цикла Survival {i}: {songPath}");
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ОШИБКА загрузки музыки цикла {i} ({songPath}): {ex.Message}");
                        _survivalCycleSongs[i] = null;
                    }
                }

                // Проверяем, что все файлы загружены
                CheckLoadedSongs();

                // Настраиваем медиаплеер
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 0.75f;

                PlayMenuMusic();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки музыки: {ex.Message}");
            }
        }

        // Новый метод для проверки загруженных песен
        private void CheckLoadedSongs()
        {
            System.Diagnostics.Debug.WriteLine("=== ПРОВЕРКА ЗАГРУЖЕННОЙ МУЗЫКИ ===");

            // Проверяем музыку циклов Survival
            for (int i = 1; i <= 4; i++)
            {
                if (_survivalCycleSongs.ContainsKey(i) && _survivalCycleSongs[i] != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✓ Survival цикл {i}: ЗАГРУЖЕН");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Survival цикл {i}: НЕ ЗАГРУЖЕН");
                }
            }

            System.Diagnostics.Debug.WriteLine("=== КОНЕЦ ПРОВЕРКИ ===");
        }

        public void PlayMenuMusic()
        {
            if (!_isMusicEnabled) return;

            try
            {
                if (_mainMenuMusic != null && MediaPlayer.State != MediaState.Playing)
                {
                    MediaPlayer.Play(_mainMenuMusic);
                    _currentMusicState = GameState.MainMenu;
                    System.Diagnostics.Debug.WriteLine("Воспроизведение музыки главного меню");
                }
                else if (_mainMenuMusic != null && _currentMusicState != GameState.MainMenu)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_mainMenuMusic);
                    _currentMusicState = GameState.MainMenu;
                    System.Diagnostics.Debug.WriteLine("Переключение на музыку главного меню");
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

            // В режиме Survival используем музыку циклов вместо музыки уровней
            if (_currentGameMode == StartMenu.GameMode.Survival)
            {
                PlaySurvivalCycleMusic(_currentCycle);
                return;
            }

            try
            {
                if (_currentLevel == level && MediaPlayer.State == MediaState.Playing)
                    return;

                _currentLevel = level;

                if (_levelSongs.ContainsKey(level) && _levelSongs[level] != null)
                {
                    if (MediaPlayer.State == MediaState.Playing)
                    {
                        MediaPlayer.Stop();
                    }

                    MediaPlayer.Play(_levelSongs[level]);
                    System.Diagnostics.Debug.WriteLine($"Воспроизведение музыки уровня {level}");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения музыки: {ex.Message}");
            }
        }

        // Новый метод для воспроизведения музыки циклов Survival
        public void PlaySurvivalCycleMusic(int cycle)
        {
            if (!_isMusicEnabled) return;

            try
            {
                // Всегда останавливаем текущую музыку и запускаем новую
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                }

                if (_survivalCycleSongs.ContainsKey(cycle) && _survivalCycleSongs[cycle] != null)
                {
                    MediaPlayer.Play(_survivalCycleSongs[cycle]);
                    System.Diagnostics.Debug.WriteLine($"Воспроизведение музыки цикла Survival: {cycle}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ОШИБКА: Музыка для цикла {cycle} не найдена!");
                    // Пробуем воспроизвести цикл 1 как запасной вариант
                    if (cycle != 1 && _survivalCycleSongs.ContainsKey(1))
                    {
                        MediaPlayer.Play(_survivalCycleSongs[1]);
                        System.Diagnostics.Debug.WriteLine($"Воспроизведение запасной музыки (цикл 1)");
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения музыки цикла {cycle}: {ex.Message}");
            }
        }

        // Новый метод для установки текущего режима игры
        public void SetGameMode(StartMenu.GameMode gameMode)
        {
            _currentGameMode = gameMode;
            System.Diagnostics.Debug.WriteLine($"MusicsManager: установлен режим {gameMode}, текущий цикл: {_currentCycle}");

            // Если устанавливаем Survival режим, сразу запускаем музыку текущего цикла
            if (gameMode == StartMenu.GameMode.Survival)
            {
                PlaySurvivalCycleMusic(_currentCycle);
            }
        }

        // Новый метод для установки текущего цикла (для Survival режима)
        public void SetCurrentCycle(int cycle)
        {
            _currentCycle = cycle;

            // ВСЕГДА воспроизводим музыку цикла при установке нового цикла
            if (_currentGameMode == StartMenu.GameMode.Survival)
            {
                PlaySurvivalCycleMusic(cycle);
            }
            else
            {
                // Если не Survival режим, но вызвали SetCurrentCycle - все равно пытаемся воспроизвести
                PlaySurvivalCycleMusic(cycle);
            }
        }

        public void StopMusicForGameStart()
        {
            try
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                    _currentMusicState = GameState.Playing;
                    System.Diagnostics.Debug.WriteLine("Музыка остановлена для начала игры");
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
                PlayMenuMusic();
            }
        }

        public void SetVolume(float volume)
        {
            MediaPlayer.Volume = MathHelper.Clamp(volume, 0f, 1f);
        }

        public void Dispose()
        {
            try
            {
                StopMusic();
                _levelSongs?.Clear();
                _survivalCycleSongs?.Clear();
            }
            catch { }
        }

        // Метод для принудительного переключения музыки (игнорирует проверку на текущий цикл)
        public void ForcePlaySurvivalCycleMusic(int cycle)
        {
            if (!_isMusicEnabled) return;

            try
            {
                // Всегда останавливаем текущую музыку
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                    System.Diagnostics.Debug.WriteLine($"Музыка остановлена для принудительного переключения на цикл {cycle}");
                }

                // Даем время на остановку
                System.Threading.Thread.Sleep(50);

                if (_survivalCycleSongs.ContainsKey(cycle) && _survivalCycleSongs[cycle] != null)
                {
                    MediaPlayer.Play(_survivalCycleSongs[cycle]);
                    _currentCycle = cycle;
                    _currentMusicState = GameState.Playing; // Важно: устанавливаем состояние игры
                    System.Diagnostics.Debug.WriteLine($"ПРИНУДИТЕЛЬНОЕ воспроизведение музыки цикла Survival: {cycle}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ОШИБКА: Музыка для цикла {cycle} не найдена для принудительного воспроизведения!");
                    // Пробуем воспроизвести цикл 1 как запасной вариант
                    if (_survivalCycleSongs.ContainsKey(1) && _survivalCycleSongs[1] != null)
                    {
                        MediaPlayer.Play(_survivalCycleSongs[1]);
                        System.Diagnostics.Debug.WriteLine($"Воспроизведение запасной музыки (цикл 1)");
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка принудительного воспроизведения музыки цикла {cycle}: {ex.Message}");
            }
        }
    }
}