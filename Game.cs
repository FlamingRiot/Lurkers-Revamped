using Astar;
using Raylib_cs;
using System.Numerics;
using Uniray_Engine;
using uniray_Project;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using static Uniray_Engine.UnirayEngine;

namespace Lurkers_revamped
{
    /// <summary>Represents an instance of a <see cref="Game"/> object.</summary>
    public static unsafe class Game
    {
        private static Vector3 RadioPosition = Vector3.Zero;
        private static Color CrosshairColor = Color.White;
        private static List<string> FreeZombies = new List<string>();

        public static ShaderCenter Shaders = new ShaderCenter();

        public static ShadowMap ShadowMap = new ShadowMap(new Vector3(-50.0f, 25.0f, -50.0f), new Vector3(0.95f, -1.0f, 1.5f));

        public static List<Spawner> Spawners = new List<Spawner>();

        public static List<Zombie> Zombies = new List<Zombie>();

        public static Ressource Ressources = new Ressource();

        public static Player Player = new Player();

        public static Camera3D Camera;

        public static CameraMotion CameraMotion;

        public static Terrain Terrain;

        public static double StartTime;

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

        /// <summary>Loads the needed ressources for the </summary>
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

            // Add default zombies to the list
            Zombies = new List<Zombie>()
            {
                new Zombie(new Vector3(-10, 0, 2), "cop", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(10, 0, 2), "cop2", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(2, 0, -10), "cop3", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(2, 0, -5), "cop4", AnimationCenter.ZombieAnimations[8])
            };
        }

