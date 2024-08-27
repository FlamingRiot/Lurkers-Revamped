using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using System.Numerics;
using static UnirayEngine.UnirayEngine;
using uniray_Project;
using System.Text;
using System.Reflection.Metadata;
using UnirayEngine;
using System.Globalization;

namespace Lurkers_revamped
{
    public unsafe class Program
    {
        const float GRAVITY = 9.81f;
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

            // Init RLoading instance
            RLoading rLoading = new RLoading();

            // Init Audio Center
            AudioCenter audio = new AudioCenter();

            // Init screen center
            ScreenCenter screen = new ScreenCenter();

            // Define 3D camera
            Camera3D camera = new Camera3D();
            camera.Position = new Vector3(0.0f, 3.0f, 0.0f);
            camera.Target = new Vector3(3.0f, 3.0f, 0.0f);
            camera.Up = Vector3.UnitY;
            camera.Projection = CameraProjection.Perspective;
            camera.FovY = 60f;
            // Camera additional variables
            float yaw = 0.0f;
            float pitch = 0.0f;
            float sideShake = 0.0f;

            // Change the current directory so the embedded materials from the models can be loaded successfully
            sbyte* dir = GetApplicationDirectory();
            string workdir = new string(dir);
            string newDir = workdir + "src\\textures\\materials\\";
            SetWorkdir(newDir);
            //ChangeDirectory()
            // Load dictionary of utilities (weapons, meds, etc.)
            Dictionary<string, Model> utilities = LoadUtilities();

            // Load rigged models
            Dictionary<string, Model> rigged = rLoading.LoadRigged();

            // Load UI textures
            Dictionary<string, Texture2D> UITextures = rLoading.LoadUITextures();

            // Set the working directory back to its original value 
            SetWorkdir(workdir);
            // Load animation lists
            List<Animation> rifleAnims = LoadAnimationList("src/animations/rifle.m3d");
            List<Animation> zombieAnims = LoadAnimationList("src/animations/walker.m3d");
            // Create player
            Player player = new Player("Anonymous254");
            // Assign a default weapon to the player<
            player.CurrentWeapon = new Weapon("Lambert 1", "rifle", 50);

            // Create list of zombies
            List<Zombie> zombies = new List<Zombie>()
            {
                new Zombie(new Vector3(-10, 0, 2), "cop")
            };

            foreach (Zombie zombie in zombies)
            {
                zombie.CurrentAnimation = zombieAnims[8]; 
            }

            // Load UI Fonts
            Font damageFont = LoadFont("src/fonts/damage.ttf");

            // Set Window state when loading is done
            SetWindowState(ConfigFlags.ResizableWindow);
            SetWindowState(ConfigFlags.MaximizedWindow);
            
            // Set target FPS
            SetTargetFPS(60);
            DisableCursor();

            Console.WriteLine(workdir);

            while (!WindowShouldClose())
            {
                // Update the camera
                UpdateCamera(ref camera, 0.3f, ref yaw, ref pitch, ref sideShake, player);

                // Update the current animation of the player
                switch (player.WeaponState)
                {
                    case PlayerWeaponState.Idle:
                        player.CurrentAnimation = rifleAnims[1];
                        break;
                    case PlayerWeaponState.Shooting:
                        player.CurrentAnimation = rifleAnims[3];
                        // Double the framerate
                       // rifleAnims[3].UpdateFrame();
                        if (rifleAnims[3].Frame == 1)
                        {
                            player.CurrentWeapon.ShootBullet(new Vector3(camera.Position.X, camera.Position.Y - 0.045f, camera.Position.Z) + GetCameraRight(ref camera) / 12, GetCameraForward(ref camera)); ;
                            // Play shooting sound
                            audio.PlaySound("rifleShoot");
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
                                    // Play headshot sound
                                    audio.PlaySound("headshot");
                                    audio.PlaySound("headshot_voice");
                                    // Add screen info for the headshot
                                    Vector2 pos = new Vector2(GetScreenWidth() / 2 - UITextures["headshot"].Width / 2 + 15, 200);
                                    screen.AddInfo(new TextureInfo(pos, UITextures["headshot"], GetTime(), 0.7f));
                                    screen.AddInfo(new TextureInfo(new Vector2(pos.X + UITextures["headshot"].Width - 100, pos.Y + UITextures["headshot"].Height - 100), UITextures["plus_coin"], GetTime(), 0.7f));
                                    // Start death animation (random)
                                    if (r.Next(0, 2) == 1) zombie.State = ZombieState.Dying1;
                                    else zombie.State = ZombieState.Dying2;
                                    // Reset the collision variable
                                    player.CurrentWeapon.bullets.Last().ResetCollision();
                                }
                            }
                            player.CurrentWeapon.bullets.RemoveAt(0);
                        }
                        break;
                    case PlayerWeaponState.Reloading:
                        player.CurrentAnimation = rifleAnims[2];
                        if (rifleAnims[2].Frame == rifleAnims[2].FrameCount() - 1) player.WeaponState = PlayerWeaponState.Idle;
                        break;
                    case PlayerWeaponState.Taking:
                        player.CurrentAnimation = rifleAnims[4];
                        break;
                    case PlayerWeaponState.Hiding:
                        player.CurrentAnimation = rifleAnims[0];
                        break;
                }

                // Update model animation
                UpdateModelAnimation(utilities[player.CurrentWeapon.ModelID], player.CurrentAnimation.Anim, player.CurrentAnimation.UpdateFrame());

                // Update the player event handler
                TickPlayer(player, rifleAnims, ref camera);

                // Update the screen center (info displayer)
                screen.Tick();

