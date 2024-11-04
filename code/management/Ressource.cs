using Raylib_cs;

namespace Lurkers_revamped
{
    public unsafe class Ressource
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
            // Change the current directory so the embedded materials from the models can be loaded successfully
            sbyte* dir = Raylib.GetApplicationDirectory();
            string workdir = new string(dir);
            string newDir = workdir + "src\\textures\\materials\\";
            Program.SetWorkdir(newDir);
            // Load
            Utilities = RLoading.LoadUtilities();
            Rigged = RLoading.LoadRigged();
            UITextures = RLoading.LoadUITextures();
            StaticBoxes = RLoading.LoadStaticBoxes();
            // Set back original app directory
            Program.SetWorkdir(workdir);
        }
    }
}
