using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using System.Numerics;
using static UnirayEngine.UnirayEngine;
using uniray_Project;
using System.ComponentModel.Design;

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

            // Init Audio Center
            AudioCenter audio = new AudioCenter();

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

            // Load dictionary of utilities (weapons, meds, etc.)
            Dictionary<string, Model> utilities = LoadUtilities();
            // Load rifle animations
            List<Animation> rifleAnims = LoadAnimationList("src/animations/rifle.m3d");

            // Create player
            Player player = new Player("Anonymous254");
            // Assign a default weapon to the player
            player.CurrentWeapon = new Weapon("Lambert 1", "rifle", 50);

            // Test zombie models
            Model cop = LoadModel("src/models/cop.m3d");
            Model officer = LoadModel("src/models/officer.m3d");

            // Set Window state when loading is done
            SetWindowState(ConfigFlags.ResizableWindow);
            SetWindowState(ConfigFlags.MaximizedWindow);

            // Set target FPS
            SetTargetFPS(60);
            DisableCursor();            
            while (!WindowShouldClose())
            {
                // Update the camera
                UpdateCamera(ref camera, 0.3f, ref yaw, ref pitch, ref sideShake, player);

                // Update the current animation of the player
                switch (player.State)
                {
                    case PlayerState.Idle:
                        player.CurrentAnimation = rifleAnims[5];
                        break;
                    case PlayerState.Running:
                        player.CurrentAnimation = rifleAnims[1];
                        break;
                    case PlayerState.Shooting:
                        player.CurrentAnimation = rifleAnims[3];
                        // Double the framerate
                        rifleAnims[3].UpdateFrame();
                        if (rifleAnims[3].Frame == 1)
                        {
                            player.CurrentWeapon.ShootBullet(new Vector3(camera.Position.X, camera.Position.Y - 0.045f, camera.Position.Z) + GetCameraRight(ref camera) / 12, GetCameraForward(ref camera)); ;
                            // Play shooting sound
                            audio.PlaySound("rifleShoot");
                        }
                        break;
                    case PlayerState.Reloading:
                        player.CurrentAnimation = rifleAnims[2];
                        if (rifleAnims[2].Frame == rifleAnims[2].FrameCount()) player.State = PlayerState.Running;
                        break;
                    case PlayerState.Taking:
                        player.CurrentAnimation = rifleAnims[4];
                        break;
                    case PlayerState.Hiding:
                        player.CurrentAnimation = rifleAnims[0];
                        break;
                }

                // Update model animation
                UpdateModelAnimation(utilities[player.CurrentWeapon.ModelID], player.CurrentAnimation.Anim, player.CurrentAnimation.UpdateFrame());

                // Update the player event handler
                TickPlayer(player, rifleAnims);

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

                // Draw bullet rays (for debug)s
                foreach (Bullet bullet in player.CurrentWeapon.bullets)
                {
                    DrawRay(bullet.Ray, Color.Red);
                }

                // Draw debug zombie models
                DrawModel(cop, new Vector3(2, 0, 2), 3.5f, Color.White);
                DrawModel(officer, new Vector3(2, 0, 0), 3.5f, Color.White);

                // End 3D mode context
                EndMode3D();

                // Draw crosshair
                DrawText("+", GetScreenWidth() / 2 - 4, GetScreenHeight() / 2 - 4, 20, Color.White);

                // Debug positions
                DrawText("Position: " + camera.Position.ToString() + "\nJump Force: " + player.VJump, 200, 200, 20, Color.Red);

                // End drawing context
                EndDrawing();
            }
            CloseWindow();
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
            if (player.State == PlayerState.Jumping)
            {
                // Add jump force
                camera.Position.Y += player.VJump;
                camera.Target.Y += player.VJump;
                // Decrease jump force
                player.VJump -= 0.02f;
                if (camera.Position.Y <= 3)
                {
                    player.State = PlayerState.Running;
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
            Model rifle = LoadModel("src/models/rifle.m3d");
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
        static void TickPlayer(Player player, List<Animation> anims)
        {
            // Manager input events
            // Reload event
            if (IsKeyPressed(KeyboardKey.R))
            {
                player.State = PlayerState.Reloading;
            }
            // Jump event
            if (IsKeyPressed(KeyboardKey.Space))
            {
                // Prevent cross-jumping
                if (player.State != PlayerState.Jumping)
                {
                    player.State = PlayerState.Jumping;
                    player.VJump = Player.JUMP_FORCE;
                }
            }
            // Shooting event
            if (IsMouseButtonDown(MouseButton.Left))
            {
                player.State = PlayerState.Shooting;
            }
            // Stop shooting event
            else if (IsMouseButtonUp(MouseButton.Left) && player.State == PlayerState.Shooting) 
            {
                player.State = PlayerState.Running;
                anims[3].Frame = 0;
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
    }
}