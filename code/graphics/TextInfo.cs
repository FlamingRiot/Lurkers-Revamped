using Raylib_cs;
using System.Numerics;

namespace uniray_Project
{
    public class TextInfo : ScreenInfo
    {
        /// <summary>
        /// Text of the info
        /// </summary>
        private string text;
        /// <summary>
        /// Used font for rendering the info
        /// </summary>
        private Font font;
        /// <summary>
        /// Text of the info
        /// </summary>
        public string Text { get { return text; } set { text = value; } }
        /// <summary>
        /// Used font for rendering the info
        /// </summary>
        public Font Font { get { return font; } set { font = value; } }
        /// <summary>
        /// Texture info constructor
        /// </summary>
        /// <param name="info">Text of the info</param>
        /// <param name="position">Position of the info on the screen</param>
        /// <param name="cooldown">Cooldown of the info before disappearing</param>
        /// <param name="start">Start time of the info</param>
        /// <param name="texture">Texture of the info</param>
        public TextInfo(Vector2 position, string text, Font font, double start, double cooldown) : base(position, start, cooldown)
        {
            this.text = text;
            this.font = font;
        }
    }
}