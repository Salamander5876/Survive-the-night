using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;

namespace Survive_the_night.Scripts.Managers
{
    public class SoundManager
    {
        private static SoundManager _instance;
        public static SoundManager Instance => _instance ??= new SoundManager();

        // Словари для хранения звуков
        private Dictionary<string, SoundEffect> _soundEffects;
        private Dictionary<string, SoundEffectInstance> _loopingSounds;

        // Глобальные настройки звука
        public float Volume { get; private set; } = 1.0f;
        public bool IsMuted { get; private set; } = false;

        // Группы звуков для категориального управления
        private Dictionary<string, List<SoundEffectInstance>> _soundGroups;

        private SoundManager()
        {
            _soundEffects = new Dictionary<string, SoundEffect>();
            _loopingSounds = new Dictionary<string, SoundEffectInstance>();
            _soundGroups = new Dictionary<string, List<SoundEffectInstance>>();
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                // Загрузка звуковых эффектов
                LoadSoundEffect(content, "Sounds/Weapons/SFXCardDeal", "card_deal");
                LoadSoundEffect(content, "Sounds/Weapons/SFXGunShooting", "gun_shoot");
                LoadSoundEffect(content, "Sounds/Weapons/SFCCasinoChips", "casino_chips");
                LoadSoundEffect(content, "Sounds/Weapons/SFXGoldenSword", "golden_sword");
                LoadSoundEffect(content, "Sounds/Weapons/SFXThrowMolotov", "molotov_throw");
                LoadSoundEffect(content, "Sounds/Weapons/SFXFireBurn", "fire_burn"); // Добавлен звук горения
                LoadSoundEffect(content, "Sounds/Weapons/SFXBigLaser", "big_laser");
                LoadSoundEffect(content, "Sounds/Weapons/SFXDiceDamage", "dice_damage");
                LoadSoundEffect(content, "Sounds/Weapons/SFXThrowBeer", "beer_throw");
                LoadSoundEffect(content, "Sounds/Weapons/SFXPuddleBeer", "beer_puddle");
                LoadSoundEffect(content, "Sounds/Weapons/SFXBombThrow", "bomb_throw");
                LoadSoundEffect(content, "Sounds/Weapons/SFXBombExplosion", "bomb_explosion");
                LoadSoundEffect(content, "Sounds/Weapons/SFXBreakerBlade", "breaker_swing");
                LoadSoundEffect(content, "Sounds/Weapons/SFXRouletteDamage", "roulette_damage");
                LoadSoundEffect(content, "Sounds/Weapons/SFXGoldenTyphoon", "golden_typhoon");
                LoadSoundEffect(content, "Sounds/Weapons/SFXEventHorizonStar", "event_horizon");

                // Звуки предметов
                LoadSoundEffect(content, "Sounds/Items/SFXDynamiteExplosion", "dynamite_explosion");

                System.Diagnostics.Debug.WriteLine("SoundManager: Все звуки загружены");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки звуков: {ex.Message}");
            }
        }

        private void LoadSoundEffect(ContentManager content, string path, string key)
        {
            try
            {
                _soundEffects[key] = content.Load<SoundEffect>(path);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки звука {key}: {ex.Message}");
            }
        }

        // Основные методы воспроизведения
        public void PlaySound(string soundKey, float volumeMultiplier = 1.0f, float pitch = 0.0f, float pan = 0.0f)
        {
            if (IsMuted || !_soundEffects.ContainsKey(soundKey)) return;

            try
            {
                float finalVolume = Volume * volumeMultiplier;
                _soundEffects[soundKey].Play(finalVolume, pitch, pan);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения звука {soundKey}: {ex.Message}");
            }
        }

        // Воспроизведение звука с созданием экземпляра (для контроля)
        public SoundEffectInstance PlaySoundInstance(string soundKey, float volumeMultiplier = 1.0f, bool isLooped = false)
        {
            if (IsMuted || !_soundEffects.ContainsKey(soundKey)) return null;

            try
            {
                var instance = _soundEffects[soundKey].CreateInstance();
                instance.Volume = Volume * volumeMultiplier;
                instance.IsLooped = isLooped;
                instance.Play();

                return instance;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания экземпляра звука {soundKey}: {ex.Message}");
                return null;
            }
        }

        // Управление зацикленными звуками
        public void PlayLoopingSound(string soundKey, string instanceKey, float volumeMultiplier = 1.0f)
        {
            if (_loopingSounds.ContainsKey(instanceKey))
            {
                StopLoopingSound(instanceKey);
            }

            var instance = PlaySoundInstance(soundKey, volumeMultiplier, true);
            if (instance != null)
            {
                _loopingSounds[instanceKey] = instance;
            }
        }

        public void StopLoopingSound(string instanceKey)
        {
            if (_loopingSounds.ContainsKey(instanceKey))
            {
                _loopingSounds[instanceKey].Stop();
                _loopingSounds[instanceKey].Dispose();
                _loopingSounds.Remove(instanceKey);
            }
        }

