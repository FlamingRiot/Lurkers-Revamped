using System.Numerics;
namespace uniray_Project
{
    public class ScreenInfo
    {
        /// <summary>
        /// Text of the info
        /// </summary>
        private string info;
        /// <summary>
        /// Position of the info on the screen
        /// </summary>
        private Vector2 position;
        /// <summary>
        /// Start time of the info
        /// </summary>
        private double start;
        /// <summary>
        /// Cooldown time of the info before disappearing
        /// </summary>
        private double cooldown;
        /// <summary>
        /// Text of the info
        /// </summary>
        public string Info { get { return info; } set { info = value; } }
        /// <summary>
        /// Position of the info the screen
        /// </summary>
        public Vector2 Position { get { return position; } set { position = value; } }
        /// <summary>
        /// X position of the screen 
        /// </summary>
        public float X { get { return position.X; } set { position.X = value; } }
        /// <summary>
        /// Y position of the screen
        /// </summary>
        public float Y { get { return position.Y; } set { position.Y = value; } }
        /// <summary>
        /// Cooldown of the info before
        /// </summary>
        public double Cooldown { get { return cooldown; } set { cooldown = value; } }
        /// <summary>
        /// Start time of the info
        /// </summary>
        public double Start { get { return start; } set { start = value; } }
        /// <summary>
        /// ScreenInfo constructor
        /// </summary>
        /// <param name="info">Text of the info</param>
        /// <param name="position">Position of the info on the screen</param>
        /// <param name="cooldown">Cooldown of the info before disappearing</param>
        /// <param name="start">Start time of the info</param>
        public ScreenInfo(string info, Vector2 position, double start, double cooldown)
        {
            this.info = info;
            this.position = position;
            this.start = start;
            this.cooldown = cooldown;
        }
    }
}