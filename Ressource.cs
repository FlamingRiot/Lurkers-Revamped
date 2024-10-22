using Raylib_cs;

namespace Lurkers_revamped
{
    public class Ressource
    {
        public Dictionary<string, Model> Utilities;
        public Dictionary<string, Model> Rigged;
        public Dictionary<string, Texture2D> UITextures;
        public List<BoundingBox> StaticBoxes;
        public Ressource() 
        {
            Utilities = new Dictionary<string, Model>();
            Rigged = new Dictionary<string, Model>();
            UITextures = new Dictionary<string, Texture2D>();
            StaticBoxes = new List<BoundingBox>();
        }

        public void Load()
        {
            Utilities = RLoading.LoadUtilities();
            Rigged = RLoading.LoadRigged();
            UITextures = RLoading.LoadUITextures();
            StaticBoxes = RLoading.LoadStaticBoxes();
        }
    }
}
