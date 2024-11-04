using Uniray_Engine;
using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Lurkers_revamped
{
    public class Menu
    {
        private static bool _enabled = false;

        private static Camera3D _camera;

        private static RenderTexture2D _renderTexture;

        private static RenderTexture2D _prevTexture;

        private static Rectangle sceneRectangle;

        private static Rectangle inverseSceneRectangle;

        private static Rectangle _playButton;

        private static Rectangle _quitButton;

        public static int Width;

        public static int Height;

        public static Font Font;

        public static Font SecondaryFont;
        public static void Init()
        {
            Width = GetScreenWidth();
            Height = GetScreenHeight();

            _camera = new Camera3D()
            {
                Position = new Vector3(0, 0.2f, 0),
                Target = new Vector3(3, 1, 1),
                FovY = 45f,
                Projection = CameraProjection.Perspective,
                Up = Vector3.UnitY
            };

            sceneRectangle = new Rectangle(0, 0, Width, Height);
            inverseSceneRectangle = new Rectangle(0, 0, Width, -Height);

            _playButton = new Rectangle(Width / 2 - 200, Height / 2 - 130, 400, 120);
            _quitButton = new Rectangle(Width / 2 - 200, Height / 2 + 130, 400, 120);

            _renderTexture = LoadRenderTexture(Width, Height);
            _prevTexture = LoadRenderTexture(Width, Height);

            Font = LoadFont("src/fonts/tasks.ttf");
            SecondaryFont = LoadFont("src/fonts/OpenSans-Medium.ttf");

            _enabled = true;
        }
        public static void Show(Mesh skybox, ShaderCenter shaders, Terrain terrain)
        {
            while (_enabled)
            {
                BeginTextureMode(_renderTexture);
                ClearBackground(Color.White);
                BeginMode3D(_camera);
                // Draw the external skybox 
                Rlgl.DisableBackfaceCulling();
                Rlgl.DisableDepthMask();
                DrawMesh(skybox, shaders.SkyboxMaterial, Raymath.MatrixIdentity());
                Rlgl.EnableBackfaceCulling();
                Rlgl.EnableDepthMask();
                UnirayEngine.DrawScene();
                // Draw terrain
                DrawMesh(terrain.Mesh, terrain.Material, terrain.Transform);
                EndMode3D();
                EndTextureMode();
                // Begin blur shader 
                BeginDrawing();
                BeginShaderMode(shaders.MotionBlurShader);
                // Set previous frame render texture
                shaders.SetBlurTexture(_prevTexture);
                // Set current time
                shaders.UpdateTime((float)GetTime());
                // Draw render texture to the screen
                DrawTexturePro(_renderTexture.Texture, inverseSceneRectangle, sceneRectangle, Vector2.Zero, 0, Color.White);
                EndShaderMode();
                // Check hovering and selecting
                if (Hover(_playButton)) DrawRectangleRounded(_playButton, 0.2f, 20, new Color(0, 0, 0, 120));
                else DrawRectangleRounded(_playButton, 0.2f, 20, new Color(30, 30, 30, 120));
                if (Hover(_quitButton)) DrawRectangleRounded(_quitButton, 0.2f, 20, new Color(0, 0, 0, 120));
                else DrawRectangleRounded(_quitButton, 0.2f, 20, new Color(30, 30, 30, 120));
                // Draw 2D UI
                if (IsMouseButtonPressed(MouseButton.Left))
                {
                    if (Hover(_playButton))
                    {
                        _quitButton.X = 40;
                        break;
                    }
                    if (Hover(_quitButton))
                    {
                        Game.Close();
                        Environment.Exit(0);
                    }
                }
                DrawRectangleRoundedLines(_playButton, 0.2f, 20, 3, new Color(158, 158, 158, 240));
                DrawRectangleRoundedLines(_quitButton, 0.2f, 20, 3, new Color(158, 158, 158, 240));
                // Draw text
                Vector2 textSize = MeasureTextEx(Font, "Play", 30, 1);
                DrawTextPro(Font, "Play", _playButton.Position + _playButton.Size / 2 - textSize / 2, Vector2.Zero, 0, 30, 1, Color.White);
                DrawTextPro(Font, "Quit", _quitButton.Position + _quitButton.Size / 2 - textSize / 2, Vector2.Zero, 0, 30, 1, Color.White);
                EndDrawing();
                // Draw to previous frame render texture
                BeginTextureMode(_prevTexture);
                DrawTexturePro(_renderTexture.Texture, inverseSceneRectangle, sceneRectangle, Vector2.Zero, 0, Color.White);
                EndTextureMode();
            }

            // Show Cutscene
            Cutscene.Show(_renderTexture.Texture, sceneRectangle, inverseSceneRectangle);
            DisableCursor();
        }
        
        public static void ShowPause()
        {
            if (Hover(_quitButton)) DrawRectangleRounded(_quitButton, 0.2f, 20, new Color(0, 0, 0, 120));
            else DrawRectangleRounded(_quitButton, 0.2f, 20, new Color(30, 30, 30, 120));
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                if (Hover(_quitButton)) Environment.Exit(0);
            }
            DrawRectangleRoundedLines(_quitButton, 0.2f, 20, 3, new Color(158, 158, 158, 240));
            Vector2 textSize = MeasureTextEx(Font, "Play", 30, 1);
            DrawTextPro(Font, "Quit", _quitButton.Position + _quitButton.Size / 2 - textSize / 2, Vector2.Zero, 0, 30, 1, Color.White);
        }

        public static void Reset()
        {
            _quitButton.X = Width / 2 - 200;
        }

        public static bool Hover(Rectangle rectangle)
        {
            Vector2 mouse = GetMousePosition();
            if (mouse.X < rectangle.X + rectangle.Width && mouse.X > rectangle.X)
            {
                if (mouse.Y < rectangle.Y + rectangle.Height && mouse.Y > rectangle.Y)
                {
                    return true;
                }
            }
            return false;
        }
    }
}