        /// <summary>Starts the game by launching the menu.</summary>
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
            StartTime = GetTime();
            DisableCursor();
        }

        /// <summary>Enters the game loop and updates the game every frame.</summary>
        public static void Update()
        {
            // Game loop
            while (!WindowShouldClose())
            {
                // Update camera actions
                UpdateCamera();
                // Render and compute shadow map
                RenderShadowMap();
                // Update player actions
                UpdatePlayer();
                // Render 3D world to render texture
                RenderWorld();
                // Update zombies
                UpdateZombies();
                // Draw 2D render to the screen
                DrawRender();
            }
        }

        /// <summary>Closes the window and unloads GPU loaded ressources.</summary>
        public static void Close()
        {
            AnimationCenter.Close();
            CloseWindow();
            UnloadGame();
        }

        // -------------------------------------------------------------
        // Primary functions
        // -------------------------------------------------------------

        /// <summary>Renders world to shadow map view point.</summary>
        private static void RenderShadowMap()
        {
            Matrix4x4 lightView = new Matrix4x4();
            Matrix4x4 lightProj = new Matrix4x4();

            BeginTextureMode(ShadowMap.Map);
            ClearBackground(Color.White);
            BeginMode3D(ShadowMap.CameraView);
            lightView = Rlgl.GetMatrixModelview();
            lightProj = Rlgl.GetMatrixProjection();

            // Draw full scene to the shadow map render texture
            DrawScene();

            foreach (Zombie zombie in Zombies)
            {
                DrawModelEx(Ressources.Rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);
            }

            DrawMesh(Terrain.Mesh, Terrain.Material, Terrain.Transform);

            EndMode3D();
            EndTextureMode();

            Matrix4x4 lightViewProj = MatrixMultiply(lightView, lightProj);

            ClearBackground(Color.RayWhite);

            Shaders.UpdateLightMatrix(lightViewProj);

            Shaders.UpdateShadowMap(ShadowMap);

            SetShaderValue(
                Shaders.LightingShader,
                Shaders.LightingShader.Locs[(int)ShaderLocationIndex.VectorView],
                Camera.Position,
                ShaderUniformDataType.Vec3
            );
        }

        /// <summary>Updates player and camera actions.</summary>
        public static void UpdatePlayer()
        {
            // Update the camera shake motion
            CameraMotion.ShakeStart = Camera.Position;
            Camera.Position = CameraMotion.Update(Camera.Position, Player.SPEED);

            // Update the current animation of the player
            switch (Player.WeaponState)
            {
                case PlayerWeaponState.Idle:
                    Player.CurrentAnimation = AnimationCenter.PlayerAnimations[1];
                    break;
                case PlayerWeaponState.Shooting:
                    Player.CurrentAnimation = AnimationCenter.PlayerAnimations[3];
                    // Check everytime a bullet is shot
                    if (Player.Frame == 1)
                    {
                        Player.CurrentWeapon.ShootBullet(new Vector3(Camera.Position.X, Camera.Position.Y - 0.045f, Camera.Position.Z) + GetCameraRight(ref Camera) / 12, GetCameraForward(ref Camera)); ;
                        // Play shooting sound
                        AudioCenter.PlaySound("rifleShoot");
                        // Set crosshair color
                        CrosshairColor = Color.Red;
                        // Check collision with zombies
                        foreach (Zombie zombie in Zombies)
                        {
                            if (zombie.Shoot(Player.CurrentWeapon.bullets.Last().Ray, Ressources.Rigged[zombie.Type].Meshes[0]))
                            {
                                if (TaskManager.IsActive(1)) TaskManager.UpdateTask(1, 1);
                                if (TaskManager.IsActive(3)) TaskManager.UpdateTask(3, 1);
                                if (TaskManager.IsActive(5)) TaskManager.UpdateTask(5, 1);
                                if (TaskManager.IsActive(7)) TaskManager.UpdateTask(7, 1);
                            }
                        }
                        // Check spawners
                        foreach (Spawner spawner in Spawners)
                        {
                            if (spawner.Shoot(Player.CurrentWeapon.bullets.Last().Ray, UnirayEngine.Ressource.GetModel("crystal").Meshes[0]))
                            {
                                // Play randomly pitched hit sound
                                AudioCenter.SetSoundVolume("crystal_hit", 12);
                                AudioCenter.SetSoundPitch("crystal_hit", Clamp((float)(Random.Shared.NextDouble() * 3), 1.5f, 3f));
                                AudioCenter.PlaySound("crystal_hit");
                                if (Zombies.Count < 4 && FreeZombies.Count != 0)
                                {
                                    Zombies.Add(spawner.CreateZombie(AnimationCenter.ZombieAnimations[8], Camera.Position, FreeZombies.First()));
                                    FreeZombies.RemoveAt(0);
                                }
                                if (spawner.Destroyed)
                                {
                                    // Play destruction sound
                                    AudioCenter.SetSoundVolume("crystal_destroyed", 12);
                                    AudioCenter.PlaySound("crystal_destroyed");
                                    // Update task if active
                                    if (TaskManager.IsActive(2)) TaskManager.UpdateTask(2, 1);

                                    // Update other spawners index
                                    Ressources.StaticBoxes.RemoveAt(spawner.RessourceIndex);
                                    Spawners.Remove(spawner);
                                    // Switch ressource index with existing spawners
                                    Spawners.ForEach(x =>
                                    {
                                        if (spawner.RessourceIndex < x.RessourceIndex)
                                        {
                                            (x.RessourceIndex, spawner.RessourceIndex) = (spawner.RessourceIndex, x.RessourceIndex);
                                        }
                                    });
                                    break;
                                }
                            }
                        }

                        Player.CurrentWeapon.bullets.RemoveAt(0);
                    }
                    else if (Player.Frame > 7) CrosshairColor = Color.White;
                    break;
                case PlayerWeaponState.Reloading:
                    Player.CurrentAnimation = AnimationCenter.PlayerAnimations[2];
                    if (Player.Frame == Player.CurrentAnimation.FrameCount - 1) Player.WeaponState = PlayerWeaponState.Idle;
                    break;
                case PlayerWeaponState.Taking:
                    Player.CurrentAnimation = AnimationCenter.PlayerAnimations[4];
                    if (Player.Frame == Player.CurrentAnimation.FrameCount - 1)
                    {
                        Player.WeaponState = PlayerWeaponState.Idle;
                    }
                    break;
            }

            // Update model animation
            UpdateModelAnimation(Ressources.Utilities[Player.CurrentWeapon.ModelID], Player.CurrentAnimation, Player.UpdateFrame());

            // Update the player event handler
            if (!TaskManager.Active) TickPlayer();

            if (IsKeyPressed(KeyboardKey.Tab))
            {
                if (!TaskManager.Active)
                {
                    TaskManager.Active = true;
                    CameraMotion.Amplitude = 0;
                    EnableCursor();
                }
                else
                {
                    TaskManager.Active = false;
                    DisableCursor();
                }
            }
        }

        /// <summary>Render the world to a render texture used for post-processing shaders afterwards.</summary>
        private static void RenderWorld()
        {
            // Render scene to texture
            BeginTextureMode(Program.RenderTexture);

            // Clear background every frame using white color
            ClearBackground(Color.Gray);

            // Begin 3D mode with the current scene's camera
            BeginMode3D(Camera);
#if DEBUG
                DrawSphereWires(ShadowMap.CameraView.Position, 2, 10, 10, Color.Red);
#endif
            // Draw the external skybox 
            Rlgl.DisableBackfaceCulling();
            Rlgl.DisableDepthMask();
            DrawMesh(Shaders.Skybox, Shaders.SkyboxMaterial, MatrixIdentity());
            Rlgl.EnableBackfaceCulling();
            Rlgl.EnableDepthMask();

            // Set terrain tiling to true
            Shaders.UpdateTiling(true);

            // Draw terrain
            DrawMesh(Terrain.Mesh, Terrain.Material, Terrain.Transform);

            // Set terrain tiling to false
            Shaders.UpdateTiling(false);

            // Check collisions between the player and the static objects
            // Add current position
            Player.Box.Min += Camera.Position;
            Player.Box.Max += Camera.Position;
            CheckCollisionPlayer(Ressources.StaticBoxes);
#if DEBUG
                // Draw player's bounding box
                DrawBoundingBox(Player.Box, Color.Red);

                // Draw node map
                AStar.Grid.DrawNodeMap();
#endif

            // Draw the gameobjects of the environment (from Uniray)
            DrawScene();

            // Transform the player's current model
            Matrix4x4 mTransform = TransformPlayer(CameraMotion.Yaw, CameraMotion.Pitch, CameraMotion.SideShake);

            // Assign new rotation matrix to the model
            Ressources.Utilities[Player.CurrentWeapon.ModelID] = SetModelTransform(Ressources.Utilities[Player.CurrentWeapon.ModelID], mTransform);

            // Draw player's current model
            DrawModel(Ressources.Utilities[Player.CurrentWeapon.ModelID], new Vector3(Camera.Position.X - GetCameraForward(ref Camera).X / 3, Camera.Position.Y - 0.2f, Camera.Position.Z - GetCameraForward(ref Camera).Z / 3), 3.5f, Color.White);

        }

        /// <summary>Updates each zombie's actions.</summary>
        private static void UpdateZombies()
        {
            // Draw and tick the current zombies of the scene
            int killIndex = -1;
            foreach (Zombie zombie in Zombies)
            {
                // Draw zombie model
                DrawModelEx(Ressources.Rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);

                // Update the zombie model according to its state 
                if (!TaskManager.Active) UpdateModelAnimation(Ressources.Rigged[zombie.Type], zombie.CurrentAnimation, zombie.UpdateFrame());
                switch (zombie.State)
                {
                    case ZombieState.Running:
                        zombie.CurrentAnimation = AnimationCenter.ZombieAnimations[8];
                        break;
                    case ZombieState.Dying1:
                        zombie.CurrentAnimation = AnimationCenter.ZombieAnimations[5];
                        // Check if the zombie is done dying
                        if (zombie.Frame == zombie.CurrentAnimation.FrameCount - 1)
                        {
                            // Remove the zombie from the list
                            killIndex = Zombies.IndexOf(zombie);
                            // Reset the frame of the animation
                            zombie.Frame = 0;
                        }
                        break;
                    case ZombieState.Dying2:
                        zombie.CurrentAnimation = AnimationCenter.ZombieAnimations[4];
                        // Check if the zombie is done dying
                        if (zombie.Frame == zombie.CurrentAnimation.FrameCount - 1)
                        {
                            // Remove the zombie from the list
                            killIndex = Zombies.IndexOf(zombie);
                            // Reste the frame of the animation
                            zombie.Frame = 0;
                        }
                        break;
                    case ZombieState.Attacking:
                        zombie.CurrentAnimation = AnimationCenter.ZombieAnimations[2];
                        zombie.UpdateFrame();
                        if (zombie.Frame == 90)
                        {
                            Player.Life -= 10;
                            TaskManager.UpdateTask(8, 10);
                            // Launch zombie kill animation
                            if (Player.Life <= 0)
                            {
                                zombie.State = ZombieState.Killing;
                                Camera.Position.Y -= 0.2f;
                            }
                        }
                        if (Math.Abs(Vector3Subtract(Camera.Position, zombie.Position).Length()) > 5)
                        {
                            zombie.State = ZombieState.Running;
                            zombie.Frame = 0;
                        }
                        break;
                    case ZombieState.Idle:
                        zombie.CurrentAnimation = AnimationCenter.ZombieAnimations[6];
                        break;
                    case ZombieState.Killing:
                        zombie.CurrentAnimation = AnimationCenter.ZombieAnimations[7];
                        break;
                }

                // Move and rotate the zombie if running
                if (zombie.State == ZombieState.Running && !TaskManager.Active)
                {
                    // Find path 
                    if (AStar.Grid.GetWorldToNode(Camera.Position).Walkable)
                    {
                        AStar.FindPath(zombie.Position, Camera.Position, zombie.Path);
                    }
                    // aStar.Grid.DrawPath(zombie.Path);

                    Node currentNode = AStar.Grid.GetWorldToNode(zombie.Position);
                    List<Node> neighbours = AStar.Grid.GetNeighbours(currentNode);
                    if (zombie.PreviousNodes[0].Position != neighbours[0].Position)
                    {
                        zombie.PreviousNodes.ForEach(x =>
                        {
                            if (!x.HARD_NODE) x.Walkable = true;
                        });
                        zombie.PreviousNodes = neighbours;
                    }
                    else
                    {
                        zombie.PreviousNodes.ForEach(x => x.Walkable = false);
                    }

                    if (zombie.Path.Count != 0)
                    {
                        // Calculate rotation and movement
                        Vector2 direction = Vector2Normalize(zombie.Path[1].Position - zombie.Path[0].Position);
                        zombie.TargetAngle = MathF.Atan2(-direction.Y, direction.X) * RAD2DEG + 90;
                        // Correct angle
                        if (zombie.TargetAngle - zombie.Angle > 180) zombie.TargetAngle -= 360;
                        zombie.Angle = Lerp(zombie.Angle, zombie.TargetAngle, Zombie.ROTATION_SPEED * GetFrameTime());

                        // Calculate movement
                        if (Math.Abs(Vector3Subtract(Camera.Position, zombie.Position).Length()) > 5)
                        {
                            zombie.X += MathF.Cos((zombie.Angle - 90) * DEG2RAD) * Zombie.SPEED * GetFrameTime();
                            zombie.Z -= MathF.Sin((zombie.Angle - 90) * DEG2RAD) * Zombie.SPEED * GetFrameTime();
                        }
                        else
                        {
                            zombie.State = ZombieState.Attacking;
                            if (zombie.Frame != 0) { zombie.Frame = 0; }
                        }
                    }
                    // Map outsider check
                    if (zombie.Path.Count > 75)
                    {
                        zombie.State = ZombieState.Dying1;
                    }

                    // Play zombie default running sound
                    AudioCenter.PlaySoundLoop("zombie_default");
                }
            }
            // Remove the zombie from the list if killed
            if (killIndex != -1)
            {
                // Remove unused security perimeter
                Node currentNode = AStar.Grid.GetWorldToNode(Zombies[killIndex].Position);
                List<Node> neighbours = AStar.Grid.GetNeighbours(currentNode);
                neighbours.ForEach(node =>
                {
                    if (!node.HARD_NODE) node.Walkable = true;
                });

                FreeZombies.Add(Zombies[killIndex].Type);
                Zombies.RemoveAt(killIndex);
            }

            // End 3D mode context
            EndMode3D();

            EndTextureMode();
        }

        /// <summary>Starts post-processing on rendered texture and then displays it to the screen.</summary>
        private static void DrawRender()
        {
            // Draw to the screen
            BeginDrawing();

            // Begin blur shader 
            BeginShaderMode(Shaders.MotionBlurShader);

            // Set previous frame render texture
            Shaders.SetBlurTexture(Program.PreviousRenderTexture);

            // Set current time
            Shaders.UpdateTime((float)GetTime());

            // Draw render texture to the screen
            DrawTexturePro(Program.RenderTexture.Texture, Program.ScreenInverseRectangle, Program.ScreenRectangle, Vector2.Zero, 0, Color.White);

            EndShaderMode();

            // Manage blood screen
            if (Player.BLOODY)
            {
                DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(112, 1, 9, (int)Clamp((float)Player.Watch.Elapsed.TotalSeconds * 100, 0, 90)));
                // Check if time's up
                if (Player.Watch.Elapsed.TotalSeconds > 10)
                {
                    Player.Watch.Restart();
                    Player.BLOODY = false;
                    AudioCenter.PlaySound("blood");
                    Zombies.ForEach(x =>
                    {
                        if (x.State != ZombieState.Dying1 && x.State != ZombieState.Dying2) x.State = ZombieState.Running;
                    });
                }
            }
            else if (!Player.BLOODY && Player.Watch.IsRunning)
            {
                DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(112, 1, 9, (int)Clamp(90 / ((float)Player.Watch.Elapsed.TotalSeconds * 100), 0, 90)));
            }

            // Draw screen infos
            ScreenCenter.DrawScreenInfos();

            // Draw current inventory case
            DrawTexture(Ressources.UITextures["inventory_case_selected"], GetScreenWidth() - 121, GetScreenHeight() - (800 - (Player.InventoryIndex) * 85) - 1, Color.White);

            // Draw crosshair
            DrawTexture(Ressources.UITextures["crosshair"], GetScreenWidth() / 2 - Ressources.UITextures["crosshair"].Width / 2, GetScreenHeight() / 2 - Ressources.UITextures["crosshair"].Height / 2, CrosshairColor);

            // Draw lifebar
            DrawRectangleGradientH(180, Program.ScreenHeight - 140, (int)(2.4f * Player.Life), 15, Color.Lime, Color.Green);
            DrawTexture(Ressources.UITextures["lifebar"], 50, Program.ScreenHeight - 250, Color.White);

            // Draw tasks manager
            TaskManager.Update();

            // Draw Gradient at the start
            if (StartTime + Cutscene.GRADIENT_TIME > GetTime()) Cutscene.Gradient(Color.Black);