        // Групповое управление звуками
        public void AddToSoundGroup(string groupName, SoundEffectInstance instance)
        {
            if (instance == null) return;

            if (!_soundGroups.ContainsKey(groupName))
            {
                _soundGroups[groupName] = new List<SoundEffectInstance>();
            }

            if (!_soundGroups[groupName].Contains(instance))
            {
                _soundGroups[groupName].Add(instance);
            }
        }

        public void RemoveFromSoundGroup(string groupName, SoundEffectInstance instance)
        {
            if (_soundGroups.ContainsKey(groupName) && instance != null)
            {
                _soundGroups[groupName].Remove(instance);
            }
        }

        public void PauseSoundGroup(string groupName)
        {
            if (_soundGroups.ContainsKey(groupName))
            {
                foreach (var sound in _soundGroups[groupName])
                {
                    if (sound != null && sound.State == SoundState.Playing)
                    {
                        sound.Pause();
                    }
                }
            }
        }

        public void ResumeSoundGroup(string groupName)
        {
            if (_soundGroups.ContainsKey(groupName))
            {
                foreach (var sound in _soundGroups[groupName])
                {
                    if (sound != null && sound.State == SoundState.Paused)
                    {
                        sound.Resume();
                    }
                }
            }
        }

        public void StopSoundGroup(string groupName)
        {
            if (_soundGroups.ContainsKey(groupName))
            {
                foreach (var sound in _soundGroups[groupName])
                {
                    if (sound != null)
                    {
                        sound.Stop();
                    }
                }
                _soundGroups[groupName].Clear();
            }
        }

        // Проверка существования звука
        public bool ContainsSound(string soundKey)
        {
            return _soundEffects.ContainsKey(soundKey);
        }

        // Метод для паузы всех игровых звуков (оружие, огонь и т.д.)
        public void PauseAllGameSounds()
        {
            PauseSoundGroup("weapon_sounds");
            PauseSoundGroup("laser_sounds");
            PauseSoundGroup("fire_sounds");
            PauseSoundGroup("environment_sounds");
        }

        // Метод для возобновления всех игровых звуков
        public void ResumeAllGameSounds()
        {
            ResumeSoundGroup("weapon_sounds");
            ResumeSoundGroup("laser_sounds");
            ResumeSoundGroup("fire_sounds");
            ResumeSoundGroup("environment_sounds");
        }

        // Метод для остановки всех игровых звуков
        public void StopAllGameSounds()
        {
            StopSoundGroup("weapon_sounds");
            StopSoundGroup("laser_sounds");
            StopSoundGroup("fire_sounds");
            StopSoundGroup("environment_sounds");
        }

        // Глобальное управление звуком
        public void SetVolume(float volume)
        {
            Volume = MathHelper.Clamp(volume, 0.0f, 1.0f);

            // Обновляем громкость всех зацикленных звуков
            foreach (var sound in _loopingSounds.Values)
            {
                if (sound != null && !sound.IsDisposed)
                {
                    sound.Volume = Volume;
                }
            }

            // Обновляем громкость звуков в группах
            foreach (var group in _soundGroups.Values)
            {
                foreach (var sound in group)
                {
                    if (sound != null && !sound.IsDisposed)
                    {
                        sound.Volume = Volume;
                    }
                }
            }
        }

        public void Mute()
        {
            IsMuted = true;
            StopAllSounds();
        }

        public void Unmute()
        {
            IsMuted = false;
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            if (IsMuted)
            {
                StopAllSounds();
            }
        }

        // Остановка всех звуков
        public void StopAllSounds()
        {
            // Останавливаем зацикленные звуки
            foreach (var sound in _loopingSounds.Values)
            {
                if (sound != null)
                {
                    sound.Stop();
                }
            }
            _loopingSounds.Clear();

            // Останавливаем звуки в группах
            foreach (var group in _soundGroups.Values)
            {
                foreach (var sound in group)
                {
                    if (sound != null)
                    {
                        sound.Stop();
                    }
                }
                group.Clear();
            }
        }

        // Пауза всех звуков (для меню паузы)
        public void PauseAllSounds()
        {
            foreach (var sound in _loopingSounds.Values)
            {
                if (sound != null && sound.State == SoundState.Playing)
                {
                    sound.Pause();
                }
            }

            foreach (var group in _soundGroups.Values)
            {
                foreach (var sound in group)
                {
                    if (sound != null && sound.State == SoundState.Playing)
                    {
                        sound.Pause();
                    }
                }
            }
        }

        // Возобновление всех звуков
        public void ResumeAllSounds()
        {
            foreach (var sound in _loopingSounds.Values)
            {
                if (sound != null && sound.State == SoundState.Paused)
                {
                    sound.Resume();
                }
            }

            foreach (var group in _soundGroups.Values)
            {
                foreach (var sound in group)
                {
                    if (sound != null && sound.State == SoundState.Paused)
                    {
                        sound.Resume();
                    }
                }
            }
        }

        // Очистка ресурсов
        public void Dispose()
        {
            StopAllSounds();

            foreach (var sound in _soundEffects.Values)
            {
                sound?.Dispose();
            }
            _soundEffects.Clear();

            _soundGroups.Clear();
        }
    }
}