using Raylib_cs;
using static Raylib_cs.Raylib;
namespace uniray_Project
{
    public class RLoading
    {
        public RLoading() 
        {
            TraceLog(TraceLogLevel.Info, "New RLoading instance launched");
        }
        /// <summary>
        /// Load rigged models
        /// </summary>
        /// <returns>The dictionary of rigged models</returns>
        public Dictionary<string, Model> LoadRigged()
        {
            Dictionary<string, Model> rigged = new Dictionary<string, Model>()
            {
                {"cop", LoadModel("../../models/cop.m3d") },
                {"officer", LoadModel("../../models/officer.m3d") },
                {"blonde", LoadModel("../../models/blonde.m3d") },
                {"yaku", LoadModel("../../models/Yaku.m3d") },
                {"torso", LoadModel("../../models/torso.m3d") }
            };
            // Return the dictionary
            return rigged;
        }

        /// <summary>
        /// Load UI textures (screen textures)
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Texture2D> LoadUITextures()
        {
            Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>()
            {
                {"headshot", LoadTexture("../headshot.png") }
            };
            // Return the dictionary
            return textures;
        }
    }
}