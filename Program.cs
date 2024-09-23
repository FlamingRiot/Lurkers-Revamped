﻿using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using System.Numerics;
using static UnirayEngine.UnirayEngine;
using uniray_Project;
using System.Text;
using UnirayEngine;
using System.Xml.Linq;

namespace Lurkers_revamped
{
    public unsafe class Program
    {
        public  static void Main(string[] args)
        {
            // Init splash window
            InitWindow(200, 200, "Lurkers: Revamped");
            SetWindowState(ConfigFlags.UndecoratedWindow);

            // Load and draw splash
            Texture2D splash = LoadTexture("src/textures/splash.png");
            BeginDrawing();
            DrawTexture(splash, 0, 0, Color.White);
            EndDrawing();

            // Init the Uniray engine background code 
            InitEngine();

            // Init Audio Center
            AudioCenter audio = new AudioCenter();

            // Init screen center
            ScreenCenter screen = new ScreenCenter();

            // Init shader center
            ShaderCenter shaders = new ShaderCenter();

            // Define 3D camera
            Camera3D camera = new Camera3D();
            camera.Position = new Vector3(9.0f, 3.0f, -15.0f);
            camera.Target = new Vector3(3.0f, 3.0f, 0.0f);
            camera.Up = Vector3.UnitY;
            camera.Projection = CameraProjection.Perspective;
            camera.FovY = 60f;
            // Additional camera motion data
            CameraMotion cameraMotion = new CameraMotion();

            // Change the current directory so the embedded materials from the models can be loaded successfully
            sbyte* dir = GetApplicationDirectory();
            string workdir = new string(dir);
            string newDir = workdir + "src\\textures\\materials\\";
            SetWorkdir(newDir);

            // Load dictionary of utilities (weapons, meds, etc.)
            Dictionary<string, Model> utilities = RLoading.LoadUtilities();

            // Load rigged models
            Dictionary<string, Model> rigged = RLoading.LoadRigged();

            // Load UI textures and set permanent Screen Infos
            Dictionary<string, Texture2D> UITextures = RLoading.LoadUITextures();

            // Load static objects' boundind box
            List<BoundingBox> staticBoxes = RLoading.LoadStaticBoxes(CurrentScene.GameObjects);

            // Set the working directory back to its original value 
            SetWorkdir(workdir);

            // Load terrain
            Terrain terrain = RLoading.GenTerrain();
            terrain.SetShader(shaders.TilingShader);

            // Load skybox
            Mesh skybox = RLoading.GenSkybox(shaders);

            // Load shadow map
            ShadowMap shadowMap = new ShadowMap(LoadShadowMapRenderTexture(4096, 4096), new Vector3(-50.0f, 25.0f, -50.0f));

            // Load lighting and init shader
            shaders.LoadLighting(shadowMap.CameraView.Target, new Color(70, 25, 0, 255));

            // Assign shader to every object of the scene
            foreach (UModel go in CurrentScene.GameObjects.Where(x => x is UModel))
            {
                go.SetShader(shaders.LightingShader);
            }

            terrain.Material.Shader = shaders.LightingShader;

            for (int i = 0; i < utilities["rifle"].MaterialCount; i++)
            {
                utilities["rifle"].Materials[i].Shader = shaders.LightingShader;    
            }

            for (int i = 0; i < rigged["cop"].MaterialCount; i++)
            {
                rigged["cop"].Materials[i].Shader = shaders.LightingShader;
            }

            // Load animation lists
            List<Animation> rifleAnims = RLoading.LoadAnimationList("src/animations/rifle.m3d");
            List<Animation> zombieAnims = RLoading.LoadAnimationList("src/animations/walker.m3d");

            // Create player and its object dependancies
            Player player = new Player("Anonymous254", new Weapon("Lambert Niv. 1", "rifle", 50, 1), rifleAnims[1]);
            // (Debug) Add a second weapon to the inventory of the player
            player.AddWeapon(new Weapon("Lambert Niv. 2", "rifle", 50, 2));

            // Create list of zombies
            List<Zombie> zombies = new List<Zombie>()
            {
                new Zombie(new Vector3(-10, 0, 2), "cop", zombieAnims[8])
            };

            // Load UI Fonts
            Font damageFont = LoadFont("src/fonts/damage.ttf");

            // Set Window state when loading is done
            SetWindowState(ConfigFlags.ResizableWindow);
            SetWindowState(ConfigFlags.MaximizedWindow);

            // Scene render texture
            RenderTexture2D renderTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());

