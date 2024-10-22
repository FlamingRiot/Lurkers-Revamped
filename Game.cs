using Astar;
using Raylib_cs;
using System.Numerics;
using Uniray_Engine;
using static Raylib_cs.Raylib;
using static Uniray_Engine.UnirayEngine;


namespace Lurkers_revamped
{
    /// <summary>Represents an instance of a <see cref="Game"/> object.</summary>
    public unsafe class Game
    {
        public ShaderCenter Shaders;

        public ShadowMap ShadowMap;

        public List<Spawner> Spawners;

        public Ressource Ressources;

        public Camera3D Camera;

        public CameraMotion CameraMotion;

        public Terrain Terrain;

        public Game()
        {
            Shaders = new ShaderCenter();
            ShadowMap = new ShadowMap(new Vector3(-50.0f, 25.0f, -50.0f), new Vector3(0.95f, -1.0f, 1.5f));
            Spawners = new List<Spawner>();
            Ressources = new Ressource();
        }

        /// <summary>Inits the game program and creates the window.</summary>
        public void Init()
        {
            // Load and draw splash
            Texture2D splash = LoadTexture("src/textures/splash.png");
            BeginDrawing();
            DrawTexture(splash, 0, 0, Color.White);
            EndDrawing();
        }

        /// <summary>Loads the needed ressources for the game.</summary>
        public void Load()
        {
            // Init Uniray Engine
            InitEngine();

            // Init game managments centers
            AudioCenter.Init();

            // Load shader relative data
            Shaders.LoadLighting(ShadowMap.CameraView.Target, new Color(70, 25, 0, 255));

            // Load default camera settings
            Camera = new Camera3D();
            Camera.Position = new Vector3(-52.0f, 3.0f, -16.0f);
            Camera.Target = new Vector3(3.0f, 3.0f, 0.0f);
            Camera.Up = Vector3.UnitY;
            Camera.Projection = CameraProjection.Perspective;
            Camera.FovY = 60f;
            // Additional camera motion data
            CameraMotion = new CameraMotion(0.0006f, 10.0f, 0.1f);

            // Change the current directory so the embedded materials from the models can be loaded successfully
            sbyte* dir = GetApplicationDirectory();
            string workdir = new string(dir);
            string newDir = workdir + "src\\textures\\materials\\";
            Program.SetWorkdir(newDir);
            Ressources.Load();
            Program.SetWorkdir(workdir);

            // Load spawners
            Spawners = RLoading.LoadSpawners();

            // Load terrain
            Terrain = RLoading.GenTerrain();

            // Load A* Grid
            AStar.Load(new Grid(Vector3.Zero, new Vector2(120), 0.5f, Ressources.StaticBoxes));
        }

        public void Start()
        {

        }

        public void Update()
        {

        }

        public void Close()
        {

        }
    }
}