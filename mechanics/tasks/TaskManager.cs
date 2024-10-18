using Raylib_cs;
using System.Numerics;
using Uniray_Engine;
using uniray_Project;
using uniray_Project.graphics;

namespace Lurkers_revamped
{
    public class TaskManager
    {
        public static Color Color;

        private static List<Task> _tasks = new List<Task>();

        private static List<Task> _closedTasks = new List<Task>();

        private static Dictionary<double[], Task> _transitTasks = new Dictionary<double[], Task>();

        public static List<Task> OpenTasks = new List<Task>();

        public static bool Active;

        private static Font font;
        
        /// <summary>Loads game tasks from the database.</summary>
        public static void LoadTasks()
        {
            font = Raylib.LoadFont("src/fonts/Kanit-Bold.ttf");
            Raylib.SetTextureFilter(font.Texture, TextureFilter.Trilinear);

            DatabaseConnection connection = new DatabaseConnection("src/@tasks.db");

            _tasks = connection.Select<Task>("tasks");

            _tasks.ForEach(task => 
            {
                task.Progression = 0;
                task.ID = _tasks.IndexOf(task);
            }); 

            // Load active tasks
            do
            {
                Task task = _tasks[Random.Shared.Next(0, _tasks.Count)];
                if (!OpenTasks.Contains(task)) OpenTasks.Add(task);
            } while (OpenTasks.Count < 3);

            Active = false;

            Color = new Color(12, 70, 160, 140);
        }

        /// <summary>Updates the task manager</summary>
        public static void Update()
        {
            if (Active)
            {
                Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(0, 0, 0, 150));

                for (int i = 0; i < OpenTasks.Count; i++)
                {
                    int yPos = 70 + i * 90;
                    Raylib.DrawRectangleRounded(new Rectangle(40, yPos, 450, 80), 0.1f, 20, Color);
                    Raylib.DrawTextPro(font, OpenTasks[i].Description, new Vector2(50, yPos + 10), Vector2.Zero, 0, 23, 1, Color.White);
                    Raylib.DrawRectangle(60, yPos + 60, 410, 5, new Color(6, 30, 90, 140));
                    Raylib.DrawRectangle(60, yPos + 60, (410 / OpenTasks[i].Amount) * OpenTasks[i].Progression, 5, new Color(36, 210, 255, 220));
                }
                Menu.ShowPause();
            }
            int index = 0;
            foreach (KeyValuePair<double[], Task> task in _transitTasks)
            {
                int yPos = 70 + index * 90;

                // Calculate horizontal delta
                float deltaTime = (float)(Raylib.GetTime() - task.Key[1]);
                int xPos = 0;
                if (deltaTime < 4)
                {
                    xPos = (int)Raymath.Clamp((float)(-450 + deltaTime * 500), -450, 40);
                    task.Key[0] = Raylib.GetTime();
                }
                else
                {
                    xPos = -(int)((Raylib.GetTime() - task.Key[0]) * 500);
                    if (xPos < -450)
                    {
                        _closedTasks.Add(task.Value);
                        _transitTasks.Remove(task.Key);
                        break;
                    }
                }
                Raylib.DrawRectangleRounded(new Rectangle(xPos, yPos, 450, 80), 0.1f, 20, Color);
                Raylib.DrawTextPro(font, task.Value.Description, new Vector2(xPos + 10, yPos + 10), Vector2.Zero, 0, 23, 1, Color.White);
                Raylib.DrawRectangle(xPos + 20, yPos + 60, 410, 5, new Color(6, 30, 90, 140));
                Raylib.DrawRectangle(xPos + 20, yPos + 60, (410 / task.Value.Amount) * task.Value.Progression, 5, new Color(36, 210, 255, 220));
            }
        }

        /// <summary>Closes an open task and add a new one.</summary>
        /// <param name="id">Id of the task to close.</param>
        public static void CloseTask(int id)
        {
            OpenTasks.Where(x => x.ID == id).ToList().ForEach(x =>
            {
                //_closedTasks.Add(x);
                _transitTasks.Add(new[] {0, Raylib.GetTime()} , x);
                OpenTasks.Remove(x);
                AudioCenter.PlaySound("task_complete");
            });

            // Add new task
            if (_closedTasks.Count < 6)
            {
                do
                {
                    Task task = _tasks[Random.Shared.Next(0, _tasks.Count)];
                    if (!OpenTasks.Contains(task) && !_closedTasks.Contains(task))
                    {
                        OpenTasks.Add(task);
                    }
                } while (OpenTasks.Count < 3);
            }
        }

        public static void UpdateTask(int id, int amount)
        {
            foreach (Task task in OpenTasks)
            {
                if (task.ID == id)
                {
                    task.Progression += amount;
                    if (task.Progression >= task.Amount)
                    {
                        CloseTask(task.ID);
                        break;
                    }
                }
            }
        }


        public static bool IsActive(int id)
        {
            foreach (Task task in OpenTasks)
            {
                if (task.ID == id) return true;
            }
            return false;
        }
    }
}