            // Previous frame render texture
            RenderTexture2D prevTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());

            // Inverse scene render texture rectangle
            Rectangle inverseSceneRectangle = new Rectangle(0, 0, GetScreenWidth(), -GetScreenHeight());

            // Scene rectangle
            Rectangle sceneRectangle = new Rectangle(0, 0, GetScreenWidth(), GetScreenHeight());

            // Set permanent informations of the screen according to the final screen size
            for (int i = 800; i >= 460; i -= 85)
            {
                // Add the 5 inventory cases displayed on the right side of the screen
                screen.AddInfo(new TextureInfo(new Vector2(GetScreenWidth() - 120, GetScreenHeight() - i), UITextures["inventory_case"], GetTime(), -1.0));
            }

            // Add current weapon splash
            screen.AddInfo(new TextureInfo(new Vector2(GetScreenWidth() - 120, GetScreenHeight() - 800), UITextures["rifle_gray_splash"], GetTime(), -1.0));
            screen.AddInfo(new TextureInfo(new Vector2(GetScreenWidth() - 120, GetScreenHeight() - (800 - (player.InventorySize) * 85)), UITextures["rifle_green_splash"], GetTime(), -1.0));

            // Crosshair color variable
            Color crosshairColor = Color.White;

            audio.PlayMusic("ambience");
            audio.SetMusicVolume("ambience", 1);

