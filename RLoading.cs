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
                {"cop", LoadModel("../../models/cop.m3d") }
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
                {"headshot", LoadTexture("../headshot.png") },
                {"plus_coin", LoadTexture("../plus_coin.png") },
                {"rifle_gray_splash", LoadTexture("../weapon_splashes/rifle_gray.png") },
                {"rifle_green_splash", LoadTexture("../weapon_splashes/rifle_green.png") },
                {"rifle_blue_splash", LoadTexture("../weapon_splashes/rifle_blue.png") },
                {"rifle_purple_splash", LoadTexture("../weapon_splashes/rifle_purple.png") },
                {"rifle_gold_splash", LoadTexture("../weapon_splashes/rifle_gold.png") },
                {"inventory_case", LoadTexture("../inventory_case.png") },
                {"crosshair", LoadTexture("../crosshair.png") },
            };
            // Return the dictionary
            return textures;
        }
    }
}