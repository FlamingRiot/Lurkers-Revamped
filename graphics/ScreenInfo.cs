using System.Numerics;
namespace uniray_Project
{
    public class ScreenInfo
    {
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
        public ScreenInfo(Vector2 position, double start, double cooldown)
        {
            this.position = position;
            this.start = start;
            this.cooldown = cooldown;
        }
    }
}