using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Text;

namespace Lurkers_revamped
{
    /// <summary>Represents an instance of the running program.</summary>
    public unsafe class Program
    {
        // Window size global variables
        public static int ScreenWidth;
        public static int ScreenHeight;
        public static Vector2 ScreenSize;
        // Screen global attributes and objects
        public static RenderTexture2D RenderTexture;
        public static RenderTexture2D PreviousRenderTexture;
        public static Rectangle ScreenRectangle;
        public static Rectangle ScreenInverseRectangle;

        /// <summary>Enters the entrypoint of the program.</summary>
        /// <param name="args">Arguments passed from outside.</param>
        public  static void Main(string[] args)
        {
            // Init splash window
            InitWindow(200, 200, "Lurkers: Revamped");
            SetWindowState(ConfigFlags.UndecoratedWindow);

            // Init game
            Game.Init();

            // Load game
            Game.Load();

            // Start game
            Game.Start();

            // Update game
            Game.Update();

            // Close game
            Game.Close();
        }
        
        /// <summary>Updates the working directory of the program.</summary>
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