            // Set target FPS
            SetTargetFPS(60);
            DisableCursor();
            // Game Loop
            while (!WindowShouldClose())
            {
                // Update music stream
                audio.UpdateMusic("ambience");

                // Update the camera
                UpdateCamera(ref camera, ref cameraMotion, player);
               
                Matrix4x4 lightView = new Matrix4x4();
                Matrix4x4 lightProj = new Matrix4x4();

                BeginTextureMode(shadowMap.Map);
                ClearBackground(Color.White);
                BeginMode3D(shadowMap.CameraView);
                lightView = Rlgl.GetMatrixModelview();
                lightProj = Rlgl.GetMatrixProjection();

                // Draw full scene to the shadow map render texture
                DrawScene();

                foreach (Zombie zombie in zombies)
                {
                    DrawModelEx(rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);
                }

                DrawMesh(terrain.Mesh, terrain.Material, terrain.Transform);

                EndMode3D();
                EndTextureMode();

                Matrix4x4 lightViewProj = MatrixMultiply(lightView, lightProj);

                ClearBackground(Color.RayWhite);

                shaders.UpdateLightMatrix(lightViewProj);

                shaders.UpdateShadowMap(shadowMap);

                SetShaderValue(
                    shaders.LightingShader,
                    shaders.LightingShader.Locs[(int)ShaderLocationIndex.VectorView],
                    camera.Position,
                    ShaderUniformDataType.Vec3
                );

                // Update the current animation of the player
                switch (player.WeaponState)
                {
                    case PlayerWeaponState.Idle:
                        player.CurrentAnimation = rifleAnims[1];
                        break;
                    case PlayerWeaponState.Shooting:
                        player.CurrentAnimation = rifleAnims[3];
                        // Check everytime a bullet is shot
                        if (rifleAnims[3].Frame == 1)
                        {
                            player.CurrentWeapon.ShootBullet(new Vector3(camera.Position.X, camera.Position.Y - 0.045f, camera.Position.Z) + GetCameraRight(ref camera) / 12, GetCameraForward(ref camera)); ;
                            // Play shooting sound
                            audio.PlaySound("rifleShoot");
                            // Set crosshair color
                            crosshairColor = Color.Red;
                            // Check collision with zombies
                            foreach (Zombie zombie in zombies)
                            {
                                // Calculate the position of the bone according to the rotation and scale of the model
                                Vector3 bonePos = RotateNormalizedBone(zombie.CurrentAnimation.Anim.FramePoses[zombie.CurrentAnimation.Frame][5].Translation, zombie.Angle, zombie.Position);
                                // Check collision between bullet and the zombie's head bone
                                player.CurrentWeapon.bullets.Last().Collision = GetRayCollisionSphere(player.CurrentWeapon.bullets.Last().Ray, bonePos, 0.4f);
                                // Check collsion details
                                if (player.CurrentWeapon.bullets.Last().Collision.Hit)
                                {

                                    Random r = new Random();
                                    // Play headshot sounds
                                    audio.PlaySound("headshot");
                                    audio.StopSound("zombie_default");
                                    audio.PlaySound("zombie_kill");
                                    // Add screen info for the headshot
                                    Vector2 pos = new Vector2(GetScreenWidth() - UITextures["headshot"].Width / 2 - 170, GetScreenHeight() - 285);
                                    screen.AddInfo(new TextureInfo(pos, UITextures["headshot"], GetTime(), 1f));
                                    screen.AddInfo(new TextureInfo(new Vector2(pos.X + UITextures["headshot"].Width - 100, pos.Y + UITextures["headshot"].Height - 100), UITextures["plus_coin"], GetTime(), 1f));
                                    // Start death animation (random)
                                    if (r.Next(0, 2) == 1) zombie.State = ZombieState.Dying1;
                                    else zombie.State = ZombieState.Dying2;
                                    // Reset the collision variable
                                    player.CurrentWeapon.bullets.Last().ResetCollision();
                                }
                            }
                            
                            player.CurrentWeapon.bullets.RemoveAt(0);
                        }
                        else if (rifleAnims[3].Frame > 7) crosshairColor = Color.White;
                        break;
                    case PlayerWeaponState.Reloading:
                        player.CurrentAnimation = rifleAnims[2];
                        if (rifleAnims[2].Frame == rifleAnims[2].FrameCount() - 1) player.WeaponState = PlayerWeaponState.Idle;
                        break;
                    case PlayerWeaponState.Taking:
                        player.CurrentAnimation = rifleAnims[4];
                        if (player.CurrentAnimation.Frame == player.CurrentAnimation.FrameCount() - 1)
                        {
                            player.WeaponState = PlayerWeaponState.Idle;
                        }
                        break;
                }

                // Update model animation
                UpdateModelAnimation(utilities[player.CurrentWeapon.ModelID], player.CurrentAnimation.Anim, player.CurrentAnimation.UpdateFrame());

                // Update the player event handler
                TickPlayer(player, rifleAnims, ref crosshairColor, shaders);

                // Update the screen center (info displayer)
                screen.Tick();

                // Render scene to texture
                BeginTextureMode(renderTexture);

                // Clear background every frame using white color
                ClearBackground(Color.Gray);

                // Begin 3D mode with the current scene's camera
                BeginMode3D(camera);
#if DEBUG
                DrawSphereWires(shadowMap.CameraView.Position, 2, 10, 10, Color.Red);
#endif
                // Draw the external skybox 
                Rlgl.DisableBackfaceCulling();
                Rlgl.DisableDepthMask();
                DrawMesh(skybox, shaders.SkyboxMaterial, MatrixIdentity());
                Rlgl.EnableBackfaceCulling();
                Rlgl.EnableDepthMask();

                // Set terrain tiling to true
                shaders.UpdateTiling(true);

                // Draw terrain
                DrawMesh(terrain.Mesh, terrain.Material, terrain.Transform);

                // Set terrain tiling to false
                shaders.UpdateTiling(false);

                // Check collisions between the player and the static objects
                // Add current position
                player.MinBox += camera.Position;
                player.MaxBox += camera.Position;
                CheckCollisionPlayer(player.Box, staticBoxes, player, camera);
#if DEBUG
                // Draw player's bounding box
                DrawBoundingBox(player.Box, Color.Red);
#endif
                // Draw the gameobjects of the environment (from Uniray)
                DrawScene();

                // Transform the player's current model
                Matrix4x4 mTransform = TransformPlayer(cameraMotion.Yaw, cameraMotion.Pitch, cameraMotion.SideShake);

                // Assign new rotation matrix to the model
                utilities[player.CurrentWeapon.ModelID] = SetModelTransform(utilities[player.CurrentWeapon.ModelID], mTransform);
                    
                // Draw player's current model
                DrawModel(utilities[player.CurrentWeapon.ModelID], new Vector3(camera.Position.X - GetCameraForward(ref camera).X / 3, camera.Position.Y - 0.2f, camera.Position.Z - GetCameraForward(ref camera).Z / 3), 3.5f, Color.White);

                // Draw and tick the current zombies of the scene
                int killIndex = -1;
                foreach (Zombie zombie in zombies)
                { 
                    // Draw zombie model
                    DrawModelEx(rigged[zombie.Type], zombie.Position, Vector3.UnitY, zombie.Angle, new Vector3(3.5f), Color.White);

                    // Update the zombie model according to its state 
                    UpdateModelAnimation(rigged[zombie.Type], zombie.CurrentAnimation.Anim, zombie.CurrentAnimation.UpdateFrame());
                    switch (zombie.State)
                    {
                        case ZombieState.Running:
                            zombie.CurrentAnimation = zombieAnims[8];
                            break;
                        case ZombieState.Dying1:
                            zombie.CurrentAnimation = zombieAnims[5];
                            // Check if the zombie is done dying
                            if (zombie.CurrentAnimation.Frame == zombie.CurrentAnimation.FrameCount() - 1)
                            {
                                // Remove the zombie from the list
                                killIndex = zombies.IndexOf(zombie);
                                // Reset the frame of the animation
                                zombie.CurrentAnimation.Frame = 0;
                            }
                            break;
                        case ZombieState.Dying2:
                            zombie.CurrentAnimation = zombieAnims[4];
                            // Check if the zombie is done dying
                            if (zombie.CurrentAnimation.Frame == zombie.CurrentAnimation.FrameCount() - 1)
                            {
                                // Remove the zombie from the list
                                killIndex = zombies.IndexOf(zombie);
                                // Reste the frame of the animation
                                zombie.CurrentAnimation.Frame = 0;
                            }
                            break;
                    }
#if DEBUG
                    // Debug bone drawing
                    Vector3 posA = RotateNormalizedBone(zombie.CurrentAnimation.Anim.FramePoses[zombie.CurrentAnimation.Frame][5].Translation, zombie.Angle, zombie.Position);
#endif
                    // Move and rotate the zombie if running
                    if (zombie.State == ZombieState.Running)
                    {
                        // Define origin vector of the rotation
                        Vector3 origin = Vector3.UnitZ;
                        // Define direction of the rotation
                        Vector3 direction = Vector3Normalize(Vector3Subtract(camera.Position, zombie.Position)) * 0.1f;

                        // Calculate cosine of the angle
                        float angle = (Vector3DotProduct(origin, direction)) / (Vector3Length(origin) * Vector3Length(direction));
                        // Calculate the angle from the cosine
                        float alpha = (float)Math.Acos(angle) * RAD2DEG;

                        // Reverse angle if needed
                        if (camera.Position.X < zombie.Position.X)
                        {
                            alpha = -alpha;
                        }

                        // Set the rotation of the zombie
                        zombie.Angle = alpha;

                        // Move the zombie along its direction axis
                        zombie.Position += new Vector3(direction.X, 0, direction.Z);

                        // Play zombie default running sound
                        audio.PlaySoundLoop("zombie_default");
                    }
                }
                // Remove the zombie from the list if killed
                if (killIndex != -1)
                {
                    zombies.RemoveAt(killIndex);

                    // Spawn a new zombie (debug sandbox only)
                    Random r = new Random();

                    Zombie zombzomb = new Zombie(new Vector3(r.Next(-50, 50), 0, r.Next(-50, 50)), "cop", zombieAnims[8]);
                    zombies.Add(zombzomb);
                }

                // End 3D mode context
                EndMode3D();

                EndTextureMode();

                // Draw to the screen
                BeginDrawing();

                // Begin blur shader 
                BeginShaderMode(shaders.MotionBlurShader);

                // Set previous frame render texture
                shaders.SetBlurTexture(prevTexture);

                // Draw render texture to the screen
                DrawTexturePro(renderTexture.Texture, inverseSceneRectangle, sceneRectangle, Vector2.Zero, 0, Color.White);

                EndShaderMode();

                // Draw screen infos
                screen.DrawScreenInfos();
#if DEBUG

                // Debug positions
                DrawText("Position: " + camera.Position.ToString() +
                    "\nJump Force: " + player.VJump +
                    "\nFrame: " + zombies.First().CurrentAnimation.Frame +
                    "\nInventory Index: " + player.InventoryIndex +
                    "\nWeapon Level: " + player.CurrentWeapon.Level +
                    "\nCamera Target:" + camera.Target.ToString() +
                    "\nMotion Constraint: " + player.MotionConstraint.Value +
                    "\nConstraint: " + player.MotionConstraint.Constraint
                    , 200, 200, 20, Color.Red);

                DrawFPS(0, 0);
#endif

                // Draw current inventory case
                DrawTexture(UITextures["inventory_case_selected"], GetScreenWidth() - 121, GetScreenHeight() - (800 - (player.InventoryIndex) * 85) - 1, Color.White);

                // Draw crosshair
                DrawTexture(UITextures["crosshair"], GetScreenWidth() / 2 - UITextures["crosshair"].Width / 2, GetScreenHeight() / 2 - UITextures["crosshair"].Height / 2, crosshairColor);

                // End drawing context
                EndDrawing();

                // Draw to previous frame render texture
                BeginTextureMode(prevTexture);

                DrawTexturePro(renderTexture.Texture, inverseSceneRectangle, sceneRectangle, Vector2.Zero, 0, Color.White);

                EndTextureMode();

                // Reset the player's box
                player.ResetBox();
            }
            CloseWindow();