#if DEBUG

                // Debug positions
                DrawText("Position: " + Camera.Position.ToString() +
                    "\nJump Force: " + Player.VJump +
                    "\nInventory Index: " + Player.InventoryIndex +
                    "\nWeapon Level: " + Player.CurrentWeapon.Level +
                    "\nCamera Target:" + Camera.Target.ToString() +
                    "\nMotion Constraint: " + Player.MotionConstraint.Value +
                    "\nConstraint: " + Player.MotionConstraint.Constraint
                    , 200, 200, 20, Color.Red);

                DrawFPS(0, 0);
#endif
            // End drawing context
            EndDrawing();

            // Draw to previous frame render texture
            BeginTextureMode(Program.PreviousRenderTexture);

            DrawTexturePro(Program.RenderTexture.Texture, Program.ScreenInverseRectangle, Program.ScreenRectangle, Vector2.Zero, 0, Color.White);

            EndTextureMode();

            // Reset the player's box
            Player.ResetBox();
        }

        /// <summary>Updates the camera's actions, as well as input actions relative to its position.</summary>
        public static void UpdateCamera()
        {
            // Update the camera
            if (!TaskManager.Active)
            {
                // Calculate the Camera rotation
                Vector2 mouse = GetMouseDelta();
                CameraMotion.Yaw -= mouse.X * 0.003f;
                CameraMotion.Pitch -= mouse.Y * 0.003f;

                CameraMotion.Pitch = Math.Clamp(CameraMotion.Pitch, -1.5f, 1.5f);

                // Calculate Camera direction
                Vector3 direction;
                direction.X = (float)(Math.Cos(CameraMotion.Pitch) * Math.Sin(CameraMotion.Yaw));
                direction.Y = (float)Math.Sin(CameraMotion.Pitch);
                //direction.Y = 0;
                direction.Z = (float)(Math.Cos(CameraMotion.Pitch) * Math.Cos(CameraMotion.Yaw));

                // Calculate the Camera movement
                Vector3 movement = Vector3.Zero;
                // Adjust Camera position and target
                if (IsKeyDown(KeyboardKey.W))
                {
                    //movement += GetCameraForward(ref Camera);
                    movement -= Vector3CrossProduct(GetCameraRight(ref Camera), Vector3.UnitY);
                }
                if (IsKeyDown(KeyboardKey.S))
                {
                    movement += Vector3CrossProduct(GetCameraRight(ref Camera), Vector3.UnitY);
                }
                if (IsKeyDown(KeyboardKey.D))
                {
                    movement += GetCameraRight(ref Camera);
                    if (CameraMotion.SideShake < 0.15f) CameraMotion.SideShake += 0.01f;
                }
                if (IsKeyDown(KeyboardKey.A))
                {
                    movement -= GetCameraRight(ref Camera);
                    if (CameraMotion.SideShake > -0.15f) CameraMotion.SideShake -= 0.01f;
                }
                if (IsKeyUp(KeyboardKey.A) && IsKeyUp(KeyboardKey.D))
                {
                    if (CameraMotion.SideShake < 0.0f) CameraMotion.SideShake += 0.01f;
                    else if (CameraMotion.SideShake > 0.0f) CameraMotion.SideShake -= 0.01f;
                }
                if (IsKeyPressed(KeyboardKey.E))
                {
                    // Secret radio
                    foreach (UModel radio in CurrentScene.GameObjects.Where(x => x is UModel).Where(x => ((UModel)x).ModelID == "radio"))
                    {
                        RadioPosition = radio.Position;
                        if (Vector3Distance(RadioPosition, Camera.Position) <= 3)
                        {
                            AudioCenter.PlayMusic("Linkin");
                            AudioCenter.SetMusicVolume("Linkin", 8);
                            AudioCenter.PlaySound("radio");
                            AudioCenter.SetSoundVolume("radio", 10);
                            if (TaskManager.IsActive(4)) TaskManager.UpdateTask(4, 1);
                        }
                    }
                    // Phone booths
                    foreach (UModel phone in CurrentScene.GameObjects.Where(x => x is UModel).Where(x => ((UModel)x).ModelID == "phone"))
                    {
                        if (Vector3Distance(phone.Position, Camera.Position) <= 5)
                        {
                            if (!AudioCenter.IsSoundPlaying("rick") && !AudioCenter.IsSoundPlaying("daryl"))
                            {
                                AudioCenter.PlaySound("radio");
                                int rand = Random.Shared.Next(0, 2);
                                if (rand == 1) AudioCenter.PlaySound("rick");
                                else AudioCenter.PlaySound("daryl");
                            }
                            if (TaskManager.IsActive(0)) TaskManager.UpdateTask(0, 1);
                        }
                    }
                    // Dead bodies
                    foreach (UModel body in CurrentScene.GameObjects.Where(x => x is UModel).Where(x => ((UModel)x).ModelID == "dead_body"))
                    {
                        if (Vector3Distance(body.Position, Camera.Position) <= 5)
                        {
                            AudioCenter.PlaySound("blood");
                            Player.BLOODY = true;
                            Player.Watch.Restart();
                            foreach (Zombie zombie in Zombies)
                            {
                                zombie.State = ZombieState.Idle;
                                zombie.Angle = Random.Shared.Next(0, 360);
                            }
                            if (TaskManager.IsActive(6)) TaskManager.UpdateTask(6, 1);
                        }
                    }
                }
                if (AudioCenter.IsMusicPlaying("Linkin"))
                {
                    AudioCenter.UpdateMusic("Linkin");
                    float volume = 20 - Vector3Distance(RadioPosition, Camera.Position);
                    volume = Clamp(volume, 0f, 8f);
                    AudioCenter.SetMusicVolume("Linkin", volume);
                }

                // Final movement transformations
                if (Vector3Length(movement) > 0)
                {
                    // Normalize vector
                    movement = Vector3Normalize(movement) * Player.SPEED * Player.MotionConstraint.Value;
                    // Block movement according to the motion constraint
                    MotionConstraint.ComputeMovement(ref movement, Player.MotionConstraint.Constraint);
                    // Limit the movement to X and Z axis and normalize
                    movement = new Vector3(movement.X, 0.0f, movement.Z);
                    // Set Camera shake
                }
                else CameraMotion.Amplitude = 0.0006f;

                // Increment movement
                Camera.Position += movement;
                Camera.Target = Vector3Add(Camera.Position, direction);

                // Make the player jump if needed
                if (Player.MoveState == PlayerMoveState.Jumping)
                {
                    // Add jump force
                    Camera.Position.Y += Player.VJump;
                    Camera.Target.Y += Player.VJump;
                    // Decrease jump force
                    Player.VJump -= 0.02f;
                    if (Camera.Position.Y <= 3)
                    {
                        Player.MoveState = PlayerMoveState.Running;
                        // Fix jump offset
                        Camera.Position.Y = (float)Math.Round(Camera.Position.Y, 3);
                    }
                }
            }
        }

        /// <summary>Unloads the GPU loaded ressources of the game.</summary>
        public static void UnloadGame()
        {
            // Unload all ressources that ARE NOT from Uniray
            foreach (KeyValuePair<string, Model> utilitesList in Ressources.Utilities)
            {
                for (int i = 0; i < utilitesList.Value.MaterialCount; i++)
                {
                    UnloadMaterial(utilitesList.Value.Materials[i]);
                }
                UnloadModel(utilitesList.Value);
            }
            foreach (KeyValuePair<string, Model> riggedObject in Ressources.Rigged)
            {
                UnloadModel(riggedObject.Value);
            }
            // Unload shaders
            Shaders.UnloadShaderCenter();
        }

        // -------------------------------------------------------------
        // Secondary functions
        // -------------------------------------------------------------

        /// <summary>Ticks the player events based on its state.</summary>
        private static void TickPlayer()
        {
            // Manager input events
            // Reload event
            if (IsKeyPressed(KeyboardKey.R))
            {
                Player.WeaponState = PlayerWeaponState.Reloading;
            }
            // Jump event
            if (IsKeyPressed(KeyboardKey.Space))
            {
                // Prevent cross-jumping
                if (Player.MoveState != PlayerMoveState.Jumping)
                {
                    Player.MoveState = PlayerMoveState.Jumping;
                    Player.VJump = Player.JUMP_FORCE;
                }
            }

            // Sprinting event
            if (IsKeyDown(KeyboardKey.LeftShift))
            {
                Player.SPEED = 0.2f;
                CameraMotion.Amplitude = 0.006f;
                AudioCenter.StopSound("walking");
                AudioCenter.PlaySoundLoop("running");
            }
            else if (!IsKeyDown(KeyboardKey.LeftShift))
            {
                Player.SPEED = 0.1f;
                CameraMotion.Amplitude = 0.003f;
                AudioCenter.StopSound("running");
                AudioCenter.PlaySoundLoop("walking");
            }

            // Shooting event
            if (IsMouseButtonDown(MouseButton.Left))
            {
                Player.WeaponState = PlayerWeaponState.Shooting;
            }
            // Stop shooting event
            else if (IsMouseButtonUp(MouseButton.Left) && Player.WeaponState == PlayerWeaponState.Shooting)
            {
                Player.WeaponState = PlayerWeaponState.Idle;
                CrosshairColor = Color.White;
                Player.Frame = 0;

            }

            // Inventory changing event
            float wheel = GetMouseWheelMove();
            if (wheel == -1 && Player.WeaponState != PlayerWeaponState.Taking)
            {
                // Manage inventory size
                if (Player.InventoryIndex == Player.InventorySize) Player.SetCurrentWeapon(0);
                else Player.SetCurrentWeapon(Player.InventoryIndex + 1);

                // Set appropriate color of the new weapon
                SetShaderValue(Shaders.OutlineShader, GetShaderLocation(Shaders.OutlineShader, "highlightCol"), Weapon.Colors[Player.CurrentWeapon.Level - 1], ShaderUniformDataType.Vec4);


                // Set player state
                Player.WeaponState = PlayerWeaponState.Taking;
            }
            else if (wheel == 1 && Player.WeaponState != PlayerWeaponState.Taking)
            {
                // Manage inventory size
                if (Player.InventoryIndex == 0) Player.SetCurrentWeapon(Player.InventorySize);
                else Player.SetCurrentWeapon(Player.InventoryIndex - 1);

                // Set appropriate color of the new weapon
                SetShaderValue(Shaders.OutlineShader, GetShaderLocation(Shaders.OutlineShader, "highlightCol"), Weapon.Colors[Player.CurrentWeapon.Level - 1], ShaderUniformDataType.Vec4);


                // Set player state
                Player.WeaponState = PlayerWeaponState.Taking;
            }
        }

        /// <summary>Check collisions between the player and the static objects of the map</summary>
        public static void CheckCollisionPlayer(List<BoundingBox> boxes)
        {
            bool hit = false;
            // Loop over all the static objects
            foreach (BoundingBox staticBox in boxes)
            {
#if DEBUG
                DrawBoundingBox(staticBox, Color.Red);
#endif
                // Check individual collision
                if (CheckCollisionBoxes(Player.Box, staticBox))
                {
                    Vector3 cf = GetCameraForward(ref Camera);
                    // Calculate the motion constraint corresponding to the angle between target and box
                    Player.CalculateMotionConstraint(new Vector3(cf.X, 0, cf.Z), staticBox, Camera.Position);

                    hit = true;
                }
            }
            if (!hit) Player.MotionConstraint = MotionConstraint.Default;
        }

        /// <summary>Applies a transform to a model</summary>
        /// <param name="model">Model to modifiy</param>
        /// <param name="transform">Transform to set</param>
        /// <returns>The modified model.</returns>
        public static Model SetModelTransform(Model model, Matrix4x4 transform)
        {
            Model copy = model;
            copy.Transform = transform;
            return copy;
        }

        /// <summary>Computes player's rotation informations to a 4x4 matrix.</summary>
        /// <param name="yaw">Camera yaw rotation</param>
        /// <param name="pitch">Camera pitch rotation</param>
        /// <param name="sideShake">Camera side shake</param>
        /// <returns><see cref="Matrix4x4"/> relative to the passed data.</returns>
        public static Matrix4x4 TransformPlayer(float yaw, float pitch, float sideShake)
        {
            Matrix4x4 rotationYaw = MatrixRotateY(yaw);
            Matrix4x4 rotationPitch = MatrixRotateX(-pitch);
            Matrix4x4 rotationRoll = MatrixRotateZ(sideShake);
            Matrix4x4 weaponRotation = MatrixMultiply(rotationPitch, rotationYaw);
            // Return final transform
            return MatrixMultiply(rotationRoll, weaponRotation);
        }
    }
}