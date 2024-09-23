using Raylib_cs;
using static Raylib_cs.Raylib;
namespace uniray_Project
{
    /// <summary>Represents an audio managment instance</summary>
    public class AudioCenter
    {
        /// <summary>Dictionary of sounds</summary>
        private Dictionary<string, Sound> sounds;

        /// <summary>Dictionary of musics</summary>
        private Dictionary<string, Music> musics;

        /// <summary>AudioCenter constructor</summary>
        public AudioCenter()
        {
            InitAudioDevice();
            sounds = RLoading.LoadSounds();
            musics = RLoading.LoadMusics();  
        }

        /// <summary>Plays a sound</summary>
        /// <param name="key">Dictionary key of the sound</param>
        public void PlaySound(string key)
        {
            Raylib.PlaySound(sounds[key]);
        }

        /// <summary>Plays a sound on loop</summary>
        /// <param name="key">Dictionary key of the sound</param>
        public void PlaySoundLoop(string key)
        {
            if (!IsSoundPlaying(sounds[key]))
            {
                Raylib.PlaySound(sounds[key]);
            }
        }

        /// <summary>Stops a playing sound</summary>
        /// <param name="key">Dictionary key of the sound</param>
        public void StopSound(string key)
        {
            Raylib.StopSound(sounds[key]);
        }

        /// <summary>Plays a selected music</summary>
        /// <param name="key">Dictionary key of the music</param>
        public void PlayMusic(string key)
        {
            PlayMusicStream(musics[key]);
        }

        /// <summary>Updates a selected music</summary>
        /// <param name="key">Dictionary key of the music</param>
        public void UpdateMusic(string key)
        {
            UpdateMusicStream(musics[key]);
        }

        /// <summary>Sets music volume</summary>
        /// <param name="key">Dictionary key of the music</param>
        /// <param name="volume">Volume to set</param>
        public void SetMusicVolume(string key, float volume)
        {
            Raylib.SetMusicVolume(musics[key], volume);
        }
    }
}