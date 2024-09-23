using Raylib_cs;
using static Raylib_cs.Raylib;
namespace uniray_Project
{
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
            sounds = LoadSounds();
            musics = LoadMusics();  
        }

        /// <summary>Loads all the sounds of the game</summary>
        /// <returns>The list of loaded sounds</returns>
        private Dictionary<string, Sound> LoadSounds()
        {
            Dictionary<string, Sound> sounds = new()
            {
                { "rifleShoot", LoadSound("src/sounds/rifle/shoot.wav") },
                { "headshot", LoadSound("src/sounds/rifle/headshot.wav") },
                { "headshot_voice", LoadSound("src/sounds/headshot_voice.wav") },
                { "zombie_default", LoadSound("src/sounds/zombie/zombie_default.wav") },
                { "zombie_kill", LoadSound("src/sounds/zombie/zombie_kill.wav") },
                { "zombie_herd", LoadSound("src/sounds/zombie/zombie_herd.wav") },
                { "zombie_eating", LoadSound("src/sounds/zombie/zombie_eating.wav") },
            };
            return sounds;
        }

        /// <summary>Loads every music of the game</summary>
        /// <returns>The list of loaded musics</returns>
        private Dictionary<string, Music> LoadMusics()
        {
            Dictionary<string, Music> musics = new() 
            {
                {"reapers", LoadMusicStream("src/sounds/musics/reapers.mp3") }
            };
            return musics;
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
    }
}