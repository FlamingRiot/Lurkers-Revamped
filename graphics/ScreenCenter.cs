using System.Numerics;
using Raylib_cs;

namespace uniray_Project
{
    public class ScreenCenter
    {
        /// <summary>
        /// List of infos displayed on the screen
        /// </summary>
        private List<ScreenInfo> infos;
        /// <summary>
        /// The list of the index to remove when out of the foreach loop
        /// </summary>
        private List<int> removeIndexes;
        /// <summary>
        /// ScreenCenter constructor
        /// </summary>
        public ScreenCenter()
        {
            infos = new List<ScreenInfo>();
            removeIndexes = new List<int>();
        }
        /// <summary>
        /// Tick the screen center
        /// </summary>
        public void Tick()
        {
            // Check all the infos time
            foreach (ScreenInfo info in infos)
            {
                if (info.Cooldown != -1.0)
                {
                    if (Raylib.GetTime() - info.Start >= info.Cooldown)
                    {
                        removeIndexes.Add(infos.IndexOf(info));
                    }
                }
            }

            // Remove all the infos needed
            if (removeIndexes.Count > 0)
            {
                for (int i = 0; i < removeIndexes.Count; i++)
                {
                    infos.RemoveAt(removeIndexes[i]);
                    for (int j = 0; j < removeIndexes.Count; j++)
                    {
                        removeIndexes[j]--;
                    }
                }
                // Clear list for future use
                removeIndexes.Clear();
            }
        }
        /// <summary>
        /// Draw every active screen infos
        /// </summary>
        public void DrawScreenInfos()
        {
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
                    case uniray_Project.TextInfo text:
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
        /// <summary>
        /// Add a new info to the list
        /// </summary>
        /// <param name="info">The info to add</param>
        public void AddInfo(ScreenInfo info)
        {
            infos.Add(info);
        }
    }
}