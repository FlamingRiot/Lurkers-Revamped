using Raylib_cs;
using UnirayEngine;
using static Raylib_cs.Raylib;
namespace uniray_Project
{
    public unsafe class RLoading
    {
        public RLoading() 
        {
            Console.ForegroundColor = ConsoleColor.Green;
            TraceLog(TraceLogLevel.Info, "New RLoading instance launched");
            Console.ForegroundColor = ConsoleColor.White;
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
                {"inventory_case_selected", LoadTexture("../inventory_case_selected.png") },
            };
            // Return the dictionary
            return textures;
        }
        /// <summary>
        /// Load all the utilities 3D models
        /// </summary>
        /// <returns>The dictionary containing all the loaded models</returns>
        public Dictionary<string, Model> LoadUtilities()
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
        /// <summary>
        /// Load animation list from m3d file
        /// </summary>
        /// <param name="animation">The path to the file</param>
        /// <returns>The list of the animations</returns>
        public List<Animation> LoadAnimationList(string animation)
        {
            // Load animations into the RAM from the m3d file
            int animCount = 0;
            ModelAnimation* animations = LoadModelAnimations(animation, ref animCount);
            // Create and fill animation list 
            List<Animation> list = new List<Animation>();
            for (int i = 0; i < animCount; i++)
            {
                list.Add(new Animation(animations[i]));
            }
            return list;
        }
        /// <summary>
        /// Load the list of static boxes from the current scene's game objects
        /// </summary>
        /// <param name="gos"></param>
        /// <returns></returns>
        public List<BoundingBox> LoadStaticBoxes(List<GameObject3D> gos)
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
                    // Add the go's position
                    box.Min += gos[i].Position;
                    box.Max += gos[i].Position;


                    boxes.Add(box);
                    // Unload the model
                    //UnloadModel(model);
                }
            }
            // Return all the models
            return boxes;
        }
    }
}