                // Begin drawing context
                BeginDrawing();

                // Clear background every frame using white color
                ClearBackground(Color.Gray);

                // Begin 3D mode with the current scene's camera
                BeginMode3D(camera);

                // Draw temporary grid for development
                DrawGrid(10, 10);

                // Draw the gameobjects of the environment (from Uniray)
                DrawScene();

                // Transform the player's current model
                Matrix4x4 mTransform = TransformPlayer(yaw, pitch, sideShake);

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

                    // Debug bone drawing
                    // Vector3 posA = RotateNormalizedBone(zombie.CurrentAnimation.Anim.FramePoses[zombie.CurrentAnimation.Frame][5].Translation, zombie.Angle, zombie.Position);
                    //DrawSphere(posA, 0.3f, Color.Red);

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
                    }
                }
                // Remove the zombie from the list if killed
                if (killIndex != -1)
                {
                    zombies.RemoveAt(killIndex);

                    // Spawn a new zombie (debug sandbox only)
                    Random r = new Random();

                    Zombie zombzomb = new Zombie(new Vector3(r.Next(-50, 50), 0, r.Next(-50, 50)), "cop");
                    zombzomb.CurrentAnimation = zombieAnims[8];
                    zombies.Add(zombzomb);
                }

                // End 3D mode context
                EndMode3D();

                // Draw screen infos
                screen.DrawScreenInfos();

                // Draw crosshair
                DrawText("+", GetScreenWidth() / 2 - 4, GetScreenHeight() / 2 - 4, 20, Color.White);
                
                // Debug positions
                DrawText("Position: " + camera.Position.ToString() + "\nJump Force: " + player.VJump + "\nFrame: " + zombies.First().CurrentAnimation.Frame, 200, 200, 20, Color.Red);

                // End drawing context
                EndDrawing();
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
        }
        /// <summary>
        /// Update camera movement
        /// </summary>
        /// <param name="camera">The camera to update</param>
        /// <param name="speed">The camera speed</param>
        /// <param name="yaw">The camera yaw rotation</param>
        /// <param name="pitch">The camera pithc rotation</param>
        static void UpdateCamera(ref Camera3D camera, float speed, ref float yaw, ref float pitch, ref float sideShake, Player player)
        {
            // Calculate the camera rotation
            Vector2 mouse = GetMouseDelta();
            yaw -= mouse.X * 0.003f;
            pitch -= mouse.Y * 0.003f;

            pitch = Math.Clamp(pitch, -1.5f, 1.5f);

            // Calculate camera direction
            Vector3 direction;
            direction.X = (float)(Math.Cos(pitch) * Math.Sin(yaw));
            direction.Y = (float)Math.Sin(pitch);
            //direction.Y = 0;
            direction.Z = (float)(Math.Cos(pitch) * Math.Cos(yaw));

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
                if (sideShake < 0.15f) sideShake += 0.01f;
            }
            if (IsKeyDown(KeyboardKey.A))
            {
                movement -= GetCameraRight(ref camera);
                if (sideShake > -0.15f) sideShake -= 0.01f;
            }
            if (IsKeyUp(KeyboardKey.A) && IsKeyUp(KeyboardKey.D))
            {
                if (sideShake < 0.0f) sideShake += 0.01f;
                else if (sideShake > 0.0f) sideShake -= 0.01f;
            }
            // Normalize the vector if length is higher than 0
            if (Vector3Length(movement) > 0)
            {
                movement = Vector3Normalize(movement) * speed;
                movement = new Vector3(movement.X, 0.0f, movement.Z);
            }

            camera.Position += movement;
            camera.Target = Vector3Add(camera.Position, direction);

            // Make the camera jump if needed
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
                    player.WeaponState = PlayerWeaponState.Idle;
                    // Fix jump offset
                    camera.Position.Y = (float)Math.Round(camera.Position.Y, 3);
                }
            }
        }
        /// <summary>
        /// Load animation list from m3d file
        /// </summary>
        /// <param name="animation">The path to the file</param>
        /// <returns>The list of the animations</returns>
        static List<Animation> LoadAnimationList(string animation)
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
        /// Load all the utilities 3D models
        /// </summary>
        /// <returns>The dictionary containing all the loaded models</returns>
        static Dictionary<string, Model> LoadUtilities()
        {
            // Initialize the models dictionary 
            Dictionary<string, Model> models = new Dictionary<string, Model>();

            // Load Rifle Model
            Model rifle = LoadModel("../../models/rifle.m3d");
            for (int j = 0; j < rifle.Meshes[0].VertexCount * 4; j++)
                rifle.Meshes[0].Colors[j] = 255;
            UpdateMeshBuffer(rifle.Meshes[0], 3, rifle.Meshes[0].Colors, rifle.Meshes[0].VertexCount * 4, 0);

            models.Add("rifle", rifle);

            return models;
        }
        /// <summary>
        /// Tick the player events
        /// </summary>
        /// <param name="player">The player to check</param>
        static void TickPlayer(Player player, List<Animation> anims, ref Camera3D camera)
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
            // Aiming event
            if (IsMouseButtonDown(MouseButton.Right))
            {
                Vector3 direction = GetCameraForward(ref camera);
                camera.Position += new Vector3(direction.X, 0, direction.Z);
                camera.Target += new Vector3(direction.X, 0, direction.Z);
            }
            else if (IsMouseButtonUp(MouseButton.Right) && player.WeaponState == PlayerWeaponState.Aiming)
            {
                player.WeaponState = PlayerWeaponState.Idle;
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
                anims[3].Frame = 0;
            }
        }
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
    }
}