using System.Numerics;
using Raylib_cs;

namespace uniray_Project
{
    /// <summary>Represents an instance of a <see cref="ScreenCenter"/> object.</summary>
    public static class ScreenCenter
    {
        /// <summary>List of infos displayed on the screen.</summary>
        private static List<ScreenInfo> infos = new List<ScreenInfo>();

        /// <summary></summary>
        public static void DrawScreenInfos()
        {
            // Tick center
            Tick();

            // Draw non-permanent infos
            foreach (ScreenInfo info in infos.Where(x => x.Cooldown != -1.0))
            {
                double chrono = Raylib.GetTime() - info.Start;
                double alpha = Raymath.Clamp(-(int)(info.Cooldown - chrono * 1000), 0, 255);

                // Draw the different types of informations
                switch (info)
                {
                    case TextureInfo texture:
                        Raylib.DrawTexture(texture.Texture, (int)texture.X, (int)texture.Y, new Color(255, 255, 255, (int)alpha));
                        break;
                    case TextInfo text:
                        Raylib.DrawTextPro(text.Font, text.Text, text.Position, Vector2.Zero, 0, 50, 1, new Color(0, 0, 0, (int)alpha));
                        Raylib.DrawTextPro(text.Font, text.Text, text.Position, Vector2.Zero, 0, 40, 1, new Color(255, 255, 0, (int)alpha));
                        break;
                }
            }
            // Draw permanent infos
            foreach (ScreenInfo info in infos.Where(x => x.Cooldown == -1.0))
            {
                // Draw the different types of informations
                switch (info)
                {
                    case TextureInfo texture:
                        Raylib.DrawTexture(texture.Texture, (int)texture.X, (int)texture.Y, Color.White);
                        break;
                    case TextInfo text:
                        Raylib.DrawTextPro(text.Font, text.Text, text.Position, Vector2.Zero, 0, 50, 1, Color.White);
                        Raylib.DrawTextPro(text.Font, text.Text, text.Position, Vector2.Zero, 0, 40, 1, Color.White);
                        break;
                }
            }
        }

        /// <summary>Ticks the screen center.</summary>
        private static void Tick()
        {
            // Check all the infos time
            foreach (ScreenInfo info in infos)
            {
                if (info.Cooldown != -1.0)
                {
                    if (Raylib.GetTime() - info.Start >= info.Cooldown)
                    {
                        infos.Remove(info);
                        break;
                    }
                }
            }
        }

        /// <summary>Adds a new info to the screen center.</summary>
        /// <param name="info">The info to add.</param>
        public static void AddInfo(ScreenInfo info)
        {
            infos.Add(info);
        }
    }
}