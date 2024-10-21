using static Raylib_cs.Raylib;
using Raylib_cs;
using System.Numerics;
using Lurkers_revamped;

namespace uniray_Project.graphics
{
    public class Cutscene
    {
        public const double GRADIENT_TIME = 1.2;
        public const double LENGTH = 6.22;
        private static double _start;
        private static int TipFontSize = 26;

        private static double _gradientStart;
        private static double _gradientEnd;
        public static void Show(Texture2D background, Rectangle sceneRectangle, Rectangle inverseSceneRectangle)
        {
            _gradientStart = 0.0;
            _gradientEnd = 0.0;
            // Init timer
            _start = GetTime();
            // Show cut scene while time is not over
            while (GetTime() - _start < LENGTH)
            {
                BeginDrawing();
                ClearBackground(Color.White);
                // Draw background
                DrawTexturePro(background, inverseSceneRectangle, sceneRectangle, Vector2.Zero, 0, Color.White);
                // Draw negative gradient
                Gradient(Color.Black);
                EndDrawing();

                if (_gradientStart == 0.001f) break;
            }
        }

        public static void Gradient(Color color)
        {
            // Start if not started
            if (_gradientStart == 0.0) _gradientStart = GetTime();
            // End if length done and end not started
            double time = GetTime();
            if ((time > (_start + LENGTH - GRADIENT_TIME)) && _gradientEnd == 0.0)
            {
                _gradientEnd = GetTime();
                _gradientStart = 0.001f;
            }
            // Apply color gradient
            double delta = Raymath.Clamp(
                Raymath.Clamp((float)(GetTime() - _gradientStart), 0, (float)GRADIENT_TIME) -
                ClampZero((float)(GetTime() - _gradientEnd), 0, (float)GRADIENT_TIME),
                0f,
                (float)GRADIENT_TIME);
            int alpha = (int)(255 / (GRADIENT_TIME / delta));
            color.A = (byte)alpha;
            // Draw gradient
            DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), color);
            Vector2 textSize = MeasureTextEx(Menu.SecondaryFont, "Tip: Press TAB to see your tasks", TipFontSize, 1);
            DrawTextPro(Menu.SecondaryFont, "Tip: Press TAB to see your tasks", Program.ScreenSize / 2 - textSize / 2 + Vector2.UnitY * 400, Vector2.Zero, 0, TipFontSize, 1, new Color(255, 255, 255, alpha));
        }

        private static float ClampZero(float value, float min, float max)
        {
            if (value < min) { return min; }
            else if (value > max) { return 0; }
            return value;
        }
    }
}
