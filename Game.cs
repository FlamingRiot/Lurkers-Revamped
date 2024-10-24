using Astar;
using Raylib_cs;
using System.Numerics;
using Uniray_Engine;
using static Raylib_cs.Raylib;
using static Uniray_Engine.UnirayEngine;


namespace Lurkers_revamped
{
    /// <summary>Represents an instance of a <see cref="Game"/> object.</summary>
    public static unsafe class Game
    {
        public static ShaderCenter Shaders = new ShaderCenter();

        public static ShadowMap ShadowMap = new ShadowMap(new Vector3(-50.0f, 25.0f, -50.0f), new Vector3(0.95f, -1.0f, 1.5f));

        public static List<Spawner> Spawners = new List<Spawner>();

        public static Ressource Ressources = new Ressource();

        public static Player Player = new Player();

        public static Camera3D Camera;

        public static CameraMotion CameraMotion;

        public static Terrain Terrain;

        /// <summary>Inits the game program and creates the window.</summary>
        public static void Init()
        {
            // Load and draw splash
            Texture2D splash = LoadTexture("src/textures/splash.png");
            BeginDrawing();
            DrawTexture(splash, 0, 0, Color.White);
            EndDrawing();

            // Init game managment centers
            AnimationCenter.Init();
            AudioCenter.Init();

            // Init Uniray engine build library
            InitEngine();

            // Init task manager
            TaskManager.LoadTasks();
            TaskManager.Active = false;
        }

        /// <summary>Loads the needed ressources for the game.</summary>
        public static void Load()
        {
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

            // Load material embedded ressources
            Ressources.Load();

            // Load spawners
            Spawners = RLoading.LoadSpawners();

            // Load terrain
            Terrain = RLoading.GenTerrain();

            // Load A* Grid
            AStar.Load(new Grid(Vector3.Zero, new Vector2(120), 0.5f, Ressources.StaticBoxes));

            // Load shader on model materials
            foreach (UModel go in CurrentScene.GameObjects.Where(x => x is UModel))
            {
                go.SetShader(Shaders.LightingShader);
            }
            Terrain.Material.Shader = Shaders.LightingShader;
            for (int i = 0; i < Ressources.Utilities["rifle"].MaterialCount; i++)
            {
                Ressources.Utilities["rifle"].Materials[i].Shader = Shaders.LightingShader;
            }

            foreach (KeyValuePair<string, Model> m in Ressources.Rigged)
            {
                for (int i = 0; i < m.Value.MaterialCount; i++)
                {
                    m.Value.Materials[i].Shader = Shaders.LightingShader;
                }
            }

            // Create and load player default attributes
            Player = new Player("Anonymous254", new Weapon("Lambert Niv. 1", "rifle", 50, 1), AnimationCenter.PlayerAnimations[1]);
            // (Debug) Add a second weapon to the inventory of the player
            Player.AddWeapon(new Weapon("Lambert Niv. 2", "rifle", 50, 2));
        }

        public static void Start()
        {
            // Set Window state when loading is done
            SetWindowState(ConfigFlags.ResizableWindow);
            SetWindowState(ConfigFlags.MaximizedWindow);

            // Get window size
            Program.ScreenWidth = GetScreenWidth();
            Program.ScreenHeight = GetScreenHeight();
            Program.ScreenSize = new Vector2(GetScreenWidth(), GetScreenHeight());

            // Get screen attributes and objects
            Program.RenderTexture = LoadRenderTexture(Program.ScreenWidth, Program.ScreenHeight);
            Program.PreviousRenderTexture = LoadRenderTexture(Program.ScreenWidth, Program.ScreenHeight);
            Program.ScreenRectangle = new Rectangle(0, 0, Program.ScreenWidth, Program.ScreenHeight);
            Program.ScreenInverseRectangle = new Rectangle(0, 0, Program.ScreenWidth, -Program.ScreenHeight);

            // Start Menu
            Menu.Init();
            Menu.Show(Shaders.Skybox, Shaders, Terrain);
            // Set target FPS
            SetTargetFPS(60);
        }

        public static void Update()
        {

        }

        public static void Close()
        {

        }
    }
}