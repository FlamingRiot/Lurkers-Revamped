using Raylib_cs;
using Uniray_Engine;
using static Uniray_Engine.UnirayEngine;
using static Raylib_cs.Raylib;
using System.Numerics;
using uniray_Project.mechanics;

namespace uniray_Project
{
    /// <summary>Loading instance used to centralize all the GPU loading actions provided by Raylib</summary>
    public static unsafe class RLoading
    {
        /// <summary>Loads the rigged (animation-ready) complex character models</summary>
        /// <returns>The dictionary of rigged models</returns>
        public static Dictionary<string, Model> LoadRigged()
        {
            Dictionary<string, Model> rigged = new Dictionary<string, Model>()
            {
                {"cop", LoadModel("../../models/cop.m3d") },
                {"cop2", LoadModel("../../models/cop.m3d") },
                {"cop3", LoadModel("../../models/cop.m3d") },
                {"cop4", LoadModel("../../models/cop.m3d") },
            };
            // Return the dictionary
            return rigged;
        }

        /// <summary>Loads the textures used for 2D rendering on the screen</summary>
        /// <returns>The list of loaded textures</returns>
        public static Dictionary<string, Texture2D> LoadUITextures()
        {
            Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>()
            {
                {"rifle_gray_splash", LoadTexture("../weapon_splashes/rifle_gray.png") },
                {"rifle_green_splash", LoadTexture("../weapon_splashes/rifle_green.png") },
                {"rifle_blue_splash", LoadTexture("../weapon_splashes/rifle_blue.png") },
                {"rifle_purple_splash", LoadTexture("../weapon_splashes/rifle_purple.png") },
                {"rifle_gold_splash", LoadTexture("../weapon_splashes/rifle_gold.png") },
                {"inventory_case", LoadTexture("../inventory_case.png") },
                {"crosshair", LoadTexture("../crosshair.png") },
                {"inventory_case_selected", LoadTexture("../inventory_case_selected.png") },
                {"lifebar", LoadTexture("../lifebar.png") }
            };
            // Return the dictionary
            return textures;
        }

        /// <summary>Loads the utilities, which consist of various game objects that do not include animations</summary>
        /// <returns>The dictionary containing all the loaded models</returns>
        public static Dictionary<string, Model> LoadUtilities()
        {
            // Initialize the models dictionary 
            Dictionary<string, Model> models = new Dictionary<string, Model>();

            // Load Rifle Model
            Model rifle = LoadModel("../../models/rifle.m3d");

            models.Add("rifle", rifle);

            // Load Rifle Model
            Model rifleSplash = LoadModel("../../models/rifle_splash.m3d");

            models.Add("rifle_splash", rifleSplash);

            return models;
        }

        /// <summary>Loads animation list from .m3d file</summary>
        /// <param name="animation">Relative path to the file containg the animations</param>
        /// <returns>The list of the animations contained in the file</returns>
        public static List<ModelAnimation> LoadAnimationList(string animation)
        {
            // Load animations into the RAM from the m3d file
            int animCount = 0;
            ModelAnimation* animations = LoadModelAnimations(animation, ref animCount);
            // Create and fill animation list 
            List<ModelAnimation> list = new List<ModelAnimation>();
            for (int i = 0; i < animCount; i++)
            {
                list.Add(animations[i]);
            }
            return list;
        }

        /// <summary>Loads all the sounds of the game</summary>
        /// <returns>The list of loaded sounds</returns>
        public static Dictionary<string, Sound> LoadSounds()
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
                { "radio", LoadSound("src/sounds/radio.wav") },
                { "rick", LoadSound("src/sounds/rick.wav") },
                { "daryl", LoadSound("src/sounds/daryl.wav") },
            };
            return sounds;
        }

        /// <summary>Loads every music of the game</summary>
        /// <returns>The list of loaded musics</returns>
        public static Dictionary<string, Music> LoadMusics()
        {
            Dictionary<string, Music> musics = new()
            {
                {"ambience", LoadMusicStream("src/sounds/musics/ambience.mp3") }, 
                {"lerob", LoadMusicStream("src/sounds/musics/lerob.mp3") } 
            };
            return musics;
        }

        /// <summary>Loads the list of static boxes from the current scene's game objects</summary>
        /// <param name="gos">The game objects to use to load the static bounding-boxes</param>
        /// <returns>The list of bounding boxes corresponding to the passed game objects</returns>
        public static List<BoundingBox> LoadStaticBoxes(List<GameObject3D> gos)
        {
            List<BoundingBox> boxes = new List<BoundingBox>();
            
            for (int i = 0; i < gos.Count; i++)
            {
                // Load the box only for UModels
                if (gos[i] is UModel)
                {
                    // Load model from the mesh
                    Model model = LoadModelFromMesh(((UModel)gos[i]).Mesh);
                    // Load the box from the model
                    BoundingBox box = GetModelBoundingBox(model);

                    // Rotate for static buildings
                    if (((UModel)gos[i]).Yaw % 180 != 0)
                    {
                        Vector3 min = box.Min;
                        box.Min = new Vector3(box.Min.Z, box.Min.Y, -box.Max.X);
                        box.Max = new Vector3(box.Max.Z, box.Max.Y, -min.X);
                    }

                    // Add the go's position
                    box.Min += gos[i].Position;
                    box.Max += gos[i].Position;

                    boxes.Add(box);
                }
            }
            // Return all the models
            return boxes;
        }

        /// <summary>Loads spawners positon from game data</summary>
        /// <returns>List of spawners</returns>
        public static List<Spawner> LoadSpawners()
        {
            List<Spawner> spawners = new List<Spawner>();
            foreach (UModel go in CurrentScene.GameObjects.Where(x => x is UModel).Where(x => ((UModel)x).ModelID == "crystal")){
                spawners.Add(new Spawner(go.Position, go.Transform, 5, 7, CurrentScene.GameObjects.IndexOf(go)));
            }
            return spawners;
        }

        /// <summary>Generates the base terrain for the game</summary>
        /// <returns>The generated terrain with the material, mesh and transform already set and loaded</returns>
        public static Terrain GenTerrain()
        {
            Mesh mesh = GenMeshPlane(1, 1, 1, 1);
            Material mat = LoadMaterialDefault();
            SetMaterialTexture(ref mat, MaterialMapIndex.Diffuse, LoadTexture("src/textures/terrain.png"));
            Matrix4x4 transform = Raymath.MatrixScale(250, 250, 250);

            return new Terrain(mesh, mat, transform);
        }

        /// <summary>Generates the general skybox for the game</summary>
        /// <param name="shaders">Shader center to use for loading shader interactions</param>
        /// <returns>The generated mesh corresponding to the skybox. Background texturing and loading is done as well</returns>
        public static Mesh GenSkybox(ShaderCenter shaders)
        {
            Mesh skybox = GenMeshCube(1, 1, 1);
            Texture2D panorama = LoadTexture("src/textures/skyboxes/sunset_skybox.hdr");
            Texture2D cubemap = shaders.GenTexureCubemap(panorama, 256, PixelFormat.UncompressedR8G8B8A8);
            shaders.SetCubemap(cubemap);
            UnloadTexture(panorama);

            return skybox;
        }
    }
}