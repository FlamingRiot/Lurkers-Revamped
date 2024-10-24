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
        // Screen global attributes and objects
        public static RenderTexture2D RenderTexture;
        public static RenderTexture2D PreviousRenderTexture;
        public static Rectangle ScreenRectangle;
        public static Rectangle ScreenInverseRectangle;
        public  static void Main(string[] args)
        {
            // Init splash window
            InitWindow(200, 200, "Lurkers: Revamped");
            SetWindowState(ConfigFlags.UndecoratedWindow);

            // Init game
            Game.Init();

            // Load game
            Game.Load();

            Vector3 radioPosition = Vector3.Zero;

            // Create list of zombies
            List<Zombie> zombies = new List<Zombie>()
            {
                new Zombie(new Vector3(-10, 0, 2), "cop", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(10, 0, 2), "cop2", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(2, 0, -10), "cop3", AnimationCenter.ZombieAnimations[8]),
                new Zombie(new Vector3(2, 0, -5), "cop4", AnimationCenter.ZombieAnimations[8])
            };
            List<string> _freeZombies = new List<string>();

            // Crosshair color variable
            Color crosshairColor = Color.White;

            Game.Start();

            // Get starting time
            GameStartTime = GetTime();
			DisableCursor();
            // Game Loop
            while (!WindowShouldClose())
            {
                // Update the camera
                if (!TaskManager.Active)
                {
                    UpdateCamera(ref Game.Camera, ref Game.CameraMotion, Game.Player, ref radioPosition, zombies);
                }
                // Update the camera shake motion
                Game.CameraMotion.ShakeStart = Game.Camera.Position;
                Game.Camera.Position = Game.CameraMotion.Update(Game.Camera.Position, Game.Player.SPEED);
               
                Matrix4x4 lightView = new Matrix4x4();
                Matrix4x4 lightProj = new Matrix4x4();

                BeginTextureMode(Game.ShadowMap.Map);
                ClearBackground(Color.White);
                BeginMode3D(Game.ShadowMap.CameraView);
                lightView = Rlgl.GetMatrixModelview();
                lightProj = Rlgl.GetMatrixProjection();

                // Draw full scene to the shadow map render texture
                DrawScene();

                foreach (Zombie zombie in zombies)
                {
                    DrawModelEx(Game.Ressources.Rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);
                }

                DrawMesh(Game.Terrain.Mesh, Game.Terrain.Material, Game.Terrain.Transform);

                EndMode3D();
                EndTextureMode();

                Matrix4x4 lightViewProj = MatrixMultiply(lightView, lightProj);

                ClearBackground(Color.RayWhite);

                Game.Shaders.UpdateLightMatrix(lightViewProj);

                Game.Shaders.UpdateShadowMap(Game.ShadowMap);

                SetShaderValue(
                    Game.Shaders.LightingShader,
                    Game.Shaders.LightingShader.Locs[(int)ShaderLocationIndex.VectorView],
                    Game.Camera.Position,
                    ShaderUniformDataType.Vec3
                );

                // Update the current animation of the player
                switch (Game.Player.WeaponState)
                {
                    case PlayerWeaponState.Idle:
                        Game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[1];
                        break;
                    case PlayerWeaponState.Shooting:
                        Game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[3];
                        // Check everytime a bullet is shot
                        if (Game.Player.Frame == 1)
                        {
                            Game.Player.CurrentWeapon.ShootBullet(new Vector3(Game.Camera.Position.X, Game.Camera.Position.Y - 0.045f, Game.Camera.Position.Z) + GetCameraRight(ref Game.Camera) / 12, GetCameraForward(ref Game.Camera)); ;
                            // Play shooting sound
                            AudioCenter.PlaySound("rifleShoot");
                            // Set crosshair color
                            crosshairColor = Color.Red;
                            // Check collision with zombies
                            foreach (Zombie zombie in zombies)
                            {
                                if (zombie.Shoot(Game.Player.CurrentWeapon.bullets.Last().Ray, Game.Ressources.Rigged[zombie.Type].Meshes[0]))
                                {
                                    if (TaskManager.IsActive(1)) TaskManager.UpdateTask(1, 1);
                                    if (TaskManager.IsActive(3)) TaskManager.UpdateTask(3, 1);
                                    if (TaskManager.IsActive(5)) TaskManager.UpdateTask(5, 1);
                                    if (TaskManager.IsActive(7)) TaskManager.UpdateTask(7, 1);
                                }
                            }
                            // Check spawners
                            foreach (Spawner spawner in Game.Spawners)
                            {
                                if (spawner.Shoot(Game.Player.CurrentWeapon.bullets.Last().Ray, UnirayEngine.Ressource.GetModel("crystal").Meshes[0]))
                                {
                                    // Play randomly pitched hit sound
                                    AudioCenter.SetSoundVolume("crystal_hit", 12);
                                    AudioCenter.SetSoundPitch("crystal_hit", Clamp((float)(Random.Shared.NextDouble() * 3), 1.5f, 3f));
                                    AudioCenter.PlaySound("crystal_hit");
                                    if (zombies.Count < 4 && _freeZombies.Count != 0)
                                    {
                                        zombies.Add(spawner.CreateZombie(AnimationCenter.ZombieAnimations[8], Game.Camera.Position, _freeZombies.First()));
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
                                        Game.Ressources.StaticBoxes.RemoveAt(spawner.RessourceIndex);
                                        Game.Spawners.Remove(spawner);
                                        // Switch ressource index with existing spawners
                                        Game.Spawners.ForEach(x =>
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
                            
                            Game.Player.CurrentWeapon.bullets.RemoveAt(0);
                        }
                        else if (Game.Player.Frame > 7) crosshairColor = Color.White;
                        break;
                    case PlayerWeaponState.Reloading:
                        Game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[2];
                        if (Game.Player.Frame == Game.Player.CurrentAnimation.FrameCount - 1) Game.Player.WeaponState = PlayerWeaponState.Idle;
                        break;
                    case PlayerWeaponState.Taking:
                        Game.Player.CurrentAnimation = AnimationCenter.PlayerAnimations[4];
                        if (Game.Player.Frame == Game.Player.CurrentAnimation.FrameCount - 1)
                        {
                            Game.Player.WeaponState = PlayerWeaponState.Idle;
                        }
                        break;
                }

                // Update model animation
                UpdateModelAnimation(Game.Ressources.Utilities[Game.Player.CurrentWeapon.ModelID], Game.Player.CurrentAnimation, Game.Player.UpdateFrame());

                // Update the player event handler
                if (!TaskManager.Active) TickPlayer(Game.Player, AnimationCenter.PlayerAnimations, ref crosshairColor, Game.Shaders, ref Game.CameraMotion);

                if (IsKeyPressed(KeyboardKey.Tab))
                {
                    if (!TaskManager.Active)
                    {
                        TaskManager.Active = true;
                        Game.CameraMotion.Amplitude = 0;
                        EnableCursor();
                    }
                    else
                    {
                        TaskManager.Active = false;
                        DisableCursor();
                    }
                }

                // Render scene to texture
                BeginTextureMode(RenderTexture);

                // Clear background every frame using white color
                ClearBackground(Color.Gray);

                // Begin 3D mode with the current scene's camera
                BeginMode3D(Game.Camera);
#if DEBUG
                DrawSphereWires(Game.ShadowMap.CameraView.Position, 2, 10, 10, Color.Red);
#endif
                // Draw the external skybox 
                Rlgl.DisableBackfaceCulling();
                Rlgl.DisableDepthMask();
                DrawMesh(Game.Shaders.Skybox, Game.Shaders.SkyboxMaterial, MatrixIdentity());
                Rlgl.EnableBackfaceCulling();
                Rlgl.EnableDepthMask();

                // Set terrain tiling to true
                Game.Shaders.UpdateTiling(true);

                // Draw terrain
                DrawMesh(Game.Terrain.Mesh, Game.Terrain.Material, Game.Terrain.Transform);

                // Set terrain tiling to false
                Game.Shaders.UpdateTiling(false);

                // Check collisions between the player and the static objects
                // Add current position
                Game.Player.Box.Min += Game.Camera.Position;
                Game.Player.Box.Max += Game.Camera.Position;
                CheckCollisionPlayer(Game.Player.Box, Game.Ressources.StaticBoxes, Game.Player, Game.Camera);
#if DEBUG
                // Draw player's bounding box
                DrawBoundingBox(Game.Player.Box, Color.Red);

                // Draw node map
                AStar.Grid.DrawNodeMap();
#endif

                // Draw the gameobjects of the environment (from Uniray)
                DrawScene();

                // Transform the player's current model
                Matrix4x4 mTransform = TransformPlayer(Game.CameraMotion.Yaw, Game.CameraMotion.Pitch, Game.CameraMotion.SideShake);

                // Assign new rotation matrix to the model
                Game.Ressources.Utilities[Game.Player.CurrentWeapon.ModelID] = SetModelTransform(Game.Ressources.Utilities[Game.Player.CurrentWeapon.ModelID], mTransform);
                    
                // Draw player's current model
                DrawModel(Game.Ressources.Utilities[Game.Player.CurrentWeapon.ModelID], new Vector3(Game.Camera.Position.X - GetCameraForward(ref Game.Camera).X / 3, Game.Camera.Position.Y - 0.2f, Game.Camera.Position.Z - GetCameraForward(ref Game.Camera).Z / 3), 3.5f, Color.White);

                // Draw and tick the current zombies of the scene
                int killIndex = -1;
                foreach (Zombie zombie in zombies)
                { 
                    // Draw zombie model
                    DrawModelEx(Game.Ressources.Rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);

                    // Update the zombie model according to its state 
                    if (!TaskManager.Active) UpdateModelAnimation(Game.Ressources.Rigged[zombie.Type], zombie.CurrentAnimation, zombie.UpdateFrame());
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
                                Game.Player.Life -= 10;
                                TaskManager.UpdateTask(8, 10);
                                // Launch zombie kill animation
                                if (Game.Player.Life <= 0)
                                {
                                    zombie.State = ZombieState.Killing;
                                    Game.Camera.Position.Y -= 0.2f;
                                }
                            }
                            if (Math.Abs(Vector3Subtract(Game.Camera.Position, zombie.Position).Length()) > 5)
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
                        if (AStar.Grid.GetWorldToNode(Game.Camera.Position).Walkable)
                        {
                           AStar.FindPath(zombie.Position, Game.Camera.Position, zombie.Path);
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
                            if (Math.Abs(Vector3Subtract(Game.Camera.Position, zombie.Position).Length()) > 5)
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
                BeginShaderMode(Game.Shaders.MotionBlurShader);

                // Set previous frame render texture
                Game.Shaders.SetBlurTexture(PreviousRenderTexture);

                // Set current time
                Game.Shaders.UpdateTime((float)GetTime());

                // Draw render texture to the screen
                DrawTexturePro(RenderTexture.Texture, ScreenInverseRectangle, ScreenRectangle, Vector2.Zero, 0, Color.White);

                EndShaderMode();

                // Manage blood screen
                if (Game.Player.BLOODY)
                {
                    DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(112, 1, 9, (int)Clamp((float)Game.Player.Watch.Elapsed.TotalSeconds * 100, 0, 90)));
                    // Check if time's up
                    if (Game.Player.Watch.Elapsed.TotalSeconds > 10)
                    {
                        Game.Player.Watch.Restart();
                        Game.Player.BLOODY = false;
                        AudioCenter.PlaySound("blood");
                        zombies.ForEach(x => 
                        {
                            if (x.State != ZombieState.Dying1 && x.State != ZombieState.Dying2) x.State = ZombieState.Running;
                        });
                    }
                }
                else if (!Game.Player.BLOODY && Game.Player.Watch.IsRunning)
                {
                    DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(112, 1, 9, (int)Clamp(90 / ((float)Game.Player.Watch.Elapsed.TotalSeconds * 100), 0, 90)));
                }

                // Draw screen infos
                ScreenCenter.DrawScreenInfos();

                // Draw current inventory case
                DrawTexture(Game.Ressources.UITextures["inventory_case_selected"], GetScreenWidth() - 121, GetScreenHeight() - (800 - (Game.Player.InventoryIndex) * 85) - 1, Color.White);

                // Draw crosshair
                DrawTexture(Game.Ressources.UITextures["crosshair"], GetScreenWidth() / 2 - Game.Ressources.UITextures["crosshair"].Width / 2, GetScreenHeight() / 2 - Game.Ressources.UITextures["crosshair"].Height / 2, crosshairColor);

                // Draw lifebar
                DrawRectangleGradientH(180, ScreenHeight - 140, (int)(2.4f * Game.Player.Life), 15, Color.Lime, Color.Green);
                DrawTexture(Game.Ressources.UITextures["lifebar"], 50, ScreenHeight - 250, Color.White);

                // Draw tasks manager
                TaskManager.Update();

                // Draw Gradient at the start
                if (GameStartTime + Cutscene.GRADIENT_TIME > GetTime()) Cutscene.Gradient(Color.Black);
#if DEBUG

                // Debug positions
                DrawText("Position: " + Game.Camera.Position.ToString() +
                    "\nJump Force: " + Game.Player.VJump +
                    "\nInventory Index: " + Game.Player.InventoryIndex +
                    "\nWeapon Level: " + Game.Player.CurrentWeapon.Level +
                    "\nCamera Target:" + Game.Camera.Target.ToString() +
                    "\nMotion Constraint: " + Game.Player.MotionConstraint.Value +
                    "\nConstraint: " + Game.Player.MotionConstraint.Constraint
                    , 200, 200, 20, Color.Red);

                DrawFPS(0, 0);
#endif
                // End drawing context
                EndDrawing();

                // Draw to previous frame render texture
                BeginTextureMode(PreviousRenderTexture);

                DrawTexturePro(RenderTexture.Texture, ScreenInverseRectangle, ScreenRectangle, Vector2.Zero, 0, Color.White);

                EndTextureMode();

                // Reset the player's box
                Game.Player.ResetBox();
            }
            CloseWindow();            // Unload all ressources that ARE NOT from Uniray
            foreach (KeyValuePair<string, Model> utilitesList in Game.Ressources.Utilities)
            {
                for (int i = 0; i < utilitesList.Value.MaterialCount; i++)
                {
                    UnloadMaterial(utilitesList.Value.Materials[i]);
                }
                UnloadModel(utilitesList.Value);
            }
            foreach (KeyValuePair<string, Model> riggedObject in Game.Ressources.Rigged)
            {
                UnloadModel(riggedObject.Value);
            }
            // Unload shaders
            Game.Shaders.UnloadShaderCenter();

            Game.Close();
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