            // Unload all ressources that ARE NOT from Uniray
            foreach (KeyValuePair<string, Model> utilitesList in utilities)
            {
                for (int i = 0; i < utilitesList.Value.MaterialCount; i++)
                {
                    UnloadMaterial(utilitesList.Value.Materials[i]);
                }
                UnloadModel(utilitesList.Value);
            }
            foreach (KeyValuePair<string, Model> riggedObject in rigged)
            {
                UnloadModel(riggedObject.Value);
            }
            // Unload shaders
            shaders.UnloadShaderCenter();
        }
        /// <summary>
        /// Update camera movement
        /// </summary>
        /// <param name="camera">The camera to update</param>
        /// <param name="cameraMotion">The camera motion additional variables</param>
        /// <param name="player">The player associated to the camera</param>
        static void UpdateCamera(ref Camera3D camera, ref CameraMotion cameraMotion, Player player)
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
                movement += GetCameraForward(ref camera);
            }
            if (IsKeyDown(KeyboardKey.S))
            {
                movement -= GetCameraForward(ref camera);
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

            // Final movement transformations
            if (Vector3Length(movement) > 0)
            {
                // Normalize vector
                movement = Vector3Normalize(movement) * player.SPEED * player.MotionConstraint.Value;
                // Block movement according to the motion constraint
                AddConstraintMovement(ref movement, player.MotionConstraint.Constraint);
                // Limit the movement to X and Z axis and normalize
                movement = new Vector3(movement.X, 0.0f, movement.Z);
            }

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
        static void TickPlayer(Player player, List<Animation> anims, ref Color crosshairColor, ShaderCenter shaders)
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
            }
            else if (!IsKeyDown(KeyboardKey.LeftShift))
            {
                player.SPEED = 0.1f;
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
                anims[3].Frame = 0;
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
        static void SetWorkdir(string directory)
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
        static Vector3 RotateNormalizedBone(Vector3 normalizedPos, float alpha, Vector3 pos)
        {
            // Create the new vector according to the passed parameters 
            Vector3 spacePos = new Vector3(
                normalizedPos.X * (float)Math.Cos(alpha / RAD2DEG) + normalizedPos.Z * (float)Math.Sin(alpha / RAD2DEG), normalizedPos.Y,
                -normalizedPos.X * (float)Math.Sin(alpha / RAD2DEG) + normalizedPos.Z * (float)Math.Cos(alpha / RAD2DEG)) * 3.5f + pos;
            // Return the newly calculated position
            return spacePos;
        }

        static RenderTexture2D LoadShadowMapRenderTexture(int width, int height)
        {
            RenderTexture2D target = new RenderTexture2D();

            target.Id = Rlgl.LoadFramebuffer(width, height);
            target.Texture.Width = width;
            target.Texture.Height = height;

            if (target.Id > 0)
            {
                Rlgl.EnableFramebuffer(target.Id);

                target.Depth.Id = Rlgl.LoadTextureDepth(width, height, false);
                target.Depth.Width = width;
                target.Depth.Height = height;
                target.Depth.Format = PixelFormat.CompressedPvrtRgba;
                target.Depth.Mipmaps = 1;

                Rlgl.FramebufferAttach(target.Id, target.Depth.Id, FramebufferAttachType.Depth, FramebufferAttachTextureType.Texture2D, 0);

                if (Rlgl.FramebufferComplete(target.Id)) TraceLog(TraceLogLevel.Info, "FBO: [ID %i] Framebuffer object created successfully");

                Rlgl.DisableFramebuffer();
            }
            else
            {
                TraceLog(TraceLogLevel.Info, "FBO: [ID %i] Framebuffer object can not be created");
            }

            return target;
        }
    }
}