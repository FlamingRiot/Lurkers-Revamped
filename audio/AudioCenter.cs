using Raylib_cs;
using static Raylib_cs.Raylib;
namespace uniray_Project
{
    /// <summary>Represents an audio managment instance</summary>
    public class AudioCenter
    {
        /// <summary>Dictionary of sounds</summary>
        private static Dictionary<string, Sound> sounds;

        /// <summary>Dictionary of musics</summary>
        private static Dictionary<string, Music> musics;

        /// <summary>Loads every sound and musics of the game</summary>
        public static void Init()
        {
            InitAudioDevice();
            sounds = RLoading.LoadSounds();
            musics = RLoading.LoadMusics();
        }

        /// <summary>Plays a sound</summary>
        /// <param name="key">Dictionary key of the sound</param>
        public static void PlaySound(string key)
        {
            Raylib.PlaySound(sounds[key]);
        }

        /// <summary>Plays a sound on loop</summary>
        /// <param name="key">Dictionary key of the sound</param>
        public static void PlaySoundLoop(string key)
        {
            if (!Raylib.IsSoundPlaying(sounds[key]))
            {
                Raylib.PlaySound(sounds[key]);
            }
        }

        /// <summary>Stops a playing sound</summary>
        /// <param name="key">Dictionary key of the sound</param>
        public static void StopSound(string key)
        {
            Raylib.StopSound(sounds[key]);
        }

        /// <summary>Plays a selected music</summary>
        /// <param name="key">Dictionary key of the music</param>
        public static void PlayMusic(string key)
        {
            PlayMusicStream(musics[key]);
        }

        /// <summary>Updates a selected music</summary>
        /// <param name="key">Dictionary key of the music</param>
        public static void UpdateMusic(string key)
        {
            UpdateMusicStream(musics[key]);
        }

        /// <summary>Checks if a music is already playing.</summary>
        /// <param name="key">Key of the music to check.</param>
        /// <returns><see langword="true"/> if the music is indeed playing. <see langword="false"/> otherwise.</returns>
        public static bool IsMusicPlaying(string key)
        {
            return IsMusicStreamPlaying(musics[key]);
        }

        /// <summary>Checks if a sound is already playing.</summary>
        /// <param name="key">Key of the sound to check.</param>
        /// <returns><see langword="true"/> if the sound is indeed playing. <see langword="false"/> otherwise.</returns>
        public static bool IsSoundPlaying(string key)
        {
            return Raylib.IsSoundPlaying(sounds[key]);
        }

        /// <summary>Sets music volume</summary>
        /// <param name="key">Dictionary key of the music</param>
        /// <param name="volume">Volume to set</param>
        public static void SetMusicVolume(string key, float volume)
        {
            Raylib.SetMusicVolume(musics[key], volume);
        }

        /// <summary>Sets music pitch</summary>
        /// <param name="key">Dictionary key of the music</param>
        /// <param name="volume">Pitch to set</param>
        public static void SetMusicPitch(string key, float pitch)
        {
            Raylib.SetMusicPitch(musics[key], pitch);
        }

        /// <summary>Sets music pitch</summary>
        /// <param name="key">Dictionary key of the music</param>
        /// <param name="volume">Pitch to set</param>
        public static void SetSoundPitch(string key, float pitch)
        {
            Raylib.SetSoundPitch(sounds[key], pitch);
        }


        /// <summary>Sets sound volume</summary>
        /// <param name="key">Dictionary key of the sound</param>
        /// <param name="volume">Volume to set</param>
        public static void SetSoundVolume(string key, float volume)
        {
            Raylib.SetSoundVolume(sounds[key], volume);
        }
    }
}