using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using System.Numerics;
using Uniray_Engine;
using static Uniray_Engine.UnirayEngine;
using uniray_Project;
using System.Text;
using Astar;

namespace Lurkers_revamped
{
    /// <summary>Represents an instance of the running program.</summary>
    public unsafe class Program
    {
        public static double GameStartTime;
        // Window size global variables
        public static int ScreenWidth;
        public static int ScreenHeight;
        public static Vector2 ScreenSize;
        public  static void Main(string[] args)
        {
            // Init splash window
            InitWindow(200, 200, "Lurkers: Revamped");
            SetWindowState(ConfigFlags.UndecoratedWindow);

            // Create game
            Game game = new Game();
            // Init game
            game.Init();

            // Load game
            game.Load();

            Vector3 radioPosition = Vector3.Zero;

            // Init task manager
            TaskManager.LoadTasks();
            TaskManager.Active = false;

            // Create list of zombies
            List<Zombie> zombies = new List<Zombie>()
            {
                new Zombie(new Vector3(-10, 0, 2), "cop", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(10, 0, 2), "cop2", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(2, 0, -10), "cop3", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(2, 0, -5), "cop4", AnimationCenter.ZombieAnimations[8])
            };
            List<string> _freeZombies = new List<string>();

            // Load UI Fonts
            Font damageFont = LoadFont("src/fonts/damage.ttf");
            Font chronoFont = LoadFont("src/fonts/Kanit-Bold.ttf");
            SetTextureFilter(chronoFont.Texture, TextureFilter.Trilinear);

            // Set Window state when loading is done
            SetWindowState(ConfigFlags.ResizableWindow);
            SetWindowState(ConfigFlags.MaximizedWindow);

            // Get window size
            ScreenWidth = GetScreenWidth();
            ScreenHeight = GetScreenHeight(); 
            ScreenSize = new Vector2(GetScreenWidth(), GetScreenHeight());

            // Scene render texture
            RenderTexture2D renderTexture = LoadRenderTexture(ScreenWidth, ScreenHeight);

            // Previous frame render texture
            RenderTexture2D prevTexture = LoadRenderTexture(ScreenWidth, ScreenHeight);

            // Inverse scene render texture rectangle
            Rectangle inverseSceneRectangle = new Rectangle(0, 0, ScreenWidth, -ScreenHeight);

            // Scene rectangle
            Rectangle sceneRectangle = new Rectangle(0, 0, ScreenWidth, ScreenHeight);

            // Set permanent informations of the screen according to the final screen size
            for (int i = 800; i >= 460; i -= 85)
            {
                // Add the 5 inventory cases displayed on the right side of the screen
                ScreenCenter.AddInfo(new TextureInfo(new Vector2(ScreenWidth - 120, ScreenHeight - i), game.Ressources.UITextures["inventory_case"], GetTime(), -1.0));
            }

            // Add current weapon splash
            ScreenCenter.AddInfo(new TextureInfo(new Vector2(ScreenWidth - 120, ScreenHeight - 800), game.Ressources.UITextures["rifle_gray_splash"], GetTime(), -1.0));
            ScreenCenter.AddInfo(new TextureInfo(new Vector2(ScreenWidth - 120, ScreenHeight - (800 - (game.Player.InventorySize) * 85)), game.Ressources.UITextures["rifle_green_splash"], GetTime(), -1.0));

            // Crosshair color variable
            Color crosshairColor = Color.White;

            // Set target FPS
			SetTargetFPS(60);
            // Start Menu
            Menu.Init();
            Menu.Show(game.Shaders.Skybox, game.Shaders, game.Terrain);
            // Get starting time
            GameStartTime = GetTime();
			DisableCursor();
            // Game Loop
            while (!WindowShouldClose())
            {
                // Update the camera
                if (!TaskManager.Active)
                {
                    UpdateCamera(ref game.Camera, ref game.CameraMotion, game.Player, ref radioPosition, zombies);
                }
                // Update the camera shake motion
                game.CameraMotion.ShakeStart = game.Camera.Position;
                game.Camera.Position = game.CameraMotion.Update(game.Camera.Position, game.Player.SPEED);
               
                Matrix4x4 lightView = new Matrix4x4();
                Matrix4x4 lightProj = new Matrix4x4();

                BeginTextureMode(game.ShadowMap.Map);
                ClearBackground(Color.White);
                BeginMode3D(game.ShadowMap.CameraView);
                lightView = Rlgl.GetMatrixModelview();
                lightProj = Rlgl.GetMatrixProjection();

                // Draw full scene to the shadow map render texture
                DrawScene();

                foreach (Zombie zombie in zombies)
                {
                    DrawModelEx(game.Ressources.Rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);
                }

                DrawMesh(game.Terrain.Mesh, game.Terrain.Material, game.Terrain.Transform);

                EndMode3D();
                EndTextureMode();

                Matrix4x4 lightViewProj = MatrixMultiply(lightView, lightProj);

                ClearBackground(Color.RayWhite);

                game.Shaders.UpdateLightMatrix(lightViewProj);

                game.Shaders.UpdateShadowMap(game.ShadowMap);

                SetShaderValue(
                    game.Shaders.LightingShader,
                    game.Shaders.LightingShader.Locs[(int)ShaderLocationIndex.VectorView],
                    game.Camera.Position,
                    ShaderUniformDataType.Vec3
                );

                // Update the current animation of the player
                switch (game.Player.WeaponState)
                {
                    case PlayerWeaponState.Idle:
                        game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[1];
                        break;
                    case PlayerWeaponState.Shooting:
                        game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[3];
                        // Check everytime a bullet is shot
                        if (game.Player.Frame == 1)
                        {
                            game.Player.CurrentWeapon.ShootBullet(new Vector3(game.Camera.Position.X, game.Camera.Position.Y - 0.045f, game.Camera.Position.Z) + GetCameraRight(ref game.Camera) / 12, GetCameraForward(ref game.Camera)); ;
                            // Play shooting sound
                            AudioCenter.PlaySound("rifleShoot");
                            // Set crosshair color
                            crosshairColor = Color.Red;
                            // Check collision with zombies
                            foreach (Zombie zombie in zombies)
                            {
                                if (zombie.Shoot(game.Player.CurrentWeapon.bullets.Last().Ray, game.Ressources.Rigged[zombie.Type].Meshes[0]))
                                {
                                    if (TaskManager.IsActive(1)) TaskManager.UpdateTask(1, 1);
                                    if (TaskManager.IsActive(3)) TaskManager.UpdateTask(3, 1);
                                    if (TaskManager.IsActive(5)) TaskManager.UpdateTask(5, 1);
                                    if (TaskManager.IsActive(7)) TaskManager.UpdateTask(7, 1);
                                }
                            }
                            // Check spawners
                            foreach (Spawner spawner in game.Spawners)
                            {
                                if (spawner.Shoot(game.Player.CurrentWeapon.bullets.Last().Ray, UnirayEngine.Ressource.GetModel("crystal").Meshes[0]))
                                {
                                    // Play randomly pitched hit sound
                                    AudioCenter.SetSoundVolume("crystal_hit", 12);
                                    AudioCenter.SetSoundPitch("crystal_hit", Clamp((float)(Random.Shared.NextDouble() * 3), 1.5f, 3f));
                                    AudioCenter.PlaySound("crystal_hit");
                                    if (zombies.Count < 4 && _freeZombies.Count != 0)
                                    {
                                        zombies.Add(spawner.CreateZombie(AnimationCenter.ZombieAnimations[8], game.Camera.Position, _freeZombies.First()));
                                        _freeZombies.RemoveAt(0);
                                    }
                                    if (spawner.Destroyed)
                                    {
                                        // Play destruction sound
                                        AudioCenter.SetSoundVolume("crystal_destroyed", 12);
                                        AudioCenter.PlaySound("crystal_destroyed");
                                        // Update task if active
                                        if (TaskManager.IsActive(2)) TaskManager.UpdateTask(2, 1);

                                        // Update other spawners index
                                        game.Ressources.StaticBoxes.RemoveAt(spawner.RessourceIndex);
                                        game.Spawners.Remove(spawner);
                                        // Switch ressource index with existing spawners
                                        game.Spawners.ForEach(x =>
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
                            
                            game.Player.CurrentWeapon.bullets.RemoveAt(0);
                        }
                        else if (game.Player.Frame > 7) crosshairColor = Color.White;
                        break;
                    case PlayerWeaponState.Reloading:
                        game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[2];
                        if (game.Player.Frame == game.Player.CurrentAnimation.FrameCount - 1) game.Player.WeaponState = PlayerWeaponState.Idle;
                        break;
                    case PlayerWeaponState.Taking:
                        game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[4];
                        if (game.Player.Frame == game.Player.CurrentAnimation.FrameCount - 1)
                        {
                            game.Player.WeaponState = PlayerWeaponState.Idle;
                        }
                        break;
                }

                // Update model animation
                UpdateModelAnimation(game.Ressources.Utilities[game.Player.CurrentWeapon.ModelID], game.Player.CurrentAnimation, game.Player.UpdateFrame());

                // Update the player event handler
                if (!TaskManager.Active) TickPlayer(game.Player, AnimationCenter.PlayerAnimations, ref crosshairColor, game.Shaders, ref game.CameraMotion);

                if (IsKeyPressed(KeyboardKey.Tab))
                {
                    if (!TaskManager.Active)
                    {
                        TaskManager.Active = true;
                        game.CameraMotion.Amplitude = 0;
                        EnableCursor();
                    }
                    else
                    {
                        TaskManager.Active = false;
                        DisableCursor();
                    }
                }

                // Render scene to texture
                BeginTextureMode(renderTexture);

                // Clear background every frame using white color
                ClearBackground(Color.Gray);

                // Begin 3D mode with the current scene's camera
                BeginMode3D(game.Camera);
#if DEBUG
                DrawSphereWires(game.ShadowMap.CameraView.Position, 2, 10, 10, Color.Red);
#endif
                // Draw the external skybox 
                Rlgl.DisableBackfaceCulling();
                Rlgl.DisableDepthMask();
                DrawMesh(game.Shaders.Skybox, game.Shaders.SkyboxMaterial, MatrixIdentity());
                Rlgl.EnableBackfaceCulling();
                Rlgl.EnableDepthMask();

                // Set terrain tiling to true
                game.Shaders.UpdateTiling(true);

                // Draw terrain
                DrawMesh(game.Terrain.Mesh, game.Terrain.Material, game.Terrain.Transform);

                // Set terrain tiling to false
                game.Shaders.UpdateTiling(false);

                // Check collisions between the player and the static objects
                // Add current position
                game.Player.Box.Min += game.Camera.Position;
                game.Player.Box.Max += game.Camera.Position;
                CheckCollisionPlayer(game.Player.Box, game.Ressources.StaticBoxes, game.Player, game.Camera);
#if DEBUG
                // Draw player's bounding box
                DrawBoundingBox(game.Player.Box, Color.Red);

                // Draw node map
                AStar.Grid.DrawNodeMap();
#endif

                // Draw the gameobjects of the environment (from Uniray)
                DrawScene();

                // Transform the player's current model
                Matrix4x4 mTransform = TransformPlayer(game.CameraMotion.Yaw, game.CameraMotion.Pitch, game.CameraMotion.SideShake);

                // Assign new rotation matrix to the model
                game.Ressources.Utilities[game.Player.CurrentWeapon.ModelID] = SetModelTransform(game.Ressources.Utilities[game.Player.CurrentWeapon.ModelID], mTransform);
                    
                // Draw player's current model
                DrawModel(game.Ressources.Utilities[game.Player.CurrentWeapon.ModelID], new Vector3(game.Camera.Position.X - GetCameraForward(ref game.Camera).X / 3, game.Camera.Position.Y - 0.2f, game.Camera.Position.Z - GetCameraForward(ref game.Camera).Z / 3), 3.5f, Color.White);

                // Draw and tick the current zombies of the scene
                int killIndex = -1;
                foreach (Zombie zombie in zombies)
                { 
                    // Draw zombie model
                    DrawModelEx(game.Ressources.Rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);

                    // Update the zombie model according to its state 
                    if (!TaskManager.Active) UpdateModelAnimation(game.Ressources.Rigged[zombie.Type], zombie.CurrentAnimation, zombie.UpdateFrame());
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
                                killIndex = zombies.IndexOf(zombie);
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
                                killIndex = zombies.IndexOf(zombie);
                                // Reste the frame of the animation
                                zombie.Frame = 0;
                            }
                            break;
                        case ZombieState.Attacking:
                            zombie.CurrentAnimation = AnimationCenter.ZombieAnimations[2];
                            zombie.UpdateFrame();
                            if (zombie.Frame == 90)
                            {
                                game.Player.Life -= 10;
                                TaskManager.UpdateTask(8, 10);
                                // Launch zombie kill animation
                                if (game.Player.Life <= 0)
                                {
                                    zombie.State = ZombieState.Killing;
                                    game.Camera.Position.Y -= 0.2f;
                                }
                            }
                            if (Math.Abs(Vector3Subtract(game.Camera.Position, zombie.Position).Length()) > 5)
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
                        if (AStar.Grid.GetWorldToNode(game.Camera.Position).Walkable)
                        {
                           AStar.FindPath(zombie.Position, game.Camera.Position, zombie.Path);
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
                            if (Math.Abs(Vector3Subtract(game.Camera.Position, zombie.Position).Length()) > 5)
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
                    Node currentNode = AStar.Grid.GetWorldToNode(zombies[killIndex].Position);
                    List<Node> neighbours = AStar.Grid.GetNeighbours(currentNode);
                    neighbours.ForEach(node =>
                    {
                        if (!node.HARD_NODE) node.Walkable = true;
                    });

                    _freeZombies.Add(zombies[killIndex].Type);
                    zombies.RemoveAt(killIndex);
                }

                // End 3D mode context
                EndMode3D();

                EndTextureMode();

                // Draw to the screen
                BeginDrawing();

                // Begin blur shader 
                BeginShaderMode(game.Shaders.MotionBlurShader);

                // Set previous frame render texture
                game.Shaders.SetBlurTexture(prevTexture);

                // Set current time
                game.Shaders.UpdateTime((float)GetTime());

                // Draw render texture to the screen
                DrawTexturePro(renderTexture.Texture, inverseSceneRectangle, sceneRectangle, Vector2.Zero, 0, Color.White);

                EndShaderMode();

                // Manage blood screen
                if (game.Player.BLOODY)
                {
                    DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(112, 1, 9, (int)Clamp((float)game.Player.Watch.Elapsed.TotalSeconds * 100, 0, 90)));
                    // Check if time's up
                    if (game.Player.Watch.Elapsed.TotalSeconds > 10)
                    {
                        game.Player.Watch.Restart();
                        game.Player.BLOODY = false;
                        AudioCenter.PlaySound("blood");
                        zombies.ForEach(x => 
                        {
                            if (x.State != ZombieState.Dying1 && x.State != ZombieState.Dying2) x.State = ZombieState.Running;
                        });
                    }
                }
                else if (!game.Player.BLOODY && game.Player.Watch.IsRunning)
                {
                    DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(112, 1, 9, (int)Clamp(90 / ((float)game.Player.Watch.Elapsed.TotalSeconds * 100), 0, 90)));
                }

                // Draw screen infos
                ScreenCenter.DrawScreenInfos();

                // Draw current inventory case
                DrawTexture(game.Ressources.UITextures["inventory_case_selected"], GetScreenWidth() - 121, GetScreenHeight() - (800 - (game.Player.InventoryIndex) * 85) - 1, Color.White);

                // Draw crosshair
                DrawTexture(game.Ressources.UITextures["crosshair"], GetScreenWidth() / 2 - game.Ressources.UITextures["crosshair"].Width / 2, GetScreenHeight() / 2 - game.Ressources.UITextures["crosshair"].Height / 2, crosshairColor);

                // Draw lifebar
                DrawRectangleGradientH(180, ScreenHeight - 140, (int)(2.4f * game.Player.Life), 15, Color.Lime, Color.Green);
                DrawTexture(game.Ressources.UITextures["lifebar"], 50, ScreenHeight - 250, Color.White);

                // Draw tasks manager
                TaskManager.Update();

                // Draw Gradient at the start
                if (GameStartTime + Cutscene.GRADIENT_TIME > GetTime()) Cutscene.Gradient(Color.Black);
#if DEBUG

                // Debug positions
                DrawText("Position: " + game.Camera.Position.ToString() +
                    "\nJump Force: " + game.Player.VJump +
                    "\nInventory Index: " + game.Player.InventoryIndex +
                    "\nWeapon Level: " + game.Player.CurrentWeapon.Level +
                    "\nCamera Target:" + game.Camera.Target.ToString() +
                    "\nMotion Constraint: " + game.Player.MotionConstraint.Value +
                    "\nConstraint: " + game.Player.MotionConstraint.Constraint
                    , 200, 200, 20, Color.Red);

                DrawFPS(0, 0);
#endif
                // End drawing context
                EndDrawing();

                // Draw to previous frame render texture
                BeginTextureMode(prevTexture);

                DrawTexturePro(renderTexture.Texture, inverseSceneRectangle, sceneRectangle, Vector2.Zero, 0, Color.White);

                EndTextureMode();

                // Reset the player's box
                game.Player.ResetBox();
            }
            CloseWindow();            // Unload all ressources that ARE NOT from Uniray
            foreach (KeyValuePair<string, Model> utilitesList in game.Ressources.Utilities)
            {
                for (int i = 0; i < utilitesList.Value.MaterialCount; i++)
                {
                    UnloadMaterial(utilitesList.Value.Materials[i]);
                }
                UnloadModel(utilitesList.Value);
            }
            foreach (KeyValuePair<string, Model> riggedObject in game.Ressources.Rigged)
            {
                UnloadModel(riggedObject.Value);
            }
            // Unload shaders
            game.Shaders.UnloadShaderCenter();
        }
        /// <summary>
        /// Update camera movement
        /// </summary>
        /// <param name="camera">The camera to update</param>
        /// <param name="cameraMotion">The camera motion additional variables</param>
        /// <param name="player">The player associated to the camera</param>
        static void UpdateCamera(ref Camera3D camera, ref CameraMotion cameraMotion, Player player, ref Vector3 radioPosition, List<Zombie> zombies)
        {
            // Calculate the camera rotation
            Vector2 mouse = GetMouseDelta();
            cameraMotion.Yaw -= mouse.X * 0.003f;
            cameraMotion.Pitch -= mouse.Y * 0.003f;

            cameraMotion.Pitch = Math.Clamp(cameraMotion.Pitch, -1.5f, 1.5f);

            // Calculate camera direction
            Vector3 direction;
            direction.X = (float)(Math.Cos(cameraMotion.Pitch) * Math.Sin(cameraMotion.Yaw));
            direction.Y = (float)Math.Sin(cameraMotion.Pitch);
            //direction.Y = 0;
            direction.Z = (float)(Math.Cos(cameraMotion.Pitch) * Math.Cos(cameraMotion.Yaw));

            // Calculate the camera movement
            Vector3 movement = Vector3.Zero;
            // Adjust camera position and target
            if (IsKeyDown(KeyboardKey.W)) 
            {
                //movement += GetCameraForward(ref camera);
                movement -= Vector3CrossProduct(GetCameraRight(ref camera), Vector3.UnitY);
            }
            if (IsKeyDown(KeyboardKey.S))
            {
                movement += Vector3CrossProduct(GetCameraRight(ref camera), Vector3.UnitY);
            }
            if (IsKeyDown(KeyboardKey.D))
            {
                movement += GetCameraRight(ref camera);
                if (cameraMotion.SideShake < 0.15f) cameraMotion.SideShake += 0.01f;
            }
            if (IsKeyDown(KeyboardKey.A))
            {
                movement -= GetCameraRight(ref camera);
                if (cameraMotion.SideShake > -0.15f) cameraMotion.SideShake -= 0.01f;
            }
            if (IsKeyUp(KeyboardKey.A) && IsKeyUp(KeyboardKey.D))
            {
                if (cameraMotion.SideShake < 0.0f) cameraMotion.SideShake += 0.01f;
                else if (cameraMotion.SideShake > 0.0f) cameraMotion.SideShake -= 0.01f;
            }
            if (IsKeyPressed(KeyboardKey.E))
            {
                // Secret radio
                foreach (UModel radio in CurrentScene.GameObjects.Where(x => x is UModel).Where(x => ((UModel)x).ModelID == "radio"))
                {
                    radioPosition = radio.Position;
                    if (Vector3Distance(radioPosition, camera.Position) <= 3){
                        AudioCenter.PlayMusic("lerob");
                        AudioCenter.SetMusicVolume("lerob", 8);
                        AudioCenter.PlaySound("radio");
                        AudioCenter.SetSoundVolume("radio", 10);
                        if (TaskManager.IsActive(4)) TaskManager.UpdateTask(4, 1);
                    }
                }
                // Phone booths
                foreach (UModel phone in CurrentScene.GameObjects.Where(x => x is UModel).Where(x => ((UModel)x).ModelID == "phone"))
                {
                    if (Vector3Distance(phone.Position, camera.Position) <= 5) 
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
                    if (Vector3Distance(body.Position, camera.Position) <= 5)
                    {
                        AudioCenter.PlaySound("blood");
                        player.BLOODY = true;
                        player.Watch.Restart();
                        foreach (Zombie zombie in zombies) 
                        {
                            zombie.State = ZombieState.Idle;
                            zombie.Angle = Random.Shared.Next(0, 360);
                        }
                        if (TaskManager.IsActive(6)) TaskManager.UpdateTask(6, 1);
                    }
                }
            }
            if (AudioCenter.IsMusicPlaying("lerob"))
            {
                AudioCenter.UpdateMusic("lerob");
                float volume = 20 - Vector3Distance(radioPosition, camera.Position);
                volume = Clamp(volume, 0f, 8f);
                AudioCenter.SetMusicVolume("lerob", volume);
            }

            // Final movement transformations
            if (Vector3Length(movement) > 0)
            {
                // Normalize vector
                movement = Vector3Normalize(movement) * player.SPEED * player.MotionConstraint.Value;
                // Block movement according to the motion constraint
                AddConstraintMovement(ref movement, player.MotionConstraint.Constraint);
                // Limit the movement to X and Z axis and normalize
                movement = new Vector3(movement.X, 0.0f, movement.Z);
                // Set camera shake
            }
            else cameraMotion.Amplitude = 0.0006f;

            // Increment movement
            camera.Position += movement;
            camera.Target = Vector3Add(camera.Position, direction);

            // Make the player jump if needed
            if (player.MoveState == PlayerMoveState.Jumping)
            {
                // Add jump force
                camera.Position.Y += player.VJump;
                camera.Target.Y += player.VJump;
                // Decrease jump force
                player.VJump -= 0.02f;
                if (camera.Position.Y <= 3)
                {
                    player.MoveState = PlayerMoveState.Running;
                    // Fix jump offset
                    camera.Position.Y = (float)Math.Round(camera.Position.Y, 3);
                }
            }
        }

        /// <summary>
        /// Tick the player events
        /// </summary>
        /// <param name="player">The player to check</param>
        static void TickPlayer(Player player, List<ModelAnimation> anims, ref Color crosshairColor, ShaderCenter shaders, ref CameraMotion cameraMotion)
        {
            // Manager input events
            // Reload event
            if (IsKeyPressed(KeyboardKey.R))
            {
                player.WeaponState = PlayerWeaponState.Reloading;
            }
            // Jump event
            if (IsKeyPressed(KeyboardKey.Space))
            {
                // Prevent cross-jumping
                if (player.MoveState != PlayerMoveState.Jumping)
                {
                    player.MoveState = PlayerMoveState.Jumping;
                    player.VJump = Player.JUMP_FORCE;
                }
            }

            // Sprinting event
            if (IsKeyDown(KeyboardKey.LeftShift))
            {
                player.SPEED = 0.2f;
                cameraMotion.Amplitude = 0.006f;
                AudioCenter.StopSound("walking");
                AudioCenter.PlaySoundLoop("running");
            }
            else if (!IsKeyDown(KeyboardKey.LeftShift))
            {
                player.SPEED = 0.1f;
                cameraMotion.Amplitude = 0.003f;
                AudioCenter.StopSound("running");
                AudioCenter.PlaySoundLoop("walking");
            }

            // Shooting event
            if (IsMouseButtonDown(MouseButton.Left))
            {
                player.WeaponState = PlayerWeaponState.Shooting;
            }
            // Stop shooting event
            else if (IsMouseButtonUp(MouseButton.Left) && player.WeaponState == PlayerWeaponState.Shooting) 
            {
                player.WeaponState = PlayerWeaponState.Idle;
                crosshairColor = Color.White;
                player.Frame = 0;

            }

            // Inventory changing event
            float wheel = GetMouseWheelMove();
            if (wheel == -1 && player.WeaponState != PlayerWeaponState.Taking)
            {
                // Manage inventory size
                if (player.InventoryIndex == player.InventorySize) player.SetCurrentWeapon(0);
                else player.SetCurrentWeapon(player.InventoryIndex + 1);

                // Set appropriate color of the new weapon
                SetShaderValue(shaders.OutlineShader, GetShaderLocation(shaders.OutlineShader, "highlightCol"), Weapon.Colors[player.CurrentWeapon.Level - 1], ShaderUniformDataType.Vec4);
                

                // Set player state
                player.WeaponState = PlayerWeaponState.Taking;
            }
            else if (wheel == 1 && player.WeaponState != PlayerWeaponState.Taking) 
            {
                // Manage inventory size
                if (player.InventoryIndex == 0) player.SetCurrentWeapon(player.InventorySize);
                else player.SetCurrentWeapon(player.InventoryIndex - 1);

                // Set appropriate color of the new weapon
                SetShaderValue(shaders.OutlineShader, GetShaderLocation(shaders.OutlineShader, "highlightCol"), Weapon.Colors[player.CurrentWeapon.Level - 1], ShaderUniformDataType.Vec4);
                

                // Set player state
                player.WeaponState = PlayerWeaponState.Taking;
            }
        }
        /// <summary>
        /// Check collisions between the player and the static objects of the map
        /// </summary>
        /// <param name="playerBox">Player Bounding Box</param>
        /// <param name="boxes">Static objects Bounding Boxes</param>
        static void CheckCollisionPlayer(BoundingBox playerBox, List<BoundingBox> boxes, Player player, Camera3D camera)
        {
            bool hit = false;
            // Loop over all the static objects
            foreach (BoundingBox staticBox in boxes)
            {
#if DEBUG
                DrawBoundingBox(staticBox, Color.Red);
#endif
                // Check individual collision
                if (CheckCollisionBoxes(playerBox, staticBox))
                {
                    Vector3 cf = GetCameraForward(ref camera);
                    // Calculate the motion constraint corresponding to the angle between target and box
                    player.CalculateMotionConstraint(new Vector3(cf.X, 0, cf.Z), staticBox, camera.Position);

                    hit = true;
                }
            }
            if (!hit) player.MotionConstraint = MotionConstraint.Default;
        }
        /// <summary>
        /// Set the working directory of Raylib
        /// </summary>
        /// <param name="directory">New directory</param>
        public static void SetWorkdir(string directory)
        {
            // Transform the sent string to a byte array
            byte[] array = Encoding.UTF8.GetBytes(directory);
            fixed (byte* p = array)
            {
                sbyte* sp = (sbyte*)p;
                ChangeDirectory(sp);
            }
        }
        /// <summary>
        /// Add constraint to the incoming movement
        /// </summary>
        /// <param name="movement">Incoming movement</param>
        /// <param name="constraint">Constraint to apply</param>
        static void AddConstraintMovement(ref Vector3 movement, Vector3 constraint)
        {
            if (constraint.Z == 1 && constraint.X == 0)
            {
                if (movement.X > 0)
                {
                    movement *= new Vector3(Math.Abs(constraint.X), 0, Math.Abs(constraint.Z));
                }
            }
            else if (constraint.Z == -1 && constraint.X == 0)
            {
                if (movement.X < 0)
                {
                    movement *= new Vector3(Math.Abs(constraint.X), 0, Math.Abs(constraint.Z));
                }
            }
            else if (constraint.X == 1 && constraint.Z == 0)
            {
                if (movement.Z > 0)
                {
                    movement *= new Vector3(Math.Abs(constraint.X), 0, Math.Abs(constraint.Z));
                }
            }
            else if (constraint.X == -1 && constraint.Z == 0)
            {
                if (movement.Z < 0)
                {
                    movement *= new Vector3(Math.Abs(constraint.X), 0, Math.Abs(constraint.Z));
                }
            }
        }
        /// <summary>
        /// Set a model's transform
        /// </summary>
        /// <param name="model">Model to modifiy</param>
        /// <param name="transform">Transform to set</param>
        /// <returns></returns>
        static Model SetModelTransform(Model model, Matrix4x4 transform)
        {
            Model copy = model;
            copy.Transform = transform;
            return copy;
        }
        /// <summary>
        /// Transform the player's model according to rotation and shake
        /// </summary>
        /// <param name="yaw">Camera yaw rotation</param>
        /// <param name="pitch">Camera pitch rotation</param>
        /// <param name="sideShake">Camera side shake</param>
        /// <returns></returns>
        static Matrix4x4 TransformPlayer(float yaw, float pitch, float sideShake)
        {
            Matrix4x4 rotationYaw = MatrixRotateY(yaw);
            Matrix4x4 rotationPitch = MatrixRotateX(-pitch);
            Matrix4x4 rotationRoll = MatrixRotateZ(sideShake);
            Matrix4x4 weaponRotation = MatrixMultiply(rotationPitch, rotationYaw);
            // Return final transform
            return MatrixMultiply(rotationRoll, weaponRotation);
        }
        /// <summary>
        /// Get the rotate vector (XZ) of a normalized point from a model bone position
        /// </summary>
        /// <param name="normalizedPos"></param>
        /// <param name="alpha"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3 RotateNormalizedBone(Vector3 normalizedPos, float alpha, Vector3 pos)
        {
            // Create the new vector according to the passed parameters 
            Vector3 spacePos = new Vector3(
                normalizedPos.X * (float)Math.Cos(alpha / RAD2DEG) + normalizedPos.Z * (float)Math.Sin(alpha / RAD2DEG), normalizedPos.Y,
                -normalizedPos.X * (float)Math.Sin(alpha / RAD2DEG) + normalizedPos.Z * (float)Math.Cos(alpha / RAD2DEG)) * 3.5f + pos;
            // Return the newly calculated position
            return spacePos;
        }
    }
}