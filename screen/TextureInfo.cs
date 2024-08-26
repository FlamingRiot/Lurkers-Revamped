using Raylib_cs;
using System.Numerics;

namespace uniray_Project
{
    public class TextureInfo : ScreenInfo
    {
        /// <summary>
        /// The texture of the info
        /// </summary>
        Texture2D texture;
        /// <summary>
        /// The texture of the info
        /// </summary>
        public Texture2D Texture { get { return texture; } set { texture = value; } }
        /// <summary>
        /// Texture info constructor
        /// </summary>
        /// <param name="info">Text of the info</param>
        /// <param name="position">Position of the info on the screen</param>
        /// <param name="cooldown">Cooldown of the info before disappearing</param>
        /// <param name="start">Start time of the info</param>
        /// <param name="texture">Texture of the info</param>
        public TextureInfo(Vector2 position, Texture2D texture, double start, double cooldown) : base(position, start, cooldown)
        {
            this.texture = texture;
        }
    }
}