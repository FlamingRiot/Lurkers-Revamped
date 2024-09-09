using Raylib_cs;
using static Raylib_cs.Raylib;
namespace uniray_Project
{
    public class AudioCenter
    {
        /// <summary>
        /// Dictionary of sounds
        /// </summary>
        private Dictionary<string, Sound> sounds;
        /// <summary>
        /// Dictionary of musics 
        /// </summary>
        private Dictionary<string, Music> musics;
        /// <summary>
        /// Every currently playing sound
        /// </summary>
        private List<Sound> playingSounds;
        /// <summary>
        /// Every currently playing music
        /// </summary>
        private List<Music> playingMusics;
        /// <summary>
        /// Audio center constructor
        /// </summary>
        public AudioCenter()
        {
            InitAudioDevice();
            sounds = LoadSounds();
            musics = LoadMusics();  
            // Initialize playing audio lists
            playingSounds = new List<Sound>();
            playingMusics = new List<Music>();
        }
        /// <summary>
        /// Load all the sounds of the game
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Sound> LoadSounds()
        {
            Dictionary<string, Sound> sounds = new()
            {
                { "rifleShoot", LoadSound("src/sounds/rifle/shoot.wav") },
                { "headshot", LoadSound("src/sounds/rifle/headshot.wav") },
                { "headshot_voice", LoadSound("src/sounds/headshot_voice.wav") }
            };
            return sounds;
        }
        /// <summary>
        /// Load all the musics of the game
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Music> LoadMusics()
        {
            Dictionary<string, Music> musics = new() 
            {
                {"reapers", LoadMusicStream("src/sounds/musics/reapers.mp3") }
            };
            return musics;
        }
        /// <summary>
        /// Play a selected sound
        /// </summary>
        /// <param name="key">Dictionary key of the sound</param>
        public void PlaySound(string key)
        {
            Raylib.PlaySound(sounds[key]);
        }
        /// <summary>
        /// Play a selected music
        /// </summary>
        /// <param name="key">Dictionary key of the music</param>
        public void PlayMusic(string key)
        {
            Raylib.PlayMusicStream(musics[key]);
        }
        /// <summary>
        /// UPdate a selected music
        /// </summary>
        /// <param name="key">Dictionary key of the music</param>
        public void UpdateMusic(string key)
        {
            Raylib.UpdateMusicStream(musics[key]);
        }
    }
}
