using Microsoft.Xna.Framework.Audio;
using Survive_the_night.Scripts.Managers;

namespace Survive_the_night.Gamedata.Config.Items
{
    public static class ItemSoundManager
    {
        // === ÌÅÒÎÄÛ ÄËß ÎÄÍÎĞÀÇÎÂÛÕ ÇÂÓÊÎÂ ===

        public static void PlayCoinSound(float volumeMultiplier = 1.0f)
        {
            SoundManager.Instance.PlaySound("take_coin", volumeMultiplier);
        }

        public static void PlayExperienceSound(float volumeMultiplier = 1.0f)
        {
            SoundManager.Instance.PlaySound("take_experience", volumeMultiplier);
        }

        public static void PlayHealingSound(float volumeMultiplier = 1.0f)
        {
            SoundManager.Instance.PlaySound("healing", volumeMultiplier);
        }

        public static void PlayDynamiteExplosionSound(float volumeMultiplier = 1.0f)
        {
            SoundManager.Instance.PlaySound("dynamite_explosion", volumeMultiplier);
        }

        // === ÌÅÒÎÄÛ ÄËß ÇÂÓÊÎÂ Ñ ÏĞÎÄÎËÆÈÒÅËÜÍÎÑÒÜŞ ===

        public static SoundEffectInstance PlayMagneticSoundLooping(float volumeMultiplier = 1.0f)
        {
            var instance = SoundManager.Instance.PlaySoundInstance("magnetic_sound", volumeMultiplier, true);

            // Äîáàâëÿåì â ãğóïïó äëÿ óïğàâëåíèÿ ïàóçîé
            if (instance != null)
            {
                SoundManager.Instance.AddToSoundGroup("item_sounds", instance);
            }

            return instance;
        }

        public static void StopMagneticSoundInstance(SoundEffectInstance instance)
        {
            if (instance != null && !instance.IsDisposed)
            {
                instance.Stop();
                instance.Dispose();

                // Óäàëÿåì èç ãğóïïû çâóêîâ
                SoundManager.Instance.RemoveFromSoundGroup("item_sounds", instance);
            }
        }

        // === ÌÅÒÎÄÛ ÄËß ÃĞÓÏÏÎÂÎÃÎ ÓÏĞÀÂËÅÍÈß ===

        public static void PauseAllItemSounds()
        {
            SoundManager.Instance.PauseSoundGroup("item_sounds");
        }

        public static void ResumeAllItemSounds()
        {
            SoundManager.Instance.ResumeSoundGroup("item_sounds");
        }

        public static void StopAllItemSounds()
        {
            SoundManager.Instance.StopSoundGroup("item_sounds");
        }
    }
}